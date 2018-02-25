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
        protected static Dictionary<MapObject, int> Priorities = new Dictionary<MapObject, int>();//General priorities for each map object chosen without pirates
        protected static Dictionary<MapObject, int> AssignedPirates = new Dictionary<MapObject, int>();//number of assigned pirates for each heading
        protected Dictionary<Capsule, int> capsulePushes;
        protected Dictionary<Pirate, Location> pirateDestinations;
        protected List<Asteroid> livingAsteroids;
        protected const int MAX_PRIORITY = 10;
        protected const int MIN_PRIORITY=1;
        protected bool stickedBomb = false;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            // HandleSwitchPirateStates();
            PushAsteroids();
            HandleBombCarriers();
            PushEnemyCapsulesAggressively();
            if (!game.GetMyMotherships().Any() || !game.GetMyCapsules().Any())
                PerformDefensiveBunker();
            else
            {
                DeliverCapsules();
                PerformAggressiveBunker();
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
            bunkerCount = new Dictionary<Mothership, int>();
            foreach (var mothership in game.GetEnemyMotherships())
                bunkerCount[mothership] = 0;
            pirateDestinations = new Dictionary<Pirate, Location>();
            capsulePushes = new Dictionary<Capsule, int>();
            foreach (var capsule in game.GetEnemyCapsules())
                capsulePushes[capsule] = 0;
            livingAsteroids = game.GetLivingAsteroids().ToList();
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