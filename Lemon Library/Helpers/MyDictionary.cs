using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLibrary
{
    public class MyDictionary<TKey,TValue>
    {
        public Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        public void Add(TKey key,TValue data) {
            if (dic.Count == 50)
                dic.Remove(dic.Last().Key);
            if(!dic.ContainsKey(key))
                dic.Add(key,data);
        }
    }
}
