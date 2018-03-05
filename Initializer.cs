using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public static PirateGame game;
        public const bool Debug = false;
        protected List<Pirate> availablePirates;
        protected Dictionary<Pirate, Location> pirateDestinations;
        protected Dictionary<Capsule, int> enemyCapsulePushes;
        protected Dictionary<Pirate, int> myPiratesWithCapsulePushes;
        public static List<Pirate> bunkeringPirates; //List to add pirates used in bunker to, used in swapping states and finding preferred states.
        protected const int MAX_PRIORITY = 10;
        protected const int MIN_PRIORITY = 1;
        protected bool stickedBomb = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            PushAsteroids();
            PlantBombs();
            HandleBombCarriers();
            PushEnemyCapsulesAggressively();
            if (!game.GetMyMotherships().Any() || !game.GetMyCapsules().Any())
            {
                PerformDefensiveBunker();
                HandleSwitchPirateStates();
            }
            else
            {
                PerformAggressiveBunker();
                HandleSwitchPirateStates();
                DeliverCapsules();
                CaptureCapsules();
            }
            HandlePriorities();
            PrintTargetLocations(GetAllTargetLocations());
            MovePirates();
        }
        protected void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            availablePirates = pirateGame.GetMyLivingPirates().ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            enemyCapsulePushes = new Dictionary<Capsule, int>();
            foreach (var capsule in game.GetEnemyCapsules())
                enemyCapsulePushes[capsule] = 0;
            bunkeringPirates = new List<Pirate>();
            myPiratesWithCapsulePushes = new Dictionary<Pirate, int>();
            foreach(Pirate pirate in game.GetMyLivingPirates().Where(p => p.HasCapsule()))
            {
                myPiratesWithCapsulePushes.Add(pirate, 0);
            }
        }

        protected void MovePirates()
        {
            foreach (var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                pirate.Sail(destination);
                (pirate + " sails towards " + destination).Print();
            }
        }
    }
}