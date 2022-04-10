using System;

namespace JackFrame.DesignPattern {

    public interface IFSM {

        void Activate();
        void Deactivate();
        void EnterState(int stateId);
        void RegisterState(FSMStateBase state);
        
    }

}