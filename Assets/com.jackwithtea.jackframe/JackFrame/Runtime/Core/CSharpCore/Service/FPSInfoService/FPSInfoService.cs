
namespace JackFrame {

    public class FPSInfoService : IFpsInfoService {

        bool isEnable;

        float fpsByDeltaTime;
        int frameCount;
        float passedTime;
        float realTimeFPS;

        public FPSInfoService() {
            this.isEnable = false;
            this.fpsByDeltaTime = 1.5f;
            this.frameCount = 0;
            this.passedTime = 0;
            this.realTimeFPS = 0;
        }

        public bool IsEnable() {
            return isEnable;
        }

        public void SetEnable(bool isEnable) {
            this.isEnable = isEnable;
        }

        public void Execute(float deltaTime) {

            if (!isEnable) {
                return;
            }

            frameCount += 1;
            passedTime += deltaTime;

            if (passedTime >= fpsByDeltaTime) {
                realTimeFPS = frameCount / passedTime;
                passedTime = 0;
                frameCount = 0;
            }

        }

        public float GetFPS() {
            return realTimeFPS;
        }

    }

}