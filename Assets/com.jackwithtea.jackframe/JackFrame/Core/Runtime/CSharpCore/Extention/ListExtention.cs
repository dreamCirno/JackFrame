using System;
using System.Linq;
using System.Collections;
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
    public static class ListExtention {

        static Random Random { get; } = new Random();
        
        public static List<T> Shuffle<T>(this List<T> list) {
            for (int i = 0; i < list.Count; i += 1) {
                T cur = list[i];
                int rdIndex = Random.Next(list.Count);
                list[i] = list[rdIndex];
                list[rdIndex] = cur;
            }
            return list;
        }

        public static void AddIfNotExist<T>(this List<T> l, T obj) {
            if (!l.Contains(obj)) {
                l.Add(obj);
            }
        }

        public static void ForeachBack<T>(this List<T> l, Action<T> handle) {
            for (int i = l.Count - 1; i >= 0; i -= 1) {
                handle.Invoke(l[i]);
            }
        }

        public static T Shift<T>(this List<T> list) {
            T t = list.First();
            list.Remove(t);
            return t;
        }

        public static T Pop<T>(this List<T> list) {
            T t = list.Last();
            int index = list.Count - 1;
            list.RemoveAt(index);
            return t;
        }

        public static string ToListString<T>(this List<T> list, string splitChar) {
            string s = string.Empty;
            for (int i = 0; i < list.Count; i += 1) {
                s += list[i].ToString();
                if (i != list.Count - 1) {
                    s += splitChar;
                }
            }
            return s;
        }

    }

}