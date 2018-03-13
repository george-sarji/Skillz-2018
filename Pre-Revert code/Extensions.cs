using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    public static class GameExtensions
    {
        public static int Steps(this Pirate pirate, MapObject mapObject)
        {
            return pirate.Distance(mapObject) / pirate.MaxSpeed + 1;
        }

        public static double Sqrt(this double num)
        {
            return System.Math.Sqrt(num);
        }

        public static int Power(this int num, int power)
        {
            return (int) System.Math.Pow(num, power);
        }

        public static double Power(this double num, int power)
        {
            return System.Math.Pow(num, power);
        }

        public static void Print(this string s)
        {
            if (SSJS12Bot.Debug)
            {
                System.Console.WriteLine(s);
            }
        }

        public static bool IsHeavy(this Pirate pirate)
        {
            return pirate.StateName == "heavy";
        }

        public static bool IsNormal(this Pirate pirate)
        {
            return pirate.StateName == "normal";
        }

        public static bool IsSameState(this Pirate pirate, Pirate second)
        {
            return pirate.StateName == second.StateName;
        }
    }
}