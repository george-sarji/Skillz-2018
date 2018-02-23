using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public static PirateGame game;
        public const bool Debug = true;

        protected List<Pirate> availablePirates;
        protected List<Capsule> myCapsules;
        protected List<Capsule> enemyCapsules;
        protected List<Pirate> myPirates;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
        }
        public void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            availablePirates = pirateGame.GetMyLivingPirates().ToList();
            myCapsules = game.GetMyCapsules().ToList();
            enemyCapsules = game.GetEnemyCapsules().ToList();
            myPirates = game.GetMyLivingPirates().ToList();
        }
    }
}