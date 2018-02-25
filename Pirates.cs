using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        // a) Enemy is a capsule holder
        // b) Enemy pirate with high concentration around it
        private List<TargetLocation> GetTargetLocationsEnemyPirates()
        {
            var targetPirates = new List<TargetLocation>();
            var enemyCapsuleHolders = game.GetEnemyLivingPirates().Where(enemy => enemy.HasCapsule());
            foreach (var capsuleHolder in enemyCapsuleHolders)
            {
                targetPirates.Add(new TargetLocation(capsuleHolder.Location, LocationType.EnemyPirate, 1,
                    (game.StickyBombReloadTurns != 0) ? capsuleHolder.NumPushesForCapsuleLoss : 1));
            }
            if (!enemyCapsuleHolders.Any())
            {
                var bestEnemy = game.GetEnemyLivingPirates().Where(enemy => GetEnemiesInBombRange(enemy).Count() >= 2)
                    .OrderByDescending(enemy => GetEnemiesInBombRange(enemy).Count()).FirstOrDefault();
                // var centeredEnemy = game.GetEnemyLivingPirates().Where(enemy => GetEnemiesInBombRange(enemy).Count() >= 2)
                //     .OrderByDescending(enemy => GetEnemiesInBombRange(enemy)).FirstOrDefault();
                if (bestEnemy != null)
                    targetPirates.Add(new TargetLocation(bestEnemy.Location, LocationType.EnemyPirate,
                        ScaleToRange(0, game.GetAllEnemyPirates().Count(), MAX_PRIORITY, MIN_PRIORITY, GetEnemiesInBombRange(bestEnemy).Count()), 1));
            }

            return targetPirates;
        }

        // TODO: Move to the right file and implement.
        private IEnumerable<TargetLocation> GetTargetLocationsMyPirates()
        {
            return Enumerable.Empty<TargetLocation>();
        }
    }
}