using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TetAIDotNET.GeneticAlgorithm;

namespace TetAIDotNET
{
    class ISortWay : IComparer<Way>
    {
        public static ISortWay Instance = null;

        public static ISortWay GetInstance()
        {
            if (Instance == null)
                Instance = new ISortWay();
            return Instance;
        }

        int IComparer<Way>.Compare(Way x, Way y)
        {
            if (x.Evaluation > y.Evaluation)
                return 1;

            if (x.Evaluation < y.Evaluation)
                return -1;
            int xcount = x.Actions.ActionCount();
            int ycount = y.Actions.ActionCount();

            if (xcount < ycount)
                return 1;
            if (xcount > ycount)
                return -1;

            return 0;
        }


    }

    class ISortPattern : IComparer<Pattern>
    {
     static ISortPattern Instance = null;


        public static ISortPattern GetInstance()
        {
            if (Instance == null)
                Instance = new ISortPattern();
            return Instance;
        }

        int IComparer<Pattern>.Compare(Pattern x, Pattern y)
        {
            if (x.Eval > y.Eval)
                return -1;
            else if (x.Eval < y.Eval)
                return 1;

            return 0;
        }
    }

    class ISortIndivisual : IComparer<Indivisual>
    {
        int IComparer<Indivisual>.Compare(Indivisual x, Indivisual y)
        {
            if (x.Evaluation > y.Evaluation)
                return -1;
            else if (x.Evaluation < y.Evaluation)
                return 1;

            return 0;
        }
    }

    class ISortVector2Y
    {

    }
}
