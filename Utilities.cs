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

        private bool IsOnTheWay(Location a, Location b, Location c, int buffer)
        {
            return b.Distance(c) <= a.Distance(c) && DistanceLP(a, b, c) <= buffer;
        }

        private int DistanceLP(Location a, Location b, Location c)
        {
            int numerator = System.Math.Abs((b.Col - a.Col) * c.Row - (b.Row - a.Row) * c.Col + b.Row * a.Col - b.Col * a.Row);
            double denominator = a.Distance(b);
            return denominator == 0 ? 0 : (int) System.Math.Round(numerator / denominator);
        }
        public int NumberOfEnemiesOnTheWay(Pirate myPirate, Location b)
        {
            return game.GetEnemyLivingPirates().Where(p => IsOnTheWay(myPirate.Location, b, p.Location, p.MaxSpeed) && myPirate.Steps(p) < p.PushReloadTurns).ToList().Count;
        }

        public int NumberOfAvailableEnemyPushers(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(pirate)).Count();
        }

        public int NumberOfPushersAtLocation(Location location)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(location, enemy.PushRange) && enemy.PushReloadTurns != 0).Count();
        }

        public bool CheckIfCapsuleCanReachMothership(Pirate capsuleHolder, Mothership mothership) //Working on this Function -Mahmoud
        {
            if (mothership == null) return false;
            if (capsuleHolder.InRange(mothership, mothership.UnloadRange * 3) &&
                NumberOfAvailableEnemyPushers(capsuleHolder) < capsuleHolder.NumPushesForCapsuleLoss &&
                NumberOfEnemiesOnTheWay(capsuleHolder, mothership.Location) < capsuleHolder.NumPushesForCapsuleLoss)
            {
                AssignDestination(capsuleHolder, mothership.Location);
                availablePirates.Remove(capsuleHolder);
                return true;
            }
            return false;
        }

        public bool IsPirateInExtremeDanger(Pirate pirate)
        {
            // Checks if a pirate is in extreme danger, and maybe then it will be woth swapping him with a heavy pirate. Working on it for StateMachine.
            return game.GetEnemyLivingPirates().Where(enemy => enemy.Distance(pirate) < game.PushRange).Count() > game.NumPushesForCapsuleLoss;
        }
        public bool IsCapsuleHolderInDanger(Pirate pirate)
        {
            // Checks if the capsule holder is in danger by checking if there are enough close enemies that are in range of pushing, or close to being in range to make the capsule holder lose his capsule.
            return pirate.HasCapsule() && game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(pirate, enemy.PushRange + game.PirateMaxSpeed)).Count() >= game.NumPushesForCapsuleLoss;
        }

        public bool CheckIfPirateCanReach(Pirate CapsuleCapturer, Location destination) //Working on this Function -Mahmoud
        {
            if (destination == null)
                return false;
            if (CapsuleCapturer.InRange(destination, CapsuleCapturer.MaxSpeed) || NumberOfEnemiesOnTheWay(CapsuleCapturer, destination) < CapsuleCapturer.NumPushesForCapsuleLoss)
            {
                return true;
            }
            return false;
        }

        public Location Interception(Location a, Location b, Location c)
        {
            int numerator = (c.Row - a.Row).Power(2) + (c.Col - a.Col).Power(2);
            int denominator = 2 * ((b.Row - a.Row) * (c.Row - a.Row) + (b.Col - a.Col) * (c.Col - a.Col));
            double s = denominator == 0 ? 0 : (double) numerator / denominator;
            var d = new Location(a.Row + (int) (s * (b.Row - a.Row)), a.Col + (int) (s * (b.Col - a.Col)));
            if (IsOnTheWay(a, b, c, game.PushRange) || c.Distance(d) < game.PirateMaxSpeed)
            {
                return a.Towards(b, game.PirateMaxSpeed);
            }
            return d;
        }
        protected Mothership[] GetPlayerMotherships(Player player)
        {
            return player == game.GetMyself() ? game.GetMyMotherships() : game.GetEnemyMotherships();
        }

        protected Capsule[] GetPlayerCapsules(Player player)
        {
            return player == game.GetMyself() ? game.GetMyCapsules() : game.GetEnemyCapsules();
        }

        protected Mothership GetBestMothership(MapObject mapObject, Player player)
        {
            // Returns the best mothership for a given player and a mapobject, taking into consideration the mothership's value multiplier and distance.
            return GetPlayerMotherships(player).OrderBy(mothership => mothership.Distance(mapObject) / mothership.ValueMultiplier).FirstOrDefault();
        }
        protected Capsule GetClosestCapsule(MapObject mapObject, Player player)
        {
            // Returns the best capsule for a given player and a mapobject, taking into consideration the capsule's distance.
            return GetPlayerCapsules(player).OrderBy(capsule => capsule.InitialLocation.Distance(mapObject)).FirstOrDefault();
        }

        private int ScaleToRange(int a, int b, int c, int d, int x)
        {
            return c + (x - a) * (d - c) / ((b - a == 0) ? 1 : b - a);
        }

        protected IEnumerable<Pirate> GetEnemiesInBombRange(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(pirate, game.StickyBombExplosionRange)).AsEnumerable();
        }

        private void PrintTargetLocations(List<TargetLocation> targetLocations)
        {
            ("Target Locations:").Print();

            string header = string.Format("{0,-11}   {1,12}{2,10}", "", "  Location  ", "Priority");
            foreach (var pirate in game.GetMyLivingPirates().OrderBy(pirate => pirate.Id))
            {
                header += string.Format("{0,10}", "Pirate " + pirate.Id);
            }
            (header).Print();

            foreach (var targetLocation in targetLocations)
            {
                string line = string.Format("{0,-11} @ {1,12}{2,10}", targetLocation.Type, targetLocation.Location, targetLocation.Priority);
                foreach (var pirate in game.GetMyLivingPirates().OrderBy(pirate => pirate.Id))
                {
                    line += string.Format("{0,10}", targetLocation.Score(pirate));
                }
                (line).Print();
            }
        }

    }
}