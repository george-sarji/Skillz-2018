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

            private const int PenaltyPerExtraPirate = MAX_PRIORITY/10;

            public TargetLocation(Location location, LocationType type, int priority, int desiredPirates = 1)
            {
                Location = location;
                Type = type;
                Priority = priority;
                DesiredPirates = desiredPirates;
                AssignedPirates = 0;
            }

            public int Score(Pirate pirate)
            {
                int score = ScaledDistance(pirate);
                score -= DesiredPirates;
                score += AssignedPirates;
                score += Priority;
                return score;
            }

            public int ScoreForPirate(Pirate pirate)
            {
                int penaltyForNoPush = (Type == LocationType.MyPirate || Type == LocationType.EnemyPirate) ?
                    (pirate.PushReloadTurns - pirate.Steps(Location)).Clamp(0, MAX_PRIORITY) : 0;
                return this.Priority + this.ScaledDistance(pirate) +
                    PenaltyPerExtraPirate * (this.AssignedPirates >= this.DesiredPirates ? (AssignedPirates - DesiredPirates + 1) : 0) +
                    penaltyForNoPush;

            }

            private int ScaledDistance(Pirate pirate)
            {
                // TODO(Mahmoud): consider clamping or scaling this depending on map size.
                return pirate.Distance(Location);
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
                if(bestLocation.Type == LocationType.Wormhole)
                    AssignDestination(bestPirate, bestLocation.Location.Towards(bestPirate, game.WormholeRange));
                else
                    AssignDestination(bestPirate, bestLocation.Location);
                bestLocation.AssignedPirates++;
                availablePirates.Remove(bestPirate);
            }
        }

        private List<TargetLocation> GetAllTargetLocations()
        {
            var targetLocations = new List<TargetLocation>();
            targetLocations.AddRange(GetTargetLocationsWormholes());
            targetLocations.AddRange(GetTargetLocationsAsteroids());
            targetLocations.AddRange(GetTargetLocationsMyPirates());
            targetLocations.AddRange(GetTargetLocationsEnemyPirates());
            return targetLocations;
        }

        // TODO: Move to the right file and implement.
        private IEnumerable<TargetLocation> GetTargetLocationsAsteroids()
        {
            return Enumerable.Empty<TargetLocation>();
        }

        
    }
}