using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class Evaluation
    {
        static int[,] newfield = new int[Environment.FIELD_WIDTH, Environment.FIELD_HEIGHT];
        static int[] rowheight = new int[Environment.FIELD_WIDTH];
        static public float[] Weight = new float[7];
        //        static int[] tempPos = new int[4];
        public static float Evaluate(int[,] field, Mino mino)
        {
            newfield = field.CloneArray();


            foreach (var pos in mino.Positions)
            {
                newfield[pos.x, pos.y] = 1;
            }

            var cleared = Environment.CheckClearedLine(newfield);
            float clearedValue = 0;
            switch (cleared)
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

            for (int x = 0; x < Environment.FIELD_WIDTH; x++)
            {
                var flag = true;
                for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
                {
                    if (newfield[x, y] == 1)
                    {
                        rowheight[x] = y;
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    rowheight[x] = 0;

            }

            int sumofheight = rowheight.Sum();

            int holecount = 0;
            for (int y = Environment.FIELD_HEIGHT - 1; y >= 1; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (newfield[x, y] == 1 &&
                        newfield[x, y - 1] == 0)
                        holecount++;
                }
            }

            int bump = 0;
            for (int i = 0; i < rowheight.Length - 1; i++)
            {
                bump += Math.Abs(rowheight[i] - rowheight[i + 1]);
            }
            /*
            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if(newfield[x,y]==1)
                    Console.Write("■");
                    else
                    Console.Write("□");
                }
                Console.Write("\n");
            }

            Console.WriteLine((-0.51f * sumofheight));
            Console.WriteLine((0.76f * cleared));
            Console.WriteLine((-0.3566f * holecount));
            Console.WriteLine((-0.1844f * bump));
            Console.WriteLine((-0.51f * sumofheight) + (0.76f * cleared) + (-0.3566f * holecount) + (-0.1844f * bump));
            Console.ReadLine();*/

            return (Weight[0] * sumofheight) + clearedValue + (Weight[5] * holecount) + (Weight[6] * bump);
            // return (-0.51f * sumofheight) + (0.76f * cleared) + (-0.3566f * holecount) + (-0.1844f * bump);
        }
    }
}
