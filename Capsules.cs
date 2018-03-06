using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private void CaptureCapsules()
        {
            if (game.GetMyself().Score + (game.GetMyLivingPirates().Count(p => p.HasCapsule())) >= game.MaxPoints)
            {
                return;
            }
            if (availablePirates.Any())
            {
                var capsulesOrdered = game.GetMyCapsules()
                    .OrderBy(capsule => capsule.Distance(
                        availablePirates.OrderBy(p => ClosestDistance(p.Location, capsule.Location, GetViableWormholes(p))).First()));
                foreach (var capsule in capsulesOrdered)
                {
                    var piratesOrdered = availablePirates
                        .OrderBy(pirate => ClosestDistance(pirate.Location, capsule.InitialLocation, GetViableWormholes(pirate)));
                    // Check if we have a close pirate to the capsule.
                    if (piratesOrdered.Any())
                    {
                        // Send the closest pirate to the spawn.
                        var closestPirate = piratesOrdered.First();
                        availablePirates.Remove(closestPirate);

                        var adjustedDestination = AdjustDestinationForWormholes(closestPirate, capsule.InitialLocation.Towards(closestPirate, capsule.PickupRange - 1));
                        var sailTo = SmartSail(closestPirate, adjustedDestination);
                        AssignDestination(closestPirate, sailTo);
                    }
                }
            }
        }

        private void DeliverCapsules()
        {
            var capsuleHolders = availablePirates.Where(p => p.HasCapsule());
            foreach (var pirate in capsuleHolders)
            {
                var bestMothership = GetMyBestMothershipThroughWormholes(pirate);
                if (bestMothership != null)
                {
                    var adjustedDestination = AdjustDestinationForWormholes(pirate, bestMothership.Location.Towards(pirate, bestMothership.UnloadRange - 1));
                    var sailTo = SmartSail(pirate, adjustedDestination);
                    AssignDestination(pirate, sailTo);
                }
            }
            availablePirates = availablePirates.Except(capsuleHolders).ToList();
        }

        private void PushEnemyCapsulesAggressively()
        {
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null))
            {
                // Get the pirates that can push
                var pushingPirates = availablePirates.Where(p => p.CanPush(capsule.Holder))
                    .OrderByDescending(p => p.PushDistance)
                    .Take(capsule.Holder.NumPushesForCapsuleLoss);
                var distanceToBorder = capsule.Holder.Distance(GetClosestToBorder(capsule.Location)) - capsule.Holder.MaxSpeed;
                var pushDistance = pushingPirates.Sum(p => p.PushDistance);
                if (pushingPirates.Count() == capsule.Holder.NumPushesForCapsuleLoss ||
                    pushDistance >= distanceToBorder)
                {
                    Location pushLocation = (pushDistance >= distanceToBorder) ?
                        GetClosestToBorder(capsule.Location) :
                        capsule.Location.Towards(capsule.InitialLocation, -pushDistance);
                    foreach (var pirate in pushingPirates)
                    {
                        pirate.Push(capsule.Holder, pushLocation);
                        Print(pirate + " pushes pirate " + capsule.Holder + " towards " + pushLocation);
                    }
                    availablePirates = availablePirates.Except(pushingPirates).ToList();
                }
            }
        }

        public bool TryPushMyCapsule(Pirate myPirateWithCapsule, Pirate pirate)
        {
            if (!pirate.CanPush(myPirateWithCapsule) ||
                myPiratesWithCapsulePushes[myPirateWithCapsule] == myPirateWithCapsule.NumPushesForCapsuleLoss - 1)
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
            myPiratesWithCapsulePushes[myPirateWithCapsule]++;
            pirate.Push(
                myPirateWithCapsule,
                game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                .FirstOrDefault());
            return true;
        }

        private bool IsWorthPushing(Pirate pirate, Location locationOfPush, Location destination)
        {
            int count = game.GetEnemyLivingPirates()
                .Where(enemy => enemy.HasCapsule() && pirate.Distance(destination) < enemy.Distance(
                    game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).Count();
            if (count < (game.GetEnemyLivingPirates()
                    .Where(enemy => enemy.HasCapsule() && locationOfPush.Distance(destination) < enemy.Distance(
                        game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).Count()))
            {
                return true;
            }
            return false;
        }

    }
}