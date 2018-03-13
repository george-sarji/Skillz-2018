using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    public static class Extensions
    {
        public static int Power(this int num, int power)
        {
            return (int) System.Math.Pow(num, power);
        }
        public static double Power(this double num, int power)
        {
            return System.Math.Pow(num, power);
        }

        public static double Sqrt(this int num)
        {
            return System.Math.Sqrt(num);
        }
        public static double Sqrt(this double num)
        {
            return System.Math.Sqrt(num);
        }
        public static bool IsHeavy(this Pirate pirate)
        {
            return pirate.StateName == SSJS12Bot.game.STATE_NAME_HEAVY;
        }
        public static bool IsNormal(this Pirate pirate)
        {
            return pirate.StateName == SSJS12Bot.game.STATE_NAME_NORMAL;
        }
        public static bool IsSameState(this Pirate pirate, Pirate second)
        {
            return pirate.StateName == second.StateName;
        }
        public static int Steps(this Pirate pirate, MapObject mapObject)
        {
            return pirate.Distance(mapObject) / (pirate.MaxSpeed + 1);
        }

        public static int Steps(this Asteroid asteroid, MapObject mapObject)
        {
            return asteroid.Distance(mapObject) / (asteroid.Speed + 1);
        }

        public static int Steps(this Location location, int distance, int speed)
        {
            return distance/(speed+1);
        }

        public static int Clamp(this int x, int min, int max)
        {
            return x<min ? min : x> max ? max :
                x;
        }
    }
}