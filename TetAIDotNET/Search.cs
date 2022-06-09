using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public int FieldIndex;


    }

    /// <summary>
    /// 通常の探索（全探索）
    /// </summary>
    class BeemSearch
    {
        //keyがIndexを表しているtet

        //   static List<Pattern> _endSearchedPatterns = new List<Pattern>();
        /// <summary>
        /// 検索したパターンを中心hashをkeyとして収納
        /// </summary>
        static List<Dictionary<long, Pattern>> _searchedPatternsList = new List<Dictionary<long, Pattern>>();
        /// <summary>
        /// ミノ位置をそれぞれ保持して重複を最小限に
        /// </summary>
        static List<HashSet<long>> _passedTreeRoutesList = new List<HashSet<long>>();
        static List<Pattern?> _bestInThreadList = new List<Pattern?>();

        static Pattern? _best = null;
        static int _threadCount;

        public static List<List<BitArray>> _fieldsList = new List<List<BitArray>>();
        static ManualResetEvent _resetEvent;
        static int _remainThreadCount;
        public static long GetBestMove(MinoKind current, MinoKind[] nexts, MinoKind? hold, bool canHold, BitArray field, int nextCount)
        {
            _resetEvent = new ManualResetEvent(false);
            int nextint = 0;
            for (int i = 0; i < nextCount; i++)
                nextint = (int)nexts[i] * (10 * (nextCount - i - 1));

            int holdint = hold == null ? -1 : (int)hold;

            //   nextint = (int)nexts[0];
            _searchedPatternsList.Clear();
            //   _patterns.Clear();
            _fieldsList.Clear();
            _passedTreeRoutesList.Clear();
            _best = null;
            _bestInThreadList.Clear();

            _threadCount = 1;
            _passedTreeRoutesList.Add(new HashSet<long>());
            _searchedPatternsList.Add(new Dictionary<long, Pattern>());
            _fieldsList.Add(new List<BitArray>());
            int taskIndex = 0;
            GetBest((int)current, nextint, 1, holdint, canHold, field, -1, 0, ref taskIndex);

            _resetEvent.WaitOne();

            foreach (var test in _bestInThreadList)
            {
                if (_best == null || _best.Value.Move < test.Value.Move)
                    _best = test;
            }


            return _best.Value.Move;
        }

        public static void GetBest(int current, int next, int nextCount, int hold, bool canHold, BitArray field, long firstMove, float beforeEval, ref int taskIndex)
        {
            Console.WriteLine("スレッド:" + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("検索開始:" + Thread.CurrentThread.ManagedThreadId);

            //ミノの種類からミノ情報作成
            var mino = Environment.CreateMino((MinoKind)current);

            //検索関数に渡してパターンを列挙 
            SearchAndAddPatterns(mino, field, 0, 0, Action.Null, 0, ref taskIndex);
            var patternsInThisMove = _searchedPatternsList[taskIndex].Values.ToArray();
            _searchedPatternsList[taskIndex].Clear();

            Console.WriteLine("検索終了、次再帰準備開始:" + Thread.CurrentThread.ManagedThreadId);
            //ネクストカウントが0、つまり最後の先読みの場合最善手を更新して返す
            //上位２０個を持ってくる
            if (nextCount == 0)
            {
                Pattern? best = null;

                //20より大きい場合はソートして上位の順番で再帰
                int beemWidth;
                if (patternsInThisMove.Length < 10)
                    beemWidth = patternsInThisMove.Length;
                else
                {
                    Array.Sort(patternsInThisMove, ISortPattern.GetInstance());
                    beemWidth = 10;
                }

                for (int beem = 0; beem < beemWidth; beem++)
                {
                    //patternsInThisMove[beem].Eval += beforeEval;
                    long first;
                    if (firstMove == -1)
                        first = patternsInThisMove[beem].Move;
                    else
                        first = firstMove;

                    //１つの最終的な手の中で最も良い手が存在しないか今の手がより良い評価だった場合は交換
                    //最後の手は行動だけ変えとけばいいか
                    Utility.ReplaceToBetterUpdateOnlyMove(ref best, patternsInThisMove[beem], first);
                    
                }

                //評価が高ければ更新
                if (taskIndex == 0)
                {
                    Utility.ReplaceToBetter(ref _best, (Pattern)best);

                    _resetEvent.Set();
                }
                else
                {
                    Utility.ReplaceToBetter(_bestInThreadList, taskIndex, (Pattern)best);

                   
                }
            }
            else
            {
                //複製したフィールドに適用して再帰
                //上位２０個

                //20より大きい場合はソートして再帰
                int beemWidth;
                if (patternsInThisMove.Length <= 10)
                    beemWidth = patternsInThisMove.Length;
                else
                {
                    beemWidth = 10;
                    Array.Sort(patternsInThisMove, ISortPattern.GetInstance());
                }

                //全スレッドの終了通知に使用
                if (taskIndex == 0)
                    _remainThreadCount = beemWidth;

                //firstMoveがない＝最初の検索の場合、スレッドを起動する
                for (int beem = 0; beem < beemWidth; beem++)
                {
                    patternsInThisMove[beem].Eval += beforeEval;

                    //最初の行動のみ保存
                    long first;
                    if (firstMove == -1)
                        first = patternsInThisMove[beem].Move;
                    else
                        first = firstMove;

                    int newcurrent = next;
                    int newnext = next;
                    int tempDiv = 10;
                    for (int i = 0; i < nextCount - 1; i++)
                    {
                        newcurrent /= 10;
                        tempDiv *= 10;
                    }

                    newnext %= tempDiv;

                    //再帰
                    if (firstMove == -1)
                    {
                        //firstMoveが-1＝＝探索を１度もやっていない場合ここに条件分岐

                        PrepareThread();
                        _threadCount++;
                        taskIndex = _threadCount - 1;

                        var args = new ClassForThreadArgs(newcurrent, newnext, nextCount - 1, hold, canHold, _fieldsList[0][patternsInThisMove[beem].FieldIndex], first, patternsInThisMove[beem].Eval, taskIndex);
                        ThreadPool.QueueUserWorkItem(GetBest, args);
                        Thread.Sleep(100000000);
                    }
                    else
                        GetBest(newcurrent, newnext, nextCount - 1, hold, canHold, _fieldsList[taskIndex][patternsInThisMove[beem].FieldIndex], first, patternsInThisMove[beem].Eval, ref taskIndex);



                }
            }

            void PrepareThread()
            {
                _passedTreeRoutesList.Add(new HashSet<long>());
                _searchedPatternsList.Add(new Dictionary<long, Pattern>());
                _fieldsList.Add(new List<BitArray>());
                _bestInThreadList.Add(null);
            }
        }

        //

        public static void GetBest(object param)
        {
            Console.WriteLine("スレッド開始:" + Thread.CurrentThread.ManagedThreadId);

            var args = param as ClassForThreadArgs;
            int current = args.Current;
            int next = args.Next;
            int nextCount = args.NextCount;
            int hold = args.Hold;
            bool canHold = args.CanHold;
            BitArray field = args.Field;
            long firstMove = args.First;
            float beforeEval = args.Eval;
            int taskIndex = args.TaskIndex;

            GetBest(current, next, nextCount - 1, hold, canHold, field, firstMove, beforeEval, ref taskIndex);

            if (Interlocked.Decrement(ref _remainThreadCount) == 0)
                _resetEvent.Set();

            Console.WriteLine("スレッド終了:" + Thread.CurrentThread.ManagedThreadId);
        }


        static private void SearchAndAddPatterns(Mino mino, BitArray field, int moveCount, long move, Action lockDirection, int rotateCount, ref int taskIndex)
        {
            //ハードドロップ
            {
                long tempmove = (int)Action.Harddrop;
                for (int i = 0; i < moveCount; i++)
                    tempmove *= 10;

                var newmino = mino;
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
                Pattern value;

                //設置があった場合は行動回数で比較して追加判断
                //適用して計算


                //
                if (_searchedPatternsList[taskIndex].TryGetValue(hash, out value))
                {
                    _searchedPatternsList[taskIndex].Remove(hash);

                    //行動回数が少ないやつ
                    if (value.MoveCount > moveCount)
                    {
                        value.MoveCount = moveCount;
                        value.Move = move + tempmove;
                    }

                    _searchedPatternsList[taskIndex].Add(hash, value);
                }
                else
                {
                    Pattern pattern = new Pattern();
                    pattern.Position = newmino.Position;
                    pattern.MoveCount = moveCount;
                    pattern.Move = move + tempmove;

                    var fieldclone = (BitArray)field.Clone();

                    //設置したミノを適用
                    for (int i = 0; i < 4; i++)
                    {
                        var x = Mino.GetPosition(pattern.Position, i, true);
                        var y = Mino.GetPosition(pattern.Position, i, false);

                        fieldclone[x + y * 10] = true;
                    }

                    int clearedLine = Environment.CheckAndClearLine(fieldclone);
                    pattern.Eval = Evaluation.NewEvaluate(fieldclone, clearedLine);

                    _fieldsList[taskIndex].Add(fieldclone);
                    pattern.FieldIndex = _fieldsList[taskIndex].Count - 1;

                    _searchedPatternsList[taskIndex].Add(hash, pattern);
                }
            }


            //左移動
            if (lockDirection != Action.MoveRight && Environment.CheckValidPos(field, mino, Vector2.mx1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.mx1.x, Vector2.mx1.y, mino.Rotation, true, ref taskIndex))
                {
                    newmino.Move(Vector2.mx1.x, Vector2.mx1.y);

                    long temp = (int)Action.MoveLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, Action.MoveLeft, rotateCount, ref taskIndex);
                }
            }

            //右移動
            if (lockDirection != Action.MoveLeft && Environment.CheckValidPos(field, mino, Vector2.x1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.x1.x, Vector2.x1.y, mino.Rotation, true, ref taskIndex))
                {
                    newmino.Move(Vector2.x1.x, Vector2.x1.y);

                    long temp = (int)Action.MoveRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, Action.MoveRight, rotateCount, ref taskIndex);
                }
            }

            //   return;
            //右回転
            Vector2? result;
            if (rotateCount < 3 &&
                Environment.TryRotate(Rotate.Right, field, ref mino, out result))
            {
                var vec = (Vector2)result;
                var newmino = mino;
                Rotation newrotation = 0;
                Environment.GetNextRotateEnum(Rotate.Right, ref newrotation);

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true, ref taskIndex))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Right, ref newmino, 0);

                    long temp = (int)Action.RotateRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, lockDirection, rotateCount + 1, ref taskIndex);
                }
            }

            //左回転
            if (rotateCount < 3 &&
                Environment.TryRotate(Rotate.Left, field, ref mino, out result))
            {
                var vec = (Vector2)result;
                var newmino = mino;
                Rotation newrotation = 0;
                Environment.GetNextRotateEnum(Rotate.Left, ref newrotation);

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true, ref taskIndex))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Left, ref newmino, 0);

                    long temp = (int)Action.RotateLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, lockDirection, rotateCount + 1, ref taskIndex);
                }
            }


        }

        /// <summary>
        /// 一時的にpublic
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="pos"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newrotation"></param>
        /// <param name="ApplyHistory"></param>
        /// <returns></returns>
        static private bool IsPassedBefore(MinoKind kind, long pos, int x, int y, Rotation newrotation, bool ApplyHistory, ref int taskIndex)
        {
            //    pos += y;
            //    pos += x * 100;
            //  -1だったら
            Mino.AddAllPosition(ref pos, x, y);
            var hash = GetHashForPosition(kind, newrotation, pos);

            bool result = _passedTreeRoutesList[taskIndex].Contains(hash);
            if (result)
                return true;

            if (ApplyHistory)
                _passedTreeRoutesList[taskIndex].Add(hash);

            return false;

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

                    for (int j = 0; j < i; j++)
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
