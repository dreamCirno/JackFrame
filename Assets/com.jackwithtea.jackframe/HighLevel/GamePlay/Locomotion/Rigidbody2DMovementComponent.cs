using UnityEngine;

namespace JackFrame.GamePlay {

    public class Rigidbody2DMovementComponent {

        // Core
        Rigidbody2D rb;

        // Model
        public float moveSpeed;
        public float moveAccelerateTime;
        public float moveDecelerateTime;

        public float jumpSpeed;
        public float fallingSpeed;
        public float fallingSpeedMax;
        public float raiseSpeed;
        public float fallingMultipleSpeed;
        public float gravity;

        bool isJump;
        public bool IsJump => isJump;
        public void SetIsJump(bool value) => this.isJump = value;

        bool isGround;
        public bool IsGround => isGround;
        public void SetIsGround(bool value) => this.isGround = value;

        public Rigidbody2DMovementComponent() {
            this.isJump = false;
            this.isGround = false;
        }

        public void Ctor(Rigidbody2D rb) {
            this.rb = rb;
        }

        public Vector3 GetVelocity() {
            return rb.velocity;
        }

        public void SetVelocity(Vector3 velo) {
            rb.velocity = velo;
        }

        public void SetXVelocity(float x) {
            float y = rb.velocity.y;
            rb.velocity = new Vector2(x, y);
        }

        public void SetYVelocity(float y) {
            float x = rb.velocity.x;
            rb.velocity = new Vector2(x, y);
        }

        public void Move(float xAxis, float fixedDeltaTime) {
            float accelerateTime = moveAccelerateTime;
            float decelerateTime = moveDecelerateTime;
            rb.MoveInPlatform(xAxis, fixedDeltaTime, moveSpeed, accelerateTime, decelerateTime);
        }

        public void Falling(float jumpAxis, float fixedDeltaTime) {
            rb.Falling(jumpAxis, fixedDeltaTime, fallingSpeedMax, gravity, fallingMultipleSpeed, raiseSpeed);
        }

        public void Jump(float jumpAxis, float speedMultiple = 1f) {
            if (!isJump && jumpAxis > 0) {
                rb.Jump(jumpAxis, ref isJump, jumpSpeed * speedMultiple);
                isJump = true;
            }
        }

    }

}