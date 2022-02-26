
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
    public class BTSequence<TContext> : BTNode<TContext> {

        BTNode<TContext> activeChild;
        int activeIndex = -1;

        protected override TContext Context { get; set; }

        public BTSequence(BTPrecondition<TContext> _precondition = null) : base(_precondition) { }

        public override bool DoEvaluate(float time) {
            if (activeChild != null) {
                bool _res = activeChild.Evaluate(time);
                if (!_res) {
                    activeChild.Reset();
                    activeChild = null;
                    activeIndex = -1;
                }
                return _res;

            } else {

                return children[0].Evaluate(time);

            }
        }

        public override BTResult Tick() {

            if (activeChild == null) {
                activeChild = children[0];
                activeIndex = 0;
            }
            BTResult _res = activeChild.Tick();
            if (_res == BTResult.Ended) {
                activeIndex += 1;
                if (activeIndex >= children.Count) {
                    activeChild.Reset();
                    activeChild = null;
                    activeIndex = -1;
                } else {
                    activeChild.Reset();
                    activeChild = children[activeIndex];
                    _res = BTResult.Running;
                }
            }
            return _res;
        }

        public override void Reset() {
            if (activeChild != null) {
                activeChild = null;
                activeIndex = -1;
            }

            foreach (BTNode<TContext> _node in children) {
                _node.Reset();
            }
        }

    }

}