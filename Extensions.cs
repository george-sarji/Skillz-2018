using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Skillz_Code
{
    public static class Extensions
    {
        public static void Print(this string s)
        {
            if(SSJS12Bot.Debug)
                SSJS12Bot.game.Debug(s);
        }
    }

}