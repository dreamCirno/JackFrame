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
    public abstract class FrameUIManagerBase {

        Canvas uiCanvas;
        Camera mainCam;
        Image background;

        Transform[] rootArr;
        FrameUIAssets[] assetsArr;
        FrameUIRepository[] openedRepoArr;

        protected FrameUIManagerBase() {

            this.rootArr = new Transform[4];

            this.assetsArr = new FrameUIAssets[4];
            for (int i = 0; i < assetsArr.Length; i += 1) {
                assetsArr[i] = new FrameUIAssets();
            }

            this.openedRepoArr = new FrameUIRepository[4];
            for (int i = 0; i < openedRepoArr.Length; i += 1) {
                openedRepoArr[i] = new FrameUIRepository();
            }

        }

        public void Inject(Camera mainCam,
                           Canvas uiCanvas,
                           Image background,
                           Transform pageRoot,
                           Transform windowRoot,
                           Transform worldTipsRoot,
                           Transform uiTipsRoot) {
            this.mainCam = mainCam;
            this.uiCanvas = uiCanvas;
            this.background = background;

            this.rootArr[(int)UIRootLevel.WorldTips] = worldTipsRoot;
            this.rootArr[(int)UIRootLevel.Page] = pageRoot;
            this.rootArr[(int)UIRootLevel.Window] = windowRoot;
            this.rootArr[(int)UIRootLevel.UITips] = uiTipsRoot;

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

        public void LateExecute() {

        }

        public void TearDown() {

        }

        // UI 背景
        public Image GetBackground() => background;
        public void SetBackgroundImage(Sprite bg) {
            this.background.sprite = bg;
        }
        public void ShowBackground(bool isShow) {
            background.gameObject.SetActive(isShow);
        }

        public Camera GetMainCam() => mainCam;
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
        public FrameUIPanelBase GetOpened(UIRootLevel root, int id) {
            var repo = GetRepository(root);
            return repo.Get(id);
        }

        FrameUIAssets GetAssets(UIRootLevel rootLevel) {
            return assetsArr[(int)rootLevel];
        }

        Transform GetRoot(UIRootLevel rootLevel) {
            return rootArr[(int)rootLevel];
        }

        // 打开页面
        public T Open<T>(UIRootLevel rootLevel, int id) where T : FrameUIPanelBase {

            FrameUIAssets assets = GetAssets(rootLevel);
            FrameUIPanelBase p = assets.Get(id);
            if (p == null) {
                PLog.Error("未注册: " + rootLevel.ToString() + ", id: " + id.ToString());
                return null;
            }

            if (p.IsUnique) {
                var existPanel = GetOpened(rootLevel, id);
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

        public T OpenPage<T>(int pageId) where T : FrameUIPanelBase {
            return Open<T>(UIRootLevel.Page, pageId);
        }

        public T OpenWindow<T>(int windowId) where T : FrameUIPanelBase {
            return Open<T>(UIRootLevel.Window, windowId);
        }

        public T OpenWorldTips<T>(int tipsId) where T : FrameUIPanelBase {
            return Open<T>(UIRootLevel.WorldTips, tipsId);
        }

        public T OpenUITips<T>(int tipsId) where T : FrameUIPanelBase {
            return Open<T>(UIRootLevel.UITips, tipsId);
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

        public FrameUIPanelBase GetPage(int pageId) {
            return GetOpened(UIRootLevel.Page, pageId);
        }

        public FrameUIPanelBase GetWindow(int windowId) {
            return GetOpened(UIRootLevel.Window, windowId);
        }

        public FrameUIPanelBase GetWorldTips(int tipsId) {
            return GetOpened(UIRootLevel.WorldTips, tipsId);
        }

        public FrameUIPanelBase GetUITips(int tipsId) {
            return GetOpened(UIRootLevel.UITips, tipsId);
        }

    }

}