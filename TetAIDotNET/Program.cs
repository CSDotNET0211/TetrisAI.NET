using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static TetAIDotNET.GeneticAlgorithm;

namespace TetAIDotNET
{

    class Program
    {
        static void Main(string[] args)
        {

            //Console.WriteLine(Marshal.SizeOf(typeof(Mino)));
            Console.WriteLine(Convert.ToString(long.MaxValue, 10));
            Console.WriteLine(Convert.ToString(5000, 10));

            // Mino.GetPosition(ref value,);

            // MisaMinoNET.MisaMino.FindMove(,,,,,,)
            Console.WriteLine("TetrisAI.NET");
            int worker, io;
            ThreadPool.GetMaxThreads(out worker, out io);
            Console.WriteLine($"MAXThread Worker:{worker},IO:{io}\n");
            Console.WriteLine("モードを入力してください");
            Console.WriteLine("1.テスト");
            Console.WriteLine("2.学習");
            Console.WriteLine("3.手動");
            Console.WriteLine("4.デバッグ");

            var key = Console.ReadKey().Key;
            Environment environment = new Environment();
            environment.Init();
            // Evaluation.Weight = new float[] { -14937.3f, -8614.639f, -550.9185f, -487.5173f, -1525.667f, -58.71915f, 741.6671f, -325.8765f, -84.48547f, -223.764f, -54.847f, -219.4925f, 154.7065f, 481.9854f, 1180.017f, };
            Evaluation.Weight = new float[] { 200.1597f, 319.1632f, - 1149.735f, 118.6968f, 187.1296f, - 604.2106f, - 551.1594f, - 364.9467f, - 43.58047f, };
            // 110.3654 -11176.37 234.8906 907.7881 -288.0103 -29667.07 893.7445 -696.517 -124.9767 1番目の評価:23776
            //200.1597 319.1632 -1149.735 118.6968 187.1296 -604.2106 -551.1594 -364.9467 -43.58047
            if (key == ConsoleKey.D1)
            {
                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    stopwatch.Restart();
                    var result = environment.Search();
                    var printrsult = result;

                    var count = Digit(result);

                    for (int i = 0; i < count; i++)
                    {
                        environment.UserInput((Action)(result % 10));
                        result /= 10;

                    }
                    environment.PrintGame();
                    //Thread.Sleep(100);

                    stopwatch.Stop();
                    Console.WriteLine(printrsult);
                    Console.WriteLine("経過時間:" + stopwatch.Elapsed);
                    //   Console.ReadKey();
                }
            }
            else if (key == ConsoleKey.D2)
            {
                Learning();
            }
            else if (key == ConsoleKey.D3)
            {
                while (true)
                {
                REINPUT:;
                    //     Console.ReadKey();

                    environment.PrintGame();
                    var input = Console.ReadKey().Key;

                    switch (input)
                    {
                        case ConsoleKey.RightArrow:
                            environment.UserInput(Action.MoveRight);
                            break;

                        case ConsoleKey.LeftArrow:
                            environment.UserInput(Action.MoveLeft);
                            break;

                        case ConsoleKey.UpArrow:
                            environment.UserInput(Action.Harddrop);
                            break;

                        case ConsoleKey.DownArrow:
                            environment.UserInput(Action.Softdrop);
                            break;

                        case ConsoleKey.C:
                            environment.UserInput(Action.Hold);
                            break;
                        case ConsoleKey.X:
                            environment.UserInput(Action.RotateRight);
                            break;
                        case ConsoleKey.Z:
                            environment.UserInput(Action.RotateLeft);
                            break;

                        case ConsoleKey.R:
                            environment.Init();
                            break;
                        default:
                            goto REINPUT;

                    }

                    Console.WriteLine(environment._nowMino.Position);
                    Console.WriteLine(environment._nowMino.AbsolutelyPosition);
                }
            }
            else if (key == ConsoleKey.D4)
            {
                Console.WriteLine("デバッグモード");

                key = Console.ReadKey().Key;

                Console.WriteLine("1.フィールド編集");
                Console.WriteLine("2.ネクスト編集");

            }
        }

        public static int Digit(long num)
        {
            // Mathf.Log10(0)はNegativeInfinityを返すため、別途処理する。
            return (num == 0) ? 1 : ((int)Math.Log10(num) + 1);
        }


        static void Test(int[,] field, Mino mino)
        {
            Console.Clear();

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    field[x, y] = 0;

            for (int i = 0; i < 4; i++)
            {
                int x = mino.GetPosition(i, true);
                int y = mino.GetPosition(i, false);

                field[x, y] = 1;
            }

            for (int y = 2; y >= 0; y--)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (field[x, y] == 0)
                        Console.Write("□");
                    else
                        Console.Write("■");
                }
                Console.Write("\n");
            }

            Thread.Sleep(300);
        }
    }
}
