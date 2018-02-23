using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public void CaptureCapsules()
        {
            foreach (Capsule capsule in myCapsules.OrderBy(
                    capsule => capsule.Distance(
                        myPirates.OrderBy(p => ClosestDistance(
                            p.Location, capsule.Location,
                            game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation)))).FirstOrDefault())))
            {
                var PiratesOrdered = availablePirates.Where(p => !p.HasCapsule())
                    .OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()
                        .Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation))));
                if (PiratesOrdered.Any())
                {
                    var closestPirate = PiratesOrdered.First();
                    if (ClosestDistance(closestPirate.Location, capsule.InitialLocation, game.GetAllWormholes()
                            .Where(wormhole => wormhole.TurnsToReactivate < closestPirate.Steps(capsule.InitialLocation))) == closestPirate.Distance(capsule))
                    {

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