using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void CaptureCapsules()
        {
            if (availablePirates.Any())
            {
                foreach (var capsule in game.GetMyCapsules().OrderBy(
                        capsule => capsule.Distance(
                            availablePirates.OrderBy(p => ClosestDistance(
                                p.Location, capsule.Location,
                                game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation))
                            )).FirstOrDefault()
                        )
                    ))
                {
                    var piratesOrdered = availablePirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation))
                        .OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()
                            .Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation) / 4)) / p.MaxSpeed);
                    // Check if we have a close pirate to the capsule.
                    if (piratesOrdered.Any())
                    {
                        // Send the closest pirate to the spawn.
                        var closestPirate = piratesOrdered.First();
                        var bestWormhole = GetBestWormhole(capsule.InitialLocation, closestPirate);
                        availablePirates.Remove(closestPirate);
                        if (bestWormhole != null)
                        {
                            var BestPirate = availablePirates.OrderBy(p => p.Distance(closestPirate))
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
                            var BestPirate = availablePirates.OrderBy(p => p.Distance(closestPirate))
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

        public void DeliverCapsules()
        {
            var usedPirates = new List<Pirate>();
            var capsuleHolders = availablePirates.Where(p => p.HasCapsule()).ToList();
            foreach (Pirate pirateWithCapsule in capsuleHolders)
            {
                availablePirates.Remove(pirateWithCapsule);
                var bestMothership = game.GetMyMotherships().OrderBy(m => ClosestDistance(pirateWithCapsule.Location, m, game.GetAllWormholes()) / ((double) m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (bestMothership != null)
                {
                    var bestWormhole = GetBestWormhole(bestMothership.Location, pirateWithCapsule);
                    if (ClosestDistance(pirateWithCapsule.Location, bestMothership, game.GetAllWormholes()) == pirateWithCapsule.Distance(bestMothership))
                    {
                        AssignDestination(pirateWithCapsule, bestMothership.Location);
                    }
                    else
                    {
                        AssignDestination(pirateWithCapsule, bestWormhole.Location);
                    }
                }
            }
    }
    public void MakePair(Pirate first, Pirate second, Location destination)
    {
        if (second == null)
        {
            AssignDestination(first, SmartSail(first, destination));
            return;
        }
        // if (!second.IsSameState(first))
        // {
        //     ("Reached2").Print();
        //     MakeSpecialPair(first, second, destination);
        //     return;
        // }
        var intersections = new List<Location>();
        intersections.Add(Interception(first.Location, destination, second.Location));
        intersections.Add(Interception(second.Location, destination, first.Location));
        var speeds = new List<int>();
        var slowestSpeed = Min(first.MaxSpeed, second.MaxSpeed);
        // intersections.Add(MidPoint(first, second));
        var bestIntersection = intersections.Where(location => location != null).OrderBy(location => location.Distance(destination))
            .Where(location => IsOnTheWay(first.Location, destination, location, 1) &&
                IsOnTheWay(second.Location, destination, location, 1))
            .FirstOrDefault();
        Location finalDest = null;
        if (first.Location.Equals(second.Location))
        {
            finalDest = destination;
        }
        else
        {
            finalDest = bestIntersection;
        }
        if (finalDest == null)
        {
            intersections.RemoveAt(0);
            intersections.RemoveAt(0);
            intersections.Add(first.Location);
            intersections.Add(second.Location);
            finalDest = intersections.OrderBy(location => location.Distance(destination)).FirstOrDefault();
            {

            }
        }
        if (first.HasCapsule())
            AssignDestination(first, SmartSail(first, finalDest));
        else
            AssignDestination(first, first.Location.Towards(finalDest, slowestSpeed));
        if (second.HasCapsule())
            AssignDestination(second, SmartSail(second, finalDest));
        else
            AssignDestination(second, second.Location.Towards(finalDest, slowestSpeed));
    }
    protected void PushEnemyCapsulesAggressively()
    {
        foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null))
        {
            // Get the pirates that can push
            var pushingPirates = availablePirates.Where(p => p.CanPush(capsule.Holder))
                .Take(capsule.Holder.NumPushesForCapsuleLoss);
            var distanceToBorder = capsule.Holder.Distance(GetClosestToBorder(capsule.Location));
            if (pushingPirates.Count() == capsule.Holder.NumPushesForCapsuleLoss)
            {
                var pushDistance = pushingPirates.Sum(p => p.PushDistance);
                Location pushLocation = (pushDistance >= distanceToBorder) ?
                    GetClosestToBorder(capsule.Location) :
                    capsule.Location.Towards(capsule.InitialLocation, -pushDistance);
                var usedPirates = new List<Pirate>();
                foreach (var pirate in pushingPirates)
                {
                    pirate.Push(capsule.Holder, pushLocation);
                    (pirate + " pushes pirate " + capsule.Holder + " towards " + pushLocation).Print();
                    usedPirates.Add(pirate);
                }
                availablePirates = availablePirates.Except(usedPirates).ToList();
            }
        }
    }

}
}