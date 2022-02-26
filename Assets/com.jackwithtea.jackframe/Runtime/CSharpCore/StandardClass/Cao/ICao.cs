using System;

namespace JackFrame {

    public interface ICao<TKey, TValue> {
        void Add(TKey k, TValue v);
        TValue Get(TKey k);
        TValue[] GetArray();
        void Set(TKey k, TValue v);
        int GetCount();
        void Remove(TKey k);
        void Remove(TValue v);
        void Clear();
    }

}