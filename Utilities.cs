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

        public bool CheckIfCapsuleCanReach(Pirate capsuleHolder, Mothership mothership) //Working on this Function -Mahmoud
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

        public bool CheckIfCapturerCanReach(Pirate CapsuleCapturer, Location destination) //Working on this Function -Mahmoud
        {
            if (destination == null) return false;
            if (CapsuleCapturer.InRange(destination, CapsuleCapturer.MaxSpeed) &&
                NumberOfAvailableEnemyPushers(CapsuleCapturer) < CapsuleCapturer.NumPushesForCapsuleLoss &&
                NumberOfEnemiesOnTheWay(CapsuleCapturer, destination) < CapsuleCapturer.NumPushesForCapsuleLoss)
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
            return GetPlayerCapsules(player).OrderBy(capsule => capsule.Distance(mapObject)).FirstOrDefault();
        }
        protected int GetWormholeScoreForPlayer(Location wormholeLocation, Location partnerLocation, Player player)
        {
            if (!GetPlayerMotherships(player).Any() || !GetPlayerCapsules(player).Any())
            {
                return 0;
            }

            // Returns a player's wormhole's score, taking into consideration its location as well as its partner's. Returns -1 if no capsule and mothership exist for the user.
            var closestMothershipToWormhole = GetBestMothership(wormholeLocation, player);
            var closestMothershipToPartner = GetBestMothership(partnerLocation, player);
            var closestCapsuleToWormhole = GetClosestCapsule(wormholeLocation, player);
            var closestCapsuleToPartner = GetClosestCapsule(partnerLocation, player);

            // Option 1: Wormholes goes towards mothership. Partner goes towards capsule.
            int option1 = wormholeLocation.Distance(closestMothershipToWormhole) + partnerLocation.Distance(closestCapsuleToPartner);

            // Option 2: Wormholes goes towards capsule. Partner goes towards mothership.
            int option2 = wormholeLocation.Distance(closestCapsuleToWormhole) + partnerLocation.Distance(closestMothershipToPartner);

            return System.Math.Min(option1, option2);
        }

        // Returns how good having a wormhole in this location is. Lower is better (distances).
        protected int GetWormholeScore(Location wormholeLocation, Location partnerLocation)
        {
            // Gets the total score for a wormhole by finding our score as well as the enemy's.
            var enemyScore = GetWormholeScoreForPlayer(wormholeLocation, partnerLocation, game.GetEnemy());
            var myScore = GetWormholeScoreForPlayer(wormholeLocation, partnerLocation, game.GetMyself());
            return myScore - enemyScore;
        }

        // How many angles to check when circling around a location. 24 steps = 15 degrees steps (360/24 = 15).
        private const int CircleSteps = 24;

        protected Location BestWormholePushLocation(Wormhole wormhole)
        {
            // Returns the most favorable position to push the wormhole to, taking into consideration position of capsules and motherships.
            var bestOption = wormhole.Location;
            var bestOptionScore = GetWormholeScore(wormhole.Location, wormhole.Partner.Location);
            for (int i = 0; i < CircleSteps; i++)
            {
                double angle = System.Math.PI * 2 * i / CircleSteps;
                double deltaX = game.PushDistance * System.Math.Cos(angle);
                double deltaY = game.PushDistance * System.Math.Sin(angle);
                Location newWormholeLocation = new Location((int) (wormhole.Location.Row - deltaY), (int) (wormhole.Location.Col + deltaX));
                int newLocationScore = GetWormholeScore(newWormholeLocation, wormhole.Partner.Location);
                if (newLocationScore < bestOptionScore)
                {
                    bestOption = newWormholeLocation;
                    bestOptionScore = newLocationScore;
                }
            }

            return bestOption;
        }

        private bool MakesSenseToPushWormhole(Wormhole wormhole)
        {
            var bestPushLocation = BestWormholePushLocation(wormhole);
            return bestPushLocation != wormhole.Location;
        }

        private int GetWormholePriority(Wormhole wormhole)
        {
            int currentScore = GetWormholeScore(wormhole.Location, wormhole.Partner.Location);
            int improvedScore = GetWormholeScore(BestWormholePushLocation(wormhole), wormhole.Partner.Location);

            int improvementDelta = currentScore - improvedScore;
            int maxPossibleImprovement = game.PushDistance * 2; // Moved PushDistance away from enemy locations towards our locations.

            int bonus = wormhole.TurnsToReactivate == 0 ? 1 : 0;
            return 0; // TODO: Scale improvementDelta / maxPossibleImprovement to priorities range and add/subtract bonus.
        }

        private IEnumerable<TargetLocation> GetTargetLocationsWormholes()
        {
            var targetLocations = new List<TargetLocation>();
            foreach(Wormhole wormhole in game.GetAllWormholes().Where(wormhole => MakesSenseToPushWormhole(wormhole)))
            {
                var targetLocation = new TargetLocation(wormhole.Location, LocationType.Wormhole, GetWormholePriority(wormhole));
                targetLocations.Add(targetLocation);
            }
            return targetLocations;
        }

    }
}