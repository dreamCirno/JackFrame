using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

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
    public static class CameraExtention {

        static Vector2 tempV2 = Vector2.zero;
        static float camVec = 0;

        static Renderer lastHitRenderer;
        static Material lastMat;
 
        public static void FocusOn(this Camera _camera, Vector3 _targetPos) {
            _camera.transform.position = _camera.transform.position.SetKeepZ(_targetPos);
            camVec = 0;
        } 

        // 相机平滑跟随目标
        public static void FollowTarget(this Camera _camera, Vector3 _targetPos, float _smoothDeltaTime = 0) {

            if (_smoothDeltaTime == 0) {
                _smoothDeltaTime = Time.fixedDeltaTime * 3;
            }

            Vector3 _camPos = _camera.transform.position;
            _targetPos.Set(_targetPos.x, _targetPos.y, _camPos.z);

            _targetPos = Vector3.Lerp(_camPos, _targetPos, _smoothDeltaTime);
            _camera.transform.position = _targetPos;

        }

        public static void ReplaceMaterialOfHitRenderer(this Camera cam, Transform rayTarget, LayerMask rayCheckLayer, Material replaceMat) {

            Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));

            bool isHit = false;
            Renderer currentHitRenderer = null;

            if (Physics.Raycast(ray.origin, (rayTarget.position - ray.origin).normalized, out RaycastHit hit, 10000f, rayCheckLayer)) {
                if (!hit.collider.gameObject.Equals(rayTarget.gameObject)) {
                    if (lastHitRenderer == null) {
                        lastHitRenderer = hit.collider.GetComponent<Renderer>() ?? hit.collider.GetComponentInChildren<Renderer>();
                        if (lastHitRenderer == null) {
                            return;
                        }
                        currentHitRenderer = lastHitRenderer;
                        if (lastMat == null) {
                            lastMat = new Material(lastHitRenderer.material.shader);
                            lastMat.CopyPropertiesFromMaterial(lastHitRenderer.material);
                            lastHitRenderer.sharedMaterial = replaceMat;
                            // PLog.Log("命中: " + hit.collider.ToString());
                        }
                    }
                    isHit = true;
                }
            }

            if (!isHit) {
                if (lastHitRenderer != null && lastMat != null) {
                    lastHitRenderer.sharedMaterial = lastMat;
                    // PLog.Log("设回: " + cameraHitMat.ToString());
                    lastMat = null;
                    lastHitRenderer = null;
                }
            } else {
                if (lastHitRenderer != currentHitRenderer && currentHitRenderer != null && lastHitRenderer != null && lastMat != null) {
                    lastHitRenderer.sharedMaterial = lastMat;
                    lastMat = null;
                    lastHitRenderer = null;
                }
            }
        }

        public static void FollowTargetLimited(this Camera cam, bool isInstant, Vector3 targetPos, Vector3 mapPos, Vector2 mapBounds, Vector2 cameraBounds, float smoothDeltaTime = 0) {

            if (smoothDeltaTime == 0) {
                smoothDeltaTime = Time.fixedDeltaTime * 3;
            }

            Vector3 camPos = cam.transform.position;

            float minx = mapPos.x + cameraBounds.x / 2f;
            float maxx = mapPos.x + mapBounds.x - cameraBounds.x / 2f;
            float xLimit;
            if (minx <= maxx) {
                xLimit = Mathf.Clamp(targetPos.x, minx, maxx);
            } else {
                xLimit = Mathf.Clamp(targetPos.x, minx, minx);
            }

            float miny = mapPos.y + cameraBounds.y / 2f;
            float maxy = mapPos.y + mapBounds.y - cameraBounds.y / 2f;
            float yLimit;
            if (miny <= maxy) {
                yLimit = Mathf.Clamp(targetPos.y, miny, maxy);
            } else {
                yLimit = Mathf.Clamp(targetPos.y, miny, miny);
            }

            targetPos.Set(xLimit, yLimit, camPos.z);

            if (!isInstant) {
                targetPos = Vector3.Lerp(camPos, targetPos, smoothDeltaTime);
            }

            cam.transform.position = targetPos;

        }

        // 鼠标调整相机高低视野
        public static void ScrollFieldOfView(this Camera _camera, string _axisOfScroll, float _sensitivity = 0, float _min = 80, float _max = 130) {

            float _scrollVal = Input.GetAxisRaw(_axisOfScroll);

            _sensitivity += 10;

            float _smooth = Mathf.SmoothDamp(_camera.fieldOfView, _camera.fieldOfView + -_scrollVal * _sensitivity, ref camVec, Time.fixedDeltaTime * 2);

            _camera.fieldOfView = _smooth;
            if (_camera.fieldOfView >= _max) {
                _camera.fieldOfView = _max;
            } else if (_camera.fieldOfView <= _min) {
                _camera.fieldOfView = _min;
            }

        }

        // 鼠标调整相机高低视野
        public static void ScrollOrthographicSize(this Camera _camera, string _axisOfMouseScrollWheel, float _sensitivity = 0, float _min = 2, float _max = 16) {

            float _scrollVal = Input.GetAxisRaw(_axisOfMouseScrollWheel);

            _sensitivity += 10;

            float _smooth = Mathf.SmoothDamp(_camera.orthographicSize, _camera.orthographicSize + -_scrollVal * _sensitivity, ref camVec, Time.fixedDeltaTime);

            _camera.orthographicSize = _smooth;
            if (_camera.orthographicSize <= _min) {
                _camera.orthographicSize = _min;
            } else if (_camera.orthographicSize >= _max) {
                _camera.orthographicSize = _max;
            }

        }

        // 按住某键，鼠标平移相机
        public static void ClickMoveCamera(this Camera _camera, KeyCode _mousePressKey, float _sensitivity = 0.5f) {

            if (Input.GetKey(_mousePressKey)) {

                float _x = -Input.GetAxisRaw("Mouse X") * _sensitivity;
                float _y = -Input.GetAxisRaw("Mouse Y") * _sensitivity;

                tempV2.x = _x;
                tempV2.y = _y;

                _camera.transform.Translate(tempV2);

            }
        }

        public static Vector3 GetMouseWorldPosition(this Camera _camera, Vector3 _mousePosition, float _z = -99999) {
            _z = _z == -99999 ? _camera.transform.position.z : _z;
            Vector3 _tempVec = _mousePosition;
            _tempVec.Set(_mousePosition.x, _mousePosition.y, -_z);
            _tempVec = _camera.ScreenToWorldPoint(_tempVec);
            return _tempVec;
        }

    }
}