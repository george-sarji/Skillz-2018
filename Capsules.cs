using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void CaptureCapsules()
        {
            if (myPirates.Any())
            {
                foreach (var capsule in game.GetMyCapsules().OrderBy(
                        capsule => capsule.Distance(
                            myPirates.OrderBy(p => ClosestDistance(
                                p.Location, capsule.Location,
                                game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation))
                            )).FirstOrDefault()
                        )
                    ))
                {
                    var piratesOrdered = myPirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation))
                        .OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()
                            .Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation) / 4)) / p.MaxSpeed);
                    // Check if we have a close pirate to the capsule.
                    if (piratesOrdered.Any())
                    {
                        // Send the closest pirate to the spawn.
                        var closestPirate = piratesOrdered.First();
                        var bestWormhole = GetBestWormhole(capsule.InitialLocation, closestPirate);
                        myPirates.Remove(closestPirate);
                        if (bestWormhole != null)
                        {
                            var BestPirate = myPirates.OrderBy(p => p.Distance(closestPirate))
                                .OrderByDescending(p => p.IsSameState(closestPirate)).FirstOrDefault();
                            // if (CheckIfCapturerCanReach(closestPirate, bestWormhole.Location) || BestPirate == null)
                            AssignDestination(closestPirate, SmartSail(closestPirate, bestWormhole));
                            // else
                            // {
                            //     if (ShouldPushPirates(closestPirate, BestPirate, capsule.Location))
                            //     {

                            //         PushPair(closestPirate, BestPirate, capsule.Location);
                            //         myPirates.Remove(BestPirate);
                            //         continue;
                            //     }
                            //     MakePair(closestPirate, BestPirate, bestWormhole.Location);
                            //     myPirates.Remove(BestPirate);
                            // }
                        }
                        else
                        {
                            var BestPirate = myPirates.OrderBy(p => p.Distance(closestPirate))
                                .OrderByDescending(p => p.IsSameState(closestPirate)).FirstOrDefault();
                            // if (CheckIfCapturerCanReach(closestPirate, capsule.InitialLocation) || BestPirate == null)
                            AssignDestination(closestPirate, capsule.InitialLocation);
                            // else
                            // {
                            //     if (ShouldPushPirates(closestPirate, BestPirate, capsule.Location))
                            //     {
                            //         PushPair(closestPirate, BestPirate, capsule.Location);
                            //         myPirates.Remove(BestPirate);
                            //         continue;
                            //     }
                            //     MakePair(closestPirate, BestPirate, capsule.InitialLocation);
                            //     myPirates.Remove(BestPirate);
                            // }
                        }
                    }
                }
            }
        }

        public bool CheckIfCapturerCanReach(Pirate CapsuleCapturer, Location destination) //Working on this Function -Mahmoud
        {
            if (destination == null) return false;
            if (CapsuleCapturer.InRange(destination, CapsuleCapturer.MaxSpeed) &&
                NumberOfAvailableEnemyPushers(CapsuleCapturer) < CapsuleCapturer.NumPushesForCapsuleLoss &&
                NumberOfEnemiesOnTheWay(CapsuleCapturer, destination) < CapsuleCapturer.NumPushesForCapsuleLoss)
            {
                return true;
            }
            return false;
        }

        protected bool TryPushEnemyCapsuleAggressively(Pirate friendly, Capsule capsule)
        {
            if (capsule.Holder == null) return false;
            var bestMothership = GetBestMothershipThroughWormholes(capsule.Holder);
            if (friendly.CanPush(capsule.Holder))
            {
                var pushDistanceAvailable = AvailablePushDistance(capsule.Holder);
                var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                var capsuleLoss = capsule.Holder.NumPushesForCapsuleLoss;
                if (pushDistanceAvailable >= distanceToBorder)
                {
                    // Push towards the border
                    pushDistanceAvailable = -friendly.PushDistance;
                    var pushLocation = GetClosestToBorder(capsule.Location);
                    friendly.Push(capsule.Holder, pushLocation);
                    (friendly + " pushes " + capsule.Holder + " towards " + pushLocation).Print();
                    capsulePushes[capsule]++;
                    return true;
                }
                else if (NumOfAvailablePushers(capsule.Holder) >= capsuleLoss && capsulePushes[capsule] < capsuleLoss)
                {
                    // Push the pirate towards the negative direction
                    var pushLocation = capsule.Location.Towards(capsule.InitialLocation, -friendly.PushDistance);
                    friendly.Push(capsule.Holder, pushLocation);
                    (friendly + " pushes " + capsule.Holder + " towards " + pushLocation).Print();
                    capsulePushes[capsule]++;
                    return true;
                }
            }
            else if (friendly.InRange(capsule, friendly.PushRange * 2) && friendly.InRange(bestMothership, friendly.PushDistance * 2))
            {
                var sailingLocation = capsule.Location.Towards(bestMothership, (int) (friendly.PushRange * 0.9) - capsule.Holder.MaxSpeed);
                AssignDestination(friendly, sailingLocation);
                return true;
            }
            return false;
        }
    }
}