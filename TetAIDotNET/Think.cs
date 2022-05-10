using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{


    public enum Action:byte
    {
        Null,
        MoveRight,
        MoveLeft,
        RotateRight,
        RotateLeft,
        Harddrop,
        Softdrop,
        Hold
    }

    public struct Way
    {
        public Way(Action[] actions, float eval, long result)
        {
            Actions = actions;
            Evaluation = eval;
            ResultPos = result;
        }
        public float Evaluation;
        public long ResultPos;
        public Action[] Actions;

        public override int GetHashCode()
        {
            int hash = Evaluation.GetHashCode();
            for (int i = 0; i < Actions.Length; i++)
                hash ^= Actions[i].GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            var way = (Way)obj;

            if (way.Actions == Actions && Math.Abs(Evaluation - way.Evaluation) <= 0.0001f)
                return true;
            return false;
        }
    }


    class Think
    {
    }
}
