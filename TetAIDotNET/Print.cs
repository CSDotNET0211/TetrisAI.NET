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
        static public void PrintGame(bool[] field, long pos, MinoKind[] next, MinoKind? hold, float eval = 0)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            var fieldclone = (bool[])field.Clone();
            if (pos != -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    var x = Mino.GetPosition(pos, i, true);
                    var y = Mino.GetPosition(pos, i, false);

                    fieldclone[x + y * 10] =true;
                }

            }

            for (int y = Environment.FIELD_HEIGHT - 1; y >= 0; y--)
            {
                for (int x = 0; x < Environment.FIELD_WIDTH; x++)
                {
                    if (fieldclone[x + y * 10])
                        Console.Write("■");
                    else
                        Console.Write("□");
                }
                Console.Write("\n");
            }

            Console.Write("\n");

            Console.WriteLine("評価:" + eval.ToString());

            if (next != null)
            {
                Console.WriteLine("ネクスト");
                foreach (var value in next)
                    Console.WriteLine(value.ToString());
            }

            if (hold != null)
            {
                Console.WriteLine("ホールド");
                if (hold != null)
                    Console.WriteLine(((MinoKind)hold).ToString());
            }
        }
    }
}
