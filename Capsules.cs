using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void CaptureCapsules()
        {
            if (game.GetMyself().Score + (game.GetMyLivingPirates().Where(p => p.HasCapsule()).ToList().Count) >= game.MaxPoints)
            {
                return;
            }
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

        public bool TryPushMyCapsule(Pirate myPirateWithCapsule, Pirate pirate)
        {
            if(!pirate.CanPush(myPirateWithCapsule) || pushesForCapsulePirates[myPirateWithCapsule]== myPirateWithCapsule.NumPushesForCapsuleLoss-1)
            {
                return false;
            }
            var destination = game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                .FirstOrDefault();
            var locationOfPush = myPirateWithCapsule.Location.Towards(destination, pirate.PushDistance);
            if (!IsWorthPushing(pirate, locationOfPush, destination.Location))
            {
                return false;
            }
            availablePirates.Remove(pirate);
            pushesForCapsulePirates[myPirateWithCapsule]++;
            pirate.Push(
                        myPirateWithCapsule,
                        game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                        .FirstOrDefault());
            return true;
        }
        // public Location TryPushMyCapsule(Pirate myPirateWithCapsule)
        // { // Get all my pirates with capsule
        //     var usedPirates = new List<Pirate>();
        //     // PushAlliesToEnemy(myPirateWithCapsule);
        //     Location locationOfPush = null;

        //     int count = availablePirates.Where(pirate => pirate.CanPush(myPirateWithCapsule)).Count(); // Number of my living pirate who can push enemy pirates
        //     int numberOfPushesNeeded = myPirateWithCapsule.NumPushesForCapsuleLoss;
        //     int numberOfEnemiesAroundMyCapsule = game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(myPirateWithCapsule)).Count();
        //     foreach (Pirate myPirate in availablePirates.Where(pirate => pirate.CanPush(myPirateWithCapsule) && !pirate.Equals(myPirateWithCapsule))) // We push until we drop it
        //     {
        //         var destination = game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
        //             .FirstOrDefault();
        //         locationOfPush = myPirateWithCapsule.Location.Towards(destination, myPirate.PushDistance);
        //         if (!IsWorthPushing(myPirateWithCapsule, locationOfPush, destination.Location))
        //         {
        //             continue;
        //         }
        //         if ((IsInDanger(myPirate.GetLocation(), locationOfPush, myPirate) || ((numberOfPushesNeeded - numberOfEnemiesAroundMyCapsule == 1) || !availablePirates.Contains(myPirate))))
        //         {
        //             ("Breaking for loop, not enough pushes.").Print();
        //             ((!availablePirates.Contains(myPirate)).ToString()).Print();
        //             break;
        //         }
        //         if ((!IsInDanger(myPirate.GetLocation(), locationOfPush, myPirate)))
        //         {
        //             if (myPirate.HasCapsule())
        //             {
        //                 PushPair(myPirateWithCapsule, myPirate, destination.Location);
        //                 continue;
        //             }
        //             myPirate.Push(
        //                 myPirateWithCapsule,
        //                 game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
        //                 .FirstOrDefault());
        //             usedPirates.Add(myPirate);
        //             numberOfPushesNeeded--;
        //         }
        //     }
        //     availablePirates = availablePirates.Except(usedPirates).ToList();
        //     return locationOfPush;
        // }

        public bool IsWorthPushing(Pirate pirate, Location locationOfPush, Location destination)
        {
            int count = game.GetEnemyLivingPirates()
                .Where(enemy => enemy.HasCapsule() && pirate.Distance(destination) < enemy.Distance(
                    game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).ToList().Count;
            if (count < (game.GetEnemyLivingPirates()
                    .Where(enemy => enemy.HasCapsule() && locationOfPush.Distance(destination) < enemy.Distance(
                        game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).ToList().Count))
            {
                return true;
            }
            return false;
        }

        public void PushPair(Pirate pirate1, Pirate pirate2, Location destination) //Take two pirates and a destination and lets them push eachother towards the destination
        {
            pirate1.Push(pirate2, destination);
            pirate2.Push(pirate1, destination);
        }

    }
}