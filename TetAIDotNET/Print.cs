using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class Print
    {
        static public void PrintGame(int[,] field, Vector2[] mino, MinoKind[] next, MinoKind? hold, float eval = 0)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;


            var newfield = (int[,])field.Clone();

            foreach (var pos in mino)
                newfield[pos.x, pos.y] = 1;

            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (newfield[x, y] == 1)
                        Console.Write("■");
                    else
                        Console.Write("□");
                }
                Console.Write("\n");
            }

            Console.Write("\n");

            Console.WriteLine("評価:" + eval);

            Console.WriteLine("ネクスト");
            foreach (var value in next)
                Console.WriteLine(value);

            Console.WriteLine("ホールド");
            if (hold != null)
                Console.WriteLine((MinoKind)hold);
        }

        static public void PrintGameValue(int[,] field, Vector2[] mino, MinoKind[] next, MinoKind? hold, float eval = 0)
        {
            Console.CursorTop = 0;


            var newfield = (int[,])field.Clone();

            foreach (var pos in mino)
                newfield[pos.x, pos.y] = 1;

            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
            Console.CursorLeft = 25;
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    Console.Write(newfield[x, y]);
                  //  Console.Write(" ");
                }
                Console.Write("\n");
            }

            Console.Write("\n");

            Console.WriteLine("評価:" + eval);

            Console.WriteLine("ネクスト");
            foreach (var value in next)
                Console.WriteLine(value);

            Console.WriteLine("ホールド");
            if (hold != null)
                Console.WriteLine((MinoKind)hold);
        }
    }

}
