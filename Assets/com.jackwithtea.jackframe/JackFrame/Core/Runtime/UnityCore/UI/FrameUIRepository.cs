using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace JackFrame {

    public class FrameUIRepository {

        // 排列后的顺序
        List<FrameUIPanelBase> openedList;

        // 原始顺序
        List<FrameUIPanelBase> openedStack;

        public FrameUIRepository() {
            this.openedList = new List<FrameUIPanelBase>();
            this.openedStack = new List<FrameUIPanelBase>();
        }

        public void Add(FrameUIPanelBase panel) {
            this.openedList.Add(panel);
            this.openedStack.Add(panel);
        }

        public IEnumerable<FrameUIPanelBase> GetOpenedList() {
            return openedList;
        }

        public int GetCount() {
            return openedStack.Count;
        }

        public FrameUIPanelBase GetLast() {
            return openedStack.Last();
        }

        public FrameUIPanelBase GetByID(int id) {
            return openedList.Find(value => value.Id == id);
        }

        public T GetByType<T>() where T : FrameUIPanelBase {
            return openedList.Find(value => value.GetType() == typeof(T)) as T;
        }

        public FrameUIPanelBase Pop() {
            return openedList.Pop();
        } 

        public void SortSiblings(FrameUIPanelBase panel) {
            openedList.Sort();
            SortSiblings(openedList, panel);
        }

        void SortSiblings(List<FrameUIPanelBase> openedList, FrameUIPanelBase current) {
            for (int i = 0; i < openedList.Count; i += 1) {
                FrameUIPanelBase tar = openedList[i];
                if (tar.OrderInCurrentRoot > current.OrderInCurrentRoot) {
                    int index = tar.transform.GetSiblingIndex();
                    current.transform.SetSiblingIndex(index);
                    return;
                }
            }
            current.transform.SetAsLastSibling();
        }

        public void Remove(FrameUIPanelBase panel) {
            openedList.Remove(panel);
            openedStack.Remove(panel);
        }

        public void Clear() {
            openedList.Clear();
            openedStack.Clear();
        }

    }
}