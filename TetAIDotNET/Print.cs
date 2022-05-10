using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    class Print
    {
        static public void PrintGame(BitArray field, long mino, MinoKind[] next, MinoKind? hold, float eval = 0)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;


            var newfield = (BitArray)field.Clone();

            for (int i = 0; i < 4; i++)
            {
                var x = Mino.GetPosition(mino, i, true);
                var y = Mino.GetPosition(mino, i, false);

                newfield.Set(x + y * 10, true);
            }

            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (newfield.Get(x + y * 10))
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

        static public void PrintGameValue(BitArray field, Vector2[] mino, MinoKind[] next, MinoKind? hold, float eval = 0)
        {
            return;
            Console.CursorTop = 0;


            var newfield = (BitArray)field.Clone();

            foreach (var pos in mino)
                newfield.Set(pos.x + pos.y * 10, true);

            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                Console.CursorLeft = 25;
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    //  Console.Write(newfield[x, y]);
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
