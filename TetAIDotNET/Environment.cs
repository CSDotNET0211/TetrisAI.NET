﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    public struct Vector2
    {
        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x;
        public int y;

        public static readonly Vector2 zero = new Vector2(0, 0);
        public static readonly Vector2 one = new Vector2(1, 1);
        public static readonly Vector2 mone = new Vector2(-1, -1);
        public static readonly Vector2 x1 = new Vector2(1, 0);
        public static readonly Vector2 mx1 = new Vector2(-1, 0);
        public static readonly Vector2 x2 = new Vector2(2, 0);
        public static readonly Vector2 mx2 = new Vector2(-2, 0);
        public static readonly Vector2 y1 = new Vector2(0, 1);
        public static readonly Vector2 my1 = new Vector2(0, -1);
        public static readonly Vector2 y2 = new Vector2(0, 2);
        public static readonly Vector2 my2 = new Vector2(0, -2);

        public static Vector2 operator +(Vector2 obj, Vector2 obj2)
        {
            return new Vector2(obj.x + obj2.x, obj.y + obj2.y);
        }

        public static Vector2 operator -(Vector2 obj, Vector2 obj2)
        {
            return new Vector2(obj.x - obj2.x, obj.y - obj2.y);
        }
        public static Vector2 operator *(Vector2 obj, Vector2 obj2)
        {
            return new Vector2(obj.x * obj2.x, obj.y * obj2.y);
        }

    }

    public enum MinoKind : sbyte
    {
        S,
        Z,
        L,
        J,
        O,
        I,
        T,
        Null
    }

    public enum Rotation : sbyte
    {
        Zero,
        Right,
        Turn,
        Left
    }

    /// <summary>
    /// ミノの情報
    /// </summary>
    public struct Mino
    {

        public Mino(MinoKind kind, Rotation rotation, long position)
        {
            MinoKind = kind;
            Rotation = rotation;
            _positions = position;
            _absolutelyPosition = 0000;
        }

        /// <summary>
        /// ミノの位置判定用の絶対位置
        /// </summary>
        private long _absolutelyPosition;

        public long AbsolutelyPosition
        {
            get { return _absolutelyPosition; }
        }
        /// <summary>
        /// ミノの種類
        /// </summary>
        public MinoKind MinoKind;
        /// <summary>
        /// ミノの回転情報
        /// </summary>
        public Rotation Rotation;
        //xxyyxxyyxxyyxxyy
        /// <summary>
        /// ミノのそれぞれの位置
        /// </summary>
        private long _positions;
        public long Position
        {
            get { return _positions; }
        }

        public void Init(long AbsolutelyPosition = -1, long Positions = -1)
        {
            if (AbsolutelyPosition == -1)
                this._absolutelyPosition = 0000;
            else
                this._absolutelyPosition = AbsolutelyPosition;

            if (Positions == -1)
                this._positions = -1;
            else
                this._positions = Positions;
        }

        public void Move(int x = int.MaxValue, int y = int.MaxValue)
        {

            if (x != int.MaxValue)
            {
                for (int i = 0; i < 4; i++)
                    AddPosition(ref _positions, x, i, true);

                AddPosition(ref _absolutelyPosition, x, int.MaxValue, true);
            }

            if (y != int.MaxValue)
            {
                for (int i = 0; i < 4; i++)
                    AddPosition(ref _positions, y, i, false);

                AddPosition(ref _absolutelyPosition, y, int.MaxValue, false);
            }


            // AbsolutelyPosition += pos;
        }

        public void MoveForSRS(Vector2[,] srstest, Rotate rotate, Rotation rotation)
        {
            //0付近で回転すると一時的にマイナスになるから、＋５ぐらいして補正する

            if (rotate == Rotate.Right)
            {
                for (int i = 0; i < 4; i++)
                {
                    AddPosition(ref _positions, srstest[(int)rotation, i].x, i, true);
                    AddPosition(ref _positions, srstest[(int)rotation, i].y, i, false);
                }

            }
            else
            {

                for (int i = 0; i < 4; i++)
                {
                    AddPosition(ref _positions, -srstest[(int)RotateEnum(rotate, rotation), i].x, i, true);
                    AddPosition(ref _positions, -srstest[(int)RotateEnum(rotate, rotation), i].y, i, false);
                }
                //     srstest[(int)RotateEnum(rotate, mino.Rotation), i];
            }

        }

        /// <summary>
        /// ミノの位置を足して更新する
        /// </summary>
        /// <param name="value">追加差分</param>
        /// <param name="index">何個目のミノか</param>
        /// <param name="isX">xかyか</param>
        static public void AddPosition(ref long array, long value, int index, bool isX)
        {
            if (index == int.MaxValue)
                index = 0;
            else
                index = 4 - index - 1;
            //var beforevalue=GetPosition(index, isX);
            for (int i = 0; i < index * 4; i++)
                value *= 10;

            if (isX)
                value *= 100;

            array += value;
            //    beforevalue+=value;



        }

        public int GetPosition(int index, bool isX)
        {
            if (index == int.MaxValue)
                index = 0;
            else
                index = 4 - index - 1;

            long value = _positions;
            //xxyyxxyyxxyyxxyy
            for (int i = 0; i < index * 4; i++)
                value /= 10;

            long valueforsub = value / 10000 * 10000;

            value -= valueforsub;

            if (isX)
                return (int)value / 100;
            else
                return (int)value - (int)value / 100 * 100;
        }

        static public int GetPosition(long value, int index, bool isX)
        {
            if (index == int.MaxValue)
                index = 0;
            else
                index = 4 - index - 1;

            //xxyyxxyyxxyyxxyy
            for (int i = 0; i < index * 4; i++)
                value /= 10;

            long valueforsub = value / 10000 * 10000;

            value -= valueforsub;

            if (isX)
                return (int)value / 100;
            else
                return (int)value - (int)value / 100 * 100;
        }

        public static Rotation RotateEnum(Rotate rotate1, Rotation rotation, bool invert = false)
        {
            if (invert)
            {
                if (rotate1 == Rotate.Left)
                    rotate1 = Rotate.Right;
                else
                    rotate1 = Rotate.Left;

            }
            if (rotate1 == Rotate.Right)
            {
                rotation++;
                if (rotation == Rotation.Left + 1)
                    rotation = Rotation.Zero;
            }
            else
            {
                rotation--;
                if (rotation == Rotation.Zero - 1)
                    rotation = Rotation.Left;
            }

            return rotation;
        }
    }

    public enum Rotate : byte
    {
        Right,
        Left
    }

    /// <summary>
    /// テトリスの仮想環境関連 
    /// </summary>
    partial class Environment
    {
        /// <summary>
        /// 7種1順用読み込みバッグ
        /// </summary>
        readonly MinoKind[] bagArray = new MinoKind[] { MinoKind.I, MinoKind.J, MinoKind.L, MinoKind.O, MinoKind.S, MinoKind.T, MinoKind.Z };
        /// <summary>
        /// 7種1順バッグ
        /// </summary>
        List<MinoKind> _nextBag;
        /// <summary>
        /// 消去したライン
        /// </summary>
        public int _clearedLine = 0;
        /// <summary>
        /// スコア
        /// </summary>
        public int _score = 0;
        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool _dead = false;

        /// <summary>
        /// 現在操作中ミノ情報 一時的にpublic
        /// </summary>
        public Mino _nowMino;
        /// <summary>
        /// ネクスト
        /// </summary>
        MinoKind[] _next = new MinoKind[5];
        Random _random = new Random();
        /// <summary>
        /// フィールドデータ
        /// </summary>
        BitArray field = new BitArray(FIELD_WIDTH * FIELD_HEIGHT);
        /// <summary>
        /// ホールドフラグ
        /// </summary>
        bool _canHold = true;
        /// <summary>
        /// ホールドデータ
        /// </summary>
        MinoKind? _nowHold = null;

        #region RotateTable
        static readonly Vector2[,] JRotateTable = new Vector2[,]
        {
            { Vector2.x2, Vector2.one, Vector2.zero, Vector2.mone },
            { Vector2.my2,new Vector2(1,-1), Vector2.zero, new Vector2(-1,1)},
            { Vector2.mx2,Vector2.mone, Vector2.zero, Vector2.one},
            { Vector2.y2,new Vector2(-1,1), Vector2.zero, new Vector2(1,-1)},

        };
        static readonly Vector2[,] LRotateTable = new Vector2[,]
        {
            { Vector2.my2, Vector2.one, Vector2.zero, Vector2.mone },
            { Vector2.mx2,new Vector2(1,-1), Vector2.zero, new Vector2(-1,1)},
            { Vector2.y2,Vector2.mone, Vector2.zero,Vector2.one},
            { Vector2.x2,new Vector2(-1,1), Vector2.zero,new Vector2(1,-1)},
        };
        static readonly Vector2[,] SRotateTable = new Vector2[,]
        {
            { new Vector2(1, -1), Vector2.my2, Vector2.one, Vector2.zero },
            { Vector2.mone, Vector2.mx2, new Vector2(1,-1), Vector2.zero },
            { new Vector2(-1,1), Vector2.y2,Vector2.mone, Vector2.zero },
            { Vector2.one, Vector2.x2,new Vector2(-1,1), Vector2.zero },
        };
        static readonly Vector2[,] ZRotateTable = new Vector2[,]
        {
            { Vector2.x2, new Vector2(1, -1), Vector2.zero, Vector2.mone },
            { Vector2.my2,Vector2.mone, Vector2.zero, new Vector2(-1,1)},
            { Vector2.mx2,new Vector2(-1,1), Vector2.zero, Vector2.one},
            { Vector2.y2,Vector2.one, Vector2.zero, new Vector2(1,-1)},
        };
        static readonly Vector2[,] TRotateTable = new Vector2[,]
        {
            { new Vector2(1, -1), Vector2.one, Vector2.zero, Vector2.mone },
            { Vector2.mone, new Vector2(1, -1), Vector2.zero,  new Vector2(-1, 1) },
            { new Vector2(-1,1),Vector2.mone, Vector2.zero, Vector2.one},
            { Vector2.one,new Vector2(-1,1), Vector2.zero, new Vector2(1, -1)},
        };
        static readonly Vector2[,] IRotateTable = new Vector2[,]
      {
            { new Vector2(2,1), Vector2.x1, Vector2.my1, new Vector2(-1, -2) },
            {  new Vector2(1, -2),Vector2.my1, Vector2.mx1,  new Vector2(-2, 1) },
            { new Vector2(-2,-1),Vector2.mx1, Vector2.y1, new Vector2(1, 2)},
            {  new Vector2(-1, 2),Vector2.y1,  Vector2.x1, new Vector2(2, -1)},
      };
        #endregion

        #region SRSKickTable
        static readonly Vector2[,] KickTable = new Vector2[,]
        {
            { Vector2.zero,Vector2.mx1, new Vector2(-1,+1),Vector2.my2,new Vector2(-1,-2)},
            { Vector2.zero,Vector2.x1, new Vector2(1,-1), Vector2.y2,new Vector2(1,2)},
            { Vector2.zero,Vector2.x1,Vector2.one,Vector2.my2,new Vector2(1,-2)},
            { Vector2.zero,Vector2.mx1,  Vector2.mone, Vector2.y2,new Vector2(-1,2)},
        };
        static readonly Vector2[,] IKickTable = new Vector2[,]
         {
            { Vector2.zero,Vector2.mx2, Vector2.x1,new Vector2(-2,-1),new Vector2(1,2)},
            { Vector2.zero,Vector2.mx1,Vector2.x2, new Vector2(-1,+2),new Vector2(2,-1)},
            { Vector2.zero,Vector2.x2,Vector2.mx1,new Vector2(2,1),new Vector2(-1,-2)},
            { Vector2.zero,Vector2.x1,  Vector2.mx2, new Vector2(1,-2),new Vector2(-2,1)},
         };


        #endregion

        public const int FIELD_WIDTH = 10;
        public const int FIELD_HEIGHT = 26;

        public void CreateMino(MinoKind? mino = null)
        {
            _nowMino = new Mino();
            _nowMino.Rotation = Rotation.Zero;
            if (mino == null)
                _nowMino.MinoKind = _next[0];
            else
                _nowMino.MinoKind = (MinoKind)mino;

            _nowMino.Init(Positions: GetDefaultMinoPos(_nowMino.MinoKind));

            if (mino == null)
                RefreshNext(_next);

            for (int i = 0; i < 4; i++)
            {
                var x = _nowMino.GetPosition(i, true);
                var y = _nowMino.GetPosition(i, false);

                if (field.Get(x + y * 10))
                {
                    _dead = true;
                    break;
                }
            }



        }
        static long GetDefaultMinoPos(MinoKind kind)
        {
            //    var array = new Vector2[4];
            switch (kind)
            {
                case MinoKind.I:
                    return 0318041805180618;

                /* array[0] = new Vector2(3, 18);
                  array[1] = new Vector2(4, 18);
                  array[2] = new Vector2(5, 18);
                  array[3] = new Vector2(6, 18);
                  break;*/

                case MinoKind.J:
                    return 0319031804180518;

                /*    array[0] = new Vector2(3, 19);
                    array[1] = new Vector2(3, 18);
                    array[2] = new Vector2(4, 18);
                    array[3] = new Vector2(5, 18);:
                    break;*/

                case MinoKind.L:
                    return 0519031804180518;

                /*     array[0] = new Vector2(5, 19);
                     array[1] = new Vector2(3, 18);
                     array[2] = new Vector2(4, 18);
                     array[3] = new Vector2(5, 18);
                     break;*/

                case MinoKind.O:
                    return 0419051904180518;

                /*     array[0] = new Vector2(4, 19);
                     array[1] = new Vector2(5, 19);
                     array[2] = new Vector2(4, 18);
                     array[3] = new Vector2(5, 18);
                     break;*/

                case MinoKind.S:
                    return 0419051903180418;

                /*   array[0] = new Vector2(4, 19);
                   array[1] = new Vector2(5, 19);
                   array[2] = new Vector2(3, 18);
                   array[3] = new Vector2(4, 18);
                   break;*/

                case MinoKind.Z:
                    return 0319041904180518;

                /*  array[0] = new Vector2(3, 19);
                  array[1] = new Vector2(4, 19);
                  array[2] = new Vector2(4, 18);
                  array[3] = new Vector2(5, 18);
                  break;*/

                case MinoKind.T:
                    return 0419031804180518;

                /*    array[0] = new Vector2(4, 19);
                    array[1] = new Vector2(3, 18);
                    array[2] = new Vector2(4, 18);
                    array[3] = new Vector2(5, 18);
                    break;*/

                default:
                    throw new Exception();
            }

            //  return array;
        }

        public Way Search()
        {
            return DefaultSearch.Search(field, _nowMino, _next, _canHold, _nowHold);
        }

        public void PrintGame()
        {
            Print.PrintGame(field, _nowMino.Position, _next, _nowHold);
            Print.PrintGame(field, _nowMino.Position, _next, _nowHold);
        }
        public void UserInput(Action action)
        {
            Vector2? srs;
            switch (action)
            {
                case Action.MoveRight:
                    //   Console.Beep(262, 100);
                    if (CheckValidPos(field, _nowMino, Vector2.x1))
                    {
                        _nowMino.Move(Vector2.x1.x, Vector2.x1.y);
                    }
                    break;

                case Action.MoveLeft:
                    //    Console.Beep(294, 100);
                    if (CheckValidPos(field, _nowMino, Vector2.mx1))
                    {
                        _nowMino.Move(Vector2.mx1.x, Vector2.mx1.y);
                    }
                    break;

                case Action.RotateRight:
                    //   Console.Beep(330, 200);
                    if (TryRotate(Rotate.Right, field, ref _nowMino, out srs))
                    {
                        var inputsrs = (Vector2)srs;
                        _nowMino.Move(inputsrs.x, inputsrs.y);
                        SimpleRotate(Rotate.Right, ref _nowMino, 0);
                    }
                    break;

                case Action.RotateLeft:
                    //    Console.Beep(349, 100);
                    if (TryRotate(Rotate.Left, field, ref _nowMino, out srs))
                    {
                        var inputsrs = (Vector2)srs;
                        _nowMino.Move(inputsrs.x, inputsrs.y);
                        SimpleRotate(Rotate.Left, ref _nowMino, 0);
                    }
                    break;

                case Action.Harddrop:
                    //    Console.Beep(392, 100);
                    SetMino();
                    break;

                case Action.Softdrop:
                    while (true)
                    {
                        if (CheckValidPos(field, _nowMino, Vector2.my1))
                        {
                            _nowMino.Move(Vector2.my1.x, Vector2.my1.y);
                            //_nowMino._positions[i] += Vector2.my1;
                        }
                        else break;
                    }
                    break;

                case Action.Hold:
                    Hold();
                    break;
            }
        }
        private void Hold()
        {
            if (_canHold)
            {
                _canHold = false;

                if (_nowHold == null)
                {
                    _nowHold = _nowMino.MinoKind;
                    CreateMino();
                }
                else
                {
                    var tempnow = _nowMino.MinoKind;
                    CreateMino(_nowHold);
                    _nowHold = tempnow;
                }

            }
        }
        public void Init()
        {
            _nextBag = new List<MinoKind>(bagArray);

            for (int i = 0; i < _next.Length; i++)
            {
                RefreshNext(_next);
            }

            field.SetAll(false);

            CreateMino();
        }
        private void SetMino()
        {
            while (true)
            {
                if (CheckValidPos(field, _nowMino, Vector2.my1))
                {
                    _nowMino.Move(Vector2.my1.x, Vector2.my1.y);
                }
                else break;
            }

            _canHold = true;

            for (int i = 0; i < 4; i++)
            {
                int x = _nowMino.GetPosition(i, true);
                int y = _nowMino.GetPosition(i, false);

                field.Set(x + y * 10, true);
            }

            _score += 2;
            var line = CheckClearedLine(field);
            _clearedLine += line;
            switch (line)
            {
                case 1:
                    _score += 100;
                    break;

                case 2:
                    _score += 300;
                    break;

                case 3:
                    _score += 500;
                    break;

                case 4:
                    _score += 800;
                    break;
            }

            CreateMino();
        }
        public void RefreshNext(MinoKind[] next)
        {
            for (int i = 0; i < next.Length - 1; i++)
                next[i] = next[i + 1];

            if (_nextBag.Count == 0)
                _nextBag = new List<MinoKind>(bagArray);

            var index = _random.Next(0, _nextBag.Count);
            var mino = _nextBag[index];
            _nextBag.RemoveAt(index);

            next[next.Length - 1] = mino;

        }


    }

    /// <summary>
    /// static関連
    /// </summary>
    partial class Environment
    {
        static public bool CheckValidPos(BitArray field, Mino mino, Vector2 trymove, int add = 0)
        {
            for (int i = 0; i < 4; i++)
            {
                int x = mino.GetPosition(i, true) + add;
                int y = mino.GetPosition(i, false) + add;

                if (x + trymove.x < FIELD_WIDTH &&
                   x + trymove.x >= 0 &&
                   y + trymove.y >= 0 &&
                   !field.Get((x + trymove.x) + (y + trymove.y) * 10))
                {

                }
                else
                    return false;

            }


            return true;
        }
        static public int CheckClearedLine(BitArray field)
        {
            List<int> list = new List<int>();
            bool flag = true;

            for (int y = 0; y < FIELD_HEIGHT; y++)
            {
                flag = true;
                for (int x = 0; x < FIELD_WIDTH; x++)
                {
                    if (!field.Get(x + y * 10))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    list.Add(y);
            }

            list.Reverse();
            foreach (var value in list)
            {
                DownLine(value, field);
            }

            return list.Count;
        }
        static private void DownLine(int value, BitArray field)
        {
            for (int y = value; y < FIELD_HEIGHT; y++)
            {
                for (int x = 0; x < FIELD_WIDTH; x++)
                {
                    if (y == FIELD_HEIGHT - 1)
                        field.Set(x + y * 10, false);
                    else
                        field.Set(x + y * 10, field.Get(x + (y + 1) * 10));
                }
            }
        }
        static public float GetEval(float[] values)
        {
            //操作終わった後、150ライン消しか死ぬまで
            //設置も１ポイント
            Environment environment = new Environment();
            environment.Init();
            Evaluation.Weight = values;

            while (true)
            {
                var result = environment.Search();
                foreach (var action in result.Actions)
                {
                    if (action == Action.Null)
                        break;
                    environment.UserInput(action);
                }

                if (environment._dead || environment._clearedLine >= 150)
                {
                    return environment._score;
                }

            }


        }
        /// <summary>
        /// ミノの種類から位置情報を生成
        /// </summary>
        /// <param name="mino1"></param>
        /// <returns></returns>
        /// <exception cref="Exception">対応するミノの種類が存在しなかった場合</exception>
        static public Mino CreateMino(MinoKind mino1)
        {
            var mino = new Mino();
            mino.Rotation = Rotation.Zero;
            mino.MinoKind = mino1;

            mino.Init(Positions: GetDefaultMinoPos(mino1));

            return mino;
        }
    }
    /// <summary>
    /// 回転関連
    /// </summary>
    partial class Environment
    {
        static public bool TryRotate(Rotate rotate, BitArray field, ref Mino current, out Vector2? srspos)
        {
            //simplerotateの中ででかくして
            srspos = null;

            if (current.MinoKind == MinoKind.O)
                return false;


            SimpleRotate(rotate, ref current, 5);

            if (rotate == Rotate.Left)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (current.MinoKind == MinoKind.I)
                    {
                        if (CheckValidPos(field, current, IKickTable[(int)current.Rotation, i].Revert(), -5))
                        {
                            srspos = IKickTable[(int)current.Rotation, i].Revert();
                            //回転成功
                            SimpleRotate(Rotate.Right, ref current, -5);

                            return true;
                        }
                    }
                    else
                    {
                        if (CheckValidPos(field, current, KickTable[(int)current.Rotation, i].Revert(), -5))
                        {
                            srspos = KickTable[(int)current.Rotation, i].Revert();
                            //回転成功
                            SimpleRotate(Rotate.Right, ref current, -5);

                            return true;
                        }

                    }
                }
                SimpleRotate(Rotate.Right, ref current, -5);

                return false;
            }
            else if (rotate == Rotate.Right)
            {
                var beforerotate = current.Rotation;

                for (int i = 0; i < 5; i++)
                {
                    if (current.MinoKind == MinoKind.I)
                    {
                        if (CheckValidPos(field, current, IKickTable[(int)beforerotate, i], -5))
                        {
                            //回転成功
                            SimpleRotate(Rotate.Left, ref current, -5);

                            srspos = IKickTable[(int)beforerotate, i];
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckValidPos(field, current, KickTable[(int)beforerotate, i], -5))
                        {
                            //回転成功
                            SimpleRotate(Rotate.Left, ref current, -5);

                            srspos = KickTable[(int)beforerotate, i];
                            return true;
                        }

                    }

                }
                SimpleRotate(Rotate.Left, ref current, -5);

                return false;
            }
            else
                throw new Exception();


        }
        static public void SimpleRotate(Rotate rotate, ref Mino mino, int addtemp=0)
        {
            Vector2[,] movePos;
            mino.Move(addtemp, addtemp);

            switch (mino.MinoKind)
            {
                case MinoKind.J:
                    movePos = JRotateTable;
                    break;
                case MinoKind.L:
                    movePos = LRotateTable;
                    break;
                case MinoKind.S:
                    movePos = SRotateTable;
                    break;
                case MinoKind.Z:
                    movePos = ZRotateTable;
                    break;
                case MinoKind.T:
                    movePos = TRotateTable;
                    break;
                case MinoKind.I:
                    movePos = IRotateTable;
                    break;

                default: throw new Exception();
            }

            mino.MoveForSRS(movePos, rotate, mino.Rotation);
            /*  if (rotate == Rotate.Right)
              {
              }
              else
              {
                  mino.MoveForSRS(movePos, rotate, mino.Rotation);
                  //
                  //mino.MoveForSRS(-movePos[(int)mino.Rotation, i].x, -movePos[(int)mino.Rotation, i].y);
              }*/

            RefreshRotateEnum(rotate, ref mino.Rotation);

            void RefreshRotateEnum(Rotate rotate1, ref Rotation rotation)
            {
                if (rotate1 == Rotate.Right)
                {
                    rotation++;
                    if (rotation == Rotation.Left + 1)
                        rotation = Rotation.Zero;
                }
                else
                {
                    rotation--;
                    if (rotation == Rotation.Zero - 1)
                        rotation = Rotation.Left;
                }
            }

        }



    }


}


