using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
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
            return ScaleToRange(0, maxPossibleImprovement, MAX_PRIORITY, MIN_PRIORITY, improvementDelta) - bonus; // TODO: Scale improvementDelta / maxPossibleImprovement to priorities range and add/subtract bonus.
        }

        private IEnumerable<TargetLocation> GetTargetLocationsWormholes()
        {
            var targetLocations = new List<TargetLocation>();
            foreach (Wormhole wormhole in game.GetAllWormholes().Where(wormhole => MakesSenseToPushWormhole(wormhole)))
            {
                var targetLocation = new TargetLocation(wormhole.Location, LocationType.Wormhole, GetWormholePriority(wormhole));
                targetLocations.Add(targetLocation);
            }
            return targetLocations;
        }
    }
}