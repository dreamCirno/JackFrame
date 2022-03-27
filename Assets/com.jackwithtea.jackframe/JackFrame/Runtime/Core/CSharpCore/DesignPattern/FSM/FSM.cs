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
    public class FSM {

        public bool isRunning;
        public FSMStateBase currentState;
        public FSMStateBase lastState;
        Dictionary<int, FSMStateBase> stateDic;

        public FSM() {
            stateDic = new Dictionary<int, FSMStateBase>();
            isRunning = false;
        }

        public void Activate() {
            isRunning = true;
        }

        public void Deactivate() {
            isRunning = false;
        }

        public void RegisterState(FSMStateBase state) {
            state.fsm = this;
            stateDic.Add(state.StateId, state);
        }

        public void EnterState(int stateId) {

            if (currentState != null && currentState.StateId == stateId) {
                // 状态相同
                return;
            }

            FSMStateBase targetState = stateDic.GetValue(stateId);
            if (currentState != null) {
                currentState.Exit();
            }
            currentState = targetState;
            currentState.Enter();
            // BUG 这里或许有BUG
            lastState = currentState;

        }

        public void Execute(float deltaTime) {

            if (!isRunning) return;

            if (currentState == null) return;

            currentState.Execute(deltaTime);

        }

        public void FixedExecute(float fixedDeltaTime) {

            if (!isRunning) return;

            if (currentState == null) return;

            currentState.FixedExecute(fixedDeltaTime);

        }

        public void ExitCurrent() {
            currentState.Exit();
            if (lastState != null) {
                EnterState(lastState.StateId);
            }
        }

    }
}