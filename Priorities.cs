using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private class TargetLocation
        {
            private SSJS12Bot bot;
            public Location Location { get; private set; }
            public LocationType Type { get; private set; }
            public int Priority { get; private set; }
            public int DesiredPirates { get; private set; }
            public int AssignedPirates { get; set; }
            public GameObject TargetLocationObject { get; private set; }

            private const int PenaltyPerExtraPirate = MAX_PRIORITY / 10;

            public TargetLocation(Location location, LocationType type, int priority, GameObject targetLocationObject,SSJS12Bot bot , int desiredPirates = 1)
            {
                this.bot = bot;
                Location = location;
                Type = type;
                Priority = priority;
                DesiredPirates = desiredPirates;
                TargetLocationObject = targetLocationObject;
                AssignedPirates = 0;
            }

            private bool CanCatchUpAndPush(Pirate pirate, Pirate destinationPirate, Location destination)
            {
                Location intercept = bot.GetPirateOptimalInterception(pirate, destinationPirate, destination); //Fix
                int steps = pirate.Distance(intercept) / pirate.MaxSpeed;
                return (steps < pirate.PushReloadTurns && pirate.Distance(intercept) < pirate.Distance(destination)) || pirate.CanPush(destinationPirate);
            }

            public int ScoreForPirate(Pirate pirate)
            {
                int penaltyForNoPush = (Type == LocationType.MyPirate || Type == LocationType.EnemyPirate) ?
                    (pirate.PushReloadTurns - pirate.Steps(Location)).Clamp(0, MAX_PRIORITY) : 0;
                int score = this.ScaledDistance(pirate) + PenaltyPerExtraPirate * (this.AssignedPirates >= this.DesiredPirates ? (AssignedPirates - DesiredPirates + 1) : 0) +
                    penaltyForNoPush;

                score += HandleIfPirateCanReach(pirate);
                return score;

            }

            private int HandleIfPirateCanReach(Pirate pirate)
            {
                if (LocationType.MyPirate == Type)
                {
                    var bestMothership = bot.GetMyBestMothershipThroughWormholes(pirate);
                    if (!CanCatchUpAndPush(pirate, (Pirate) TargetLocationObject, bestMothership.Location))
                    {

                        return MAX_PRIORITY;
                    }
                }
                else if(LocationType.Asteroid == Type)
                {
                    Asteroid asteroid = (Asteroid)TargetLocationObject;
                    var bestCapsule = bot.GetBestCapsuleForAsteroid(asteroid, game.GetEnemyCapsules()
                                                                                                .Where(capsule => capsule.Holder != null));
                    if(pirate.Steps(asteroid) + asteroid.Steps(bestCapsule) > bestCapsule.Holder.Steps(bot.GetEnemyBestMothershipThroughWormholes(bestCapsule.Holder)))
                    {
                        return MAX_PRIORITY;
                    }
                }
                return this.Priority;
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
                        {
                            var enemyPirate = (Pirate) bestLocation.TargetLocationObject;
                            if(enemyPirate.HasCapsule() && GetEnemyBestMothershipThroughWormholes(enemyPirate)!=null &&
                                GetPirateOptimalInterception(bestPirate, enemyPirate, GetEnemyBestMothershipThroughWormholes(enemyPirate).Location)!=null)
                                AssignDestination(bestPirate, GetPirateOptimalInterception(bestPirate, enemyPirate, GetEnemyBestMothershipThroughWormholes(enemyPirate).Location));
                            else
                                AssignDestination(bestPirate, bestLocation.Location);
                        }
                        break;

                    case LocationType.Wormhole:
                        if (!TryPushWormhole(bestPirate, (Wormhole) bestLocation.TargetLocationObject))
                            AssignDestination(bestPirate, bestLocation.Location.Towards(bestPirate, game.WormholeRange));
                        break;

                    case LocationType.Asteroid:
                        if (!TryPushAsteroid(bestPirate, ((Asteroid) bestLocation.TargetLocationObject)))
                            AssignDestination(bestPirate, bestLocation.Location);
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
            targetLocations.AddRange(GetTargetLocationsMyPirates());
            targetLocations.AddRange(GetTargetLocationsEnemyPirates());
            return targetLocations;
        }

    }
}