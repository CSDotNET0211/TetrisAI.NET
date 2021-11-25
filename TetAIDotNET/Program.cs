using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class Program
    {
        static void Main(string[] args)
        {
            var environment = new Environment();
            environment.Init();

                Console.WriteLine("開始には何かキーを入力してください...");
                Console.ReadKey();
       
            while (true)
            {
            REINPUT:;
                var result = environment.Search();

                foreach (var action in result.Actions)
                {
                    if(action==Action.Null)
                        break;
                    environment.UserInput(action);
                    Thread.Sleep(30);
                environment.PrintGame();
                }


           //     Console.ReadKey();
                /*
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

                }*/
             

            }


            var mino = new Mino(MinoKind.Z, Rotation.Zero, new Vector2[] { new Vector2(0, 2), new Vector2(1, 2), new Vector2(1, 1), new Vector2(2, 1) });
            var field = new int[3, 3];

            while (true)
            {
                Test(field, mino);
                Environment.SimpleRotate(Rotate.Left, ref mino);
            }
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
