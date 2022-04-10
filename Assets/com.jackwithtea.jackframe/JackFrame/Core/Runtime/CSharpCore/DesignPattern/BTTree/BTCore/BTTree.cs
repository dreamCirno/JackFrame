using UnityEngine;

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
    public abstract class BTTree<TContext> {

        protected abstract BTNode<TContext> RootNode { get; }
        public bool isActived = false;

        public virtual void Execute(float time) {
            if (!isActived) {
                Debug.Log("未激活");
                return;
            }

            if (RootNode.Evaluate(time)) {
                RootNode.Tick();
            }

        }

        public void Activated(TContext _context) {
            RootNode.Activate(_context);
            isActived = true;
        }

        public void Reset() {
            if (RootNode != null) {
                RootNode.Reset();
            }
        }

        public void Destroy() {
            Reset();
        }

    }

}