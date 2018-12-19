using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        private bool TrySuperPush(Pirate pirate)
        {
            if (pirate.CanSuperPush())
            {
                pirate.SuperPush();
                return true;
            }
            return false;
        }

        private Pirate GetBestPushingPirate()
        {
            Dictionary<Pirate, int> NumOfPushedPirates = new Dictionary<Pirate, int>();
            foreach (Pirate pirate in availablePirates)
            {
                NumOfPushedPirates[pirate] = NumOfEnemiesOutOfBorder(pirate);
            }
            if (availablePirates.Count == 0)
                return null;
            return NumOfPushedPirates.OrderByDescending(entry => entry.Value).FirstOrDefault().Key;
        }
        private int NumOfEnemiesOutOfBorder(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Count(enemy => !enemy.Location.Towards(pirate, -game.SuperPushDistance).InMap()) +
                game.GetEnemyLivingPirates().Count(enemy => enemy.HasCapsule() && !enemy.Location.Towards(pirate, -game.SuperPushDistance).InMap());
        }

        private void SuperPush()
        {
            Pirate pirate = GetBestPushingPirate();
            if (pirate != null)
            {
                if (NumOfEnemiesOutOfBorder(pirate) >= 1)
                {
                    if (TrySuperPush(pirate))
                        availablePirates.Remove(pirate);

                }
                Pirate bestPirate = game.GetMyLivingPirates()
                    .Where(p => IsInDanger(pirate.Location, GetMyBestMothershipThroughWormholes(p).Location, p)).FirstOrDefault();
                if (bestPirate != null)
                {
                    if (TrySuperPush(bestPirate))
                    {
                        pirateDestinations.Remove(bestPirate);
                        availablePirates.Remove(bestPirate);
                    }
                }
            }
        }
    }
}