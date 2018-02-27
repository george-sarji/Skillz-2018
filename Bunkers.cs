using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {

        protected void PerformAggressiveBunker()
        {
            var header = string.Format("Mothership bunkers:\n{0, -8} {1, 7} {2, 12} {3, 12} {4,12}", "Mothership", "Capsule", "  Location  ", "  Capsule loss  ", "  Border pushes  ");
            // Go over the capsules which have a holder, ordered by the steps of their holder to the best mothership.
            foreach (var capsule in game.GetEnemyCapsules().Where(capsule => capsule.Holder != null)
                    .OrderBy(capsule => capsule.Holder.Steps(GetBestMothershipThroughWormholes(capsule.Holder))))
            {
                // Get the best mothership
                var mothership = GetBestMothershipThroughWormholes(capsule.Holder);
                // Add the bunker count at the mothership to edit the range
                bunkerCount[mothership]++;
                // Get the distance from the capsule location to the closest border
                var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                // Get the available pirates that will have a push by the time they get to the mothership, where the pirate can reach before the capsule
                // and the pirate doesnt have a capsule, ordered by the turns to the mothership.
                var useablePirates = availablePirates.Where(p => p.Steps(mothership) > p.PushReloadTurns)
                    .Where(p => p.Steps(mothership) < capsule.Holder.Steps(mothership))
                    .Where(p => p.Capsule == null)
                    .OrderBy(p => p.Steps(mothership));
                // Initialize the counters for the push distance and the amount of pirates and calculate them.
                int count = 0, pushDistanceUsed = 0;
                foreach (var pirate in useablePirates.OrderByDescending(p => p.PushDistance))
                {
                    if (pushDistanceUsed < distanceToBorder)
                    {
                        count++;
                        pushDistanceUsed += pirate.PushDistance;
                    }
                }
                // Get the minimum of the required pirates count
                var requiredPiratesCount = Min((count == 0) ? 1 : count, capsule.Holder.NumPushesForCapsuleLoss);
                // Get the best wormhole for the capsule to go through to get to the mothership.
                var bestWormhole = GetBestWormhole(mothership.Location, capsule.Holder);
                // Check if we have enough pirates to cause capsule loss (minimum pirates)
                if (useablePirates.Count() >= requiredPiratesCount)
                {
                    // Add the bunker to the table and print it if this is the last capsule
                    header += string.Format("\n{0, -8} {1, 9} @ {2, 12} {3,12} {4,12}", "ID: " + mothership.Id, "ID: " + capsule.Id, mothership.Location, capsule.Holder.NumPushesForCapsuleLoss, count);
                    if (game.GetEnemyCapsules().Where(cap => cap.Holder != null)
                        .OrderBy(cap => cap.Holder.Steps(GetBestMothershipThroughWormholes(cap.Holder))).Last().Equals(capsule))
                        header.Print();
                    // Are the pirates taken equal to the pushers required to the border? If so, order the pirates by their pushing distance to ensure push outside of border.
                    if (requiredPiratesCount == count)
                        useablePirates = useablePirates.OrderByDescending(p => p.PushDistance);
                    // Create the used pirates list to remove the pirates from the available pirates
                    var usedPirates = new List<Pirate>();
                    // Go over the pirates 
                    foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                    {
                        if (bestWormhole != null && bestWormhole.Partner != null &&
                            bestWormhole.Partner.InRange(mothership, mothership.UnloadRange * 3))
                        {
                            // Add push wormhole here
                        }
                        else
                        {
                            // Get the range needed for the bunker to be complete (distance from the mothership according to the bunker coubt)
                            var rangeNeeded = bunkerCount[mothership].Power(2) * game.PushRange;
                            // Get the destination for the pirate to go to
                            var destinationBunker = mothership.Location.Towards(capsule, rangeNeeded);
                            // Check if the capsule is in the range of the pirate and if the pirate is in range of the mothership. If so, go after the capsule
                            // and push it untill its gone.
                            if (useablePirates.Count(p => p.InRange(capsule, p.PushRange * 2) && p.InRange(mothership, p.PushDistance * 2)) >= requiredPiratesCount &&
                                pirate.InRange(capsule, pirate.PushRange * 2) && pirate.InRange(mothership, pirate.PushDistance * 2))
                            {
                                destinationBunker = capsule.Location.Towards(mothership, (int) (pirate.PushRange * 0.9));
                            }
                            // Assign the destination to the proper destination (bunker / closest capsule)
                            AssignDestination(pirate, destinationBunker);
                        }
                        // Add the pirate into the used pirates list
                        usedPirates.Add(pirate);
                        // Add the pirate into the list of the bunker pirates (used for swap states)
                        bunkeringPirates.Add(pirate);
                    }
                    // Remove the used pirates from the available pirates list to ensure no ignored actions.
                    availablePirates = availablePirates.Except(usedPirates).ToList();
                }
            }
        }

        protected void PerformDefensiveBunker()
        {
            // This bunker does not allow the enemy to gather points by clustering the bunker.
            // Check if the enemy has any capsules that have a holder. If so, initiate the table for debug.
            if (game.GetEnemyCapsules().Any(capsule => capsule.Holder != null))
            {
                ("Defensive mothership bunkers: ").Print();
                var header = string.Format("{0, -8} {1, 7} {2, 12} {3, 12} {4,12}", "Mothership", "Capsule", "  Location  ", "  Capsule loss  ", "  Border pushes  ");
                header.Print();
            }
            // Go over the capsules that have a holder, ordered by the turns it takes for them to reach the best mothership
            foreach (var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder != null)
                    .OrderBy(cap => cap.Holder.Steps(GetBestMothershipThroughWormholes(cap.Holder).Location)))
            {
                // Get the best mothership for the capsule
                var bestMothership = GetBestMothershipThroughWormholes(capsule.Holder);
                // Check if there is a "best mothership" for the capsule.
                if (bestMothership != null)
                {
                    // Get the distance to the closest border point.
                    var distanceToBorder = capsule.Distance(GetClosestToBorder(capsule.Location));
                    // Get the available pirates that can push the capsule once they reach the mothership, ordered by the turns it takes them to get to the mothership.
                    var useablePirates = availablePirates.Where(p => p.Steps(bestMothership) >= p.PushReloadTurns).OrderBy(p => p.Steps(bestMothership));
                    // Initialize counters for pushes to closest border.
                    int count = 0, pushDistanceUsed = 0;
                    // Count the pushes needed
                    foreach (var pirate in useablePirates.OrderByDescending(p => p.PushDistance))
                    {
                        if (pushDistanceUsed < distanceToBorder)
                        {
                            count++;
                            pushDistanceUsed += pirate.PushDistance;
                        }
                    }
                    // Get the minimum number of required pirates for capsule loss.
                    var requiredPiratesCount = Min((count == 0) ? 1 : count, capsule.Holder.NumPushesForCapsuleLoss);
                    // If the useable pirates count is the same as the pushes needed, order the pirates by the push distance.
                    if(requiredPiratesCount == count)
                        useablePirates = useablePirates.OrderByDescending(pirate => pirate.PushDistance);
                    // Check if we have any pirates that can defend the mothership.
                    if (useablePirates.Count() >= requiredPiratesCount)
                    {
                        // Print the bunker line in the debug table.
                        var line = string.Format("{0, -8} {1, 9} @ {2, 12} {3,12} {4,12}", "ID: " + bestMothership.Id, "ID: " + capsule.Id, bestMothership.Location, capsule.Holder.NumPushesForCapsuleLoss, count);
                        line.Print();
                        var usedPirates = new List<Pirate>();
                        // Go over the pirates required
                        foreach (var pirate in useablePirates.Take(requiredPiratesCount))
                        {
                            // Assign the destination for the pirate to bunker the mothership at range of capsule max speed
                            AssignDestination(pirate, bestMothership.Location.Towards(capsule, (int) (capsule.Holder.MaxSpeed)));
                            // Add the pirate into the used pirates list
                            usedPirates.Add(pirate);
                            // Add the pirate into the bunkering pirates list (used for swap states)
                            bunkeringPirates.Add(pirate);
                        }
                        // Remove the used pirates from the available pirates list.
                        availablePirates = availablePirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}