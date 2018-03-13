using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class Bot5 : IPirateBot
    {

        public static PirateGame game;
        public const bool Debug = false;
        public static List<Pirate> myPirates;

        public static List<Capsule> myCapsules;
        
        public static List<Mothership> myMotherships;

        public static List<Mothership> enemyMotherships;
        public static List<Pirate> enemyPirates;

        public static List<Capsule> enemyCapsules;

        public static Dictionary<Asteroid, bool> asteroids;

        private bool defence = false;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
        }

        private void Initialize(PirateGame game)
        {
            game=game;
            myPirates = game.GetMyLivingPirates().ToList();
            myCapsules = game.GetMyCapsules().ToList();
            myMotherships = game.GetMyMotherships().ToList();
            enemyMotherships = game.GetEnemyMotherships().ToList();
            enemyPirates = game.GetEnemyLivingPirates().ToList();
            enemyCapsules = game.GetEnemyCapsules().ToList();
            asteroids = new Dictionary<Asteroid, bool>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
        }
    }
}