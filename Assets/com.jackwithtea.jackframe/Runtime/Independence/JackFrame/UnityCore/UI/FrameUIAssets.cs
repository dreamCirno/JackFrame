using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace JackFrame {

    public class FrameUIAssets {

        SortedDictionary<int, FrameUIPanelBase> pagePrefabDic;
        Dictionary<Type, FrameUIPanelBase> pagePrefabDicByType;

        public FrameUIAssets() {
            this.pagePrefabDic = new SortedDictionary<int, FrameUIPanelBase>();
            this.pagePrefabDicByType = new Dictionary<Type, FrameUIPanelBase>();
        }

        public void Add(int pageId, FrameUIPanelBase panel) {
            pagePrefabDic.Add(pageId, panel);
            pagePrefabDicByType.Add(panel.GetType(), panel);
        }

        public FrameUIPanelBase GetByID(int pageId) {
            _ = pagePrefabDic.TryGetValue(pageId, out FrameUIPanelBase panel);
            return panel;
        }

        public FrameUIPanelBase GetByType(Type type) {
            _ = pagePrefabDicByType.TryGetValue(type, out FrameUIPanelBase panel);
            return panel;
        }

    }

}