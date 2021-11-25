using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{

    /// <summary>
    /// 通常の探索（全探索）
    /// </summary>
    class DefaultSearch
    {
        struct TreeHistory
        {
            public TreeHistory(Vector2 pos, int actioncount)
            {
                Position = pos;
                ActionCount = actioncount;
            }

            public Vector2 Position;
            public int ActionCount;
        }

        static public Way Search(int[,] field, Mino current, MinoKind[] nexts)
        {
            var set = new List<Way>();
            var actions = new Action[20];

            SearchTreeDeep(set, field, current, 0, new Action[20], new Dictionary<int, int>(), 0);



            set.Sort(ISortWay.GetInstance());
            //    Console.ReadKey();

            return set[set.Count - 1];
        }

        static public void SearchTree(int[,] field, Mino current, MinoKind[] nexts)
        {
            var set = new List<Way>();
            var actions = new Action[20];

            SearchTreeDeep(set, field, current, 0, new Action[20], new Dictionary<int, int>(), 0);

            var newnext = (MinoKind[])nexts.Clone();
            if (newnext[0] != MinoKind.Null)
            {
                current = Environment.CreateMino(newnext[0]);
                for (int i = 0; i < newnext.Length - 1; i++)
                    newnext[i] = newnext[i + 1];
            }
            else
                return;

            //上位20個
            set.Sort(ISortWay.GetInstance());

            var nextgen = new Way[20];
            for (int i = 0; i < nextgen.Length; i++)
            {
                nextgen[i] = set[set.Count - 1 - i];
            }

            foreach (var way in nextgen)
            {
                var newfield = field.CloneArray();

                foreach (var pos in way.ResultPos)
                    newfield[pos.x, pos.y] = 1;

                //ライン消去

                SearchTree(newfield, current, newnext);

            }
        }

        static private void SearchTreeDeep(List<Way> set, int[,] field, Mino current, int actionCount, Action[] actions, Dictionary<int, int> pastway, int rotateCount, bool historyRight = false, bool historyLeft = false)
        {
            if (HistoryContains(pastway, current, actionCount))
            {
                //含まれてる
                return;
            }
            else
            {
                //探索OK
            }

            Vector2? srs;
            //ハードドロップ
            {
                var mino = current.Clone();

                int value = 0;
                while (true)
                {
                    value++;
                    if (!Environment.CheckValidPos(field, mino, new Vector2(0, -value)))
                    {
                        value--;
                        break;
                    }

                }
                mino.Positions[0] += new Vector2(0, -value);
                mino.Positions[1] += new Vector2(0, -value);
                mino.Positions[2] += new Vector2(0, -value);
                mino.Positions[3] += new Vector2(0, -value);
                mino.AbsolutelyPosition += new Vector2(0, -value);

                var array = actions.CloneArray();
                array[actionCount] = Action.Harddrop;

                //pastチェック
                set.Add(new Way(array, Evaluation.Evaluate(field, mino), mino.Positions));
            }

            //右移動
            if (!historyLeft &&
                Environment.CheckValidPos(field, current, Vector2.x1))
            {
                var mino = current.Clone();
                mino.Positions[0] += Vector2.x1;
                mino.Positions[1] += Vector2.x1;
                mino.Positions[2] += Vector2.x1;
                mino.Positions[3] += Vector2.x1;
                mino.AbsolutelyPosition += Vector2.x1;

                /*場所チェック
                 * なかったら通過
                 * 
                 */

                var array = actions.CloneArray();
                array[actionCount] = Action.MoveRight;
                //array[actionCount] = Action.Softdrop;


                SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, 0, true, historyLeft);


            }
            //左移動
            if (!historyRight &&
                Environment.CheckValidPos(field, current, Vector2.mx1))
            {
                var mino = current.Clone();
                mino.Positions[0] += Vector2.mx1;
                mino.Positions[1] += Vector2.mx1;
                mino.Positions[2] += Vector2.mx1;
                mino.Positions[3] += Vector2.mx1;
                mino.AbsolutelyPosition += Vector2.mx1;

                var array = actions.CloneArray();
                array[actionCount] = Action.MoveLeft;

                SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, 0, historyRight, true);


            }
            if (rotateCount <= 3)
            {
                //右回転
                if (Environment.TryRotate(Rotate.Right, field, ref current, out srs))
                {
                    var mino = current.Clone();
                    Environment.SimpleRotate(Rotate.Right, ref mino);

                    Vector2 temp = (Vector2)srs;
                    mino.Positions[0] += temp;
                    mino.Positions[1] += temp;
                    mino.Positions[2] += temp;
                    mino.Positions[3] += temp;
                    mino.AbsolutelyPosition += temp;

                    var array = actions.CloneArray();
                    array[actionCount] = Action.RotateRight;

                    SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount + 1, historyRight, historyLeft);



                }
                //左回転
                if (Environment.TryRotate(Rotate.Left, field, ref current, out srs))
                {
                    var mino = current.Clone();
                    Environment.SimpleRotate(Rotate.Left, ref mino);

                    Vector2 temp = (Vector2)srs;
                    mino.Positions[0] += temp;
                    mino.Positions[1] += temp;
                    mino.Positions[2] += temp;
                    mino.Positions[3] += temp;
                    mino.AbsolutelyPosition += temp;


                    var array = actions.CloneArray();
                    array[actionCount] = Action.RotateLeft;

                    SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount + 1, historyRight, historyLeft);


                }

            }


            //ソフトドロップ

            bool HistoryContains(Dictionary<int, int> way, Mino mino, int count)
            {
                //過去の位置をハッシュで管理して同じだったら枝切り

                int testway = 0;
                testway += mino.AbsolutelyPosition.y;
                testway += mino.AbsolutelyPosition.x * 100;
                testway += (int)mino.Rotation * 10000;
                testway += 100000;

                int value;

                if (way.TryGetValue(testway, out value))
                {
                    if (value > count)
                        return false;
                    else
                        return true;
                }
                else
                    way.Add(testway, count);

                return false;
            }
        }
    }
}
