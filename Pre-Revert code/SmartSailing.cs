using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public Location SmartSail(Pirate pirate, MapObject destination)
        {
            List<Location> candidates = new List<Location>();
            var bestOption = pirate.GetLocation();
            const int steps = 50;
            Location PirateLocation = pirate.GetLocation();
            if ((pirate.Location.Distance(destination)) - bestOption.Distance(destination) >= (pirate.MaxSpeed / 2) && pirate.HasCapsule())
            {
                var LocationOfPush = TryPushMyCapsule(pirate);
                if (PirateLocation != null)
                    PirateLocation = LocationOfPush;
            }
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

        public bool IsInDanger(Location loc, Location destination, Pirate pirate)
        {
            return IsHittingAsteroid(loc) || IsInRangeOfEnemy(loc, pirate) || IsInWormholeDanger(loc, destination, pirate);
        }

        public bool IsHittingAsteroid(Location loc)
        {
            bool hitting = false;
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                if (loc.InRange(asteroid.Location.Add(asteroid.Direction), asteroid.Size))
                    hitting = true;
            }
            return hitting;
        }

        public bool IsInWormholeDanger(Location location, Location destination, Pirate pirate)
        {
            // Get the closest wormhole to the current locatio.
            var wormholes = game.GetAllWormholes()
                .Where(wormhole => wormhole.TurnsToReactivate < pirate.Steps(destination) / 4 &&
                    wormhole.InRange(location, wormhole.WormholeRange));
            var closestWormhole = wormholes.FirstOrDefault();
            if (closestWormhole == null)
                return false;
            else if (closestWormhole.Equals(GetBestWormhole(destination, pirate)))
                return false;
            return true;
        }

        public bool IsInRangeOfEnemy(Location loc, Pirate myPirate)
        {
            int count = 0;
            foreach (Pirate pirate in game.GetEnemyLivingPirates())
            {
                if (pirate.InRange(loc, pirate.PushRange + pirate.MaxSpeed) && pirate.PushReloadTurns < pirate.Steps(loc))
                {
                    count++;
                }
            }
            return count >= myPirate.NumPushesForCapsuleLoss;
        }
    }
}