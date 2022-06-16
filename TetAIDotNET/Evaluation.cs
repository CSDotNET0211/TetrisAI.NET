using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class Evaluation
    {
        public const int WEIGHT_COUNT = 9;
        [ThreadStatic]
        static int[] _rowheight = new int[Environment.FIELD_WIDTH];
    //    [ThreadStatic]
        static public float[] Weight = new float[WEIGHT_COUNT];
        [ThreadStatic]
   static     List<int> _heightsWithoutIdo;

        //  static float[] holeEval = new float[3];

        public static float NewEvaluate(bool[] field, int lineClearCount)
        {
            if (_rowheight == null)
                _rowheight = new int[Environment.FIELD_WIDTH];

            if (_heightsWithoutIdo == null)
                _heightsWithoutIdo = new List<int>();
            else
                _heightsWithoutIdo.Clear();

            float clearedValue = 0;
            switch (lineClearCount)
            {
                case 1:
                    clearedValue = Weight[1];
                    break;
                case 2:
                    clearedValue = Weight[2];
                    break;
                case 3:
                    clearedValue = Weight[3];
                    break;
                case 4:
                    clearedValue = Weight[4];
                    break;
            }

            int smallest = 50;
            for (int x = 0; x < Environment.FIELD_WIDTH; x++)
            {
                var flag = true;
                for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
                {
                    if (field[x + y * 10])
                    {
                        if (smallest > y)
                            smallest = y;
                        _rowheight[x] = y+1;
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    smallest = 0;
                    _rowheight[x] = 0;
                }

            }

            _heightsWithoutIdo.AddRange(_rowheight);

            //     List<int> heightsWithoutIdo = new List<int>(_rowheight);
            _heightsWithoutIdo.Remove(smallest);


            int sumofheight = _rowheight.Sum();

            int holecount = 0;




            for (int y = Environment.FIELD_HEIGHT - 1; y >= 1; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (field[x + y * 10] &&
                     !field[x + (y - 1) * 10])
                    {
                        holecount++;
                    }
                }
            }


            int bump = 0;
            for (int i = 0; i < _rowheight.Length - 1 - 1; i++)
            {
                bump += Math.Abs(_heightsWithoutIdo[i] - _heightsWithoutIdo[i + 1]);
                //     bump += Math.Abs(GetValue(heightsWithoutIdoLong, i) - GetValue(heightsWithoutIdoLong, i + 1));
            }

            /*
            Console.Clear();
            Print.PrintGame(field,-1,null,null);
            Console.WriteLine("ミノ合計:"+ (Weight[0] * sumofheight));
            Console.WriteLine("ライン消去:"+ clearedValue);
            Console.WriteLine("穴:"+ (Weight[5] * holecount));
            Console.WriteLine("でこぼこ:" + (Weight[6] * bump));
            Console.WriteLine("穴２乗:" + (Weight[7] * holecount * holecount));
            Console.WriteLine("でこぼこ２乗:" + (Weight[8] * bump * bump));
            Console.ReadKey();*/

            return (Weight[0] * sumofheight) +
                clearedValue +
                (Weight[5] * holecount) +//穴の数に対する評価
                (Weight[6] * bump) +
                (Weight[7] * holecount * sumofheight* sumofheight) +
                (Weight[8] * bump * sumofheight* sumofheight);//穴の数を２乗した評価
            // return (-0.51f * sumofheight) + (0.76f * cleared) + (-0.3566f * holecount) + (-0.1844f * bump);

            int GetValue(long value, int index)
            {
                for (int i = 0; i < index; i++)
                    value /= 10;

                return (int)(value % 10);
            }

        }
    }
}
