using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    /// <summary>
    /// 探索した行動のデータを保持
    /// </summary>
    public struct Pattern
    {
        public long Move;
        public long Position;
        public float Eval;
        public int MoveCount;
    }

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

                for (int i = 0; i < 4; i++)
                {
                    int x = Mino.GetPosition(way.ResultPos, i, true);
                    int y = Mino.GetPosition(way.ResultPos, i, false);

                    newfield.Set(x + y * 10, true);
                }

                //ライン消去
                Environment.CheckAndClearLine(newfield);

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
                var mino = current;

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

                mino.Move(y: -value);

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
                       new Way(array, Evaluation.Evaluate(field, mino), mino.Position));

                    }
                }
                else
                {
                    set.Add(hash,
                        new Way(array, Evaluation.Evaluate(field, mino), mino.Position));
                }

                //  set.Add();
            }

            //右移動
            if (!historyLeft &&
                Environment.CheckValidPos(field, current, Vector2.x1))
            {
                var mino = current;
                mino.Move(Vector2.x1.x, Vector2.x1.y);

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
                var mino = current;
                mino.Move(Vector2.mx1.x, Vector2.mx1.y);

                var array = actions.CloneArray();
                array[actionCount] = Action.MoveLeft;

                SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount, historyRight, true);


            }
            if (rotateCount <= 3)
            {
                //右回転
                if (Environment.TryRotate(Rotate.Right, field, ref current, out srs))
                {
                    var mino = current;

                    Vector2 temp = (Vector2)srs;
                    mino.Move(temp.x, temp.y);
                    Environment.SimpleRotate(Rotate.Right, ref mino, 0);


                    var array = actions.CloneArray();
                    array[actionCount] = Action.RotateRight;

                    SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount + 1, historyRight, historyLeft);



                }
                //左回転
                if (Environment.TryRotate(Rotate.Left, field, ref current, out srs))
                {
                    var mino = current;

                    Vector2 temp = (Vector2)srs;
                    mino.Move(temp.x, temp.y);
                    Environment.SimpleRotate(Rotate.Left, ref mino, 0);



                    var array = actions.CloneArray();
                    array[actionCount] = Action.RotateLeft;

                    SearchTreeDeep(set, field, mino, actionCount + 1, array, pastway, rotateCount + 1, historyRight, historyLeft);


                }

            }

            //ソフトドロップ

            int GetHash(Mino mino, bool containsRotate)
            {
                int testway = (int)mino.AbsolutelyPosition;


                //   testway += mino.GetAbsolutelyPosition(0, false);
                //   testway += mino.GetAbsolutelyPosition(0, true) * 100;

                if (containsRotate)
                    testway += (int)mino.Rotation * 10000;
                testway += 100000;

                return testway;
            }

            bool HistoryContains(Dictionary<int, int> way, Mino mino, int count)
            {
                //過去の位置をハッシュで管理して同じだったら枝切り

                int testway = (int)mino.AbsolutelyPosition;
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

        //   static List<Pattern> _endSearchedPatterns = new List<Pattern>();
        /// <summary>
        /// 検索したパターンを中心hashをkeyとして収納
        /// </summary>
        static Dictionary<long, int> _searchedPatterns = new Dictionary<long, int>(100);
        /// <summary>
        /// ミノ位置をそれぞれ保持して重複を最小限に
        /// </summary>
        static HashSet<long> _passedTreeRoute = new HashSet<long>();
        static List<Pattern> _patterns = new List<Pattern>();
        static Pattern? _best = null;

        public static long Get(MinoKind current, MinoKind[] nexts, MinoKind? hold, bool canHold, BitArray field)
        {
            int nextint = 0;
            for (int i = 0; i < nexts.Length; i++)
                nextint = (int)nexts[i] * 10 * (nexts.Length - i - 1);

            _searchedPatterns.Clear();
            _patterns.Clear();
            _passedTreeRoute.Clear();
            _best = null;

           // IsPassedBefore(current, 5050, 0, 0, 0, true);
            GetBest((int)current, 0, 0, hold, canHold, field, -1);
           // GetBest((int)current, nextint, nexts.Length, hold, canHold, field, -1);

            return _best.Value.Move;
        }

        public static void GetBest(int current, int next, int nextCount, MinoKind? hold, bool canHold, BitArray field, long firstMove)
        {
            //ミノの種類からミノ情報作成
            var mino = Environment.CreateMino((MinoKind)current);

            //検索関数に渡してパターンを列挙
            SearchAndAddPatterns(mino, field, 0, 0);
            var patternindexs = _searchedPatterns.Values.ToArray();
            _searchedPatterns.Clear();

            //ネクストカウントが0、つまり最後の先読みの場合最善手を更新して返す
            //このままだと評価ないよ
            if (nextCount == 0)
            {
                Pattern? best = null;

                foreach (var patternindex in patternindexs)
                {
                    var newfield = (BitArray)field.Clone();

                    //設置したミノを適用
                    for (int i = 0; i < 4; i++)
                    {
                        var pattern = _patterns[patternindex];
                        var x = Mino.GetPosition(pattern.Position, i, true);
                        var y = Mino.GetPosition(pattern.Position, i, false);

                        newfield[x + y * 10] = true;
                    }

                    //ラインを消去して評価を適用
                    int clearedLine = Environment.CheckAndClearLine(newfield);
                    var temppattern = _patterns[patternindex];
                    temppattern.Eval = Evaluation.NewEvaluate(newfield, clearedLine);
                    _patterns[patternindex] = temppattern;

                    if (best == null || _patterns[patternindex].Eval > ((Pattern)best).Eval)
                    {
                        best = _patterns[patternindex];
                    }

                }

                if (_best == null || best.Value.Eval > _best.Value.Eval)
                    _best = best;
                return;
            }

            //複製したフィールドに適用して再帰
            //上位２０個だけにしよう
            foreach (var patternindex in patternindexs)
            {
                //ハードロでやってる評価もこっちでやっちゃおう
                var newfield = (BitArray)field.Clone();

                //設置したミノを適用
                for (int i = 0; i < 4; i++)
                {
                    var pattern = _patterns[patternindex];
                    var x = Mino.GetPosition(pattern.Position, i, true);
                    var y = Mino.GetPosition(pattern.Position, i, false);

                    newfield[x + y * 10] = true;
                }

                //ラインを消去して評価を適用
                int clearedLine = Environment.CheckAndClearLine(newfield);
                var temppattern = _patterns[patternindex];
                temppattern.Eval = Evaluation.NewEvaluate(newfield, clearedLine);
                _patterns[patternindex] = temppattern;

                //最初の行動のみ保存
                long first;
                if (firstMove == -1)
                    first = _patterns[patternindex].Move;
                else
                    first = firstMove;

                //再帰
                GetBest(next / 10 * (nextCount - 1), next % 10, nextCount - 1, hold, canHold, newfield, first);
            }
        }

        static private void SearchAndAddPatterns(Mino mino, BitArray field, int moveCount, long move)
        {
            //ハードドロップ
            {
                long tempmove = (int)Action.Harddrop;
                for (int i = 0; i < moveCount; i++)
                    tempmove *= 10;

                var newmino =mino;
                //一番下までソフドロ
                int temp = 0;
                while (true)
                {
                    temp++;
                    if (!Environment.CheckValidPos(field, newmino, new Vector2(0, -temp)))
                    {
                        temp--;
                        break;
                    }
                }
                newmino.Move(y: -temp);

                //設置位置の重複判定のためのハッシュ作成
                long hash = GetHashForPosition(newmino.MinoKind, newmino.Rotation, newmino.Position);
                int valueindex;
                Pattern value;

                //設置があった場合は行動回数で比較して追加判断
                if (_searchedPatterns.TryGetValue(hash, out valueindex))
                {
                    value = _patterns[valueindex];

                    //行動回数が少ないやつ
                    if (value.MoveCount > moveCount)
                    {
                        value.MoveCount = moveCount;
                        value.Move = move + tempmove;
                        _patterns[valueindex] = value;
                    }
                }
                else
                {
                    Pattern pattern = new Pattern();
                    pattern.Position = newmino.Position;
                    pattern.MoveCount = moveCount;
                    pattern.Move = move + tempmove;


                    _patterns.Add(pattern);
                    _searchedPatterns.Add(hash, _patterns.Count - 1);
                }
            }

            //左移動
            if (Environment.CheckValidPos(field, mino, Vector2.mx1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.mx1.x, Vector2.mx1.y, mino.Rotation, true))
                {
                    newmino.Move(Vector2.mx1.x, Vector2.mx1.y);

                    long temp = (int)Action.MoveLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp);
                }
            }

            //右移動
            if (Environment.CheckValidPos(field, mino, Vector2.x1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.x1.x, Vector2.x1.y,mino.Rotation, true))
                {
                    newmino.Move(Vector2.x1.x, Vector2.x1.y);

                    long temp = (int)Action.MoveRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp);
                }
            }


            //右回転
            Vector2? result;
            if (Environment.TryRotate(Rotate.Right, field, ref mino, out result))
            {
                var vec = (Vector2)result;
                var newmino = mino;
                Rotation newrotation = 0;
                Environment.GetNextRotateEnum(Rotate.Right, ref newrotation);

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Right, ref newmino, 0);

                    long temp = (int)Action.RotateRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp);
                }
            }

            //左回転
            if (Environment.TryRotate(Rotate.Left, field, ref mino, out result))
            {
                var vec = (Vector2)result;
                var newmino = mino;
                Rotation newrotation = 0;
                Environment.GetNextRotateEnum(Rotate.Left, ref newrotation);

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Left, ref newmino, 0);

                    long temp = (int)Action.RotateLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp);
                }
            }


        }

        static private bool IsPassedBefore(MinoKind kind, long pos, int x, int y, Rotation newrotation, bool ApplyHistory)
        {
        //    pos += y;
        //    pos += x * 100;
            var hash = GetHashForPosition(kind, newrotation, pos);

            bool result = _passedTreeRoute.Contains(hash);
            if (result)
                return true;

            if (ApplyHistory)
                _passedTreeRoute.Add(hash);

            return false;/*


            //ハッシュ値出して検索、ISZで180状態だった場合は＋１して0回転状態で比較
            //Oミノは回転しないからそのままで大丈夫か

            //rxxyy
            int hash = (int)pos;
            hash += 10000 * newrotation;

            bool result = _passedTreeRoute.Contains(hash);

            //あった場合はそのまま返す絶対==true
            if (result)
                return result;

            //なかった場合は別チェックと場合によっては追加

            if (kind == MinoKind.S ||
                kind == MinoKind.Z ||
                kind == MinoKind.I)
            {
                if (newrotation == (int)Rotation.Turn)
                {
                    long hash2 = pos;
                    Mino.AddPosition(ref hash2, 1, int.MaxValue, false);
                    hash2 += 10000 * (int)Rotation.Zero;

                    result = _passedTreeRoute.Contains((int)hash2);

                    //あった場合はそのまま返す絶対==true
                    if (result)
                        return result;

                    //なかったら追加判定あったら追加
                    if (ApplyHistory)
                        _passedTreeRoute.Add((int)hash2);
                    return false;

                }
            }

            if (ApplyHistory)
                _passedTreeRoute.Add(hash);
            return false;
            */
        }

        /// <summary>
        /// 設置位置判定のためのミノの順番を正規化したハッシュを生成
        /// </summary>
        /// <param name="kind">ミノの種類</param>
        /// <param name="rotation">ミノの回転状態</param>
        /// <param name="hash">通常のハッシュ</param>
        /// <returns></returns>
        static long GetHashForPosition(MinoKind kind, Rotation rotation, long hash)
        {
            if (rotation == Rotation.Zero)
                return hash;

            switch (kind)
            {
                case MinoKind.T:
                    switch (rotation)
                    {
                        case Rotation.Right:
                            return ChangeHashOrder(hash, 1203);
                        case Rotation.Turn:
                            return ChangeHashOrder(hash, 3210);
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 3021);
                    }
                    break;

                case MinoKind.S:
                    switch (rotation)
                    {
                        case Rotation.Right:
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 2301);
                        case Rotation.Turn:
                            return ChangeHashOrder(hash, 3210);
                    }
                    break;

                case MinoKind.Z:
                    switch (rotation)
                    {
                        case Rotation.Right:
                            return ChangeHashOrder(hash, 0213);
                        case Rotation.Turn:
                            return ChangeHashOrder(hash, 3210);
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 3120);
                    }
                    break;

                case MinoKind.L:
                    switch (rotation)
                    {
                        case Rotation.Right:
                            return ChangeHashOrder(hash, 1230);
                        case Rotation.Turn:
                            return ChangeHashOrder(hash, 3210);
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 0321);
                    }
                    break;

                case MinoKind.J:
                    switch (rotation)
                    {
                        case Rotation.Right:
                            return ChangeHashOrder(hash, 1023);
                        case Rotation.Turn:
                            return ChangeHashOrder(hash, 3210);
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 3201);
                    }
                    break;

                case MinoKind.I:
                    switch (rotation)
                    {
                        case Rotation.Right:
                            return ChangeHashOrder(hash, 0123);
                        case Rotation.Turn:
                        case Rotation.Left:
                            return ChangeHashOrder(hash, 3210);

                    }
                    break;
            }

            throw new Exception();
            /*
             * 1023
            3210
            3201
             * 
            T
            0123
            1203
            3210
            3021

            S
            0123
            2301
            3210
            2301

            Z
            0123
            0213
            3210
            3120

            L
            0123
            1230
            3210
            0321

            J
            0123
            1023
            3210
            3201

            I
            0123
            0123
            3210
            3210
            */

            long ChangeHashOrder(long hashcode, int order)
            {
                //https://c-taquna.hatenablog.com/entry/2019/09/09/010513

                long result = 0;

                for (int i = 0; i < 4; i++)
                {
                    long temphash = hashcode;
                    int temporder = order;

                    for (int j = 0; j < 4; j++)
                    {
                        temphash /= 10000;
                        temporder /= 10;
                    }

                    temphash %= 10000;
                    temporder %= 10;

                    temporder = 3 - temporder;

                    for (int j = 0; j < temporder; j++)
                    {
                        temphash *= 10000;
                    }

                    result += temphash;
                }

                return result;
            }
        }
        //評価も追加、評価方法注意
        //
        //
    }
}
