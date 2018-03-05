using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private class TargetLocation
        {
            public Location Location { get; private set; }
            public LocationType Type { get; private set; }
            public int Priority { get; private set; }
            public int DesiredPirates { get; private set; }
            public int AssignedPirates { get; set; }
            public GameObject TargetLocationObject { get; private set; }

            private const int PenaltyPerExtraPirate = MAX_PRIORITY / 10;

            public TargetLocation(Location location, LocationType type, int priority, GameObject targetLocationObject, int desiredPirates = 1)
            {
                Location = location;
                Type = type;
                Priority = priority;
                DesiredPirates = desiredPirates;
                TargetLocationObject = targetLocationObject;
                AssignedPirates = 0;
            }

            private bool CanCatchUpAndPush(Pirate pirate, Pirate destinationPirate, Location destination)
            {
                Location intercept = Interception(destinationPirate.Location, destination, pirate.Location);
                int steps = pirate.Distance(intercept) / pirate.MaxSpeed;
                return (steps < pirate.PushReloadTurns && pirate.Distance(intercept) < pirate.Distance(destination)) || pirate.CanPush(destinationPirate);
            }

            public int ScoreForPirate(Pirate pirate)
            {
                int penaltyForNoPush = (Type == LocationType.MyPirate || Type == LocationType.EnemyPirate) ?
                    (pirate.PushReloadTurns - pirate.Steps(Location)).Clamp(0, MAX_PRIORITY) : 0;
                int score = this.ScaledDistance(pirate) + PenaltyPerExtraPirate * (this.AssignedPirates >= this.DesiredPirates ? (AssignedPirates - DesiredPirates + 1) : 0) +
                    penaltyForNoPush;
                if (LocationType.MyPirate == Type)
                {
                    var bestMothership = game.GetMyMotherships().OrderBy(mothership => pirate.Steps(mothership) / (int) ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();;
                    if (!CanCatchUpAndPush(pirate, (Pirate) TargetLocationObject, bestMothership.Location))
                    {
                        score += MAX_PRIORITY;
                        return score;
                    }
                    score += this.Priority;
                    return score;
                }
                score += this.Priority;
                return score;

            }

            private int ScaledDistance(Pirate pirate)
            {
                int maxDistance = (int) ((game.Cols.Power(2) + game.Rows.Power(2)).Sqrt());
                return (int) (((double) pirate.Distance(Location) / (double) maxDistance) * MAX_PRIORITY);
            }
        }

        private enum LocationType
        {
            Capsule,
            MyPirate,
            EnemyPirate,
            Wormhole,
            Asteroid,
            StickyBomb,
        }

        // Moves remaining availablePirates according to Priorities 
        private void HandlePriorities()
        {
            var targetLocations = GetAllTargetLocations();
            while (availablePirates.Any())
            {
                int min = int.MaxValue;
                TargetLocation bestLocation = null;
                Pirate bestPirate = null;
                foreach (Pirate pirate in availablePirates)
                {
                    foreach (TargetLocation targetLocation in targetLocations)
                    {
                        int score = targetLocation.ScoreForPirate(pirate);
                        if (score < min || bestLocation == null)
                        {
                            min = score;
                            bestLocation = targetLocation;
                            bestPirate = pirate;
                        }
                    }
                }
                if (bestLocation == null)
                {
                    break;
                }
                switch (bestLocation.Type)
                {
                    case LocationType.MyPirate:
                        if (!TryPushMyCapsule((Pirate) bestLocation.TargetLocationObject, bestPirate))
                        {
                            AssignDestination(bestPirate,
                                Interception(bestLocation.Location, GetMyBestMothershipThroughWormholes((Pirate) bestLocation.TargetLocationObject).Location, bestPirate.Location));
                        }
                        break;

                    case LocationType.EnemyPirate:
                        if (!TryStickBomb(bestPirate, (Pirate) bestLocation.TargetLocationObject))
                            AssignDestination(bestPirate, bestLocation.Location);
                        break;

                    case LocationType.Wormhole:
                        if (!TryPushWormhole(bestPirate, (Wormhole) bestLocation.TargetLocationObject))
                            AssignDestination(bestPirate, bestLocation.Location.Towards(bestPirate, game.WormholeRange));
                        break;

                    default:
                        AssignDestination(bestPirate, bestLocation.Location);
                        break;
                }
                bestLocation.AssignedPirates++;
                availablePirates.Remove(bestPirate);
            }
        }

        private List<TargetLocation> GetAllTargetLocations()
        {
            var targetLocations = new List<TargetLocation>();
            targetLocations.AddRange(GetTargetLocationsWormholes());
            targetLocations.AddRange(GetTargetLocationsAsteroids());
            targetLocations.AddRange(GetTargetLocationsMyPirates()); //for now not using it since we need to interesect our pirates
            targetLocations.AddRange(GetTargetLocationsEnemyPirates());
            return targetLocations;
        }

    }
}