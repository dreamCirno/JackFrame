
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
    public abstract class BTPrecondition<TContext> : BTNode<TContext> {

        public virtual bool Check() {

            return true;

        }

        public override BTResult Tick() {
            bool _res = Check();
            if (_res) {
                return BTResult.Ended;
            } else {
                return BTResult.Running;
            }
        }

    }
}