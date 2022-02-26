
namespace JackFrame {

    public interface IFpsInfoService {
        void Execute(float deltaTime);
        float GetFPS();
        bool IsEnable();
        void SetEnable(bool isEnable);
    }

}