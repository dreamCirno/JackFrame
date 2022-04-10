using System;
using System.Collections;
using System.Collections.Generic;

namespace JackFrame {

    public class MultiKeySortedDictionary<TKey1, TKey2, TValue> where TKey1 : struct {

        SortedDictionary<TKey1, TValue> primaryDic;
        Dictionary<TKey2, TValue> subDic;

        object lockObj = new Object();

        public MultiKeySortedDictionary() {
            primaryDic = new SortedDictionary<TKey1, TValue>();
            subDic = new Dictionary<TKey2, TValue>();
        }

        public void Add(TKey1 primaryKey, TKey2 subKey, TValue value) {
            lock (lockObj) {
                primaryDic.Add(primaryKey, value);
                subDic.Add(subKey, value);
            }
        }

        public void Remove(TKey1 primaryKey, TKey2 subKey) {
            lock (lockObj) {
                primaryDic.Remove(primaryKey);
                subDic.Remove(subKey);
            }
        }

        public bool ContainsKeyPrimary(TKey1 primaryKey) {
            lock (lockObj) {
                return primaryDic.ContainsKey(primaryKey);
            }
        }

        public bool ContainsKeySub(TKey2 subKey) {
            lock (lockObj) {
                return subDic.ContainsKey(subKey);
            }
        }

        public bool TryGetValueFromPrimary(TKey1 key, out TValue value) {
            lock (lockObj) {
                return primaryDic.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromSub(TKey2 key, out TValue value) {
            lock (lockObj) {
                return subDic.TryGetValue(key, out value);
            }
        }

    }

}