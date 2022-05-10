using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    static class ClassExtension
    {
  /*      static public Mino Clone(this Mino mino)
        {
            var newmino = new Mino();
            newmino.MinoKind = mino.MinoKind;
            newmino.Rotation = mino.Rotation;
            newmino.Init(mino.AbsolutelyPosition,new Vector2[4]);
            Array.Copy(mino._positions, newmino._positions, mino.Positions.Length);
            return newmino;
        }
  */
        public static int ActionCount(this Action[] array)
        {
            int count = 0;
            foreach (var value in array)
                if (value != Action.Null)
                    count++;

            return count;
        }

        public static Vector2 Revert(this Vector2 vec)
        {
            var newvec = new Vector2(vec.x * -1, vec.y * -1);
            return newvec;
        }

        public static Action[] CloneArray(this Action[] actions)
        {
            var array = new Action[actions.Length];
            Array.Copy(actions, array, actions.Length);
            //      Buffer.BlockCopy(actions, 0, array, 0, actions.Length);
            return array;
        }

        public static int[,] CloneArray(this int[,] array)
        {
            var array2 = new int[Environment.FIELD_WIDTH, Environment.FIELD_HEIGHT];
            Array.Copy(array, array2, array.Length);
            return array2;
        }

        public static int Sumof(this int[] array)
        {
            int sum = 0;
            foreach (var value in array)
            {
                sum += value;
            }

            return sum;
        }


        public static void InitWith0(this int[,] array, int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    array[x, y] = 0;

        }
    }
}
