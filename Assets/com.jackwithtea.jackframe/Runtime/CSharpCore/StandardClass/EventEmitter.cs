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
    public class EventEmitter {

        Dictionary<string, List<EmitterActionDelegate>> eventDic; // 所有事件列表
        Dictionary<int, EmitterActionDelegate> hashPairDic; // TODO 后续优化 GetHashCode()

        delegate void EmitterActionDelegate(object sender, EventArgs args);

        public EventEmitter() {
            eventDic = new Dictionary<string, List<EmitterActionDelegate>>();
            hashPairDic = new Dictionary<int, EmitterActionDelegate>();
        }

        void RegisterEvent(string eventID, EmitterActionDelegate handle) {
            var actionList = eventDic.GetValue(eventID);
            if (actionList == null) {
                actionList = new List<EmitterActionDelegate>();
                actionList.Add(handle);
                eventDic.Add(eventID, actionList);
            } else {
                actionList.Add(handle);
            }
        }

        public void RegisterEvent<T>(Action<object, T> handle) where T : EventArgs {

            int hash = handle.GetHashCode();
            if (hashPairDic.ContainsKey(hash)) {
                PLog.Error("请勿重复同一个函数注册两次");
                return;
            }

            string key = typeof(T).Name;
            void a(object sender, EventArgs args) {
                handle.Invoke(sender, (T)args);
            }
            RegisterEvent(key, a);

            hashPairDic.Add(hash, a);

        }

        public void TriggerEvent<T>(object sender, T args) where T : EventArgs {
            string key = typeof(T).Name;
            var actionList = eventDic.GetValue(key);
            if (actionList != null) {
                actionList.ForEach(value => {
                    value.Invoke(sender, args);
                });
            } else {
                PLog.Log(key + " 消息的事件不存在");
            }
        }

        public void Remove<T>(Action<object, T> handle) where T : EventArgs {
            string key = typeof(T).Name;
            var actionList = eventDic.GetValue(key);
            if (actionList != null) {
                int hash = handle.GetHashCode();
                EmitterActionDelegate a = hashPairDic.GetValue(hash);
                actionList.Remove(a);
                hashPairDic.Remove(hash);
            }
        }

        public void RemoveAllListeners() {
            foreach (var kv in eventDic) {
                if (kv.Value != null) {
                    kv.Value.Clear();
                }
            }
            eventDic.Clear();
        }

    }

}