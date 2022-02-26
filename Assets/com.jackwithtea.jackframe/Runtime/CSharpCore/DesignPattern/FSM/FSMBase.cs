using System.Collections.Generic;
using JackFrame;

namespace JackFrame.DesignPattern {

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
    public abstract class FSMBase<T> {

        public bool isRunning;
        public FSMStateBase<T> currentState;
        public FSMStateBase<T> lastState;
        Dictionary<int, FSMStateBase<T>> stateDic;

        public FSMBase() {
            stateDic = new Dictionary<int, FSMStateBase<T>>();
            isRunning = false;
        }

        public void Activate() {
            isRunning = true;
        }

        public void Deactivate() {
            isRunning = false;
        }

        public void RegisterState(FSMStateBase<T> state) {
            stateDic.Add(state.StateId, state);
        }

        public void EnterState(T actor, int stateId) {

            if (currentState != null && currentState.StateId == stateId) {
                // 状态相同
                return;
            }

            FSMStateBase<T> targetState = stateDic.GetValue(stateId);
            if (currentState != null) {
                currentState.Exit(actor);
            }
            currentState = targetState;
            currentState.Enter(actor);
            // BUG 这里或许有BUG
            lastState = currentState;

        }

        public void Execute(T actor) {

            if (!isRunning) return;

            if (currentState == null) return;

            currentState.Execute(actor);

        }

        public void FixedExecute(T actor) {

            if (!isRunning) return;

            if (currentState == null) return;

            currentState.FixedExecute(actor);

        }

        public void ExitCurrent(T actor) {
            currentState.Exit(actor);
            if (lastState != null) {
                EnterState(actor, lastState.StateId);
            }
        }

    }
}