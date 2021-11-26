using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class GeneticAlgorithm
    {
        static public void BenchMarkTest()
        {
            int genCount = 0;
            Random random = new Random();
            List<Indivisual> indivisuals = new List<Indivisual>();

            for (int i = 0; i < 30; i++)
            {
                var param = new float[] { GetRandom(-5.12f, 5.12f, random) };
                indivisuals.Add(new Indivisual(param, Function(param)));
            }

            while (true)
            {
                indivisuals.Sort(new ISortIndivisual());
                // nextgen[0] = EliteChoise(indivisuals);
                var templist = new List<Indivisual>();
                templist.AddRange(indivisuals);
                genCount++;
                Console.WriteLine("世代数:" + genCount);
                for (int i = 0; i < indivisuals.Count; i++)
                {

                    Console.WriteLine(i + "番目の評価:" + indivisuals[i].Evaluation);
                    Console.WriteLine("         " + i + "番目の評価のX座標:" + indivisuals[i].Values[0]);
                    Console.WriteLine("----------");

                }

                var index1 = random.Next(0, indivisuals.Count);
                var index2 = random.Next(0, indivisuals.Count);

                var childs = new List<Indivisual>();
                for (int i = 0; i < 15; i++)
                {
                    childs.Add(BLXAlphaCrossOver(indivisuals[index1], indivisuals[index2], 0.7f));
                    childs[childs.Count - 1].Evaluation = Function(childs[childs.Count - 1].Values);
                }
                //                childs.Sort(new ISortIndivisual());
                templist.AddRange(childs);
                templist.Sort(new ISortIndivisual());

                indivisuals.Add(templist[0]);
                indivisuals.Add(TournamentChoise(templist.ToArray(), 2));
                indivisuals.RemoveAt(index1);
                indivisuals.RemoveAt(index2);

                for (int i = 0; i < indivisuals.Count; i++)
                {
                    indivisuals[i].Evaluation = Function(indivisuals[i].Values);
                }

            }

            float Function(float[] para)
            {
                return para[0] * para[0];

                float k = 0;
                foreach (var x in para)
                    k += 10 + (x * x - 10 * (float)Math.Cos(2 * Math.PI * x));

                if (k > 0)
                    k *= -1;
                return k;
            }
        }

        static public void Learning()
        {
            Console.WriteLine("学習を開始");
            Console.Write("集団の個体数を入力:");
            int learningnum = int.Parse(Console.ReadLine());
            Console.Write("子供の個体数を入力:");
            int childnum = int.Parse(Console.ReadLine());
            Console.WriteLine("ランダム値の下限を入力:");
            int randommin = int.Parse(Console.ReadLine());
            Console.WriteLine("ランダム値の上限を入力:");
            int randommax = int.Parse(Console.ReadLine());
            Console.WriteLine("10世代ごとにその時の集団が[learning_i.txt]として実行ファイルのフォルダに作成されます");

            int genCount = 0;
            Random random = new Random();
            List<Indivisual> indivisuals = new List<Indivisual>();

            for (int i = 0; i < learningnum; i++)
            {
                var param = new float[7];
                for (int j = 0; j < param.Length; j++)
                {
                    param[j] = GetRandom(randommin, randommax, random);
                }
                indivisuals.Add(new Indivisual(param, Environment.GetEval(param)));
            }

            while (true)
            {
                indivisuals.Sort(new ISortIndivisual());

                var AllAddList = new List<Indivisual>();
                genCount++;
                AllAddList.AddRange(indivisuals);

                //出力制限
                if (genCount % 10 == 0)
                {
                    string text = "世代数:" + genCount + "\n";
                    for (int i = 0; i < indivisuals.Count; i++)
                    {
                        text += i + "番目の評価:" + indivisuals[i].Evaluation + "\n";
                        text += "値\n";
                        foreach (var value in indivisuals[i].Values)
                            text += value + " ";
                    }
                    text += "\n";
                    text += ("----------\n");
                    Console.WriteLine(text);

                    StreamWriter writer = new StreamWriter("../learning_" + genCount + ".txt", false, Encoding.UTF8);
                    writer.Write(text);
                    writer.Close();
                }

                //親から２点抜き出し
                var index1 = random.Next(0, indivisuals.Count);
                var index2 = random.Next(0, indivisuals.Count);

                var childs = new List<Indivisual>();
                for (int i = 0; i < childnum; i++)
                {
                    childs.Add(BLXAlphaCrossOver(indivisuals[index1], indivisuals[index2], 0.7f));
                    childs[childs.Count - 1].Evaluation = Environment.GetEval(childs[childs.Count-1].Values);
                }

                AllAddList.AddRange(childs);
                AllAddList.Sort(new ISortIndivisual());

                indivisuals.Add(AllAddList[0]);
                indivisuals.Add(TournamentChoise(AllAddList.ToArray(), 2));
                indivisuals.RemoveAt(index1);
                indivisuals.RemoveAt(index2);

               /* for (int i = 0; i < indivisuals.Count; i++)
                {
                    indivisuals[i].Evaluation = Function(indivisuals[i].Values);
                }*/
            }
        }

        public static Indivisual BLXAlphaCrossOver(Indivisual indivisual1, Indivisual indivisual2, float alpha)
        {
            Random random = new Random();

            Indivisual child = new Indivisual();
            child.Values = new float[indivisual1.Values.Length];

            for (int i = 0; i < indivisual1.Values.Length; i++)
            {
                float xMax, xMin;
                if (indivisual1.Values[i] < indivisual2.Values[i])
                {
                    xMax = indivisual2.Values[i];
                    xMin = indivisual1.Values[i];
                }
                else
                {
                    xMax = indivisual1.Values[i];
                    xMin = indivisual2.Values[i];
                }

                var dx = Math.Abs(xMin - xMax) * alpha;

                xMax += dx;
                xMin -= dx;

                child.Values[i] = GetRandom(xMin, xMax, random);

            }

            return child;

        }

        public static float GetRandom(float min, float max, Random random)
        {
            double range = max - min;

            double sample = random.NextDouble();
            double scaled = (sample * range) + min;
            float f = (float)scaled;

            return f;
        }

        public static int RouletteChoise(Indivisual[] indivisuals)
        {
            Random random = new Random();

            int[] test = new int[indivisuals.Length];
            for (int i = 0; i < indivisuals.Length; i++)
            {
                test[i] = (int)(indivisuals[i].Evaluation * 10000);
            }

            var result = RouletteChoice(test, random);

            return result;

        }
        static public int RouletteChoice(int[] rate, Random random)
        {
            int min = rate.Min();
            if (min < 0)
            {
                for (int i = 0; i < rate.Length; i++)
                {
                    rate[i] -= (min - 1);
                }
            }

            int max = 0;
            for (int i = 0; i < rate.Length; i++)
                max += rate[i];
            int temp = random.Next(max);
            for (int i = 0; i < rate.Length; i++)
            {
                temp -= rate[i];
                if (temp < 0)
                    return i;
            }

            throw new Exception();
        }

        public static Indivisual TournamentChoise(Indivisual[] indivisuals, int tournamentcount)
        {
            Random random = new Random();
            var temp = new List<int>();
            for (int i = 0; i < indivisuals.Length; i++)
                temp.Add(i);

            var test = new List<int>();
            for (int i = 0; i < tournamentcount; i++)
            {
                test.Add(random.Next(0, temp.Count));
                temp.RemoveAt(test[test.Count - 1]);

            }

            Indivisual best = null;
            for (int i = 0; i < test.Count; i++)
            {
                if (best == null ||
                    best.Evaluation < indivisuals[test[i]].Evaluation)
                {
                    best = indivisuals[test[i]];
                }
            }

            return best;
        }

        public static Indivisual EliteChoise(Indivisual[] indivisuals)
        {
            Indivisual result = null;

            foreach (var indi in indivisuals)
            {
                if (result == null ||
                    indi.Evaluation > result.Evaluation)
                    result = indi;
            }

            return result;
        }


        public class Indivisual
        {
            public Indivisual()
            {

            }

            public Indivisual(float[] values, float eval)
            {
                Values = values;
                Evaluation = eval;
            }

            public float[] Values;
            public float Evaluation;



        }
    }
}

