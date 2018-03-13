using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public void BuildDefensiveBunker()
        {
            var capsuleMothership = new Dictionary<Capsule, Mothership>();
            foreach (var closestMothership in game.GetEnemyMotherships())
            {
                foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null).Where(cap => GetBestMothershipThroughWormholes(cap.Holder).Equals(closestMothership)).OrderBy(cap => cap.Holder.Steps(closestMothership)))
                {
                    // Get the amount of pushes towards the border.
                    var closestToBorder = GetClosestToBorder(closestMothership.Location);
                    var requiredPirates = capsule.Holder.NumPushesForCapsuleLoss + 1;
                    var closestWormholeToCapsule = game.GetAllWormholes().Where(wormhole => wormhole.InRange(capsule, wormhole.WormholeRange * 2)).OrderBy(wormhole => wormhole.Distance(capsule)).FirstOrDefault();
                    int rangeNeeded = 0;
                    var closestWormholeToMothership = game.GetAllWormholes().Where(wormhole => wormhole.InRange(closestMothership, wormhole.WormholeRange * 2)).OrderBy(wormhole => wormhole.Distance(closestMothership)).FirstOrDefault();
                    Mothership closestMothershipThroughWormhole = null;
                    if (closestWormholeToCapsule != null)
                    {
                        closestMothershipThroughWormhole = game.GetEnemyMotherships().OrderBy(mothership => DistanceThroughWormhole(capsule.Location, mothership.Location, closestWormholeToCapsule, game.GetAllWormholes())).FirstOrDefault();
                        if (closestMothershipThroughWormhole != null)
                        {
                            capsuleMothership.Add(capsule, closestMothership);
                            rangeNeeded = capsuleMothership.Count(map => map.Value.Equals(closestMothership)).Power(2) * game.PushRange;
                        }
                    }
                    else
                    {
                        capsuleMothership.Add(capsule, closestMothership);
                        rangeNeeded = capsuleMothership.Count(map => map.Value.Equals(closestMothership)).Power(2) * game.PushRange;
                    }

                    var usablePirates = myPirates.OrderBy(p => p.Distance(closestMothership)).Where(p => p.Steps(closestMothership) > p.PushReloadTurns);
                    if (usablePirates.Count() >= requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach (var pirate in usablePirates.Take(requiredPirates))
                        {
                            if (closestMothershipThroughWormhole != null)
                            {
                                // Attempt pushing the closest wormhole.
                                var closestWormhole = game.GetAllWormholes().Where(wormhole => wormhole.InRange(closestMothershipThroughWormhole, wormhole.WormholeRange * 2)).FirstOrDefault();
                                if (closestWormhole != null)
                                {
                                    if (!TryPushEnemyCapsule(pirate, capsule.Holder) && !TryPushWormhole(pirate, closestWormhole))
                                        AssignDestination(pirate, closestWormhole.Location.Towards(closestMothershipThroughWormhole, closestWormhole.WormholeRange));
                                    usedPirates.Add(pirate);
                                    //bunkeringPirates.Add(pirate);
                                    continue;
                                }
                                else
                                {
                                    if (!TryPushEnemyCapsule(pirate, capsule.Holder))
                                        AssignDestination(pirate, closestMothership.Location.Towards(capsule, rangeNeeded));
                                }
                            }
                            else if (!TryPushEnemyCapsule(pirate, capsule.Holder))
                                AssignDestination(pirate, closestMothership.Location.Towards(capsule, rangeNeeded));
                            usedPirates.Add(pirate);
                            //bunkeringPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }

        public void BuildBunkerForDefense()
        {
            foreach (var mothership in game.GetEnemyMotherships())
            {
                foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null).Where(cap => GetBestMothershipThroughWormholes(cap.Holder).Equals(mothership)).OrderBy(cap => cap.Holder.Steps(mothership)))
                {
                    ("Entered bunker defense").Print();
                    var usablePirates = myPirates.Where(pirate => pirate.Steps(mothership) >= pirate.PushReloadTurns).ToList();
                    var closestToBorder = GetClosestToBorder(mothership.Location);
                    // Get how much pushes it needs to get to the border + 1
                    var amountOfPushes = mothership.Distance(closestToBorder) / (game.PushDistance + 1);
                    var pushesTillBorder = mothership.Distance(GetClosestToBorder(mothership.Location)) / (game.PushDistance + 1);
                    if (pushesTillBorder == 0)
                        pushesTillBorder++;
                    int requiredPirates = 0;
                    if (capsule.Holder.NumPushesForCapsuleLoss <= pushesTillBorder)
                        requiredPirates = capsule.Holder.NumPushesForCapsuleLoss;
                    else
                        requiredPirates = pushesTillBorder;
                    if (usablePirates.Count() >= requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach (var pirate in usablePirates.OrderBy(p => p.Steps(capsule)).Take(requiredPirates))
                        {
                            var destination = mothership.Location.Towards(capsule, (int) (mothership.UnloadRange * 0.5));
                            if (!TryPushEnemyCapsuleDefensively(pirate, capsule.Holder))
                            {
                                AssignDestination(pirate, destination);
                            }
                            // Add to the destination.
                            usedPirates.Add(pirate);
                            //bunkeringPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}