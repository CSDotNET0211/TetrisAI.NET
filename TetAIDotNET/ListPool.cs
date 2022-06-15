using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetAIDotNET
{
    internal class ListPool
    {
     static   List<List<BitArray>> _listPool = new List<List<BitArray>>();
       static List<int> _listPoolReleasedIndex = new List<int>();

        /// <summary>
        /// 全てのListPoolを削除します。
        /// </summary>
       static public void ClearAllPool()
        {
            _listPool.Clear();
            _listPoolReleasedIndex.Clear();
        }

       static public List<BitArray> CreatePool(out int index)
        {
            _listPool.Add(new List<BitArray>());
            _listPoolReleasedIndex.Add(_listPool.Count - 1);
            index = _listPoolReleasedIndex.Count - 1;
            return _listPool[_listPoolReleasedIndex.Count - 1];
        }

       static public List<BitArray> GetList(out int index)
        {
            if (_listPoolReleasedIndex.Count == 0)
                return CreatePool(out index);


            var zeroindex = _listPoolReleasedIndex[0];
            index = zeroindex;
            _listPoolReleasedIndex.RemoveAt(0);
            return _listPool[zeroindex];

        }

    static    public void Release(int index)
        {
            _listPoolReleasedIndex.Add(index);
            _listPool[index].Clear();
        }
    }
}
