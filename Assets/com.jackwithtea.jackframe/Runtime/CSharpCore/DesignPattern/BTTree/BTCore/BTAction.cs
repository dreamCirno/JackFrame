using JackFrame;

namespace JackFrame.DesignPattern {

    public enum BTResult : byte {
        Ready,
        Running,
        Ended,
    }

    public enum BTActionState : byte {
        Ready,
        Running,
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
    public abstract class BTAction<TActor, TContext> : BTNode<TContext> {

        protected abstract TActor Fish { get; set;}
        protected BTActionState actionState = BTActionState.Ready;

        public abstract void Init(TActor actor, BTPrecondition<TContext> _precondition = null);

        public abstract void Enter();
        public abstract BTResult Execute();
        public abstract void Exit();

        public override BTResult Tick() {
            BTResult _res = BTResult.Running;
            if (actionState == BTActionState.Ready) {
                Enter();
                actionState = BTActionState.Running;
            }

            if (actionState == BTActionState.Running) {
                _res = Execute();
                if (_res != BTResult.Running) {
                    Exit();
                    actionState = BTActionState.Ready;
                }
            }
            return _res;
        }

        public override void AddChild(BTNode<TContext> _node) {
            PLog.Error("Forbid To AddChild");
        }

        public override void RemoveChild(BTNode<TContext> _node) {
            PLog.Error("Forbid To RemoveChild");
        }

    }

}