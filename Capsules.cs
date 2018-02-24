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
                            AssignDestination(closestPirate, SmartSail(closestPirate, bestWormhole));
                        }
                        else
                        {
                            var BestPirate = availablePirates.OrderBy(p => p.Distance(closestPirate))
                                .OrderByDescending(p => p.IsSameState(closestPirate)).FirstOrDefault();
                            AssignDestination(closestPirate, SmartSail(closestPirate, capsule.InitialLocation));
                        }
                    }
                }
            }
        }

        public void DeliverCapsules()
        {
            foreach (var pirate in availablePirates.Where(p => p.HasCapsule()).ToList())
            {
                var bestMothership = game.GetMyMotherships().OrderBy(mothership => ClosestDistance(pirate.Location, mothership, game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate > pirate.Steps(mothership) / 4)) / ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (bestMothership != null)
                {
                    // Get best wormhole
                    var bestWormhole = GetBestWormhole(bestMothership.Location, pirate);
                    if (bestWormhole != null)
                    {
                        AssignDestination(pirate, SmartSail(pirate, bestWormhole));
                    }
                    else
                        AssignDestination(pirate, SmartSail(pirate, bestMothership.Location));
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
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null))
            {
                // Get the pirates that can push
                var pushingPirates = availablePirates.Where(p => p.CanPush(capsule.Holder))
                    .OrderByDescending(p => p.PushDistance)
                    .Take(capsule.Holder.NumPushesForCapsuleLoss);
                var distanceToBorder = capsule.Holder.Distance(GetClosestToBorder(capsule.Location)) - capsule.Holder.MaxSpeed;
                var pushDistance = pushingPirates.Sum(p => p.PushDistance);
                if (pushingPirates.Count() >= capsule.Holder.NumPushesForCapsuleLoss || pushDistance >= distanceToBorder)
                {
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