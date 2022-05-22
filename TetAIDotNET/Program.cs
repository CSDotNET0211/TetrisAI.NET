﻿using System;
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

            Console.WriteLine(Marshal.SizeOf(typeof(Mino)));
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
            Evaluation.Weight = new float[] { -549.1365f, -6832.648f, 2201.142f, -8525.367f, -224.7138f, };

            if (key == ConsoleKey.D1)
            {
                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    var result = environment.Search();
                    var printrsult = result;

                    stopwatch.Restart();
                    var count = Digit(result);

                    for (int i = 0; i < count; i++)
                    {
                        environment.UserInput((Action)(result % 10));
                        result /= 10;

                    }
                      Thread.Sleep(1000);

                    environment.PrintGame();
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
            else if(key == ConsoleKey.D4)
            {
                Console.WriteLine("デバッグモード");

                key = Console.ReadKey().Key;

                Console.WriteLine("1.フィールド編集");
                Console.WriteLine("2.ネクスト編集");

            }
        }

     public   static int Digit(long num)
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
