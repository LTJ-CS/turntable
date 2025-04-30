using UnityEngine;

namespace Sdk.Runtime.Utility
{
    public class GameUtil 
    {
        public static Sprite TextureToSprite(Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f); // 中心点
            Sprite sprite = Sprite.Create(texture, rect, pivot);
            return sprite;
        }

        /// <summary>
        /// 获取默认宽高(750x1334)在CanvasScaler和CanvasMatchScale共同作用下的缩放比
        /// </summary>
        /// <returns></returns>
        public static float GetDefaultCanvasScaler()
        {
            float logWidth = Mathf.Log(Screen.width / 750f, 2f);
            float logHeight = Mathf.Log(Screen.height / 1334f, 2f);
            float match = (float)Screen.width / Screen.height > 750f / 1334f ? 1f : 0f;
            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, match);
            return Mathf.Pow(2f, logWeightedAverage);
        }

        /// <summary>
        /// 获取向量与指定向量的夹角
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="referenceVector"></param>
        /// <returns></returns>
        public static float GetVectorAngle(Vector2 vector, Vector2 referenceVector)
        {
            // Vector2 referenceVector = new Vector2(0, 1);
            float dotProduct = Vector2.Dot(vector, referenceVector);
            float cosTheta = dotProduct / (vector.magnitude * referenceVector.magnitude);
            float angleInDegrees = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
            return vector.x < 0 ? 360 - angleInDegrees : angleInDegrees;
        }

        /// <summary>
        /// 过滤角度，将其约束到 [0, 360)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static int FilterAngle(int angle)
        {
            if (angle is >= 360 or <= -360)
            {
                angle %= 360;
            }
            return angle < 0 ? angle + 360 : angle;
        }
    }
}
