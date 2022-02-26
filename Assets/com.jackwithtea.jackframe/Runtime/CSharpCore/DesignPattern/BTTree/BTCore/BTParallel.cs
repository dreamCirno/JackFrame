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
    public enum ParallelType : byte {
        And,
        Or,
    }

    public class BTParallel<TContext> : BTNode<TContext> {

        List<BTResult> resList;
        ParallelType paraType;

        protected override TContext Context { get; set; }

        public BTParallel(ParallelType _type) : this(_type, null) {}

        public BTParallel(ParallelType _type, BTPrecondition<TContext> _precondition) : base(_precondition) {
            resList = new List<BTResult>();
            paraType = _type;
        }

        public override bool DoEvaluate(float time) {
            foreach (BTNode<TContext> _node in children) {
                if (!_node.Evaluate(time)) {
                    return false;
                }
            }
            return true;
        }

        public override BTResult Tick() {
            int _endCount = 0;

            for (int i = 0; i < children.Count; i += 1) {

                if (paraType == ParallelType.And) {

                    if (resList[i] == BTResult.Running) {
                        resList[i] = children[i].Tick();
                    }
                    if (resList[i] != BTResult.Running) {
                        _endCount += 1;
                    }

                } else {

                    if (resList[i] == BTResult.Running) {
                        resList[i] = children[i].Tick();
                    }
                    if (resList[i] != BTResult.Running) {
                        ResetResults();
                        return BTResult.Ended;
                    }

                }
            }

            if (_endCount == children.Count) {
                ResetResults();
                return BTResult.Ended;
            }
            return BTResult.Running;

        }

        public override void Reset() {

            ResetResults();

            foreach (BTNode<TContext> _node in children) {
                
                _node.Reset();

            }
        }

        public override void AddChild(BTNode<TContext> _node) {
            base.AddChild(_node);
            resList.Add(BTResult.Running);
        }

        public override void RemoveChild(BTNode<TContext> _node) {
            int _index = children.IndexOf(_node);
            resList.RemoveAt(_index);
            base.RemoveChild(_node);
        }

        void ResetResults() {
            for (int i = 0; i < resList.Count; i += 1) {
                resList[i] = BTResult.Running;
            }
        }

    }

}