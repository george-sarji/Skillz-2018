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
            foreach(Asteroid asteroid in livingAsteroids)
            {
                var pirate = availablePirates.Where(p => p.CanPush(asteroid)).OrderByDescending(p => AsteroidHeadingTowardsPirate(asteroid,p)).FirstOrDefault();
                if(pirate!=null && availablePirates.Where(p => p.CanPush(asteroid)).Any()&&!exceptionList.Contains(asteroid))
                {
                    var bestEnemy = game.GetEnemyLivingPirates()
                        .OrderByDescending(enemy => enemy.HasCapsule())
                        .OrderByDescending(enemy => enemy.PushReloadTurns > 0)
                        .OrderBy(enemy => enemy.Distance(asteroid)).FirstOrDefault();
                    var closestAsteroid = livingAsteroids.OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast!=asteroid).FirstOrDefault();
                    var oppositeDirection = new Location(asteroid.Location.Row*(-1),asteroid.Location.Col*(-1));
                    if(bestEnemy!=null&&!IsSelfKilling(pirate,asteroid,bestEnemy.Location)&&(closestAsteroid!=null)?(closestAsteroid.Distance(pirate)>=bestEnemy.Distance(pirate)):true)
                    {
                        // push asteroid towards the closest enemy
                        pirate.Push(asteroid,bestEnemy);
                        availablePirates.Remove(pirate);
                    }
                    else if(closestAsteroid!=null&&!IsSelfKilling(pirate,asteroid,closestAsteroid.Location))
                    {
                        // push asteroid towards the closest asteroid
                        pirate.Push(asteroid,closestAsteroid);
                        availablePirates.Remove(pirate);
                        exceptionList.Add(closestAsteroid);
                    }
                    else if(!IsSelfKilling(pirate,asteroid,asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection),pirate.PushDistance))&&asteroid.Direction.Distance(new Location(0,0))!=0)
                    {
                        // push asteroid in it's opposite direction
                        pirate.Push(asteroid,asteroid.Location.Towards(asteroid.Location.Add(oppositeDirection),pirate.PushDistance));
                        availablePirates.Remove(pirate);
                    }
                    else if(!IsSelfKilling(pirate,asteroid,GetClosestToBorder(asteroid.Location)))
                    {
                        // push asteroid towards the closest border
                        pirate.Push(asteroid,GetClosestToBorder(asteroid.Location));
                        availablePirates.Remove(pirate);
                    }
                }
            }
            
        }

        public bool AsteroidHeadingTowardsPirate(Asteroid asteroid, Pirate pirate)
        {
            return !asteroid.Location.Add(asteroid.Direction).InMap() ? false : 
            asteroid.Location.Add(asteroid.Direction).Distance(pirate) <= asteroid.Size;
        }

        public bool IsSelfKilling(Pirate pirate, Asteroid asteroid, Location pushDestination)
        {
            return !asteroid.Location.Towards(pushDestination,pirate.PushDistance).InMap() ? false :
            asteroid.Location.Towards(pushDestination,pirate.PushDistance).Distance(pirate) <= asteroid.Size;
        }
    }
}