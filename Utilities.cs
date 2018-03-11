using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private void Print(string s)
        {
            if (Debug)
            {
                System.Console.WriteLine(s);
            }
        }

        private static Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        private Location GetClosestToBorder(Location location)
        {
            var up = new Location(-5, location.Col);
            var down = new Location(game.Rows + 5, location.Col);
            var left = new Location(location.Row, -5);
            var right = new Location(location.Row, game.Cols + 5);

            return Closest(location, up, down, left, right);
        }

        private int Min(params int[] nums)
        {
            return nums.Min();
        }

        private void AssignDestination(Pirate pirate, Location destination)
        {
            pirateDestinations[pirate] = destination;
        }

        private int AvailablePushDistance(Pirate pirate)
        {
            return availablePirates.Where(p => p.CanPush(pirate)).Sum(p => p.PushDistance);
        }
        private int NumOfAvailablePushers(Pirate pirate)
        {
            return availablePirates.Count(p => p.CanPush(pirate));
        }

        private static bool IsOnTheWay(Location a, Location b, Location c, int buffer)
        {
            return b.Distance(c) <= a.Distance(c) && DistanceLP(a, b, c) <= buffer;
        }

        private static int DistanceLP(Location a, Location b, Location c)
        {
            int numerator = System.Math.Abs((b.Col - a.Col) * c.Row - (b.Row - a.Row) * c.Col + b.Row * a.Col - b.Col * a.Row);
            double denominator = a.Distance(b);
            return denominator == 0 ? 0 : (int) System.Math.Round(numerator / denominator);
        }
        private int NumberOfEnemiesOnTheWay(Pirate myPirate, Location b)
        {
            return game.GetEnemyLivingPirates().Count(p => IsOnTheWay(myPirate.Location, b, p.Location, p.MaxSpeed) && myPirate.Steps(p) < p.PushReloadTurns);
        }

        private int NumberOfAvailableEnemyPushers(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Count(enemy => enemy.CanPush(pirate));
        }

        private int NumberOfPushersAtLocation(Location location)
        {
            return game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(location, enemy.PushRange) && enemy.PushReloadTurns != 0);
        }

        private bool CheckIfCapsuleCanReachMothership(Pirate capsuleHolder, Mothership mothership) //Working on this Function -Mahmoud
        {
            if (mothership == null) return false;
            if (capsuleHolder.InRange(mothership, mothership.UnloadRange + capsuleHolder.MaxSpeed) &&
                NumberOfAvailableEnemyPushers(capsuleHolder) < capsuleHolder.NumPushesForCapsuleLoss &&
                NumberOfEnemiesOnTheWay(capsuleHolder, mothership.Location) < capsuleHolder.NumPushesForCapsuleLoss)
            {
                // AssignDestination(capsuleHolder, mothership.Location);
                availablePirates.Remove(capsuleHolder);
                return true;
            }
            return false;
        }

        private bool IsPirateInExtremeDanger(Pirate pirate)
        {
            // Checks if a pirate is in extreme danger, and maybe then it will be woth swapping him with a heavy pirate. Working on it for StateMachine.
            return game.GetEnemyLivingPirates().Count(enemy => enemy.Distance(pirate) < game.PushRange) > game.NumPushesForCapsuleLoss;
        }
        private bool IsCapsuleHolderInDanger(Pirate pirate)
        {
            var numOfNearbyEnemyPushers = game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(pirate, enemy.PushRange + game.PirateMaxSpeed) && enemy.PushReloadTurns <= 2);
            // Checks if the capsule holder is in danger by checking if there are enough close enemies that are in range of pushing, or close to being in range to make the capsule holder lose his capsule.
            return pirate.HasCapsule() && numOfNearbyEnemyPushers >= game.NumPushesForCapsuleLoss && numOfNearbyEnemyPushers <= game.HeavyNumPushesForCapsuleLoss;
        }

        private bool CheckIfPirateCanReach(Pirate CapsuleCapturer, Location destination) //Working on this Function -Mahmoud
        {
            if (destination == null)
                return false;
            if (CapsuleCapturer.InRange(destination, CapsuleCapturer.MaxSpeed) || NumberOfEnemiesOnTheWay(CapsuleCapturer, destination) < CapsuleCapturer.NumPushesForCapsuleLoss)
            {
                return true;
            }
            return false;
        }

        private static Location Interception(Location a, Location b, Location c)
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
        private Mothership[] GetPlayerMotherships(Player player)
        {
            return player == game.GetMyself() ? game.GetMyMotherships() : game.GetEnemyMotherships();
        }

        private Capsule[] GetPlayerCapsules(Player player)
        {
            return player == game.GetMyself() ? game.GetMyCapsules() : game.GetEnemyCapsules();
        }

        private Mothership GetBestMothership(MapObject mapObject, Player player)
        {
            // Returns the best mothership for a given player and a mapobject, taking into consideration the mothership's value multiplier and distance.
            return GetPlayerMotherships(player).OrderBy(mothership => mothership.Distance(mapObject) / mothership.ValueMultiplier).FirstOrDefault();
        }

        private Capsule GetClosestCapsule(MapObject mapObject, Player player)
        {
            // Returns the best capsule for a given player and a mapobject, taking into consideration the capsule's distance.
            return GetPlayerCapsules(player).OrderBy(capsule => capsule.InitialLocation.Distance(mapObject)).FirstOrDefault();
        }

        private int ScaleToRange(int a, int b, int c, int d, int x)
        {
            return c + (x - a) * (d - c) / ((b - a == 0) ? 1 : b - a);
        }

        private IEnumerable<Pirate> GetEnemiesInBombRange(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(pirate, game.StickyBombExplosionRange)).AsEnumerable();
        }

        private void PrintTargetLocations(List<TargetLocation> targetLocations)
        {
            Print("Target Locations:");

            string header = string.Format("{0,-11}   {1,12}{2,10}", "", "  Location  ", "Priority");
            foreach (var pirate in game.GetMyLivingPirates().OrderBy(pirate => pirate.Id))
            {
                header += string.Format("{0,10}", "Pirate " + pirate.Id);
            }
            Print(header);

            foreach (var targetLocation in targetLocations)
            {
                string line = string.Format("{0,-11} @ {1,12}{2,10}", targetLocation.Type, targetLocation.Location, targetLocation.Priority);
                foreach (var pirate in game.GetMyLivingPirates().OrderBy(pirate => pirate.Id))
                {
                    line += string.Format("{0,10}", targetLocation.ScoreForPirate(pirate));
                }
                Print(line);
            }
        }

    }
}