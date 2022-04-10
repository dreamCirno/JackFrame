
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
    // 挑一个满足条件的子节点进入 执行结果等同于子节点结果
    public class BTSelector<TContext> : BTNode<TContext> {

        BTNode<TContext> activeChild;
        
        protected override TContext Context { get; set; }

        public BTSelector(BTPrecondition<TContext> _precondition = null) : base(_precondition) {}

        public override bool DoEvaluate(float time) {

            foreach (BTNode<TContext> _node in children) {

                if (_node.Evaluate(time)) {

                    if (activeChild != null && activeChild != _node) {
                        activeChild.Reset();
                    }

                    activeChild = _node;

                    return true;

                }

            }

            if (activeChild != null) {
                activeChild.Reset();
                activeChild = null;
            }

            return false;

        }

        public override BTResult Tick() {

            if (activeChild == null) {

                return BTResult.Ended;

            }

            BTResult _res = activeChild.Tick();
            if (_res != BTResult.Running) {
                activeChild.Reset();
                activeChild = null;
            }

            return _res;

        }

        public override void Reset() {

            if (activeChild != null) {
                activeChild.Reset();
                activeChild = null;
            }
            
        }

    }

}