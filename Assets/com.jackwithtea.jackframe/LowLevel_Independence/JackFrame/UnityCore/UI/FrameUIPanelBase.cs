using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class FrameUIPanelBase : MonoBehaviour, IComparable<FrameUIPanelBase> {

        public enum PanelEvent {
            Pop = -3,
            Close = -2,
        }

        public abstract int Id { get; }
        public abstract UIRootLevel RootLevel { get; }
        public virtual int OrderInCurrentRoot { get; } = 100;
        public abstract bool IsUnique { get; }

        Dictionary<int, Action> eventDic = new Dictionary<int, Action>();

        [SerializeField] CanvasGroup canvasGroup;

        public virtual void Execute() {
        }

        public virtual void ExecuteWhenLastInCurrentRoot() {

        }

        public virtual void LateExecute() {

        }

        // 打开页面
        public virtual void Open() {
            if (canvasGroup != null) {
                SetInteractable(true);
                canvasGroup.alpha = 1;
                gameObject.SetActive(true);
            }
        }

        // 注册关闭页面时的方法
        public void OnClose(Action handle) {
            RegisterEvent((int)PanelEvent.Close, handle);
        }

        // 关闭并销毁页面
        public void CloseAndDestroy() {
            Close();
            GameObject.Destroy(gameObject);
        }

        // 关闭页面
        public void Close() {
            if (canvasGroup != null) {
                canvasGroup.alpha = 0;
                SetInteractable(false);
            }
            gameObject.SetActive(false);
            TriggerEvent((int)PanelEvent.Close);
            TriggerEvent((int)PanelEvent.Pop);
        }

        public CanvasGroup GetCanvasGroup() => canvasGroup;

        // 注册事件
        public void RegisterEvent(int eventId, Action handle) {
            if (!eventDic.ContainsKey(eventId)) {
                eventDic.Add(eventId, handle);
            } else {
                PLog.Warning(eventId + " 事件已存在");
            }
        }

        // 触发事件
        public void TriggerEvent(int eventId) {
            Action handle = eventDic.GetValue(eventId);
            if (handle != null) {
                handle.Invoke();
            } else {
                // PLog.Warning(eventId + " 事件未注册");
            }
        }

        // 设置当前页的可交互性
        public void SetInteractable(bool isInteractable) {
            canvasGroup.interactable = isInteractable;
        }

        // 常用方法
        public void SetWorldPos(Camera uiCam, Vector3 worldPos) {
            Vector3 screenPos = uiCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        public virtual void TearDown() {
            eventDic.Clear();
        }

        public bool IsTapOutRegion(RectTransform region) {

            if (region == null) {
                return false;
            }

            bool isPointerDown = false;
            Vector2 tapPos = Vector2.zero;

            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    isPointerDown = true;
                    tapPos = touch.position;
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                isPointerDown = true;
                tapPos = Input.mousePosition;
            }

            if (isPointerDown) {
                Vector2 regionPos = region.position;
                Vector2 regionSize = region.sizeDelta;
                // PLog.Log($"Click Out: {tapPos} / {region.position} / {region.sizeDelta}");
                return tapPos.IsPointOutRect(regionPos, regionSize);
            }

            return false;

        }

        public int CompareTo(FrameUIPanelBase other) {
            if (OrderInCurrentRoot > other.OrderInCurrentRoot) {
                return 1;
            } else if (OrderInCurrentRoot < other.OrderInCurrentRoot) {
                return -1;
            } else {
                return 0;
            }
        }
    }

}