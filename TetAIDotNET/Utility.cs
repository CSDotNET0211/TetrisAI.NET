using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    internal class Utility
    {
        static public void ReplaceToBetter(ref Pattern? nowBest, Pattern test)
        {
            if (nowBest == null || test.Eval > ((Pattern)nowBest).Eval)
                nowBest = test;
        }

        static public void ReplaceToBetterUpdateOnlyMove(ref Pattern? nowBest, Pattern test, long move)
        {
            if (nowBest == null)
            {
                test.Move = move;
                nowBest = test;
            }
            else if (test.Eval > ((Pattern)nowBest).Eval)
            {

                var temp = (Pattern)nowBest;
                temp.Move = move;
                nowBest = temp;
            }
        }

        static public void ReplaceToBetter(List<Pattern?> list, int bestIndex, Pattern test)
        {
            //taskIndexは最初のスレッドも含まれてるので-1
            if (list[bestIndex-1] == null|| test.Eval > ((Pattern)list[bestIndex]).Eval)
                list[bestIndex-1] = test;
        }
    }
}
