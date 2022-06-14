using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        static object _lock = new object();
        static Pattern? _best = null;
        public static BlockingCollection<BitArray> _fieldsList = new BlockingCollection<BitArray>(new ConcurrentBag<BitArray>());
        static ManualResetEvent _resetEvent;
        static BlockingCollection<Data> _queue = new BlockingCollection<Data>();
        static int _activeThreadCount = 0;

        public static long GetBestMove(MinoKind current, MinoKind[] nexts, MinoKind? hold, bool canHold, BitArray field, int nextCount)
        {
            _best = null;

            #if DEBUG
            if (_queue!=null||_queue.Count != 0)
            {
                throw new Exception("キューがゼロではありません。前回の探索が正常に行われなかった可能性があります。");
            _queue.Clear();
            }
            #endif

            _fieldsList.Clear();

            int nextint = 0;
            for (int i = 0; i < nextCount; i++)
                nextint = (int)nexts[i] * (10 * (nextCount - i - 1));

            int holdint = hold == null ? -1 : (int)hold;

            var data = new Data((int)current, nextint, 1, holdint, canHold, 0, -1, 0);
            Interlocked.Increment(ref _activeThreadCount);
            _queue.TryAdd(data);
            _fieldsList.TryAdd(field, Timeout.Infinite);

            var result = GetLoop();
            return result;
        }




        public struct Data
        {
            public Data(int current, int next, int nextCount, int hold, bool canHold, int fieldIndex, long firstMove, float beforeEval)
            {
                Current = current; ;
                Next = next;
                NextCount = nextCount;
                Hold = hold;
                CanHold = canHold;
                FieldIndex = fieldIndex;
                FirstMove = firstMove;
                BeforeEval = beforeEval;

            }

            public int Current;
            public int Next;
            public int NextCount;
            public int Hold;
            public bool CanHold;
            public int FieldIndex;
            public long FirstMove;
            public float BeforeEval;

        }

        static public void GetData(object dataobj)
        {
            Data data = (Data)dataobj;
            //GetBestと同じことする
            //再帰の代わりにQueue登録

            //ミノの種類からミノ情報作成
            var mino = Environment.CreateMino((MinoKind)data.Current);

            var patternsInThisMoveTemp = new Dictionary<long, Pattern>();
            HashSet<long> passedBefore = new HashSet<long>();
            //    var searchedData=new Dictionary<>

            //検索関数に渡してパターンを列挙 
            SearchAndAddPatterns(mino, _fieldsList.ElementAt(data.FieldIndex), 0, 0, Action.Null, 0,
                patternsInThisMoveTemp, passedBefore);
            var patternsInThisMove = patternsInThisMoveTemp.Values.ToArray();


            if (data.NextCount == 0)
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
                    long first;
                    if (data.FirstMove == -1)
                        first = patternsInThisMove[beem].Move;
                    else
                        first = data.FirstMove;

                    Utility.ReplaceToBetterUpdateOnlyMove(ref best, patternsInThisMove[beem], first);

                }

                lock (_lock)
                {
                    Utility.ReplaceToBetter(ref _best, (Pattern)best);
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

                for (int beem = 0; beem < beemWidth; beem++)
                {
                    patternsInThisMove[beem].Eval += data.BeforeEval;

                    //最初の行動のみ保存
                    long first;
                    if (data.FirstMove == -1)
                        first = patternsInThisMove[beem].Move;
                    else
                        first = data.FirstMove;

                    int newcurrent = data.Next;
                    int newnext = data.Next;
                    int tempDiv = 10;
                    for (int i = 0; i < data.NextCount - 1; i++)
                    {
                        newcurrent /= 10;
                        tempDiv *= 10;
                    }

                    newnext %= tempDiv;

                    //新しい検索に追加

                    Interlocked.Increment(ref _activeThreadCount);
                    var newdata = new Data(newcurrent, newnext, data.NextCount - 1, data.Hold, data.CanHold, data.FieldIndex, first, patternsInThisMove[beem].Eval);
                    if (!_queue.TryAdd(newdata, Timeout.Infinite))
                        throw new Exception();
                    //    GetBest(newcurrent, newnext, data.NextCount - 1, data.Hold, data.CanHold, _fieldsList[taskIndex][patternsInThisMove[beem].FieldIndex], first, patternsInThisMove[beem].Eval);
                    //       Console.WriteLine("キュー追加"+ _queue.Count);


                }
            }

            Interlocked.Decrement(ref _activeThreadCount);
        }

        static public long GetLoop()
        {
            while (true)
            {
                //すべてのスレッドが終了するまでは待ち続ける
                //１つのスレッドを起動したときは、１０個ぐらい検索させる
                //Queue一覧から検索する
                //スレッドが全部帰ってきたときにキューがゼロだったら完了
                Data data;
                if (_queue.TryTake(out data, 10))
                {
                    //     Interlocked.Increment(ref _activeThreadCount);

                    ThreadPool.QueueUserWorkItem(GetData, (object)data);
                    //      GetData(data);
                }
                else
                {
                    //     Console.WriteLine("待機");
                    //スレッド数を表すスレッドセーフな数字を用意して、ここに到達したときゼロだったら検索完了

                    if (_activeThreadCount == 0)
                    {
                        Console.WriteLine("終了 best：" + _best.Value.Move);
                        return _best.Value.Move;
                    }

                }
            }
        }

        static private void SearchAndAddPatterns(Mino mino, BitArray field, int moveCount, long move, Action lockDirection, int rotateCount,
            Dictionary<long, Pattern> searchedData, HashSet<long> passedTreeRouteSet)
        {
            //ハードドロップ
            {
                long newMoveDiff = (int)Action.Harddrop;
                for (int i = 0; i < moveCount; i++)
                    newMoveDiff *= 10;

                var newMino = mino;
                //一番下までソフドロ
                int temp = 0;
                while (true)
                {
                    temp++;
                    if (!Environment.CheckValidPos(field, newMino, new Vector2(0, -temp)))
                    {
                        temp--;
                        break;
                    }
                }
                newMino.Move(y: -temp);

                //設置位置の重複判定のためのハッシュ作成
                long hash = GetHashForPosition(newMino.MinoKind, newMino.Rotation, newMino.Position);
                Pattern value;

                //設置があった場合は行動回数で比較して追加判断
                //適用して計算


                //
                if (searchedData.TryGetValue(hash, out value))
                {
                    searchedData.Remove(hash);

                    //行動回数が少ないやつ
                    if (value.MoveCount > moveCount)
                    {
                        value.MoveCount = moveCount;
                        value.Move = move + newMoveDiff;
                    }

                    searchedData.Add(hash, value);
                }
                else
                {
                    Pattern pattern = new Pattern();
                    pattern.Position = newMino.Position;
                    pattern.MoveCount = moveCount;
                    pattern.Move = move + newMoveDiff;

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

                    _fieldsList.TryAdd(fieldclone, Timeout.Infinite);
                    //    fieldList.Add(fieldclone);
                    pattern.FieldIndex = _fieldsList.Count - 1;

                    searchedData.Add(hash, pattern);
                }
            }


            //左移動
            if (lockDirection != Action.MoveRight && Environment.CheckValidPos(field, mino, Vector2.mx1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.mx1.x, Vector2.mx1.y, mino.Rotation, true, passedTreeRouteSet))
                {
                    newmino.Move(Vector2.mx1.x, Vector2.mx1.y);

                    long temp = (int)Action.MoveLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, Action.MoveLeft, rotateCount, searchedData, passedTreeRouteSet);
                }
            }

            //右移動
            if (lockDirection != Action.MoveLeft && Environment.CheckValidPos(field, mino, Vector2.x1))
            {
                var newmino = mino;

                if (!IsPassedBefore(mino.MinoKind, mino.Position, Vector2.x1.x, Vector2.x1.y, mino.Rotation, true, passedTreeRouteSet))
                {
                    newmino.Move(Vector2.x1.x, Vector2.x1.y);

                    long temp = (int)Action.MoveRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, Action.MoveRight, rotateCount, searchedData, passedTreeRouteSet);
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

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true, passedTreeRouteSet))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Right, ref newmino, 0);

                    long temp = (int)Action.RotateRight;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, lockDirection, rotateCount + 1, searchedData, passedTreeRouteSet);
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

                if (!IsPassedBefore(mino.MinoKind, mino.Position, vec.x, vec.y, newrotation, true, passedTreeRouteSet))
                {
                    newmino.Move(vec.x, vec.y);
                    Environment.SimpleRotate(Rotate.Left, ref newmino, 0);

                    long temp = (int)Action.RotateLeft;
                    for (int i = 0; i < moveCount; i++)
                        temp *= 10;

                    SearchAndAddPatterns(newmino, field, moveCount + 1, move + temp, lockDirection, rotateCount + 1, searchedData, passedTreeRouteSet);
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
        static private bool IsPassedBefore(MinoKind kind, long pos, int x, int y, Rotation newrotation, bool ApplyHistory, HashSet<long> passedTreeRouteSet)
        {
            //    pos += y;
            //    pos += x * 100;
            //  -1だったら
            Mino.AddAllPosition(ref pos, x, y);
            var hash = GetHashForPosition(kind, newrotation, pos);

            bool result = passedTreeRouteSet.Contains(hash);
            if (result)
                return true;

            if (ApplyHistory)
                passedTreeRouteSet.Add(hash);

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
