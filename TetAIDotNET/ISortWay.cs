using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
