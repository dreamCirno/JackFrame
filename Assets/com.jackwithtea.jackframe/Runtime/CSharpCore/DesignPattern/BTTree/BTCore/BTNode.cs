using System.Collections.Generic;

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

    // --------
    // 行为节点基类:
    // ① 子节点
    // ② 前置条件
    // ③ 数据上下文
    // --------
    public abstract class BTNode<TContext> {

        protected List<BTNode<TContext>> children;
        protected BTPrecondition<TContext> precondition;
        protected abstract TContext Context { get; set; }
        protected bool isActived;

        public float interval = 0;
        float lastTimeEvaluated = 0;

        public BTNode(BTPrecondition<TContext> _precondition = null) {

            precondition = _precondition;

        }

        // 激活
        public virtual void Activate(TContext _context) {

            if (isActived) return;

            Context = _context;

            if (precondition != null) {

                precondition.Activate(_context);

            }

            if (children != null) {

                foreach (BTNode<TContext> _node in children) {

                    _node.Activate(_context);

                }

            }

            isActived = true;

        }

        // 评估是否进入该节点
        public bool Evaluate(float time) {

            return isActived && CheckTimer(time) && (precondition == null || precondition.Check()) && DoEvaluate(time);

        }

        public virtual bool DoEvaluate(float time) {
            
            return true;

        }

        public virtual BTResult Tick() {

            return BTResult.Ended;

        }

        public virtual void AddChild(BTNode<TContext> _node) {

            if (children == null) {

                children = new List<BTNode<TContext>>();

            }

            if (_node != null) {

                children.Add(_node);

            }
        }

        public virtual void RemoveChild(BTNode<TContext> _node) {

            if (children != null && _node != null) {

                children.Remove(_node);

            }
        }

        bool CheckTimer(float time) {
            if (time - lastTimeEvaluated > interval) {
                lastTimeEvaluated = time;
                return true;
            }
            return false;
        }

        public abstract void Reset();

    }

}