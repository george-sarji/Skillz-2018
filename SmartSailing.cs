using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private Location SmartSail(Pirate pirate, MapObject destination)
        {
            var candidates = new List<Location>();
            // if (!IsInDanger(pirate.Location, destination.GetLocation(), pirate)) {
            //     candidates.Add(pirate.Location);
            // }
            for (int i = 0; i < CircleSteps; i++)
            {
                double angle = System.Math.PI * 2 * i / CircleSteps;
                double deltaX = pirate.MaxSpeed * System.Math.Cos(angle);
                double deltaY = pirate.MaxSpeed * System.Math.Sin(angle);
                Location option = new Location((int) (pirate.Location.Row - deltaY), (int) (pirate.Location.Col + deltaX));
                if (option.InMap() && !IsInDanger(option, destination.GetLocation(), pirate))
                {
                    candidates.Add(option);
                }

            }

            var bestOption = candidates.Any() ?
                candidates.OrderBy(option => option.Distance(destination)).First() :
                destination.GetLocation();

            return bestOption;
        }

        private bool IsInDanger(Location loc, Location destination, Pirate pirate)
        {
            return IsHittingAsteroid(loc) || IsInEnemyRange(loc, pirate) || IsInWormholeDanger(loc, destination, pirate) || IsInBombRange(loc, pirate);
        }

        private bool IsHittingAsteroid(Location loc)
        {
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                if (loc.InRange(asteroid.Location.Add(asteroid.Direction), asteroid.Size))
                    return true;
            }
            return false;
        }

        private bool IsInWormholeDanger(Location location, Location destination, Pirate pirate)
        {
            // Dangerous wormholes are (1) active (2) close to location (3) and not my destination.
            var dangerousWormholes = game.GetAllWormholes()
                .Where(wormhole =>
                    wormhole.TurnsToReactivate <= 1 &&
                    wormhole.InRange(location, wormhole.WormholeRange) &&
                    wormhole.Location != destination);
            return dangerousWormholes.Any();
        }

        private bool IsInEnemyRange(Location loc, Pirate myPirate)
        {
            return (!myPirate.HasCapsule()) ? false : game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(loc, enemy.PushRange + enemy.MaxSpeed) &&
                enemy.PushReloadTurns<enemy.Steps(loc))>= myPirate.NumPushesForCapsuleLoss;
        }

        private bool IsInBombRange(Location location, Pirate pirate)
        {
            var closestBomb = game.GetAllStickyBombs().Where(bomb => !bomb.Carrier.Equals(pirate)).OrderBy(bomb => bomb.Distance(pirate)).FirstOrDefault();
            return closestBomb != null && location.InRange(closestBomb, closestBomb.ExplosionRange + pirate.MaxSpeed / 2);
        }
    }
}