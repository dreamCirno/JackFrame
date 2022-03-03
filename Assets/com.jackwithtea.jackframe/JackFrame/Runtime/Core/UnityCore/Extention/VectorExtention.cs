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
    public static class VectorExtention {

        public static Vector2 ToZ0(this Vector3 _v3) {
            _v3.z = 0;
            return _v3;
        }

        public static Vector3 GetHorizontal(this Vector3 v3) {
            v3.y = 0;
            return v3;
        }

        public static string ToFullString(this Vector2 v2) {
            return $"({v2.x}, {v2.y})";
        }

        public static string ToFullString(this Vector3 v3) {
            return $"({v3.x}, {v3.y}, {v3.z})";
        }

        public static bool IsPointOutRect(this Vector2 pointPos, Vector2 targetPos, Vector2 targetSize) {
            float leftEdge = targetPos.x - targetSize.x * 0.5f;
            float rightEdge = targetPos.x + targetSize.x * 0.5f;
            if (pointPos.x < leftEdge || pointPos.x > rightEdge) {
                return true;
            }

            float upEdge = targetPos.y + targetSize.y * 0.5f;
            float downEdge = targetPos.y - targetSize.y * 0.5f;
            if (pointPos.y < downEdge || pointPos.y > upEdge) {
                return true;
            }
            return false;
        }

        public static List<Vector3> GenerateRandomHorizontalPositions(this Vector3 v3, float widthOffset, float heightOffset, float gapDistance, int generateCount) {
            List<Vector3> all = new List<Vector3>();
            for (float i = v3.x - widthOffset; i <= v3.x + widthOffset; i += gapDistance) {
                for (float j = v3.z - heightOffset; i <= v3.z + heightOffset; i += gapDistance) {
                    Vector3 v = new Vector3(i, v3.y, j);
                    all.Add(v);
                }
            }
            all.Shuffle();
            return all.GetRange(0, generateCount);
        }

        public static float GetVerticalAngle(this Vector3 from, Vector3 to) {
            from.x = 0;
            to.x = 0;
            return Vector3.Angle(from, to);
        }

        public static float DistanceOfHorizontal(this Vector3 a, Vector3 b) {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }

        public static Vector3 SetKeepZ(this Vector3 _v3, float _x, float _y) {
            Vector3 _t = Vector3.zero;
            _t.x = _x;
            _t.y = _y;
            _t.z = _v3.z;
            return _t;
        }

        public static Vector3 JustSetX(this Vector3 v3, float x) {
            Vector3 tar = v3;
            tar.x = x;
            return tar;
        }

        public static Vector3 JustSetY(this Vector3 v3, float y) {
            Vector3 tar = v3;
            tar.y = y;
            return tar;
        }

        public static Vector3 JustSetZ(this Vector3 v3, float z) {
            Vector3 tar = v3;
            tar.z = z;
            return tar;
        }

        public static bool IsEqual(this Vector3 v3, Vector3Int v3Int) {
            return (int)v3.x == v3Int.x && (int)v3.y == v3Int.y && (int)v3.z == v3Int.z;
        }

        public static Vector3 SetKeepZ(this Vector3 _v3, Vector3 _targetV3) {
            return SetKeepZ(_v3, _targetV3.x, _targetV3.y);
        }

        public static Vector3Int SetAndReturn(ref this Vector3Int _v3, int _x, int _y, int _z = 0) {
            _v3.Set(_x, _y, _z);
            return _v3;
        }

        public static Vector3 SetAndReturn(ref this Vector3 _v3, float _x, float _y, float _z = 0) {
            _v3.Set(_x, _y, _z);
            return _v3;
        }

        public static Vector2Int SetAndReturn(ref this Vector2Int _v2, int _x, int _y) {
            _v2.Set(_x, _y);
            return _v2;
        }

        public static Vector2 SetAndReturn(ref this Vector2 _v2, float _x, float _y) {
            _v2.Set(_x, _y);
            return _v2;
        }

        public static Vector2 ToOne(this Vector2 _v2) {
            _v2.Set(_v2.x.ToOne(), _v2.y.ToOne());
            return _v2;
        }

        public static float To2DFaceAngle(this Vector2 _v2) {

            return Mathf.Atan2(_v2.y, _v2.x) * Mathf.Rad2Deg;

        }

        public static Vector2 LockMouse8Dir(this Vector2 dir) {

            //         90
            //     135     45
            // 180             0
            //    -135    -45
            //        -90

            float angle = dir.To2DFaceAngle();
            
            if ((0 <= angle && angle < 22.5f) || (-22.5 <= angle && angle < 0)) {
                dir = Vector2.right;
            } else if (22.5f <= angle && angle < 67.5f) {
                dir = new Vector2(1, 1);
                dir.Normalize();
            } else if (67.5f <= angle && angle < 112.5f) {
                dir = Vector2.up;
            } else if (112.5f <= angle && angle < 157.5f) {
                dir = new Vector2(-1, 1);
                dir.Normalize();
            } else if ((157.5f <= angle && angle < 180) || (-180 <= angle && angle < -157.5f)) {
                dir = Vector2.left;
            } else if (-157.5f <= angle && angle < -112.5f) {
                dir = new Vector2(-1, -1);
                dir.Normalize();
            } else if (-112.5f <= angle && angle < -67.5f) {
                dir = Vector2.down;
            } else if (-67.5f <= angle && angle < -22.5f) {
                dir = new Vector2(1, -1);
                dir.Normalize();
            }

            return dir;

        }

        public static Quaternion To2DFaceRotation(this Vector2 _v2) {

            return Quaternion.AngleAxis(_v2.To2DFaceAngle(), Vector3.forward);

        }

        public static int GetFace4(this Vector2 _dir, int _oldFace) {

            // 四向
            // 左右优先
            //   0
            // 3   1
            //   2
            if (_dir.x < 0) {
                return 3;
            }
            if (_dir.x > 0) {
                return 1;
            }
            if (_dir.y < 0) {
                return 0;
            }
            if (_dir.y > 0) {
                return 2;
            }

            return _oldFace;

        }

        public static Vector2 GetFace8(this Vector2 _dir, Vector2 _oldDir) {

            if (_dir.magnitude == 0) {

                return _oldDir;

            } else {

                return _dir;

            }
        }

        public static Vector2 GetFace8FromInt(this Vector2 _dir, int _oldFace) {
            switch (_oldFace) {
                case 7: _dir.Set(-1, 1); break;
                case 6: _dir.Set(-1, 0); break;
                case 5: _dir.Set(-1, -1); break;
                case 0: _dir.Set(0, 1); break;
                case 4: _dir.Set(0, -1); break;
                case 1: _dir.Set(1, 1); break;
                case 2: _dir.Set(1, 0); break;
                case 3: _dir.Set(1, -1); break;
            }
            return _dir;
        }

        public static int GetFace8(this Vector2 _dir, int _oldFace) {

            // 八向
            // 7  0  1
            // 6     2
            // 5  4  3
            if (_dir.x < 0 && _dir.y > 0) {
                return 7;
            }
            if (_dir.x < 0 && _dir.y == 0) {
                return 6;
            }
            if (_dir.x < 0 && _dir.y < 0) {
                return 5;
            }
            if (_dir.x == 0 && _dir.y > 0) {
                return 0;
            }
            if (_dir.x == 0 && _dir.y < 0) {
                return 4;
            }
            if (_dir.x > 0 && _dir.y > 0) {
                return 1;
            }
            if (_dir.x > 0 && _dir.y == 0) {
                return 2;
            }
            if (_dir.x > 0 && _dir.y < 0) {
                return 3;
            }

            return _oldFace;

        }

    }
}