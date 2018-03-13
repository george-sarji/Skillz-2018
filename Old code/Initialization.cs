using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public PirateGame game;
        public const bool Debug = true;
        private List<Pirate> myPirates;

        private static double scale;

        private static Dictionary<Wormhole, int> NumOfAssignedPiratesToWormhole;

        private static Dictionary<Asteroid, bool> asteroids;

        private static Dictionary<Pirate, Location> pirateDestinations;

        private static Dictionary<MapObject, int> GeneralPriority;

        private static Dictionary<Capsule, int> enemyCapsulesPushes;
        private static Dictionary<Wormhole, Location> NewWormholeLocation;
        private static Dictionary<Pirate, bool> FinishedTurn;

        // private static List<Pirate> bunkeringPirates;
        private static Dictionary<Pirate, Pirate> piratePairs;

        private const int MinPriorirty = 0;
        private const int MaxPriority = 10;

        private bool defense = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            if (defense)
            {
                BuildBunkerForDefense();
                MovePiratesToDestinations();
            }
            else
            {
                PushEachOther();
                PairMyPirates();
                PushAsteroidsNearby();
                // HandleSwitchPirates();
                MoveToIntersection();
                BuildDefensiveBunker();
                foreach(var pirate in game.GetMyLivingPirates().Where(p => p.HasCapsule()))
                    TryPushMyCapsule(pirate);
                SendCapsuleCaptures();
                PushWormholes();
                PushAsteroids();
                // AttackEnemies();
                MovePiratesToDestinations();
            }
        }

        private void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            FinishedTurn = new Dictionary<Pirate, bool>();
            myPirates = game.GetMyLivingPirates().ToList();
            scale = (((double) (game.Cols.Power(2) + game.Rows.Power(2))).Sqrt());
            NumOfAssignedPiratesToWormhole = new Dictionary<Wormhole, int>();
            NewWormholeLocation = new Dictionary<Wormhole, Location>();
            foreach (Wormhole wormhole in game.GetAllWormholes())
            {
                NewWormholeLocation.Add(wormhole, wormhole.Location);
                NumOfAssignedPiratesToWormhole.Add(wormhole, 0);
            }
            enemyCapsulesPushes = game.GetEnemyCapsules().ToDictionary(key => key, value => 0);
            pirateDestinations = new Dictionary<Pirate, Location>();
            //bunkeringPirates = new List<Pirate>();
            asteroids = new Dictionary<Asteroid, bool>();
            foreach (Pirate pirate in myPirates)
            {
                FinishedTurn.Add(pirate, false);
            }
            foreach (var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
            defense = game.GetMyMotherships().Count() == 0 || game.GetMyCapsules().Count() == 0;
            piratePairs = new Dictionary<Pirate, Pirate>();
        }

        private static void PrintWormhole(Dictionary<Wormhole, int> dictionary, Pirate pirate)
        {
            string str = "{ " + pirate.Id;
            foreach (var key in dictionary.Keys)
            {
                str += key.Id + ":" + dictionary[key] + ",";
            }
            (str + "}").Print();
        }

        private void MovePiratesToDestinations()
        {
            foreach (var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                if (!FinishedTurn[pirate])
                {
                    string message = "";
                    pirate.Sail(destination);
                    message = "Pirate " + pirate.ToString() + " sails towards " + destination.ToString();
                    message.Print();
                }
            }
        }

        private static void AssignDestination(Pirate pirate, Location destination)
        {
            pirateDestinations[pirate] = destination;
        }
    }
}