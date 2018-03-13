using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    partial class SSJS12Bot : IPirateBot
    {
        public static PirateGame game;
        private const bool Debug = false;
        private List<Pirate> availablePirates;
        private Dictionary<Pirate, Location> pirateDestinations;
        private Dictionary<Capsule, int> enemyCapsulePushes;
        private Dictionary<Pirate, int> myPiratesWithCapsulePushes;
        private List<Asteroid> availableAsteroids;
        private static List<Pirate> bunkeringPirates; //List to add pirates used in bunker to, used in swapping states and finding preferred states.
        private const int MAX_PRIORITY = 10;
        private const int MIN_PRIORITY = 1;
        private bool stickedBomb = false;

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
        private void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            availableAsteroids = game.GetLivingAsteroids().ToList();
            availablePirates = pirateGame.GetMyLivingPirates().ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            enemyCapsulePushes = new Dictionary<Capsule, int>();
            foreach (var capsule in game.GetEnemyCapsules())
                enemyCapsulePushes[capsule] = 0;
            bunkeringPirates = new List<Pirate>();
            myPiratesWithCapsulePushes = game.GetMyLivingPirates().Where(p => p.HasCapsule()).ToDictionary(pirate => pirate, pirate => 0);
            
        }

        private void MovePirates()
        {
            foreach (var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                pirate.Sail(destination);
                Print(pirate + " sails towards " + destination);
            }
        }
    }
}