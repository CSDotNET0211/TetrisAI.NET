using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    internal class Debug
    {
        public static BitArray CreateField()
        {
            Console.WriteLine("フィールドを作成します。");
            BitArray field = new BitArray(Environment.FIELD_WIDTH + Environment.FIELD_HEIGHT * 10);
            bool selectY = true;
            int selectedLine = -1;

            while (true)
            {
                Console.Clear();

                if (selectY)
                {
                    Console.WriteLine("段落を選択してください");

                    Print(field);

                    Console.Write(":");


                    var line = int.Parse(Console.ReadLine());
                    selectedLine = line;

                    selectY = false;
                }
                else
                {

                    var key = Console.ReadKey().Key;
                    if (key == ConsoleKey.Escape)
                    {
                        selectY = true;
                    }

                    switch (key)
                    {
                        case ConsoleKey.D1:
                            InvertField(0, selectedLine);
                            break;

                        case ConsoleKey.D2:
                            InvertField(1, selectedLine);
                            break;

                        case ConsoleKey.D3:
                            InvertField(2, selectedLine);
                            break;

                        case ConsoleKey.D4:
                            InvertField(3, selectedLine);
                            break;

                        case ConsoleKey.D5:
                            InvertField(4, selectedLine);
                            break;

                        case ConsoleKey.D6:
                            InvertField(5, selectedLine);
                            break;

                        case ConsoleKey.D7:
                            InvertField(6, selectedLine);
                            break;

                        case ConsoleKey.D8:
                            InvertField(7, selectedLine);
                            break;

                        case ConsoleKey.D9:
                            InvertField(8, selectedLine);
                            break;

                        case ConsoleKey.D0:
                            InvertField(9, selectedLine);
                            break;
                    }

                }

            }

            void InvertField(int x, int y)
            {
                if (field[x + y * 10])
                    field[x + y * 10] = false;
                else
                    field[x + y * 10] = true;

            }
        }

        static private void Print(BitArray field)
        {
            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (field.Get(x + y * 10))
                        Console.Write("■");
                    else
                        Console.Write("□");

                    Console.Write(y);
                }
                Console.Write("\n");
            }

            Console.WriteLine("1234567890");
            Console.Write("\n");

        }
    }
}
