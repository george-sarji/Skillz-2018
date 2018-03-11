using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private void PushAsteroids()
        {
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                var pirate = availablePirates.Where(p => p.CanPush(asteroid)).OrderByDescending(p => AsteroidHittingPirate(asteroid, p)).FirstOrDefault();
                if (pirate != null && availableAsteroids.Contains(asteroid))
                {
                    // get the best enemy to push asteroid towards
                    var bestEnemy = game.GetEnemyLivingPirates()
                        .OrderByDescending(enemy => enemy.HasCapsule())
                        .OrderByDescending(enemy => enemy.PushReloadTurns - asteroid.Steps(enemy) > 0)
                        .FirstOrDefault();//fixed
                    // Get the best capsule
                    var bestCapsule = game.GetEnemyCapsules().Where(capsule => capsule.Holder != null &&
                            GetEnemyBestMothershipThroughWormholes(capsule.Holder) != null &&
                            GetOptimalAsteroidInterception(capsule.Holder, pirate, asteroid, GetEnemyBestMothershipThroughWormholes(capsule.Holder).GetLocation()) != null)
                        .OrderBy(capsule => capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder))).FirstOrDefault();//Fix: orderbyDescending to normal
                    // get the closest asteroid
                    var closestAsteroid = game.GetLivingAsteroids().OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast != asteroid).FirstOrDefault();//maybe we should try pushing towards an asteroid that might threaten our pirates
                    // get the ooposite direction of the astroid you're pushing
                    var oppositeDirection = asteroid.Direction.Multiply(-1);
                    // this variable is to simplify the IF below it
                    bool bestEnemyIsCloserThanClosestAsteroid = (closestAsteroid == null) || (bestEnemy != null && bestEnemy.Steps(asteroid) <= closestAsteroid.Steps(asteroid));//Fix: changed pirate to asteroid
                    // the 4 "IF"s below check if the asteroid is going to kill the pirate pushing it before pushing.
                    if (bestCapsule != null &&
                        !IsSelfKilling(pirate, asteroid, GetOptimalAsteroidInterception(bestCapsule.Holder, pirate, asteroid,
                            GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder).GetLocation())))//Fix: in each if we added availableAsteroids.Remove
                    {
                        var interceptionPoint = GetOptimalAsteroidInterception(bestCapsule.Holder, pirate, asteroid,
                            GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder).GetLocation());
                        pirate.Push(asteroid, interceptionPoint);
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(asteroid);
                    }
                    else if (bestEnemy != null && !IsSelfKilling(pirate, asteroid, bestEnemy.Location) &&
                        bestEnemyIsCloserThanClosestAsteroid)
                    {
                        // push asteroid towards the closest enemy
                        pirate.Push(asteroid, bestEnemy);
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(asteroid);
                    }
                    else if (closestAsteroid != null && !IsSelfKilling(pirate, asteroid, closestAsteroid.Location))
                    {
                        // push asteroid towards the closest asteroid
                        pirate.Push(asteroid, closestAsteroid);
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(closestAsteroid);
                        availableAsteroids.Remove(asteroid);
                    }
                    // if the asteroid is standing still dont pushing to the opposite direction.
                    else if (!IsSelfKilling(pirate, asteroid, asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection), pirate.PushDistance)) && asteroid.Direction.Distance(new Location(0, 0)) != 0)
                    {
                        // push asteroid in it's opposite direction
                        pirate.Push(asteroid, asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection), pirate.PushDistance));
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(asteroid);
                    }
                    else if (!IsSelfKilling(pirate, asteroid, GetClosestToBorder(asteroid.Location)) && AsteroidHittingPirate(asteroid, pirate))//Fix: Added is hitting asteroid
                    {
                        // push asteroid towards the closest border
                        pirate.Push(asteroid, GetClosestToBorder(asteroid.Location));
                        availablePirates.Remove(pirate);
                        availableAsteroids.Remove(asteroid);
                    }
                    else if(AsteroidHittingPirate(asteroid, pirate))// Fix: avoid pushing the asteroid if we
                    {
                        var bestOption = asteroid.Location;
                        for (int i = 0; i < CircleSteps; i++)
                        {
                            double angle = System.Math.PI * 2 * i / CircleSteps;
                            double deltaX = pirate.PushDistance * System.Math.Cos(angle);
                            double deltaY = pirate.PushDistance * System.Math.Sin(angle);
                            Location newAsteroidLocation = new Location((int) (asteroid.Location.Row - deltaY), (int) (asteroid.Location.Col + deltaX));
                            if(!newAsteroidLocation.InMap() &&!(game.GetMyLivingPirates().Where(p => newAsteroidLocation.Distance(p) <= asteroid.Size).Any()))//Fix: removed in map from inside the where
                            {
                                pirate.Push(asteroid, newAsteroidLocation);
                                availableAsteroids.Remove(asteroid);
                                availablePirates.Remove(pirate);
                                break;
                            }
                        }
                    }

                }
            }
        }

        private bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            if (!pirate.CanPush(asteroid)) return false;
            // Push the asteroid towards: 1) capsule that can be intercepted, 2)
            // Get the best capsule.
            var bestCapsule = game.GetEnemyCapsules().Where(capsule => capsule.Holder != null &&
                    GetEnemyBestMothershipThroughWormholes(capsule.Holder) != null &&
                    GetOptimalAsteroidInterception(capsule.Holder, pirate, asteroid, GetEnemyBestMothershipThroughWormholes(capsule.Holder).GetLocation()) != null)
                .OrderBy(capsule => capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder))).FirstOrDefault();//Fix: changed orderby to normal because we should target the closest capsule to the mothership that we can hit
            var bestGrouping = game.GetEnemyLivingPirates().OrderByDescending(enemy => enemy.PushReloadTurns).
            OrderBy(enemy => game.GetEnemyLivingPirates().Count(enemyPirate => enemyPirate.InRange(enemy, asteroid.Size))).FirstOrDefault();
            if (bestCapsule != null)
            {
                // Push towards the capsule
                var pushLocation = GetOptimalAsteroidInterception(bestCapsule.Holder, pirate, asteroid, GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder).Location);
                pirate.Push(asteroid, pushLocation);
                Print(pirate + " pushes asteroid " + asteroid + " towards " + pushLocation + " to intercept " + bestCapsule);
                return true;
            }
            else if (bestGrouping != null)
            {
                // Push towards the grouping
                var pushLocation = bestGrouping.Location;
                pirate.Push(asteroid, pushLocation);
                Print(pirate + " pushes asteroid " + asteroid + " towards " + pushLocation);
                return true;
            }
            return false;
        }

        private bool AsteroidHittingPirate(Asteroid asteroid, Pirate pirate)
        {
            // check if the asteroid is heading towards the pirate
            return !asteroid.Location.Add(asteroid.Direction).InMap() ? false :
                asteroid.Location.Add(asteroid.Direction).Distance(pirate) < asteroid.Size; //Fix: changed <= to <
        }

        private bool IsSelfKilling(Pirate pirate, Asteroid asteroid, Location pushDestination)
        {
            // checks if the asteroid gonna kill the pirate after he pushes it to the pushDestination
            return !asteroid.Location.Towards(pushDestination, pirate.PushDistance).InMap() ? false :
                asteroid.Location.Towards(pushDestination, pirate.PushDistance).Distance(pirate) <= asteroid.Size;
        }

        private IEnumerable<TargetLocation> GetTargetLocationsAsteroids()
        {
            List<TargetLocation> targetAsteroids = new List<TargetLocation>();
            var availableCapsules = game.GetEnemyCapsules().Where(capsule => capsule.Holder != null).ToList();
            int count = 1;
            while (availableAsteroids.Where(a => a.Location.Add(a.Direction).Distance(a.Location) == 0).Any())
            {
                Capsule bestCapsule = null;
                Asteroid bestAsteroid = null;
                int bestScore = 0;
                foreach (var asteroid in availableAsteroids.Where(a => a.Location.Add(a.Direction).Distance(a.Location) == 0))
                {
                    var bestCapsuleForAsteroid = GetBestCapsuleForAsteroid(asteroid, availableCapsules);
                    if (bestCapsuleForAsteroid == null)
                        break;
                    var score = asteroid.Steps(bestCapsuleForAsteroid) +
                        bestCapsuleForAsteroid.Holder.Steps(GetEnemyBestMothershipThroughWormholes(bestCapsuleForAsteroid.Holder));
                    if (bestCapsule == null || bestAsteroid == null || score < bestScore)
                    {
                        bestScore = score;//Fix: replace it with score
                        bestCapsule = bestCapsuleForAsteroid;
                        bestAsteroid = asteroid;
                    }
                }
                if(bestCapsule == null || bestAsteroid == null)
                    break;
                targetAsteroids.Add(new TargetLocation(bestAsteroid.Location, LocationType.Asteroid, count, bestAsteroid, this));
                availableAsteroids.Remove(bestAsteroid);
                availableCapsules.Remove(bestCapsule);
                count++;
            }
            return targetAsteroids;
        }

        private Location GetOptimalAsteroidInterception(Pirate enemy, Pirate friendly, Asteroid asteroid, Location destination)
        {
            if(asteroid.InRange(enemy,asteroid.Size))
                return null;
            var steps = enemy.Steps(destination.Towards(enemy, enemy.MaxSpeed));
            var asteroidLocation = asteroid.Location;
            for (int i = 1; i < steps; i++)//Fix: changed I to 1
            {
                // Get the location.
                var enemyLocation = enemy.Location.Towards(destination, enemy.MaxSpeed * i);
                if (asteroidLocation.Towards(enemyLocation, friendly.PushDistance + asteroid.Speed * (i-1)).InRange(enemyLocation, asteroid.Size))
                    return enemyLocation;
            }
            return null;
        }

        private Capsule GetBestCapsuleForAsteroid(Asteroid asteroid, IEnumerable<Capsule> availableCapsules) //takes available capsule with carriers and returns the best for the asteroid.
        {
            return availableCapsules
                .Where(capsule => asteroid.Steps(capsule) < capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder)))
                .OrderBy(capsule => asteroid.Steps(capsule) + capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder))).FirstOrDefault();
        }
    }
}