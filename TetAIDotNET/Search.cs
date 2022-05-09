using System;
using System.Collections;
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
        const int MAX_NEXT = 5;
        static ISortWay _sortwayInstance = new ISortWay();
        static Dictionary<BitArray, Way[]> TreeData = new Dictionary<BitArray, Way[]>();
        //ネクストの数
        static Dictionary<BitArray, Way>[] Data = new Dictionary<BitArray, Way>[MAX_NEXT];
        static Dictionary<Tree, Way[]> TreeStructure = new Dictionary<Tree, Way[]>();
        static List<Tree>[] LevelTree = new List<Tree>[MAX_NEXT];
        static Tree RootTree;
        static Way? best = null;
        //keyがIndexを表しているtet
        class Tree
        {
            public Tree(int index, int parentindex)
            {
                NowIndex = index;
                ParentIndex = parentindex;
                Childs = new List<Tree>();
            }

            public Way Data;
            public int NowIndex;
            public int ParentIndex;
            public List<Tree> Childs;
        }

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

        static public void RefreshRootTree(int treeindex)
        {


        }

        static public Way Search(BitArray field, Mino current, MinoKind[] nexts, bool canhold, MinoKind? hold)
        {
            var set = new Dictionary<int, Way>();
            var sethold = new Dictionary<int, Way>();
            var actions = new Action[20];

            SearchTreeDeep(set, field, current, 0, new Action[30], new Dictionary<int, int>(), 0);
            Console.WriteLine("検索量:" + set.Count);

            Task task = null;
            if (canhold)
            {
                task = Task.Run(() =>
                  {
                      var newnext = (MinoKind[])nexts.Clone();

                      if (hold == null)
                      {
                          var mino = newnext[0];

                          for (int i = 0; i < newnext.Length - 1; i++)
                              newnext[i] = newnext[i + 1];
                          newnext[newnext.Length - 1] = MinoKind.Null;
                          SearchTreeDeep(sethold, field, Environment.CreateMino(mino), 0, new Action[30], new Dictionary<int, int>(), 0);
                          Console.WriteLine("検索量:" + sethold.Count);

                      }
                      else
                      {
                          SearchTreeDeep(sethold, field, Environment.CreateMino((MinoKind)hold), 0, new Action[30], new Dictionary<int, int>(), 0);
                          Console.WriteLine("検索量:" + sethold.Count);
                      }

                  });
            }
            if (task != null)
                task.Wait();

            var test1 = set.Values.ToArray();
            var test2 = sethold.Values.ToArray();
            Array.Sort(test1, ISortWay.GetInstance());
            Array.Sort(test2, ISortWay.GetInstance());

            Way result;

            if (sethold.Count != 0)
            {
                if (test1[test1.Length - 1].Evaluation > test2[test2.Length - 1].Evaluation)
                    result = test1[test1.Length - 1];
                else
                {
                    result = test2[test2.Length - 1];

                    for (int i = 0; i < result.Actions.Length - 1; i++)
                        result.Actions[i] = Action.Null;

                    result.Actions[0] = Action.Hold;
                }
            }
            else
                result = test1[test1.Length - 1];

            return result;
        }

        static private void GetBest(Tree tree, float[] selectindexes, int nextlevel, float sumofvalue)
        {
            //ネクストがそろってないから修正したほうがいいかも
            if (tree.Childs.Count == 0)
                foreach (var data in tree.Childs)
                {
                    var clone = (float[])selectindexes.Clone();
                    clone[nextlevel] = data.NowIndex;
                    GetBest(data, clone, nextlevel + 1, sumofvalue + data.Data.Evaluation);


                }
        }

        /// <summary>
        /// 非同期でビームサーチを続ける
        /// </summary>
        /// <param name="best"></param>
        /// <param name="field"></param>
        /// <param name="current"></param>
        /// <param name="nexts"></param>
        /// <param name="eval"></param>
        static void SearchTree(ref Way? best, BitArray field, Mino current, MinoKind[] nexts, float eval)
        {
            var set = new Dictionary<int, Way>();
            var actions = new Action[20];

            //ふつうに検索
            SearchTreeDeep(set, field, current, 0, new Action[20], new Dictionary<int, int>(), 0);

            //ソート
            var testways = set.Values.ToArray();
            Array.Sort(testways, _sortwayInstance);

            if (!TreeData.ContainsKey(field))
                TreeData.Add(field, testways);

            //ネクスト更新
            var newnext = (MinoKind[])nexts.Clone();
            if (newnext[0] != MinoKind.Null)
            {
                current = Environment.CreateMino(newnext[0]);
                for (int i = 0; i < newnext.Length - 1; i++)
                    newnext[i] = newnext[i + 1];
            }
            else
            {
                //更新するネクストがない
                if (best == null ||
                    testways[testways.Length - 1].Evaluation > ((Way)best).Evaluation)
                {
                    best = testways[testways.Length - 1];
                }

                return;
            }


            //ビームサーチ準備
            //foreach用に抜き出す
            var nextgen = new Way[10];
            for (int i = 0; i < nextgen.Length; i++)
            {
                nextgen[i] = testways[testways.Length - 1 - i];
                nextgen[i].Evaluation += eval * 0.6f;
            }

            //ビームサーチ　ビーム幅
            foreach (var way in nextgen)
            {
                var newfield = (BitArray)field.Clone();

                foreach (var pos in way.ResultPos)
                    newfield.Set(pos.x + pos.y * 10, true);

                //ライン消去
                Environment.CheckClearedLine(newfield);

                SearchTree(ref best, newfield, current, newnext, way.Evaluation);

            }
        }

        static private void SearchTreeDeep(Dictionary<int, Way> set, BitArray field, Mino current, int actionCount, Action[] actions, Dictionary<int, int> pastway, int rotateCount, bool historyRight = false, bool historyLeft = false)
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

                mino.Move(new Vector2(0, -value));

                var array = actions.CloneArray();
                array[actionCount] = Action.Harddrop;



                var hash = GetHash(mino, true);
                //pastチェック
                Way testvalue;
                if (set.TryGetValue(hash, out testvalue))
                {
                    //存在したら小さい場合のみ
                    if (testvalue.Actions.ActionCount() > array.ActionCount())
                    {
                        set.Remove(hash);
                        set.Add(hash,
                       new Way(array, Evaluation.Evaluate(field, mino), mino.Positions));

                    }
                }
                else
                {
                    set.Add(hash,
                        new Way(array, Evaluation.Evaluate(field, mino), mino.Positions));
                }

                //  set.Add();
            }

            //右移動
            if (!historyLeft &&
                Environment.CheckValidPos(field, current, Vector2.x1))
            {
                var mino = current.Clone();
                mino.Move(Vector2.x1);

                /*場所チェック
                 * なかったら通過
                 * 
                 */

                var array = actions.CloneArray();
                array[actionCount] = Action.MoveRight;
                //array[actionCount] = Action.Softdrop;


                SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount, true, historyLeft);


            }
            //左移動
            if (!historyRight &&
                Environment.CheckValidPos(field, current, Vector2.mx1))
            {
                var mino = current.Clone();
                mino.Move(Vector2.mx1);

                var array = actions.CloneArray();
                array[actionCount] = Action.MoveLeft;

                SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount, historyRight, true);


            }
            if (rotateCount <= 3)
            {
                //右回転
                if (Environment.TryRotate(Rotate.Right, field, ref current, out srs))
                {
                    var mino = current.Clone();
                    Environment.SimpleRotate(Rotate.Right, ref mino);

                    Vector2 temp = (Vector2)srs;
                    mino.Move(temp);

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
                    mino.Move(temp);


                    var array = actions.CloneArray();
                    array[actionCount] = Action.RotateLeft;

                    SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount + 1, historyRight, historyLeft);


                }

            }

            //ソフトドロップ

            int GetHash(Mino mino, bool containsRotate)
            {
                int testway = 0;
                testway += mino.AbsolutelyPosition.y;
                testway += mino.AbsolutelyPosition.x * 100;
                if (containsRotate)
                    testway += (int)mino.Rotation * 10000;
                testway += 100000;

                return testway;
            }

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

        static private Way[] GetPatterns()
        {
            return null;
        }



        SortedList<int, string> Fields = new SortedList<int, string>();//いる？


        List<List<(int hash, int action)>> _seachedPatterns = new List<List<(int hash, int action)>>();
        HashSet<int> _passedTreeRoute = new HashSet<int>();

        public static int GetBest()
        {
            return -1;
            //操作ループから最も高い評価を取り出す
        }

        //渡された操作ミノとフィールドの情報からパターン一覧を追加
        //パターンのリストに追加
        private void SearchAndAddPatterns(Mino mino, BitArray field)
        {
            //中央の位置を専用の変数用意して管理

            //右移動
            if (Environment.CheckValidPos(field, mino, Vector2.x1))
            {
                if (!IsPassedBefore(mino.MinoKind, mino.AbsolutelyPosition, (int)mino.Rotation, true))
                    mino.Move(Vector2.x1);

                //新しいの作れよ
            }


            //移動チェック
            {
                if (!IsPassedBefore(mino.MinoKind, mino.AbsolutelyPosition, (int)mino.Rotation, true))
                {

                }
            }

            //ハードドロップ
            {
                //I,S,Zは180回転状態にy+1したら0状態
                //設置判断いる？そもそも同じ状態にはならないと思うけど
                //↑なるからいるよ

                //    _seachedPatterns.Add()
            }
        }

        private bool IsPassedBefore(MinoKind kind, Vector2 pos, int rotation, bool ApplyHistory)
        {
            //ハッシュ値出して検索、ISZで180状態だった場合は＋１して0回転状態で比較

            //rxxyy
            int hash = pos.y;
            hash += 100 * pos.x;
            hash += 10000 * rotation;

            bool result = _passedTreeRoute.Contains(hash);

            //あった場合はそのまま返す絶対==true
            if (result)
                return result;

            //なかった場合は別チェックと場合によっては追加

            if (kind == MinoKind.S ||
                kind == MinoKind.Z ||
                kind == MinoKind.I)
            {
                if (rotation == (int)Rotation.Turn)
                {
                    int hash2 = pos.y + 1;
                    hash2 += 100 * pos.x;
                    hash += 10000 * (int)Rotation.Zero;

                    result = _passedTreeRoute.Contains(hash2);

                    //あった場合はそのまま返す絶対==true
                    if (result)
                        return result;

                    //なかったら追加判定あったら追加
                    if (ApplyHistory)
                        _passedTreeRoute.Add(hash2);
                    return false;

                }
            }

            if (ApplyHistory)
                _passedTreeRoute.Add(hash);
            return false;

        }

        //評価も追加、評価方法注意

        //
    }
}
