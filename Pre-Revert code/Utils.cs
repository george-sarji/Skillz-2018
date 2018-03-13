using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {

        public static int Max(params int[] numbers)
        {
            return numbers.OrderByDescending(num => num).First();
        }

        public static int Min(params int[] numbers)
        {
            return numbers.OrderBy(num => num).First();
        }

        public static Location IntersectionPoint(Location enemyLoc, Location myLoc, Location destLoc, int S1, int S2)
        {
            int Xa = enemyLoc.Col, Ya = enemyLoc.Row,
                Xb = destLoc.Col, Yb = destLoc.Row,
                Xc = myLoc.Col, Yc = myLoc.Row;
            double a = ((Xb - Xa).Power(2) + (Yb - Ya).Power(2)) / S2.Power(2) - (((Xb - Xa).Power(2) + (Yb - Ya).Power(2)) / S1.Power(2));
            double b = 2 * ((Xb - Xa) * (Xa - Xc) + (Yb - Ya) * (Ya - Yc)) / S2.Power(2);
            double c = ((Xa - Xc).Power(2) + (Ya - Yc).Power(2)) / S2.Power(2);
            double T1 = -b + (b.Power(2) - 4 * a * c).Sqrt();
            double T2 = -b - (b.Power(2) - 4 * a * c).Sqrt();
            if (T1 <= 1 && T1 >= 0)
                return new Location((int) (Ya + T1 * (Yb - Ya)), (int) (Xa + T1 * (Xb - Xa)));
            else if (T2 <= 1 && T2 >= 0)
                return new Location((int) (Ya + T2 * (Yb - Ya)), (int) (Xa + T2 * (Xb - Xa)));
            return null;
        }

        // Given an enemy pirate's location a, and destination b, and a friendly pirate's location c,
        // calculates and returns the optimal point d for the friendly pirate to intercept the path of the enemy pirate.
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
        // Returns true if the point c is within buffer distance to the line from a and b.
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
            return game.GetEnemyLivingPirates().Where(p => IsOnTheWay(myPirate.Location,b,p.Location,p.MaxSpeed) && myPirate.Steps(p)<p.PushReloadTurns ).ToList().Count;
        }

        public int NumberOfAvailableEnemyPushers(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(pirate)).Count();
        }

        public int NumberOfPushersAtLocation(Location location)
        {
            return game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(location, enemy.PushRange) && enemy.PushReloadTurns != 0).Count();
        }

        public static Location MidPoint(Pirate pirate1, Pirate pirate2)
        {
            int x1 = pirate1.Location.Col, x2 = pirate2.Location.Col;
            int y1 = pirate1.Location.Row, y2 = pirate2.Location.Row;
            return new Location((y1 + y2) / 2, (x1 + x2) / 2);
        }

        public static int DistanceThroughWormhole(Location from, MapObject to, Wormhole wormhole, IEnumerable<Wormhole> wormholes)
        {
            return from.Distance(wormhole) +
                ClosestDistance(wormhole.Partner.Location, to,
                    wormholes.Where(w => w.Id != wormhole.Id && w.Id != wormhole.Partner.Id));
        }
        public static int WormholePossibleLocationDistance(Location from, Location to, Location wormhole, Location partner)
        {
            return Min(from.Distance(wormhole) + to.Distance(partner), from.Distance(partner) + to.Distance(wormhole));
        }
        public static int ClosestDistance(Location from, MapObject to, IEnumerable<Wormhole> wormholes)
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
        public bool CapsuleHolderInDanger (Pirate pirate)
        {
            // need to change IsInRangeOfEnemy - mypirate -> game
            if(game.GetEnemyLivingPirates().Where(enemy => enemy.Distance(pirate) < game.PushRange * 3).Count() > game.NumPushesForCapsuleLoss)
                return true;
            // var bestMothership = game.GetMyMotherships()
            //     .OrderBy(mothership => mothership.Distance(pirate) / mothership.ValueMultiplier)
            //     .FirstOrDefault();
            // int steps = 24;
            // for (int i = 0; i < steps; i++)
            // {
            //     double angle = System.Math.PI * 2 * i / steps;
            //     double deltaX = pirate.MaxSpeed * System.Math.Cos(angle);
            //     double deltaY = pirate.MaxSpeed * System.Math.Sin(angle);
            //     Location option1 = new Location((int) (pirate.Location.Row - deltaY), (int) (pirate.Location.Col + deltaX));
            //     Location option2 = new Location((int) (pirate.Location.Row - (2 * deltaY)), (int) (pirate.Location.Col + (2 * deltaX)));
            //     if(!option1.InMap() || !option2.InMap()) continue;
            //     if (IsInRangeOfEnemy(option1, pirate) || IsInRangeOfEnemy(option2, pirate))
            //     {
            //         return true;
            //     }
            // }
            return false;
        }
        // public bool CapsuleHolderInDanger(Pirate pirate)
        // {
        //     if (!pirate.HasCapsule()) return false;
        //     var bestMothership = game.GetMyMotherships()
        //         .OrderBy(mothership => mothership.Distance(pirate) / mothership.ValueMultiplier)
        //         .FirstOrDefault();
        //     if (bestMothership != null)
        //     {
        //         if (game.GetEnemyLivingPirates()
        //             .Where(enemy => enemy.InPushRange(pirate.Location.Towards(bestMothership, game.PirateMaxSpeed)) || enemy.InPushRange(pirate.Location.Towards(bestMothership, game.PirateMaxSpeed * 2)))
        //             .Count() >= game.NumPushesForCapsuleLoss)
        //             return true;
        //     }
        //     return false;
        // }
        public Wormhole GetBestWormhole(Location destination, Pirate pirate)
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

        public Mothership GetBestMothershipThroughWormholes(Pirate pirate)
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
    }
}
