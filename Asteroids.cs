using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private void PushAsteroids()
        {
            var exceptionList = new List<Asteroid>();
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                var pirate = availablePirates.Where(p => p.CanPush(asteroid)).OrderByDescending(p => AsteroidHeadingTowardsPirate(asteroid, p)).FirstOrDefault();
                if (pirate != null && availablePirates.Where(p => p.CanPush(asteroid)).Any() && availableAsteroids.Contains(asteroid))
                {
                    // get the best enemy to push asteroid towards
                    var bestEnemy = game.GetEnemyLivingPirates()
                        .OrderByDescending(enemy => enemy.HasCapsule())
                        .OrderByDescending(enemy => enemy.PushReloadTurns > 0)
                        .OrderBy(enemy => enemy.Distance(asteroid)).FirstOrDefault();
                    // Get the best capsule
                    var bestCapsule = game.GetEnemyCapsules().Where(capsule => capsule.Holder != null &&
                            GetEnemyBestMothershipThroughWormholes(capsule.Holder) != null &&
                            GetOptimalAsteroidInterception(capsule.Holder, pirate, asteroid, GetEnemyBestMothershipThroughWormholes(capsule.Holder).GetLocation()) != null)
                        .OrderByDescending(capsule => capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder))).FirstOrDefault();
                    // get the closest asteroid
                    var closestAsteroid = game.GetLivingAsteroids().OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast != asteroid).FirstOrDefault();
                    // get the ooposite direction of the astroid you're pushing
                    var oppositeDirection = new Location(asteroid.Location.Row * (-1), asteroid.Location.Col * (-1));
                    // this variable is to simplify the IF below it
                    bool bestEnemyIsCloserThanClosestAsteroid = (closestAsteroid == null) || (bestEnemy != null && bestEnemy.Distance(pirate) <= closestAsteroid.Distance(pirate));
                    // the 4 "IF"s below check if the asteroid is going to kill the pirate pushing it before pushing.
                    if (bestCapsule != null &&
                        !IsSelfKilling(pirate, asteroid, GetOptimalAsteroidInterception(bestCapsule.Holder, pirate, asteroid,
                            GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder).GetLocation())))
                    {
                        var interceptionPoint = GetOptimalAsteroidInterception(bestCapsule.Holder, pirate, asteroid,
                            GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder).GetLocation());
                        pirate.Push(asteroid, interceptionPoint);
                        availablePirates.Remove(pirate);
                    }
                    else if (bestEnemy != null && !IsSelfKilling(pirate, asteroid, bestEnemy.Location) &&
                        bestEnemyIsCloserThanClosestAsteroid)
                    {
                        // push asteroid towards the closest enemy
                        pirate.Push(asteroid, bestEnemy);
                        availablePirates.Remove(pirate);
                    }
                    else if (closestAsteroid != null && !IsSelfKilling(pirate, asteroid, closestAsteroid.Location))
                    {
                        // push asteroid towards the closest asteroid
                        pirate.Push(asteroid, closestAsteroid);
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(closestAsteroid);
                    }
                    // if the asteroid is standing still dont pushing to the opposite direction.
                    else if (!IsSelfKilling(pirate, asteroid, asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection), pirate.PushDistance)) && asteroid.Direction.Distance(new Location(0, 0)) != 0)
                    {
                        // push asteroid in it's opposite direction
                        pirate.Push(asteroid, asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection), pirate.PushDistance));
                        availablePirates.Remove(pirate);
                    }
                    else if (!IsSelfKilling(pirate, asteroid, GetClosestToBorder(asteroid.Location)))
                    {
                        // push asteroid towards the closest border
                        pirate.Push(asteroid, GetClosestToBorder(asteroid.Location));
                        availablePirates.Remove(pirate);
                    }
                }
            }
        }

        private void PushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            // Push the asteroid towards: 1) capsule that can be intercepted, 2) 
        }

        private bool AsteroidHeadingTowardsPirate(Asteroid asteroid, Pirate pirate)
        {
            // check if the asteroid is heading towards the pirate
            return !asteroid.Location.Add(asteroid.Direction).InMap() ? false :
                asteroid.Location.Add(asteroid.Direction).Distance(pirate) <= asteroid.Size;
        }

        private bool IsSelfKilling(Pirate pirate, Asteroid asteroid, Location pushDestination)
        {
            // checks if the asteroid gonna kill the pirate after he pushes it to the pushDestination
            return !asteroid.Location.Towards(pushDestination, pirate.PushDistance).InMap() ? false :
                asteroid.Location.Towards(pushDestination, pirate.PushDistance).Distance(pirate) <= asteroid.Size;
        }

        private IEnumerable<TargetLocation> GetTargetLocationsAsteroids()
        {
            var AsteroidCapsulePair = new Dictionary<Asteroid, Capsule>();
            List<TargetLocation> targetAsteroids = new List<TargetLocation>();
            var availableCapsules = game.GetEnemyCapsules().Where(capsule => capsule.InitialLocation != capsule.Location);
            foreach (var asteroid in availableAsteroids.Where( a => a.Location.Add(a.Direction) == a.Location))
            {
                Capsule bestCapsule = null;
                foreach (Capsule capsule in availableCapsules)
                {
                    var bestMothership = GetEnemyBestMothershipThroughWormholes(capsule.Holder);
                    int distance = asteroid.Steps(capsule) + capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder));
                    if (bestCapsule == null ||
                        (asteroid.Steps(capsule) < capsule.Holder.Steps(bestMothership)) &&
                        distance < bestCapsule.Distance(asteroid) + bestCapsule.Holder.Distance(GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder)))
                    {
                        bestCapsule = capsule;
                    }
                }
                if (bestCapsule != null &&
                    availablePirates.OrderBy(p => p.Steps(asteroid)).FirstOrDefault().Steps(asteroid) <
                    bestCapsule.Holder.Distance(GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder)))
                {
                    AsteroidCapsulePair.Add(asteroid, bestCapsule);
                }
            }
            AsteroidCapsulePair.OrderBy( entry => entry.Key.Steps(entry.Value) + entry.Value.Holder.Steps(GetEnemyBestMothershipThroughWormholes(entry.Value.Holder)));
            int count = 1;
            foreach( var entry in AsteroidCapsulePair)
            {
                targetAsteroids.Add(new TargetLocation(entry.Key.Location,LocationType.Asteroid,count,entry.Key));
                count++;
            }
            return targetAsteroids;
        }

        private Location GetOptimalAsteroidInterception(Pirate enemy, Pirate friendly, Asteroid asteroid, Location destination)
        {
            var steps = enemy.Steps(destination.Towards(enemy, enemy.MaxSpeed));
            var asteroidLocation = asteroid.Location;
            for (int i = 0; i < steps; i++)
            {
                // Get the location.
                var enemyLocation = enemy.Location.Towards(destination, enemy.MaxSpeed * i);
                if (asteroidLocation.Towards(enemyLocation, friendly.PushDistance + asteroid.Speed * i).InRange(enemyLocation, (int) (asteroid.Size * 0.8)))
                    return enemyLocation;
            }
            return null;
        }
    }
}