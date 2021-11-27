using System;
using System.Collections.Generic;
using System.Linq;
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
            Console.WriteLine("モードを入力してください");
            Console.WriteLine("1.テスト");
            Console.WriteLine("2.学習");
            Console.WriteLine("3.手動");
            var key = Console.ReadKey().Key;
            Environment environment = new Environment();
            environment.Init();
            Evaluation.Weight = new float[] { -0.28932f, -20.3678f, -12.60971f, -17.53663f, -9.156619f, -72.93766f, 56.74286f, -6.684973f, -6.536374f, };

            if (key == ConsoleKey.D1)
            {
                while (true)
                {
                    var result = environment.Search();

                    foreach (var action in result.Actions)
                    {
                        if (action == Action.Null)
                            break;
                        environment.UserInput(action);
                        Thread.Sleep(50);
                        environment.PrintGame();
                    }

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


                }
            }



            var mino = new Mino(MinoKind.Z, Rotation.Zero, new Vector2[] { new Vector2(0, 2), new Vector2(1, 2), new Vector2(1, 1), new Vector2(2, 1) });
            var field = new int[3, 3];

        }

        static void Test(int[,] field, Mino mino)
        {
            Console.Clear();

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    field[x, y] = 0;

            foreach (var pos in mino.Positions)
                field[pos.x, pos.y] = 1;

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

            //            Console.ReadKey();
            Thread.Sleep(300);
        }
    }
}
