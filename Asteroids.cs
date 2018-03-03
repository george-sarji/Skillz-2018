using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void PushAsteroids()
        {
            var exceptionList = new List<Asteroid>();
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                var pirate = availablePirates.Where(p => p.CanPush(asteroid)).OrderByDescending(p => AsteroidHeadingTowardsPirate(asteroid, p)).FirstOrDefault();
                if (pirate != null && availablePirates.Where(p => p.CanPush(asteroid)).Any() && !exceptionList.Contains(asteroid))
                {
                    // get the best enemy to push asteroid towards
                    var bestEnemy = game.GetEnemyLivingPirates()
                        .OrderByDescending(enemy => enemy.HasCapsule())
                        .OrderByDescending(enemy => enemy.PushReloadTurns > 0)
                        .OrderBy(enemy => enemy.Distance(asteroid)).FirstOrDefault();
                    // get the closest asteroid
                    var closestAsteroid = game.GetLivingAsteroids().OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast != asteroid).FirstOrDefault();
                    // get the ooposite direction of the astroid you're pushing
                    var oppositeDirection = new Location(asteroid.Location.Row * (-1), asteroid.Location.Col * (-1));
                    // this variable is to simplify the IF below it
                    bool bestEnemyIsCloserThanClosestAsteroid = (closestAsteroid == null) || (bestEnemy != null && bestEnemy.Distance(pirate) <= closestAsteroid.Distance(pirate));
                    // the 4 "IF"s below check if the asteroid is going to kill the pirate pushing it before pushing.
                    if (bestEnemy != null && !IsSelfKilling(pirate, asteroid, bestEnemy.Location) &&
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
                        exceptionList.Add(closestAsteroid);
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

        public bool AsteroidHeadingTowardsPirate(Asteroid asteroid, Pirate pirate)
        {
            // check if the asteroid is heading towards the pirate
            return !asteroid.Location.Add(asteroid.Direction).InMap() ? false :
                asteroid.Location.Add(asteroid.Direction).Distance(pirate) <= asteroid.Size;
        }

        public bool IsSelfKilling(Pirate pirate, Asteroid asteroid, Location pushDestination)
        {
            // checks if the asteroid gonna kill the pirate after he pushes it to the pushDestination
            return !asteroid.Location.Towards(pushDestination, pirate.PushDistance).InMap() ? false :
                asteroid.Location.Towards(pushDestination, pirate.PushDistance).Distance(pirate) <= asteroid.Size;
        }

        private IEnumerable<TargetLocation> GetTargetLocationsAsteroids()
        {
            IEnumerable<Asteroid> targetAsteroids = null;
            List<TargetLocation> targetLocations = new List<TargetLocation>();
            // foreach(Mothership enemyShip in game.GetEnemyMotherships())
            // {
            //     Capsule closestCapsuleToMothership = game.GetEnemyCapsules().Where(cap => cap.Holder!=null)
            //         .OrderBy(cap => cap.Distance(enemyShip)).FirstOrDefault();
            //     targetAsteroids = game.GetLivingAsteroids().Where(ast => !usedAsteroids.Contains(ast))
            //         .Where(ast => ast.Distance(enemyShip)<=closestCapsuleToMothership.Distance(enemyShip));
            //     foreach(Asteroid asteroid in targetAsteroids)
            //     {
            //         targetLocations.Add(new TargetLocation(asteroid.Location,LocationType.Asteroid,(int)(closestCapsuleToMothership.Distance(enemyShip)/asteroid.Distance(enemyShip)),asteroid));
            //     }
            // }
            return targetLocations;
        }
    }
}