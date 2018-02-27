using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        // This function sends best pirates to capture capsules
        public void CaptureCapsules()
        {
            // Check if we have any available pirates.
            if (availablePirates.Any())
            {
                // Go over the capsules ordered by the closest pirate to it.
                foreach (var capsule in game.GetMyCapsules().OrderBy(
                        capsule => capsule.Distance(
                            availablePirates.OrderBy(p => ClosestDistance(
                                p.Location, capsule.Location,
                                game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation))
                            )).FirstOrDefault()
                        )
                    ))
                {
                    // Get the ordered pirates by the turns to the capsule.
                    var piratesOrdered = availablePirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation))
                        .OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()
                            .Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation) / 4)) / p.MaxSpeed);
                    // Check if we have a close pirate to the capsule.
                    if (piratesOrdered.Any())
                    {
                        // Send the closest pirate to the spawn.
                        var closestPirate = piratesOrdered.First();
                        // Get the best wormhole to go through to get to the capsule
                        var bestWormhole = GetBestWormhole(capsule.InitialLocation, closestPirate);
                        // Remove the pirate from the available pirates list to ensure no ignored actions.
                        availablePirates.Remove(closestPirate);
                        // Check if there is a best wormhole.
                        if (bestWormhole != null)
                        {
                            var BestPirate = availablePirates.OrderBy(p => p.Distance(closestPirate))
                                .OrderByDescending(p => p.IsSameState(closestPirate)).FirstOrDefault();
                            // Assign the closest pirate to the wormhole through smart sail
                            AssignDestination(closestPirate, SmartSail(closestPirate, bestWormhole));
                        }
                        else
                        {
                            var BestPirate = availablePirates.OrderBy(p => p.Distance(closestPirate))
                                .OrderByDescending(p => p.IsSameState(closestPirate)).FirstOrDefault();
                            // Assign the closest pirate to the capsule spawn through smart sail.
                            AssignDestination(closestPirate, SmartSail(closestPirate, capsule.InitialLocation));
                        }
                    }
                }
            }
        }
        // This function goes over the capsule holders and gets them to the mothership.
        public void DeliverCapsules()
        {
            // Go over the pirates that have a capsule
            foreach (var pirate in availablePirates.Where(p => p.HasCapsule()).ToList())
            {
                // Get the best mothership for the current pirate through distance divided by value multiplier.
                var bestMothership = game.GetMyMotherships().OrderBy(mothership => ClosestDistance(pirate.Location, mothership, game.GetAllWormholes()
                    .Where(wormhole => wormhole.TurnsToReactivate > pirate.Steps(mothership) / 4)) / ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();
                // Check if we have a best mothership.
                if (bestMothership != null)
                {
                    // Get best wormhole
                    var bestWormhole = GetBestWormhole(bestMothership.Location, pirate);
                    // Check if we have a best wormhole
                    if (bestWormhole != null)
                    {
                        // Assign the pirate to the wormhole through smartsail.
                        AssignDestination(pirate, SmartSail(pirate, bestWormhole));
                    }
                    else
                        // Assign the pirate to the best mothership through smart sail
                        AssignDestination(pirate, SmartSail(pirate, bestMothership.Location));
                    // Remove the pirate from the available pirates list.
                    availablePirates.Remove(pirate);
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
            var intersections = new List<Location>();
            intersections.Add(Interception(first.Location, destination, second.Location));
            intersections.Add(Interception(second.Location, destination, first.Location));
            var speeds = new List<int>();
            var slowestSpeed = Min(first.MaxSpeed, second.MaxSpeed);
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
            // Go over all the enemy capsules that have a holder
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null))
            {
                // Get the pirates that can push, ordered by their push distance.
                var pushingPirates = availablePirates.Where(p => p.CanPush(capsule.Holder))
                    .OrderByDescending(p => p.PushDistance)
                    .Take(capsule.Holder.NumPushesForCapsuleLoss);
                    // Get the distance to the closest border.
                var distanceToBorder = capsule.Holder.Distance(GetClosestToBorder(capsule.Location)) - capsule.Holder.MaxSpeed;
                // Get the push distance that the pirates have.
                var pushDistance = pushingPirates.Sum(p => p.PushDistance);
                // Check if we have enough pushing pirates to either 1) cause capsule loss or 2) push holder out of map
                if (pushingPirates.Count() >= capsule.Holder.NumPushesForCapsuleLoss || pushDistance >= distanceToBorder)
                {
                    // Get the push location to push the pirate to. If we can push to the border, get the closest border.
                    // If not, assign the push location towards the capsule's spawn in a negative range (push away from spawn)
                    Location pushLocation = (pushDistance >= distanceToBorder) ?
                        GetClosestToBorder(capsule.Location) :
                        capsule.Location.Towards(capsule.InitialLocation, -pushDistance);
                        // Initiate the used pirates list.
                    var usedPirates = new List<Pirate>();
                    // Go over the pirates that meet the criteria
                    foreach (var pirate in pushingPirates)
                    {
                        // Push the capsule holder to the push location
                        pirate.Push(capsule.Holder, pushLocation);
                        // Debug the push
                        (pirate + " pushes pirate " + capsule.Holder + " towards " + pushLocation).Print();
                        // Add the pirate to the used pirates list
                        usedPirates.Add(pirate);
                    }
                    // Remove the used pirates from the available pirates list.
                    availablePirates = availablePirates.Except(usedPirates).ToList();
                }
            }
        }
    }
}