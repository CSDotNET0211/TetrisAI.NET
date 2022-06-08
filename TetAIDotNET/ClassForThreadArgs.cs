using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    internal class ClassForThreadArgs
    {
        /// <summary>
        /// 変数群をオブジェクトに変換するだけ
        /// </summary>
        public ClassForThreadArgs(int current,int next,int nextCount,int hold,bool canHold,BitArray field,long first,float eval,int taskIndex)
        {
            Current=current;    
            Next=next;
            NextCount=nextCount;    
            Hold=hold;
            CanHold=canHold;
            Field=field;
            First=first;
            Eval=eval;
            TaskIndex=taskIndex;

        }


        public int Current;
        public int Next;
        public int NextCount;
        public int Hold;
        public bool CanHold;
        public BitArray Field;
        public long First;
        public float Eval;
        public int TaskIndex;


    }
}
