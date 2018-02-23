using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void PerformAggressiveBunker()
        {
            foreach (var mothership in game.GetEnemyMotherships())
            {
                foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null &&
                        GetBestMothershipThroughWormholes(cap.Holder).Equals(mothership)))
                {
                    bunkerCount[mothership]++;
                    var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                    var useablePirates = availablePirates.Where(p => p.Steps(mothership) > p.PushReloadTurns).OrderBy(p => p.Steps(mothership));
                    int count = 0, pushDistanceUsed = 0;
                    foreach (var pirate in useablePirates.OrderByDescending(p => p.PushDistance))
                    {
                        if (pushDistanceUsed < distanceToBorder)
                        {
                            count++;
                            pushDistanceUsed += pirate.PushDistance;
                        }
                    }
                    var requiredPiratesCount = Min((count == 0) ? 1 : count, capsule.Holder.NumPushesForCapsuleLoss) + 1;
                    ("Mothership: " + mothership + ", Capsule: " + capsule).Print();
                    ("Required pirates: " + requiredPiratesCount).Print();
                    ("Push pirates: " + count).Print();
                    ("Capsule loss: " + capsule.Holder.NumPushesForCapsuleLoss).Print();
                    var bestWormhole = GetBestWormhole(mothership.Location, capsule.Holder);
                    if (useablePirates.Count() >= requiredPiratesCount)
                    {
                        if (requiredPiratesCount == count)
                            useablePirates = useablePirates.OrderByDescending(p => p.PushDistance);
                        var usedPirates = new List<Pirate>();
                        foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                        {
                            if (bestWormhole != null && bestWormhole.Partner != null &&
                                bestWormhole.Partner.InRange(mothership, mothership.UnloadRange * 3))
                            {
                                // Add push wormhole here
                            }
                            else
                            {
                                var rangeNeeded = bunkerCount[mothership].Power(2) * game.PushRange;
                                var destinationBunker = mothership.Location.Towards(capsule, rangeNeeded);
                                if (useablePirates.Count(p => p.InRange(capsule, p.PushRange * 2) && p.InRange(mothership, p.PushDistance * 2)) >= requiredPiratesCount &&
                                    pirate.InRange(capsule, pirate.PushRange * 2) && pirate.InRange(mothership, pirate.PushDistance * 2))
                                {
                                    destinationBunker = capsule.Location.Towards(mothership, (int) (pirate.PushRange * 0.9));
                                }
                                AssignDestination(pirate, destinationBunker);
                            }
                            usedPirates.Add(pirate);
                        }
                        availablePirates = availablePirates.Except(usedPirates).ToList();

                    }
                }
            }
        }

        protected void PerformDefensiveBunker()
        {
            ("Entered bunker").Print();
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null)
                    .OrderBy(cap => cap.Holder.Steps(GetBestMothershipThroughWormholes(cap.Holder).Location)))
            {
                var bestMothership = GetBestMothershipThroughWormholes(capsule.Holder);
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
                        var usedPirates = new List<Pirate>();
                        foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                        {
                            AssignDestination(pirate, bestMothership.Location.Towards(capsule, (int) (capsule.Holder.MaxSpeed)));
                            usedPirates.Add(pirate);

                        }
                        availablePirates = availablePirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}