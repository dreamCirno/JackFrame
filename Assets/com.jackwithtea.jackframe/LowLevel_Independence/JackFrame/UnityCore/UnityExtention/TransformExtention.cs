using System;
using System.Collections.Generic;
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
    public static class TransformExtention {

        public static Transform GetBoneByName(this Transform tf, string name) {
            Transform[] hips = tf.GetComponentsInChildren<Transform>();
            for (int j = 0; j < hips.Length; j++) {
                if (hips[j].name == name) {
                    return hips[j];
                }
            }
            return null;
        }

        public static T[] FindChildrenWithDeep<T>(this Transform tf, int deepCount, List<T> res = null) {

            if (res == null) {
                res = new List<T>(tf.childCount * deepCount * 2);
            }

            if (deepCount > 0) {

                deepCount -= 1;

                List<Transform> children = new List<Transform>(tf.childCount);
                for (int i = 0; i < tf.childCount; i += 1) {
                    var child = tf.GetChild(i);
                    children.Add(child);
                    res.Add(child.GetComponent<T>());
                }

                children.ForEach(value => {
                    FindChildrenWithDeep<T>(value, deepCount, res);
                });

            } else {
                for (int i = 0; i < tf.childCount; i += 1) {
                    var child = tf.GetChild(i);
                    res.Add(child.GetComponent<T>());
                }
            }

            return res.ToArray();

        }

        public static T GunShoot<T>(this Transform _trans, T _bulletPrefab, Vector2 _targetPos, float _shootSpeed) where T : MonoBehaviour {

            Vector2 _pos = _trans.position; // 发射的起点坐标

            Vector2 _dir = _targetPos - _pos; // 发射方向

            T _go = GameObject.Instantiate(_bulletPrefab, _trans.parent); // 生成子弹

            _go.transform.position = _pos; // 子弹起始坐标

            _go.transform.rotation = _dir.To2DFaceRotation(); // 子弹面向

            Rigidbody2D _rig = _go.gameObject.GetComponent<Rigidbody2D>();

            if (_rig == null) {

                Debug.LogError("子弹必须包含刚体");

                return null;

            }

            _rig.velocity = _dir.normalized * _shootSpeed; // 飞行

            // 忽略碰撞
            Collider2D _gunCol = _trans.gameObject.GetComponent<Collider2D>();

            if (_gunCol != null) {

                Collider2D _bulletCol = _go.GetComponent<Collider2D>();

                if (_bulletCol != null) {

                    Physics2D.IgnoreCollision(_gunCol, _bulletCol);

                }

            }

            return _go;

        }

        public static void LookAtHorizontal(this Transform t, Vector3 target) {
            target.y = t.position.y;
            t.LookAt(target);
        }

        /// <summary>
        /// 物体朝向鼠标位置
        /// </summary>
        /// <param name="layer">要剔除的层级编号</param>
        /// <returns></returns>
        public static Vector3 LookAtMousePoint(this Transform transform, Camera mainCamera, int layer) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            int layerMask = ~(1 << layer);
            if (Physics.Raycast(ray, out hit, 10000, layerMask)) {
                transform.LookAt(hit.point);
            }
            return hit.point;
        }

        /// <summary>
        /// 朝调用物体前方发射射线
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public static float RayDiscover(this Transform rayTransform, RaycastHit hit, Ray ray) {
            ray = new Ray(rayTransform.position, rayTransform.forward);
            bool isHit = Physics.Raycast(ray, out hit);
            if (isHit) {
                return hit.distance;
            }
            return 10000;
        }

    }
}