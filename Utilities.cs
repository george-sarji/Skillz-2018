using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        protected static int DistanceThroughWormhole(Location from, MapObject to, Wormhole wormhole, IEnumerable<Wormhole> wormholes)
        {
            return from.Distance(wormhole) +
                ClosestDistance(wormhole.Partner.Location, to,
                    wormholes.Where(w => w.Id != wormhole.Id && w.Id != wormhole.Partner.Id));
        }

        protected static int ClosestDistance(Location from, MapObject to, IEnumerable<Wormhole> wormholes)
        {
            if (wormholes.Any())
            {
                int distanceWithoutWormholes = from.Distance(to);
                int distanceWithWormholes = wormholes
                    .Select(wormhole => DistanceThroughWormhole(from, to, wormhole, wormholes))
                    .Min();
                return System.Math.Min(distanceWithoutWormholes, distanceWithWormholes);
            }
            return from.Distance(to);
        }

        protected Wormhole GetBestWormhole(Location destination, Pirate pirate)
        {
            var wormholeDistances = new Dictionary<Wormhole, int>();
            var wormholes = game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < pirate.Steps(destination) / 4);
            foreach (var wormhole in wormholes)
            {
                //    Assign the closest distance for the wormhole
                wormholeDistances.Add(wormhole, DistanceThroughWormhole(pirate.Location, destination, wormhole, wormholes));
            }
            //    Get the minimum
            var bestWormhole = wormholeDistances.OrderBy(map => map.Value).FirstOrDefault();
            if (bestWormhole.Key != null)
            {
                // Check the regular distance.
                var normalDistance = pirate.Distance(destination);
                if (bestWormhole.Value < normalDistance)
                    return bestWormhole.Key;
            }
            return null;
        }

        protected Mothership GetBestMothershipThroughWormholes(Pirate pirate)
        {
            var mothershipWormholes = new Dictionary<Mothership, int>();
            Mothership bestMothership = null;
            int distance = int.MaxValue;
            foreach (var mothership in game.GetEnemyMotherships())
            {
                var distances = new List<int>();
                foreach (var wormhole in game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < pirate.Steps(mothership) / 4))
                {
                    var distanceThroughCurrent = DistanceThroughWormhole(pirate.Location, mothership.Location, wormhole, game.GetAllWormholes().Where(hole => hole.TurnsToReactivate < pirate.Steps(mothership) / 4));
                    distances.Add(distanceThroughCurrent);
                }
                var normalDistance = pirate.Distance(mothership);
                if (distances.Any() && distances.Min() < distance)
                {
                    bestMothership = mothership;
                    distance = distances.Min();
                }
                if (distances.Any() && normalDistance < distance)
                {
                    bestMothership = mothership;
                    distance = normalDistance;
                }
            }
            if (bestMothership == null)
            {
                bestMothership = game.GetEnemyMotherships().OrderBy(mothership => pirate.Steps(mothership) / (int) ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();
            }
            return bestMothership;
        }

        protected static Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        protected Location GetClosestToBorder(Location location)
        {
            var up = new Location(-5, location.Col);
            var down = new Location(game.Rows + 5, location.Col);
            var left = new Location(location.Row, -5);
            var right = new Location(location.Row, game.Cols + 5);

            return Closest(location, up, down, left, right);
        }

        protected int Min(params int[] nums)
        {
            return nums.Min();
        }

        protected void AssignDestination(Pirate pirate, Location destination)
        {
            pirateDestinations[pirate] = destination;
        }

        protected int AvailablePushDistance(Pirate pirate)
        {
            return availablePirates.Where(p => p.CanPush(pirate)).Sum(p => p.PushDistance);
        }
        protected int NumOfAvailablePushers(Pirate pirate)
        {
            return availablePirates.Where(p => p.CanPush(pirate)).Count();
        }
    }
}