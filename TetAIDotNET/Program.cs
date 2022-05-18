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

            var key = Console.ReadKey().Key;
            Environment environment = new Environment();
            environment.Init();
            Evaluation.Weight = new float[] { 0.2890262f, -19.73375f, -31.28189f, -12.06249f, 32.33967f, -45.3013f, -2.729009f, -47.00863f, -0.1363466f, -0.7741607f, -10.14647f, -23.20074f, 5.343863f, -56.77925f, 3.595039f, };

            if (key == ConsoleKey.D1)
            {
                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    var result = environment.Search();

                    stopwatch.Restart();

                    for (int i = 0; i < Digit(result); i++)
                    {
                        environment.UserInput((Action)(result % 10));
                        result /= 10;

                        Thread.Sleep(50);
                    }

                    environment.PrintGame();
                    stopwatch.Stop();
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
        }

        static int Digit(long num)
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
