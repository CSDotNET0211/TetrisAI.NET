using System;
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

    public enum MinoKind:byte
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

    public enum Rotation
    {
        Zero,
        Right,
        Turn,
        Left
    }

    public struct Mino
    {

        public Mino(MinoKind kind, Rotation rotation, Vector2[] position)
        {
            MinoKind = kind;
            Rotation = rotation;
            Positions = position;
            AbsolutelyPosition = new Vector2(50, 50);
        }

        public Vector2 AbsolutelyPosition;
        public MinoKind MinoKind;
        public Rotation Rotation;
        public Vector2[] Positions;

    }

    public enum Rotate:byte
    {
        Right,
        Left
    }


    class Environment
    {
        MinoKind[] bagArray = new MinoKind[] { MinoKind.I, MinoKind.J, MinoKind.L, MinoKind.O, MinoKind.S, MinoKind.T, MinoKind.Z };
        List<MinoKind> _bag;
        public int _clearedLine = 0;
        public int _score = 0;
        public bool _dead = false;

        Mino _nowMino;
        MinoKind[] _next = new MinoKind[5];
        Random _random = new Random();
        BitArray field = new BitArray(FIELD_WIDTH * FIELD_HEIGHT);
        bool _canHold = true;
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

            _nowMino.Positions = GetDefaultMinoPos(_nowMino.MinoKind);
            _nowMino.AbsolutelyPosition = new Vector2(50, 50);

            if (mino == null)
                RefreshNext(_next);

            foreach (var test in _nowMino.Positions)
            {
                if (field.Get(test.x + test.y * 10))
                {
                    _dead = true;
                    break;
                }
            }

            Vector2[] GetDefaultMinoPos(MinoKind kind)
            {
                var array = new Vector2[4];
                switch (kind)
                {
                    case MinoKind.I:
                        array[0] = new Vector2(3, 18);
                        array[1] = new Vector2(4, 18);
                        array[2] = new Vector2(5, 18);
                        array[3] = new Vector2(6, 18);
                        break;

                    case MinoKind.J:
                        array[0] = new Vector2(3, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.L:
                        array[0] = new Vector2(5, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.O:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(5, 19);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.S:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(5, 19);
                        array[2] = new Vector2(3, 18);
                        array[3] = new Vector2(4, 18);
                        break;
                    case MinoKind.Z:
                        array[0] = new Vector2(3, 19);
                        array[1] = new Vector2(4, 19);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;
                    case MinoKind.T:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    default:
                        throw new Exception();
                }

                return array;
            }
        }

        static public Mino CreateMino(MinoKind mino1)
        {
            var mino = new Mino();
            mino.Rotation = Rotation.Zero;
            mino.MinoKind = mino1;
            mino.Positions = GetDefaultMinoPos(mino1);
            mino.AbsolutelyPosition = new Vector2(50, 50);

            return mino;
            Vector2[] GetDefaultMinoPos(MinoKind kind)
            {
                var array = new Vector2[4];
                switch (kind)
                {
                    case MinoKind.I:
                        array[0] = new Vector2(3, 18);
                        array[1] = new Vector2(4, 18);
                        array[2] = new Vector2(5, 18);
                        array[3] = new Vector2(6, 18);
                        break;

                    case MinoKind.J:
                        array[0] = new Vector2(3, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.L:
                        array[0] = new Vector2(5, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.O:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(5, 19);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    case MinoKind.S:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(5, 19);
                        array[2] = new Vector2(3, 18);
                        array[3] = new Vector2(4, 18);
                        break;
                    case MinoKind.Z:
                        array[0] = new Vector2(3, 19);
                        array[1] = new Vector2(4, 19);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;
                    case MinoKind.T:
                        array[0] = new Vector2(4, 19);
                        array[1] = new Vector2(3, 18);
                        array[2] = new Vector2(4, 18);
                        array[3] = new Vector2(5, 18);
                        break;

                    default:
                        throw new Exception();
                }

                return array;
            }
        }


        public Way Search()
        {
            return DefaultSearch.Search(field, _nowMino, _next, _canHold, _nowHold);
        }

        public void PrintGame()
        {
            Print.PrintGame(field, _nowMino.Positions, _next, _nowHold);
            Print.PrintGame(field, _nowMino.Positions, _next, _nowHold);
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
                        _nowMino.Positions[0] += Vector2.x1;
                        _nowMino.Positions[1] += Vector2.x1;
                        _nowMino.Positions[2] += Vector2.x1;
                        _nowMino.Positions[3] += Vector2.x1;
                    }
                    break;

                case Action.MoveLeft:
                    //    Console.Beep(294, 100);
                    if (CheckValidPos(field, _nowMino, Vector2.mx1))
                    {
                        _nowMino.Positions[0] += Vector2.mx1;
                        _nowMino.Positions[1] += Vector2.mx1;
                        _nowMino.Positions[2] += Vector2.mx1;
                        _nowMino.Positions[3] += Vector2.mx1;
                    }
                    break;

                case Action.RotateRight:
                    //   Console.Beep(330, 200);
                    if (TryRotate(Rotate.Right, field, ref _nowMino, out srs))
                    {
                        SimpleRotate(Rotate.Right, ref _nowMino);
                        _nowMino.Positions[0] += (Vector2)srs;
                        _nowMino.Positions[1] += (Vector2)srs;
                        _nowMino.Positions[2] += (Vector2)srs;
                        _nowMino.Positions[3] += (Vector2)srs;
                    }
                    break;

                case Action.RotateLeft:
                    //    Console.Beep(349, 100);
                    if (TryRotate(Rotate.Left, field, ref _nowMino, out srs))
                    {
                        SimpleRotate(Rotate.Left, ref _nowMino);
                        _nowMino.Positions[0] += (Vector2)srs;
                        _nowMino.Positions[1] += (Vector2)srs;
                        _nowMino.Positions[2] += (Vector2)srs;
                        _nowMino.Positions[3] += (Vector2)srs;
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
                            for (int i = 0; i < _nowMino.Positions.Length; i++)
                                _nowMino.Positions[i] += Vector2.my1;
                        }
                        else break;
                    }
                    break;

                case Action.Hold:
                    Hold();
                    break;
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
            _bag = new List<MinoKind>(bagArray);

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
                    for (int i = 0; i < _nowMino.Positions.Length; i++)
                        _nowMino.Positions[i] += Vector2.my1;
                }
                else break;
            }

            _canHold = true;

            foreach (Vector2 pos in _nowMino.Positions)
            {
                field.Set(pos.x + pos.y * 10, true);
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

        public void RefreshNext(MinoKind[] next)
        {
            for (int i = 0; i < next.Length - 1; i++)
                next[i] = next[i + 1];

            if (_bag.Count == 0)
                _bag = new List<MinoKind>(bagArray);

            var index = _random.Next(0, _bag.Count);
            var mino = _bag[index];
            _bag.RemoveAt(index);

            next[next.Length - 1] = mino;

        }
        static public bool CheckValidPos(BitArray field, Mino mino, Vector2 trymove)
        {
            foreach (var pos in mino.Positions)
            {
                if (pos.x + trymove.x < FIELD_WIDTH &&
                   pos.x + trymove.x >= 0 &&
                   pos.y + trymove.y >= 0 &&
                   !field.Get((pos.x + trymove.x) + (pos.y + trymove.y) * 10))
                {

                }
                else
                    return false;

            }


            return true;
        }

        static public bool TryRotate(Rotate rotate, BitArray field, ref Mino current, out Vector2? srspos)
        {
            srspos = null;

            if (current.MinoKind == MinoKind.O)
                return false;

            var beforerotate = current.Rotation;
            SimpleRotate(rotate, ref current);

            if (rotate == Rotate.Left)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (current.MinoKind == MinoKind.I)
                    {
                        if (CheckValidPos(field, current, IKickTable[(int)current.Rotation, i].Revert()))
                        {
                            srspos = IKickTable[(int)current.Rotation, i].Revert();
                            //回転成功
                            SimpleRotate(Rotate.Right, ref current);

                            return true;
                        }
                    }
                    else
                    {
                        //if (CheckValidPos(field, current, KickTable[(int)beforerotate, i].Revert()))
                        if (CheckValidPos(field, current, KickTable[(int)current.Rotation, i].Revert()))
                        {
                            srspos = KickTable[(int)current.Rotation, i].Revert();
                            //回転成功
                            SimpleRotate(Rotate.Right, ref current);

                            return true;
                        }

                    }
                }
                SimpleRotate(Rotate.Right, ref current);

                return false;
            }
            else if (rotate == Rotate.Right)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (current.MinoKind == MinoKind.I)
                    {
                        if (CheckValidPos(field, current, IKickTable[(int)beforerotate, i]))
                        {
                            //回転成功
                            SimpleRotate(Rotate.Left, ref current);

                            srspos = IKickTable[(int)beforerotate, i];
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckValidPos(field, current, KickTable[(int)beforerotate, i]))
                        {
                            //回転成功
                            SimpleRotate(Rotate.Left, ref current);

                            srspos = KickTable[(int)beforerotate, i];
                            return true;
                        }

                    }

                }
                SimpleRotate(Rotate.Left, ref current);

                return false;
            }
            else
                throw new Exception();


        }

        static public void SimpleRotate(Rotate rotate, ref Mino mino)
        {
            if (rotate == Rotate.Right)
            {
                switch (mino.MinoKind)
                {
                    case MinoKind.J:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += JRotateTable[(int)mino.Rotation, i];
                        break;
                    case MinoKind.L:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += LRotateTable[(int)mino.Rotation, i];
                        break;
                    case MinoKind.S:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += SRotateTable[(int)mino.Rotation, i];
                        break;
                    case MinoKind.Z:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += ZRotateTable[(int)mino.Rotation, i];
                        break;
                    case MinoKind.T:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += TRotateTable[(int)mino.Rotation, i];
                        break;
                    case MinoKind.I:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] += IRotateTable[(int)mino.Rotation, i];
                        break;

                    default: throw new Exception();
                }
            }
            else
            {
                switch (mino.MinoKind)
                {
                    case MinoKind.J:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= JRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;
                    case MinoKind.L:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= LRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;
                    case MinoKind.S:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= SRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;
                    case MinoKind.Z:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= ZRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;
                    case MinoKind.T:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= TRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;
                    case MinoKind.I:
                        for (int i = 0; i < mino.Positions.Length; i++)
                            mino.Positions[i] -= IRotateTable[(int)RotateEnum(rotate, mino.Rotation), i];
                        break;

                    default: throw new Exception();
                }
            }
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
        static Rotation RotateEnum(Rotate rotate1, Rotation rotation, bool invert = false)
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
}
