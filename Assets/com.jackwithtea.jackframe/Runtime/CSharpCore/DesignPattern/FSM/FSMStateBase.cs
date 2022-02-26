
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
    public abstract class FSMStateBase<T> {

        public abstract int StateId { get; }
        public abstract void Enter(T actor);
        public abstract void Execute(T actor);
        public abstract void FixedExecute(T actor);
        public abstract void Exit(T actor);

    }
}