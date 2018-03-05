using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private void PlantBombs()
        {
            if (game.GetMyself().TurnsToStickyBomb != 0) return;
            foreach (Pirate pirate in availablePirates)
            {
                foreach (Pirate enemy in game.GetEnemyLivingPirates().Where(enemyPirate => enemyPirate.InStickBombRange(pirate)))
                {
                    if (game.GetEnemyLivingPirates().Where(enemyPirate => enemy != enemyPirate && enemy.Distance(enemyPirate) < game.StickyBombExplosionRange).Count() > 2)
                    {
                        pirate.StickBomb(enemy);
                        stickedBomb = true;
                        Print(pirate + " sticks a bomb on " + enemy);
                        availablePirates.Remove(pirate);
                        return;
                    }
                }
            }
        }
        // a) Enemy is a capsule holder
        // b) Enemy pirate with high concentration around it
        private List<TargetLocation> GetTargetLocationsEnemyPirates()
        {
            var targetPirates = new List<TargetLocation>();
            var enemyCapsuleHolders = game.GetEnemyLivingPirates().Where(enemy => enemy.HasCapsule());
            foreach (var capsuleHolder in enemyCapsuleHolders)
            {
                int priority = 1;
                if (capsuleHolder.StickyBombs.Any())
                    priority = 10;
                targetPirates.Add(new TargetLocation(capsuleHolder.Location, LocationType.EnemyPirate, priority, capsuleHolder,
                    (game.StickyBombReloadTurns != 0) ? capsuleHolder.NumPushesForCapsuleLoss : 1));
            }
            if (!enemyCapsuleHolders.Any())
            {
                var bestEnemy = game.GetEnemyLivingPirates().Where(enemy => GetEnemiesInBombRange(enemy).Count() >= 2 &&
                    !enemy.StickyBombs.Any()).OrderByDescending(enemy => GetEnemiesInBombRange(enemy).Count()).FirstOrDefault();
                // var centeredEnemy = game.GetEnemyLivingPirates().Where(enemy => GetEnemiesInBombRange(enemy).Count() >= 2)
                //     .OrderByDescending(enemy => GetEnemiesInBombRange(enemy)).FirstOrDefault();
                if (bestEnemy != null)
                    targetPirates.Add(new TargetLocation(bestEnemy.Location, LocationType.EnemyPirate,
                        ScaleToRange(0, game.GetAllEnemyPirates().Count(), MAX_PRIORITY, MIN_PRIORITY, GetEnemiesInBombRange(bestEnemy).Count()), bestEnemy, 1));
            }

            return targetPirates;
        }

        private IEnumerable<TargetLocation> GetTargetLocationsMyPirates()
        {
            List<Pirate> PiratesWithCapsule = game.GetMyLivingPirates().Where(p => p.HasCapsule()).ToList();
            List<TargetLocation> targetLocations = new List<TargetLocation>();
            foreach (Pirate pirate in PiratesWithCapsule)
            {
                var bestMothership = game.GetMyMotherships().OrderBy(mothership => pirate.Steps(mothership) / (int) ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (!CheckIfCapsuleCanReachMothership(pirate, bestMothership))
                {
                    targetLocations.Add(new TargetLocation(pirate.Location, LocationType.MyPirate, 1, pirate));
                }
            }
            // foreach (var pair in pirateDestinations)
            // {
            //     if (!pair.Key.HasCapsule() && !CheckIfPirateCanReach(pair.Key, pair.Value))
            //     {
            //         targetLocations.Add(new TargetLocation(pair.Key.Location, LocationType.MyPirate, 4, pair.Key));
            //     }
            // }
            return targetLocations;
        }

        private bool TryStickBomb(Pirate bomber, Pirate enemyToBomb)
        {
            if (!stickedBomb && game.GetMyself().TurnsToStickyBomb == 0 && bomber.InStickBombRange(enemyToBomb))
            {
                bomber.StickBomb(enemyToBomb);
                stickedBomb = true;
                Print(bomber + " sticks a bomb on " + enemyToBomb);
                return true;
            }
            return false;
        }

        private bool HandleSwitchPirateStates()
        {
            // First we create 4 lists that describe the pirates' preference in terms of states.
            // Each pirate will be in 1 category if he wants to change(wantToBeXXXX), or doesn't mind it(willingToBeXXXXX). Otherwise he will remain in the same state.
            // We consider the following factors:
            // 1. Whether the pirate has a capsule. 2. Whether the pirate is in danger. 3. Whether the pirate is currently performing a bunker/wants to be.
            Print(availablePirates.Count().ToString());
            foreach (Pirate pirate in availablePirates.Where(pirate => pirate.HasCapsule()))
            {
                Print(IsCapsuleHolderInDanger(pirate).ToString());
            }
            List<Pirate> wantToBeHeavy = availablePirates
                .Where(pirate => pirate.HasCapsule() && pirate.IsNormal() && IsCapsuleHolderInDanger(pirate))
                .ToList();
            // wantToBeHeavy.AddRange(bunkeringPirates.Where(pirate => (pirateDestinations.ContainsKey(pirate) && pirate.Location.InRange(pirateDestinations[pirate], pirate.MaxSpeed)) || !pirateDestinations.ContainsKey(pirate)));
            wantToBeHeavy.AddRange(bunkeringPirates.Where(pirate =>(game.GetMyMotherships().Where(mothership => mothership.Distance(pirate) < game.MothershipUnloadRange).Count() > 0 || game.GetEnemyMotherships()
                .Where(mothership => mothership.Distance(pirate) < game.MothershipUnloadRange).Count() > 0)));
            List<Pirate> wantToBeNormal = availablePirates.Where(pirate => pirate.HasCapsule() && pirate.IsHeavy() && !IsCapsuleHolderInDanger(pirate)).ToList();
            List<Pirate> willingToBeNormal = game.GetMyLivingPirates()
                .Except(bunkeringPirates)
                .Where(pirate => !pirate.HasCapsule() && pirate.IsHeavy()).ToList();
            List<Pirate> willingToBeHeavy = game.GetMyLivingPirates()
                .Except(bunkeringPirates)
                .Where(pirate => !pirate.HasCapsule() && pirate.IsNormal()).ToList();
            Print(wantToBeHeavy.Count().ToString());
            Print(wantToBeNormal.Count().ToString());
            Print(willingToBeNormal.Count().ToString());
            Print(willingToBeHeavy.Count().ToString());
            Print(bunkeringPirates.Count().ToString());
            return TrySwitchPirates(wantToBeHeavy, wantToBeNormal) ||
                TrySwitchPirates(willingToBeHeavy, wantToBeNormal) ||
                TrySwitchPirates(willingToBeNormal, wantToBeHeavy);
            // Finally it tries to find pirates who want to swap with eachother, and if unsuccessful, looks for pirates who don't really have a preference to switch them with ones that do.
        }

        private bool TrySwitchPirates(List<Pirate> group1, List<Pirate> group2)
        {
            // Tries to swap the states of two pirates from two groups, if successful returns true.
            var pirate1 = group1.FirstOrDefault();
            var pirate2 = group2.FirstOrDefault();
            if (pirate1 != null && pirate2 != null)
            {
                if (pirateDestinations.ContainsKey(pirate1))
                    pirateDestinations.Remove(pirate1);
                pirate1.SwapStates(pirate2);
                availablePirates.Remove(pirate1);
                return true;
            }
            return false;
        }

        private void HandleBombCarriers()
        {
            var bombCarriers = game.GetMyLivingPirates().Where(pirate => pirate.StickyBombs.Any());
            foreach (var carrier in bombCarriers)
            {
                // Get the best place to go to where the pirate can reach!
                var bombCarried = carrier.StickyBombs.First();
                var enemyPirate = game.GetEnemyLivingPirates().Where(enemy => carrier.Steps(enemy) <= bombCarried.Countdown)
                    .OrderByDescending(enemy => GetEnemiesInBombRange(carrier).Count()).FirstOrDefault();
                if (enemyPirate != null)
                {
                    AssignDestination(carrier, enemyPirate.Location);
                    availablePirates.Remove(carrier);
                }
            }
        }
    }
}