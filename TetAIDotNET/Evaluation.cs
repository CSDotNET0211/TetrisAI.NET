﻿using System;
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
        // static BitArray newfield = new BitArray(Environment.FIELD_WIDTH * Environment.FIELD_HEIGHT);
        static int[] rowheight = new int[Environment.FIELD_WIDTH];
        static public float[] Weight = new float[WEIGHT_COUNT];
        //  static float[] holeEval = new float[3];

        public static float Evaluate(BitArray field, Mino mino)
        {
            var newfield = (BitArray)field.Clone();

            for (int i = 0; i < 4; i++)
            {
                int x = mino.GetPosition(i, true);
                int y = mino.GetPosition(i, false);

                newfield.Set(x + y * 10, true);
            }


            var cleared = Environment.CheckAndClearLine(newfield);
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

            int smallest = 50;
            for (int x = 0; x < Environment.FIELD_WIDTH; x++)
            {
                var flag = true;
                for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
                {
                    if (newfield.Get(x + y * 10))
                    {
                        if (smallest > y)
                            smallest = y;
                        rowheight[x] = y;
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    smallest = -1;
                    rowheight[x] = -1;
                }

            }
            List<int> heightswithoutido = new List<int>(rowheight);
            heightswithoutido.Remove(smallest);


            int sumofheight = rowheight.Sum();
            float[] holeEval = new float[3];
            for (int i = 0; i < holeEval.Length; i++)
                holeEval[i] = 0;

            int holecount = 0;
            for (int y = Environment.FIELD_HEIGHT - 1; y >= 1; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (newfield.Get(x + y * 10) &&
                     !newfield.Get(x + (y - 1) * 10))
                    {
                        if (holecount < 3)
                        {
                            int testy = 0;
                            while (true)
                            {
                                if (testy + y < Environment.FIELD_HEIGHT &&
                                    newfield.Get(x + (y + testy) * 10))
                                {
                                    testy++;
                                    holeEval[holecount]++;
                                }
                                else
                                    break;
                            }


                        }


                        holecount++;
                    }
                }
            }


            int bump = 0;
            for (int i = 0; i < heightswithoutido.Count - 1; i++)
            {
                bump += Math.Abs(heightswithoutido[i] - heightswithoutido[i + 1]);
            }


            return (Weight[0] * sumofheight) +
                clearedValue +
                (Weight[5] * holecount) +
                (Weight[6] * bump) +
                (Weight[7] * holecount * holecount) +
                (Weight[8] * bump * bump) +
                  (holeEval[0] * holeEval[0] * Weight[9]) +
                  (holeEval[1] * holeEval[1] * Weight[10]) +
                  (holeEval[2] * holeEval[2] * Weight[11]) +
                   (holeEval[0] * Weight[12]) +
                   (holeEval[1] * Weight[13]) +
                   (holeEval[2] * Weight[14]);
            // return (-0.51f * sumofheight) + (0.76f * cleared) + (-0.3566f * holecount) + (-0.1844f * bump);
        }

        public static float NewEvaluate(BitArray field, int lineClearCount)
        {

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
                    if (field.Get(x + y * 10))
                    {
                        if (smallest > y)
                            smallest = y;
                        rowheight[x] = y;
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    smallest = -1;
                    rowheight[x] = -1;
                }

            }
            /* 
            long heightsWithoutIdoLong = 0;

            bool tempFlag = false;
            int tempCount = 1;
           for (int i = 0; i < rowheight.Length; i++)
            {

                if (rowheight[i] != smallest || tempFlag)
                {
                    long temp = rowheight[i];
                    heightsWithoutIdoLong += temp * tempCount;
                    tempCount *= 10;
                }


                if (rowheight[i] == smallest)
                    tempFlag = true;
            }*/


            List<int> heightsWithoutIdo = new List<int>(rowheight);
            heightsWithoutIdo.Remove(smallest);


            int sumofheight = rowheight.Sum();

            int holecount = 0;




            for (int y = Environment.FIELD_HEIGHT - 1; y >= 1; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (field.Get(x + y * 10) &&
                     !field.Get(x + (y - 1) * 10))
                    {
                        holecount++;
                    }
                }
            }


            int bump = 0;
            for (int i = 0; i < rowheight.Length - 1 - 1; i++)
            {
                bump += Math.Abs(heightsWithoutIdo[i] - heightsWithoutIdo[i + 1]);
                //     bump += Math.Abs(GetValue(heightsWithoutIdoLong, i) - GetValue(heightsWithoutIdoLong, i + 1));
            }


            return (Weight[0] * sumofheight) +
                clearedValue +
                (Weight[5] * holecount* sumofheight) +//穴の数に対する評価
                (Weight[6] * bump* sumofheight) +
                (Weight[7] * holecount * holecount* sumofheight) +
                (Weight[8] * bump * bump* sumofheight);//穴の数を２乗した評価
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
