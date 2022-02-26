using System;
using System.Collections.Generic;

namespace JackFrame {

    /*
        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系

        修改此类的风险非常大
        请与主程或架构负责人联系
    */
    [Serializable]
    public abstract class BaseCao<TKey, TValue> : ICao<TKey, TValue> where TValue : class {

        public Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        protected TValue[] arr = new TValue[4];

        public virtual void Add(TKey k, TValue v) {
            dic.Add(k, v);
            arr = arr.Add(v);
        }

        public virtual TValue Get(TKey k) => dic.GetValue(k);

        public virtual void Remove(TKey k) {
            TValue v = dic.GetValue(k);
            if (v != null) {
                arr.Remove(v);
                dic.Remove(k);
            }
        }

        public virtual void Remove(TValue v) {
            arr.Remove(v);
            TKey k = dic.GetKeyWithValue(v);
            dic.TryRemove(k);
        }

        public virtual void Set(TKey k, TValue v) {
            dic.AddOrReplace(k, v);
            int index = arr.IndexOf(dic.GetValue(k));
            if (index >= 0) {
                arr[index] = v;
            }
            else {
                arr = arr.Add(v);
            }
        }

        public TValue[] GetArray() {
            return arr;
        }

        // TODO 尽量不用这个方法
        public List<TValue> GetList() {
            arr = arr.RemoveAllNull();
            return new List<TValue>(GetArray());
        }

        public int GetCount() {
            return dic.Count;
        }

        public void Clear() {
            arr.Clear();
            dic.Clear();
        }

    }

}