using System;
using UnityEngine;

namespace JackFrame {
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
    public static class RigidBodyExtention {

        public static bool IsRelativeMove(this Rigidbody rig) {
            Vector3 vel = rig.velocity;
            vel.y = 0;
            if (vel.magnitude < 0.1f && vel.magnitude > -0.1f) {
                return false;
            } else {
                return true;
            }
        }

        // Topdown角色移动
        public static void MoveInTopDown(this Rigidbody r, Vector2 moveAxis, float moveSpeed, Transform cameraForwardTrans, Transform playerRootTrans) {

            // Move
            Vector3 verticalMove = cameraForwardTrans.forward * moveAxis.y;
            Vector3 horizontalMove = cameraForwardTrans.right * moveAxis.x;

            Vector3 moveDir = verticalMove + horizontalMove;
            moveDir.Normalize();
            moveDir.y = 0;

            float verticalVelocity = r.velocity.y;
            r.velocity = moveDir * moveSpeed;
            r.velocity = new Vector3(r.velocity.x, verticalVelocity, r.velocity.z);
            playerRootTrans.LookAt(playerRootTrans.position + moveDir * 10f);

        }

        // FPS角色移动
        public static void MoveInFPS(this Rigidbody r, Vector2 moveAxis, float moveSpeed, float turnSpeed, Transform playerRootTrans = null) {

            // Move
            Vector3 verticalMove = r.transform.forward * moveAxis.y;
            // Vector3 horizontalMove = r.transform.right * moveAxis.x;

            // Rotate
            if (playerRootTrans == null) {
                playerRootTrans = r.transform;
            }
            playerRootTrans.Rotate(new Vector3(0, moveAxis.x, 0) * turnSpeed);

            Vector3 moveDir = verticalMove;
            moveDir.Normalize();

            float verticalVelocity = r.velocity.y;
            r.velocity = moveDir * moveSpeed;
            r.velocity = new Vector3(r.velocity.x, verticalVelocity, r.velocity.z);

        }

        // 下落
        public static void Falling(this Rigidbody r, float fallingSpeed, float maxFallingSpeed, float deltaTime) {

            r.velocity += Vector3.down * fallingSpeed * deltaTime;
            if (r.velocity.y <= -maxFallingSpeed) {
                r.velocity = new Vector3(r.velocity.x, -maxFallingSpeed, r.velocity.z);
            }

        }

        // 跳
        public static void Jump(this Rigidbody r, float jumpSpeed) {
            r.velocity = new Vector3(r.velocity.x, jumpSpeed, r.velocity.z);
        }

        // 在太空中旋转
        public static void LookAroundInSpace(this Rigidbody r, Vector2 lookAxis, float rotateSpeed, Transform playerRoot = null) {

            if (playerRoot == null) {
                playerRoot = r.transform;
            }

            if (lookAxis.y != 0) {
                float xAngle = lookAxis.y * rotateSpeed;
                playerRoot.rotation *= Quaternion.AngleAxis(xAngle, Vector3.right);
            }

            if (lookAxis.x != 0) {
                float yAngle = lookAxis.x * rotateSpeed;
                playerRoot.rotation *= Quaternion.AngleAxis(yAngle, Vector3.up);
            }

        }

        // 在地面左右看
        public static void LookAround(this Rigidbody r, float xAxis, float rotateSpeed, Transform playerRoot = null) {

            if (playerRoot == null) {
                playerRoot = r.transform;
            }

            if (xAxis != 0) {
                float yAngle = xAxis * rotateSpeed;
                playerRoot.rotation *= Quaternion.AngleAxis(yAngle, Vector3.up);
            }

        }

        // 在地面上下看
        public static void LookUpOrDownSide(this Rigidbody r, float yAxis, float rotateSpeed, Transform cameraTrans) {
            if (cameraTrans != null) {
                float rotationY = cameraTrans.localEulerAngles.x;
                if (yAxis != 0) {
                    rotationY += yAxis * rotateSpeed * -1;
                    rotationY = CheckAngle(rotationY);
                    if (rotationY > 30) {
                        rotationY = 30;
                    }
                    if (rotationY < -40) {
                        rotationY = -40;
                    }
                    cameraTrans.localEulerAngles = new Vector3(rotationY, 0, 0);
                }
            }
        }

        // 检测角度
        static float CheckAngle(float angle) {
            if (angle > 180) {
                angle = angle - 360;
            }
            return angle;
        }

        /// <summary>
        /// 通过初速度让角色自己前方移动（逐帧调用）
        /// </summary>
        /// <param name="trans">要移动的物体</param>
        /// <param name="moveSpeed">移动速度</param>
        public static void ForwardMove(this Rigidbody r, Transform trans, float moveSpeed) {
            r.velocity = trans.forward * moveSpeed;
        }

        /// <summary>
        /// 根据键盘按键和轮盘方向跳跃（一次性调用）
        /// </summary>
        /// <param name="trans">目标物体</param>
        /// <param name="jumpSpeed">跳跃初速度</param>
        /// <param name="forwardSpeed">向前初速度</param>
        /// <param name="h">X轴方向</param>
        /// <param name="v">y轴方向</param>
        public static void FreeJump(this Rigidbody r,Transform trans, float jumpSpeed, float forwardSpeed, float h, float v) {
            float val = Math.Abs(v) >= Math.Abs(h) ? Math.Abs(v) : Math.Abs(h);
            r.velocity += trans.forward * val * forwardSpeed;
            r.velocity += trans.up * jumpSpeed;
        }

    }
}