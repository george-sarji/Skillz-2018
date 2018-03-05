using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private Location SmartSail(Pirate pirate, MapObject destination)
        {
            List<Location> candidates = new List<Location>();
            var bestOption = pirate.GetLocation();
            const int steps = 30;
            Location PirateLocation = pirate.GetLocation();
            // if ((pirate.Location.Distance(destination)) - bestOption.Distance(destination) >= (pirate.MaxSpeed / 2) && pirate.HasCapsule())
            // {
            //     var LocationOfPush = TryPushMyCapsule(pirate);
            //     if (PirateLocation != null)
            //         PirateLocation = LocationOfPush;
            // }
            for (int i = 0; i < steps; i++)
            {
                double angle = System.Math.PI * 2 * i / steps;
                double deltaX = pirate.MaxSpeed * System.Math.Cos(angle);
                double deltaY = pirate.MaxSpeed * System.Math.Sin(angle);
                Location option = new Location((int) (PirateLocation.Row - deltaY), (int) (PirateLocation.Col + deltaX));
                if (!IsInDanger(option, destination.GetLocation(), pirate) && option.InMap())
                {
                    candidates.Add(option);
                }

            }
            if (candidates.Any())
            {
                bestOption = candidates.OrderBy(option => option.Distance(destination)).First();
            }
            return bestOption;
        }

        private bool IsInDanger(Location loc, Location destination, Pirate pirate)
        {
            return IsHittingAsteroid(loc) || IsInEnemyRange(loc, pirate) || IsInWormholeDanger(loc, destination, pirate) || IsInBombRange(loc, pirate);
        }

        private bool IsHittingAsteroid(Location loc)
        {
            bool hitting = false;
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                if (loc.InRange(asteroid.Location.Add(asteroid.Direction), asteroid.Size))
                    hitting = true;
            }
            return hitting;
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
            return game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(loc, enemy.PushRange + enemy.MaxSpeed) &&
                enemy.PushReloadTurns<enemy.Steps(loc))>= myPirate.NumPushesForCapsuleLoss;
        }

        private bool IsInBombRange(Location location, Pirate pirate)
        {
            var closestBomb = game.GetAllStickyBombs().Where(bomb => !bomb.Carrier.Equals(pirate)).OrderBy(bomb => bomb.Distance(pirate)).FirstOrDefault();
            return closestBomb != null && location.InRange(closestBomb, closestBomb.ExplosionRange + pirate.MaxSpeed / 2);
        }
    }
}