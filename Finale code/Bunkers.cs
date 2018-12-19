using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {

        private void PerformAggressiveBunker()
        {
            Dictionary<Mothership, int> bunkerCount = game.GetEnemyMotherships()
                .ToDictionary(mothership => mothership, mothership => 0);

            var header = string.Format("Mothership bunkers:\n{0, -8} {1, 7} {2, 12} {3, 12} {4,12}", "Mothership", "Capsule", "  Location  ", "  Capsule loss  ", "  Border pushes  ");
            foreach (var capsule in game.GetEnemyCapsules().Where(capsule => capsule.Holder != null)
                    .OrderBy(capsule => capsule.Holder.Steps(GetEnemyBestMothershipThroughWormholes(capsule.Holder))))
            {
                var mothership = GetEnemyBestMothershipThroughWormholes(capsule.Holder);

                bunkerCount[mothership]++;
                var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                var useablePirates = availablePirates.Where(p =>  capsule.Holder.Steps(mothership)  > p.PushReloadTurns ) //Fix: add the wait turns until we get to the enemy pirates
                    .Where(p => p.Steps(mothership) < capsule.Holder.Steps(mothership))
                    .Where(p => p.Capsule == null)
                    .OrderBy(p => p.Steps(mothership));
                int count = 0, pushDistanceUsed = 0;
                foreach (var pirate in useablePirates.OrderByDescending(p => p.PushDistance))
                {
                    if (pushDistanceUsed < distanceToBorder)
                    {
                        count++;
                        pushDistanceUsed += pirate.PushDistance;
                    }
                }
                var requiredPiratesCount = Min((count == 0) ? 1 : count, capsule.Holder.NumPushesForCapsuleLoss);
                var bestWormhole = GetBestWormhole(capsule.Holder, mothership.Location);
                if (useablePirates.Count() >= requiredPiratesCount)
                {
                    header += string.Format("\n{0, -8} {1, 9} @ {2, 12} {3,12} {4,12}", "ID: " + mothership.Id, "ID: " + capsule.Id, mothership.Location, capsule.Holder.NumPushesForCapsuleLoss, count);
                    if (game.GetEnemyCapsules().Where(cap => cap.Holder != null)
                        .OrderBy(cap => cap.Holder.Steps(GetEnemyBestMothershipThroughWormholes(cap.Holder))).Last().Equals(capsule))
                        Print(header);
                    useablePirates = useablePirates.OrderByDescending(p => p.PushDistance);
                    var usedPirates = new List<Pirate>();
                    foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                    {
                        if (bestWormhole != null && bestWormhole.Partner != null &&
                            bestWormhole.Partner.InRange(mothership, mothership.UnloadRange * 3))
                        {
                            // Get the steps needed for the pirate to go to the wormhole's partner, push it and come back.
                             int friendlyStepsNeeded =
                                System.Math.Max(pirate.Steps(bestWormhole.Partner.Location.Towards(pirate, pirate.PushRange)), pirate.PushReloadTurns) +
                                System.Math.Max(pirate.Steps(bestWormhole.Partner.Location.Towards(pirate, pirate.PushRange)), game.PushMaxReloadTurns);//Fix, assumed pirate.PushRange, needs review
                            // Get the steps needed for the enemy pirate to arrive to the mothership
                            var enemyStepsNeeded = capsule.Holder.Steps(bestWormhole) + (bestWormhole.Partner.Distance(capsule) / capsule.Holder.MaxSpeed); //Fix
                            if (friendlyStepsNeeded < enemyStepsNeeded)
                            {
                                var wormhole = bestWormhole.Partner;
                                if (!TryPushWormhole(pirate, wormhole))
                                    AssignDestination(pirate, wormhole.Location.Towards(pirate, wormhole.WormholeRange));
                                usedPirates.Add(pirate);
                            }
                            // Add push wormhole here
                        }
                        else
                        {
                            var rangeNeeded = bunkerCount[mothership] * game.PushDistance /2;
                            var destinationBunker = mothership.Location.Towards(capsule, rangeNeeded);
                            if (useablePirates.Count(p => p.InRange(capsule, p.PushRange * 2) && p.InRange(mothership, p.PushDistance * 2)) >= requiredPiratesCount &&
                                pirate.InRange(capsule, pirate.PushRange * 2) && pirate.InRange(mothership, pirate.PushDistance * 2))
                            {
                                destinationBunker = capsule.Location.Towards(mothership, (int) (pirate.PushRange * 0.9));
                            }
                            AssignDestination(pirate, destinationBunker);
                        }
                        usedPirates.Add(pirate);
                        bunkeringPirates.Add(pirate);

                    }
                    availablePirates = availablePirates.Except(usedPirates).ToList();
                }
            }
        }

        private void PerformDefensiveBunker()
        {
            if (game.GetEnemyCapsules().Any(capsule => capsule.Holder != null))
            {
                Print("Defensive mothership bunkers: ");
                var header = string.Format("{0, -8} {1, 7} {2, 12} {3, 12} {4,12}", "Mothership", "Capsule", "  Location  ", "  Capsule loss  ", "  Border pushes  ");
                Print(header);
            }
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null)
                    .OrderBy(cap => cap.Holder.Steps(GetEnemyBestMothershipThroughWormholes(cap.Holder).Location)))
            {
                var bestMothership = GetEnemyBestMothershipThroughWormholes(capsule.Holder);
                if (bestMothership != null)
                {
                    var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                    var useablePirates = availablePirates.Where(p => p.Steps(bestMothership) >= p.PushReloadTurns).OrderBy(p => p.Steps(bestMothership));
                    int count = 0, pushDistanceUsed = 0;
                    foreach (var pirate in useablePirates.OrderByDescending(p => p.PushDistance))
                    {
                        if (pushDistanceUsed < distanceToBorder)
                        {
                            count++;
                            pushDistanceUsed += pirate.PushDistance;
                        }
                    }
                    var requiredPiratesCount = Min((count == 0) ? 1 : count, capsule.Holder.NumPushesForCapsuleLoss);
                    if (useablePirates.Count() >= requiredPiratesCount)
                    {
                        var line = string.Format("{0, -8} {1, 9} @ {2, 12} {3,12} {4,12}", "ID: " + bestMothership.Id, "ID: " + capsule.Id, bestMothership.Location, capsule.Holder.NumPushesForCapsuleLoss, count);
                        Print(line);
                        var usedPirates = new List<Pirate>();
                        foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                        {
                            AssignDestination(pirate, bestMothership.Location.Towards(capsule, (int) (capsule.Holder.MaxSpeed)));
                            usedPirates.Add(pirate);
                            bunkeringPirates.Add(pirate);
                        }
                        availablePirates = availablePirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}