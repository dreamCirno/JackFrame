using System;
using UnityEngine;
using UnityEngine.UI;

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
    public static class LineRendererExtention {

        public static void DrawSquare(this LineRenderer lr, Vector2 startPos, Vector2 endPos, Material material, Color color, float weight) {

            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = weight;
            lr.endWidth = weight;

            lr.material = material;

            lr.positionCount = 5;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, new Vector3(startPos.x, endPos.y));
            lr.SetPosition(2, endPos);
            lr.SetPosition(3, new Vector3(endPos.x, startPos.y));
            lr.SetPosition(0, startPos);
        }

        public static void DrawHollowCircle(this LineRenderer lr, Camera camera, Vector3 mousePosition, Color color, float border, float radius) {
            Vector3 targetPosition = camera.GetMouseWorldPosition(mousePosition);
            DrawHollowCircle(lr, targetPosition, color, border, radius);
        }

        public static void DrawHollowCircle(this LineRenderer lr, Vector3 startWorldPos, Color color, float border, float radius) {
            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = border;
            lr.endWidth = border;

            int pointCount = 362;
            lr.positionCount = pointCount;

            // 以鼠标为中心画圆
            for (int i = 0; i < pointCount; i += 1) {

                float x = Mathf.Cos((360 * (i + 1) / pointCount) * Mathf.Deg2Rad) * radius + startWorldPos.x;
                float z = Mathf.Sin((360 * (i + 1) / pointCount) * Mathf.Deg2Rad) * radius + startWorldPos.z;
                lr.SetPosition(i, new Vector3(x, startWorldPos.y, z));
                
            }
        }

        public static void DrawSolidCircle(this LineRenderer lr, Camera camera, Vector3 mousePosition, Color color, float radius) {
            Vector3 targetPosition = camera.GetMouseWorldPosition(mousePosition);
            DrawSolidCircle(lr, targetPosition, color, radius);
        }

        public static void DrawSolidCircle(this LineRenderer lr, Vector3 worldPos, Color color, float radius) {
            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = radius;
            lr.endWidth = radius;

            lr.positionCount = 2;

            lr.SetPosition(0, worldPos);
            lr.SetPosition(1, worldPos);
        }

        public static void DrawSolidLine(this LineRenderer lr, Vector3 fromWorldPos, Vector3 toWorldPos, Color color, float width) {
            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = width;
            lr.endWidth = width;

            lr.positionCount = 2;

            lr.SetPosition(0, fromWorldPos);
            lr.SetPosition(1, toWorldPos);
        }

        public static void DrawSolidRay(this LineRenderer lr, Camera camera, Vector3 fromPosition, Vector3 mousePosition, Color color, float width, LayerMask layer) {

            lr.startColor = color;
            lr.endColor = color;

            lr.startWidth = width;
            lr.endWidth = width;

            lr.positionCount = 2;

            Vector3 targetPosition = camera.GetMouseWorldPosition(mousePosition);
            RaycastHit2D hit = Physics2D.Linecast(fromPosition, targetPosition, layer);
            if (hit) {
                targetPosition = hit.point;
            }
            lr.SetPosition(0, new Vector2(fromPosition.x, fromPosition.y));
            lr.SetPosition(1, new Vector2(targetPosition.x, targetPosition.y));

        }

    }
}