using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private Dictionary<Wormhole, Location> movedWormholeLocations = new Dictionary<Wormhole, Location>();

        private int GetWormholeScoreForPlayer(Location wormholeLocation, Location partnerLocation, Player player)
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
        private int GetWormholeScore(Location wormholeLocation, Location partnerLocation)
        {
            // Gets the total score for a wormhole by finding our score as well as the enemy's.
            var enemyScore = GetWormholeScoreForPlayer(wormholeLocation, partnerLocation, game.GetEnemy());
            var myScore = GetWormholeScoreForPlayer(wormholeLocation, partnerLocation, game.GetMyself());
            return myScore - enemyScore;
        }

        // How many angles to check when circling around a location. 24 steps = 15 degrees steps (360/24 = 15).
        private const int CircleSteps = 24;

        private Location BestWormholePushLocation(Wormhole wormhole)
        {
            // Returns the most favorable position to push the wormhole to, taking into consideration position of capsules and motherships.
            var bestOption = wormhole.Location;
            var bestOptionScore = GetWormholeScore(wormhole.Location, GetWormholeLocation(wormhole.Partner));
            for (int i = 0; i < CircleSteps; i++)
            {
                double angle = System.Math.PI * 2 * i / CircleSteps;
                double deltaX = game.HeavyPushDistance * System.Math.Cos(angle);
                double deltaY = game.HeavyPushDistance * System.Math.Sin(angle);
                Location newWormholeLocation = new Location((int) (wormhole.Location.Row - deltaY), (int) (wormhole.Location.Col + deltaX));
                int newLocationScore = GetWormholeScore(newWormholeLocation, GetWormholeLocation(wormhole.Partner));
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
            int currentScore = GetWormholeScore(wormhole.Location, GetWormholeLocation(wormhole.Partner));
            int improvedScore = GetWormholeScore(BestWormholePushLocation(wormhole), GetWormholeLocation(wormhole.Partner));

            int improvementDelta = currentScore - improvedScore;
            int maxPossibleImprovement = game.PushDistance * 2; // Moved PushDistance away from enemy locations towards our locations.

            int bonus = wormhole.TurnsToReactivate == 0 ? 1 : 0;
            return ScaleToRange(0, maxPossibleImprovement, MAX_PRIORITY, MIN_PRIORITY, improvementDelta) - bonus; // TODO: Scale improvementDelta / maxPossibleImprovement to priorities range and add/subtract bonus.
        }

        private Location GetWormholeLocation(Wormhole wormhole)
        {
            return movedWormholeLocations.ContainsKey(wormhole) ? movedWormholeLocations[wormhole] : wormhole.Location;
        }

        private IEnumerable<TargetLocation> GetTargetLocationsWormholes()
        {
            var targetLocations = new List<TargetLocation>();
            foreach (Wormhole wormhole in game.GetAllWormholes().Where(wormhole => MakesSenseToPushWormhole(wormhole)))
            {
                var targetLocation = new TargetLocation(wormhole.Location, LocationType.Wormhole, GetWormholePriority(wormhole), wormhole, this);
                targetLocations.Add(targetLocation);
            }
            return targetLocations;
        }
        private bool TryPushWormhole(Pirate pirate, Wormhole wormhole)
        {
            if (pirate.CanPush(wormhole))
            {
                // Push the wormhole
                var pushLocation = BestWormholePushLocation(wormhole);
                pirate.Push(wormhole, pushLocation);
                movedWormholeLocations[wormhole] = pushLocation;
                Print(pirate + " pushes " + wormhole + " towards " + pushLocation);
                return true;
            }
            return false;
        }
         private static int DistanceThroughWormhole(Location from, MapObject to, Wormhole wormhole, IEnumerable<Wormhole> wormholes, int turnsElapsed, int pirateSpeed)
        {
            // return from.Distance(wormhole) +
            //     ClosestDistance(wormhole.Partner.Location, to,
            //         wormholes.Where(w => w.Id != wormhole.Id && w.Id != wormhole.Partner.Id));
            int turns = 0;
            if(to.Distance(wormhole) / pirateSpeed < wormhole.TurnsToReactivate - turnsElapsed)
                turns = wormhole.TurnsToReactivate - (to.Distance(wormhole) / pirateSpeed) - turnsElapsed;
            return from.Distance(wormhole) + turns * pirateSpeed
                + ClosestDistance(wormhole.Partner.Location, to,
                wormholes.Where(w => w.Id != wormhole.Id && w.Id != wormhole.Partner.Id), to.Distance(wormhole) / pirateSpeed +  turns + turnsElapsed, pirateSpeed);
        }

        private static int ClosestDistance(Location from, MapObject to, IEnumerable<Wormhole> wormholes, int turnsElapsed, int pirateSpeed)
        {
            if (wormholes.Any())
            {
                int distanceWithoutWormholes = from.Distance(to);
                int distanceWithWormholes = wormholes
                    .Select(wormhole => DistanceThroughWormhole(from, to, wormhole, wormholes, turnsElapsed, pirateSpeed))
                    .Min();
                return System.Math.Min(distanceWithoutWormholes, distanceWithWormholes);
            }
            return from.Distance(to);
        }

        private IEnumerable<Wormhole> GetViableWormholes(Pirate pirate)
        {
            return game.GetAllWormholes() //.Except(usedWormholes)
                .Where(wormhole => wormhole.TurnsToReactivate <= pirate.Steps(wormhole) + 2);
        }

        // Returns the best wormhole for the pirate to get to the destination through, or null if there are no good wormholes.
        private Wormhole GetBestWormhole(Pirate pirate, Location destination)
        {
            var wormholeDistances = new Dictionary<Wormhole, int>();
            var wormholes = GetViableWormholes(pirate);
            foreach (var wormhole in wormholes)
            {
                //    Assign the closest distance for the wormhole
                wormholeDistances.Add(wormhole, DistanceThroughWormhole(pirate.Location, destination, wormhole, wormholes, 0, pirate.MaxSpeed));
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

        private Mothership GetBestMothershipThroughWormholes(Pirate pirate, IEnumerable<Mothership> motherships)
        {
            return motherships
                .OrderBy(mothership => ClosestDistance(pirate.Location, mothership, GetViableWormholes(pirate), 0, pirate.MaxSpeed))
                .FirstOrDefault();
        }

        private Mothership GetMyBestMothershipThroughWormholes(Pirate myPirate)
        {
            return GetBestMothershipThroughWormholes(myPirate, game.GetMyMotherships());
        }

        private Mothership GetEnemyBestMothershipThroughWormholes(Pirate enemy)
        {
            return GetBestMothershipThroughWormholes(enemy, game.GetEnemyMotherships());
        }

        private Location AdjustDestinationForWormholes(Pirate pirate, Location destination)
        {
            var bestWormhole = GetBestWormhole(pirate, destination);

            return (bestWormhole == null) ? destination : bestWormhole.Location;
        }
    }
}