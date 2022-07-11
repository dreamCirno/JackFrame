using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace JackFrame {

    // 页面所属层级
    [Serializable]
    public enum UIRootLevel {
        WorldTips = 0,
        Page = 1,
        Window = 2,
        UITips = 3,
    }

    // 关闭页面时的策略
    [Serializable]
    public enum UICloseStrategy {
        Close,
        CloseAndDestroy,
    }

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
    public class FrameUIManager : MonoBehaviour {

        Canvas uiCanvas;
        Image background;

        Transform[] rootArr;
        FrameUIAssets[] assetsArr;
        FrameUIRepository[] openedRepoArr;

        public void Ctor() {

            uiCanvas = GetComponent<Canvas>();
            background = uiCanvas.transform.Find("UI_BG").GetComponent<Image>();
            PLog.ForceAssert(background != null, "FrameUIManagerBase.Awake() -> background is null");

            this.rootArr = new Transform[4];
            this.rootArr[(int)UIRootLevel.WorldTips] = transform.Find("UI_WORLD_TIPS_ROOT");
            this.rootArr[(int)UIRootLevel.Page] = transform.Find("UI_PAGE_ROOT");
            this.rootArr[(int)UIRootLevel.Window] = transform.Find("UI_WINDOW_ROOT");
            this.rootArr[(int)UIRootLevel.UITips] = transform.Find("UI_UI_TIPS_ROOT");
            for (int i = 0; i < rootArr.Length; i += 1) {
                PLog.ForceAssert(rootArr[i] != null, "FrameUIManagerBase.Awake() -> rootArr[" + i + "] is null");
            }

            this.assetsArr = new FrameUIAssets[4];
            for (int i = 0; i < assetsArr.Length; i += 1) {
                assetsArr[i] = new FrameUIAssets();
            }

            this.openedRepoArr = new FrameUIRepository[4];
            for (int i = 0; i < openedRepoArr.Length; i += 1) {
                openedRepoArr[i] = new FrameUIRepository();
            }

            PLog.ForceAssert(EventSystem.current != null, "FrameUIManagerBase.Awake() -> EventSystem is null");

        }

        // 主循环
        public virtual void Execute() {

            // Tick Last
            var pageRepo = GetRepository(UIRootLevel.Page);
            if (pageRepo.GetCount() > 0) {
                pageRepo.GetLast().ExecuteWhenLastInCurrentRoot();
            }

            var windowRepo = GetRepository(UIRootLevel.Window);
            if (windowRepo.GetCount() > 0) {
                windowRepo.GetLast().ExecuteWhenLastInCurrentRoot();
            }

            var worldTipsRepo = GetRepository(UIRootLevel.WorldTips);
            if (worldTipsRepo.GetCount() > 0) {
                worldTipsRepo.GetLast().ExecuteWhenLastInCurrentRoot();
            }

            var uiTipsRepo = GetRepository(UIRootLevel.UITips);
            if (uiTipsRepo.GetCount() > 0) {
                uiTipsRepo.GetLast().ExecuteWhenLastInCurrentRoot();
            }

            // Tick List
            var list = worldTipsRepo.GetOpenedList();
            foreach (var panel in list) {
                panel.Execute();
            }

            list = windowRepo.GetOpenedList();
            foreach (var panel in list) {
                panel.Execute();
            }

            list = pageRepo.GetOpenedList();
            foreach (var panel in list) {
                panel.Execute();
            }

            list = uiTipsRepo.GetOpenedList();
            foreach (var panel in list) {
                panel.Execute();
            }

        }

        // UI 背景
        public Image GetBackground() => background;
        public void SetBackgroundImage(Sprite bg) {
            this.background.sprite = bg;
        }
        public void ShowBackground(bool isShow) {
            background.gameObject.SetActive(isShow);
        }

        public Canvas GetCanvas() => uiCanvas;

        // 注册 Prefab
        public void RegisterUIPrefab(FrameUIPanelBase uiPanelPrefab) {
            FrameUIAssets assets = assetsArr[(int)uiPanelPrefab.RootLevel];
            assets.Add(uiPanelPrefab.Id, uiPanelPrefab);
        }

        public FrameUIRepository GetRepository(UIRootLevel rootLevel) {
            return openedRepoArr[(int)rootLevel];
        }

        // 获取页面
        public FrameUIPanelBase GetOpenedByID(UIRootLevel root, int id) {
            var repo = GetRepository(root);
            return repo.GetByID(id);
        }

        public T GetOpenedByType<T>(UIRootLevel root) where T : FrameUIPanelBase {
            var repo = GetRepository(root);
            return repo.GetByType<T>();
        }

        public FrameUIPanelBase GetOpenedPageByID(int pageId) {
            return GetOpenedByID(UIRootLevel.Page, pageId);
        }

        public T GetOpenedPageByType<T>() where T : FrameUIPanelBase {
            return GetOpenedByType<T>(UIRootLevel.Page);
        }

        public FrameUIPanelBase GetOpenedWindowByID(int windowId) {
            return GetOpenedByID(UIRootLevel.Window, windowId);
        }

        public T GetOpenedWindowByType<T>() where T : FrameUIPanelBase {
            return GetOpenedByType<T>(UIRootLevel.Window);
        }

        public FrameUIPanelBase GetOpenedWorldTipsByID(int tipsId) {
            return GetOpenedByID(UIRootLevel.WorldTips, tipsId);
        }

        public T GetOpenedWorldTipsByType<T>() where T : FrameUIPanelBase {
            return GetOpenedByType<T>(UIRootLevel.WorldTips);
        }

        public FrameUIPanelBase GetOpenedUITipsByID(int tipsId) {
            return GetOpenedByID(UIRootLevel.UITips, tipsId);
        }

        public T GetOpenedUITipsByType<T>() where T : FrameUIPanelBase {
            return GetOpenedByType<T>(UIRootLevel.UITips);
        }

        FrameUIAssets GetAssets(UIRootLevel rootLevel) {
            return assetsArr[(int)rootLevel];
        }

        Transform GetRoot(UIRootLevel rootLevel) {
            return rootArr[(int)rootLevel];
        }

        FrameUIPanelBase GetUIPanelByID(UIRootLevel rootLevel, int id) {
            var assets = GetAssets(rootLevel);
            return assets.GetByID(id);
        }

        FrameUIPanelBase GetUIPanelByType(UIRootLevel rootLevel, Type type) {
            var assets = GetAssets(rootLevel);
            return assets.GetByType(type);
        }

        // 打开页面
        public T OpenByType<T>(UIRootLevel rootLevel) where T : FrameUIPanelBase {
            var panel = GetUIPanelByType(rootLevel, typeof(T));
            if (panel == null) {
                PLog.Error("FrameUIManagerBase.OpenByType() -> panel is null");
            }
            return Open<T>(panel);
        }

        public T OpenByID<T>(UIRootLevel rootLevel, int id) where T : FrameUIPanelBase {
            var panel = GetUIPanelByID(rootLevel, id);
            if (panel == null) {
                PLog.Error("FrameUIManagerBase.OpenByID() -> panel is null");
            }
            return Open<T>(panel);
        }

        public T Open<T>(FrameUIPanelBase p) where T : FrameUIPanelBase {
            var rootLevel = p.RootLevel;
            var id = p.Id;
            if (p.IsUnique) {
                var existPanel = GetOpenedByID(rootLevel, id);
                if (existPanel != null) {
                    PLog.Warning("Panel is unique, don't open twice.");
                    return existPanel as T;
                }
            }

            var repo = GetRepository(rootLevel);

            Transform root = GetRoot(rootLevel);
            p = GameObject.Instantiate(p, root);
            p.RegisterEvent((int)FrameUIPanelBase.PanelEvent.Pop, () => PopBlur(p));
            p.Open();
            repo.Add(p);
            repo.SortSiblings(p);

            return p as T;

        }

        //获取所有UI界面
        public List<FrameUIPanelBase> GetAllOpenedUIPanel() {
            List<FrameUIPanelBase> all = new List<FrameUIPanelBase>();
            for (int i = 0; i < openedRepoArr.Length; i += 1) {
                var repo = openedRepoArr[i];
                all.AddRange(repo.GetOpenedList());
            }
            return all;
        }

        // 关闭时Pop页面
        public void PopBlur(FrameUIPanelBase panel) {
            var repo = GetRepository(panel.RootLevel);
            repo.Remove(panel);
        }

        public T OpenPageByID<T>(int pageId) where T : FrameUIPanelBase {
            return OpenByID<T>(UIRootLevel.Page, pageId);
        }

        public T OpenPageByType<T>() where T : FrameUIPanelBase {
            return OpenByType<T>(UIRootLevel.Page);
        }

        public T OpenWindowByID<T>(int windowId) where T : FrameUIPanelBase {
            return OpenByID<T>(UIRootLevel.Window, windowId);
        }

        public T OpenWindowByType<T>() where T : FrameUIPanelBase {
            return OpenByType<T>(UIRootLevel.Window);
        }

        public T OpenWorldTipsByID<T>(int tipsId) where T : FrameUIPanelBase {
            return OpenByID<T>(UIRootLevel.WorldTips, tipsId);
        }

        public T OpenWorldTipsByType<T>() where T : FrameUIPanelBase {
            return OpenByType<T>(UIRootLevel.WorldTips);
        }

        public T OpenUITipsByID<T>(int tipsId) where T : FrameUIPanelBase {
            return OpenByID<T>(UIRootLevel.UITips, tipsId);
        }

        public T OpenUITipsByType<T>() where T : FrameUIPanelBase {
            return OpenByType<T>(UIRootLevel.UITips);
        }

        public void CloseAndDestroyAllPage() {
            CloseAll(UIRootLevel.Page);
        }

        public void CloseAndDestroyAllWindow() {
            CloseAll(UIRootLevel.Window);
        }

        void CloseAll(UIRootLevel rootLevel) {
            var repo = GetRepository(rootLevel);
            while (repo.GetCount() > 0) {
                repo.Pop().CloseAndDestroy();
            }
            repo.Clear();
        }

    }

}