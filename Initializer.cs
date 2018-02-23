using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public static PirateGame game;
        public const bool Debug = true;
        public Dictionary<Mothership, int> bunkerCount;
        protected List<Pirate> availablePirates;
        protected List<Capsule> myCapsules;
        protected List<Capsule> enemyCapsules;
        protected List<Pirate> myPirates;
        protected Dictionary<Capsule, int> capsulePushes;

        protected Dictionary<Pirate, Location> pirateDestinations;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            if (!game.GetMyMotherships().Any() || !game.GetMyCapsules().Any())
                PerformDefensiveBunker();
            else
            {
                PushEnemyCapsulesAggressively();
                CaptureCapsules();
                PerformAggressiveBunker();
            }
            MovePirates();
        }
        protected void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            availablePirates = pirateGame.GetMyLivingPirates().ToList();
            bunkerCount = new Dictionary<Mothership, int>();
            foreach (var mothership in game.GetEnemyMotherships())
                bunkerCount[mothership] = 0;
            myCapsules = game.GetMyCapsules().ToList();
            enemyCapsules = game.GetEnemyCapsules().ToList();
            myPirates = game.GetMyLivingPirates().ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            capsulePushes = new Dictionary<Capsule, int>();
            foreach (var capsule in enemyCapsules)
                capsulePushes[capsule] = 0;
        }

        protected void MovePirates()
        {
            foreach (var pirate in pirateDestinations.Keys)
            {
                var destination = pirateDestinations[pirate];
                pirate.Sail(destination);
                (pirate + " sails towards " + destination).Print();
            }
        }
    }
}