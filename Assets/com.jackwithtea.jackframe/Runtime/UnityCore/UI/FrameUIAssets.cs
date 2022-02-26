using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace JackFrame {

    public class FrameUIAssets {

        SortedDictionary<int, FrameUIPanelBase> pagePrefabDic;

        public FrameUIAssets() {
            this.pagePrefabDic = new SortedDictionary<int, FrameUIPanelBase>();
        }

        public void Add(int pageId, FrameUIPanelBase panel) {
            pagePrefabDic.Add(pageId, panel);
        }

        public FrameUIPanelBase Get(int pageId) {
            _ = pagePrefabDic.TryGetValue(pageId, out FrameUIPanelBase panel);
            return panel;
        }

    }

}