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

            public TargetLocation(Location location, LocationType type, int priority, int desiredPirates = 1)
            {
                Location = location;
                Type = type;
                Priority = priority;
                DesiredPirates = desiredPirates;
                AssignedPirates = 0;
            }

            public int ScoreForPirate(Pirate pirate)
            {
                int score = ScaledDistance(pirate);
                score -= DesiredPirates;
                score += AssignedPirates;
                score += Priority;
                return score;
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
            int min = int.MaxValue;
            TargetLocation bestLocation = null;
            Pirate bestPirate = null;
            var targetLocations = GetAllTargetLocations();
            while (availablePirates.Any())
            {
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
        private IEnumerable<TargetLocation> GetTargetLocationsWormholes()
        {
            return Enumerable.Empty<TargetLocation>();
        }

        // TODO: Move to the right file and implement.
        private IEnumerable<TargetLocation> GetTargetLocationsAsteroids()
        {
            return Enumerable.Empty<TargetLocation>();
        }

        // TODO: Move to the right file and implement.
        private IEnumerable<TargetLocation> GetTargetLocationsMyPirates()
        {
            return Enumerable.Empty<TargetLocation>();
        }

        // TODO: Move to the right file and implement.
        private IEnumerable<TargetLocation> GetTargetLocationsEnemyPirates()
        {
            return Enumerable.Empty<TargetLocation>();
        }
    }
}