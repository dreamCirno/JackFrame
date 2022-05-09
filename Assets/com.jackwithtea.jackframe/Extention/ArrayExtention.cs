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
    public static class ArrayExtention {

        public static T[] Shuffle<T>(this T[] arr, System.Random random = null) {

            if (random == null) random = new System.Random();

            for (int i = 0; i < arr.Length; i += 1) {

                T cur = arr[i];

                int _rdIndex = random.Next(arr.Length);

                arr[i] = arr[_rdIndex];

                arr[_rdIndex] = cur;

            }

            return arr;

        }

        public static bool TryGetEmptySlotIndex<T>(this T[] arr, out int index) {

            for (int i = 0; i < arr.Length; i += 1) {
                T cur = arr[i];
                if (cur == null) {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;

        }

        public static T[] Add<T>(this T[] arr, T obj, int expandCount = 4) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t == null) {
                    arr[i] = obj;
                    return arr;
                }
            }
            arr = arr.Expand(expandCount);
            arr.Add(obj);
            return arr;
        }

        public static T[] AddIfNotExist<T>(this T[] arr, T obj, int expandCount = 4) where T : class {

            T exists = arr.Find(value => value.Equals(obj));
            if (exists != null) {
                return arr;
            }

            return arr.Add(obj, expandCount);

        }

        public static int IndexOf<T>(this T[] arr, T obj) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T value = arr[i];
                if (value != null) {
                    if (value.Equals(obj)) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int GetNotNullLength<T>(this T[] arr) where T : class {
            int notNullLength = 0;
            for (int i = 0; i < arr.Length; i += 1) {
                T obj = arr[i];
                if (obj != null) {
                    notNullLength += 1;
                }
            }
            return notNullLength;
        }

        public static void Foreach<T>(this T[] arr, Action<T> handle) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t != null) {
                    handle.Invoke(t);
                } else {
                    throw new Exception($"index: {i.ToString()} 为空");
                }
            }
        }

        public static T[] ForeachSetValue<T>(this T[] arr, T value) where T : struct {
            for (int i = 0; i < arr.Length; i += 1) {
                arr[i] = value;
            }
            return arr;
        }

        public static void ForeachWithoutNull<T>(this T[] arr, Action<T> handle) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t != null) {
                    handle.Invoke(t);
                }
            }
        }

        public static int FindIndex<T>(this T[] arr, Predicate<T> predicate) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t == null) {
                    continue;
                }
                bool isFind = predicate.Invoke(t);
                if (isFind) {
                    return i;
                }
            }
            return -1;
        }

        public static int FindValueIndex<T>(this T[] arr, Predicate<T> predicate) where T : struct {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                bool isFind = predicate.Invoke(t);
                if (isFind) {
                    return i;
                }
            }
            return -1;
        }

        public static bool ContainsValue<T>(this T[] arr, Predicate<T> predicate) where T : struct {
            for (int i = 0; i < arr.Length; i += 1) {
                if (predicate(arr[i])) {
                    return true;
                }
            }
            return false;
        }

        public static T Find<T>(this T[] arr, Predicate<T> predicate) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t == null) {
                    continue;
                }
                bool isFind = predicate.Invoke(t);
                if (isFind) {
                    return t;
                }
            }
            return null;
        }

        public static T[] FindAll<T>(this T[] arr, Predicate<T> predicate) where T : class {
            List<T> list = new List<T>();
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t == null) {
                    continue;
                }
                bool isFind = predicate.Invoke(t);
                if (isFind) {
                    list.Add(t);
                }
            }
            return list.ToArray();
        }

        public static T[] RemoveAllNull<T>(this T[] arr) where T : class {
            List<T> list = new List<T>();
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t != null) {
                    list.Add(t);
                }
            }
            arr = list.ToArray();
            return arr;
        }

        public static T[] Expand<T>(this T[] arr, int expandCount) {
            T[] newArr = new T[arr.Length + expandCount];
            Array.Copy(arr, newArr, arr.Length);
            return newArr;
        }

        public static void Remove<T>(this T[] arr, T obj) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                T t = arr[i];
                if (t != null) {
                    if (t.Equals(obj)) {
                        arr[i] = null;
                        return;
                    }
                }
            }
        }

        public static void Clear<T>(this T[] arr) where T : class {
            for (int i = 0; i < arr.Length; i += 1) {
                arr[i] = null;
            }
        }


        public static void QuickSort(this int[] numbers, int left, int right) {
            if (left < right) {
                int middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true) {
                    while (numbers[++i] < middle) ;

                    while (numbers[--j] > middle) ;

                    if (i >= j)
                        break;

                    int temp = numbers[i];
                    numbers[i] = numbers[j];
                    numbers[j] = temp;
                }

                QuickSort(numbers, left, i - 1);
                QuickSort(numbers, j + 1, right);
            }
        }

        public static int BinaryFind(this int[] nums, int target) {
            int low = 0, high = nums.Length - 1;
            while (low <= high) {
                int mid = (high - low) / 2 + low;
                int num = nums[mid];
                if (num == target) {
                    return mid;
                } else if (num > target) {
                    high = mid - 1;
                } else {
                    low = mid + 1;
                }
            }
            return -1;
        }

        public static void QuickSort(this long[] numbers, int left, int right) {
            if (left < right) {
                long middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true) {
                    while (numbers[++i] < middle) ;

                    while (numbers[--j] > middle) ;

                    if (i >= j)
                        break;

                    long temp = numbers[i];
                    numbers[i] = numbers[j];
                    numbers[j] = temp;
                }

                QuickSort(numbers, left, i - 1);
                QuickSort(numbers, j + 1, right);
            }
        }

        public static int BinaryFind(this long[] nums, long target) {
            int low = 0, high = nums.Length - 1;
            while (low <= high) {
                int mid = (high - low) / 2 + low;
                long num = nums[mid];
                if (num == target) {
                    return mid;
                } else if (num > target) {
                    high = mid - 1;
                } else {
                    low = mid + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// 这个方法目前使用冒泡排序，可根据类的某个字段进行判断，当返回值为true时会调换前后两个类的顺序
        /// 例如：某个数组.Sort((x, y) => x.ID > y.ID);
        /// 此时以ID进行判断，若 x.ID > y.ID 则为升序，若 x.ID < y.ID 则降序
        /// 详细输出见ArrayExtentionDemo
        /// </summary>
        public static void Sort<T>(this T[] arr, Func<T, T, bool> comparison) {
            if (arr == null) {
                return;
            }

            for (int i = 0; i < arr.Length - 1; i++) {
                for (int j = 0; j < arr.Length - 1 - i; j++) {
                    if (comparison(arr[j], arr[j + 1])) {
                        T tempT = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = tempT;
                    }
                }
            }
        }


        /// <summary>
        /// 自定义二分查找
        /// 【注意】自定义二分查找前先根据对应的字段进行自定义排序！
        /// 需要在外部进行左右侧判断，详细使用方法见ArrayExtentionDemo
        /// </summary>
        public static int BinaryFind<T>(this T[] tArray, Func<T, int> func) {
            int low = 0, high = tArray.Length - 1;
            while (low <= high) {
                int mid = (high - low) / 2 + low;
                int decide = func(tArray[mid]);
                if (decide == 0) {
                    return mid;
                } else if (decide > 0) {
                    high = mid - 1;
                } else {
                    low = mid + 1;
                }
            }
            return -1;
        }
    }
}