using DG.Tweening;
using UnityEngine;
namespace GameScript.Runtime.Util
{
    public static class TweenUtil
    {

        /// <summary>
        /// 弹性缩放动画（OutElastic弹的太猛了，OutBack又太柔，所以这里用三段动画实现了一个可控的Elastic）
        /// ------------- _ ----------------- scale + ctrl
        ///            /     \
        ///          /         \
        /// -------/-------------\------- . - scale
        ///      /                 \    /
        /// ----/------------------- _ ------ scale - ctrl * 0.5
        ///    /
        ///   /                           ｜1
        ///  /                       ｜5/6
        /// /             ｜3/6
        /// ---------------------------- base scale
        /// </summary>
        /// <param name="trans">动画对象</param>
        /// <param name="duration">总时长</param>
        /// <param name="scale">目标缩放</param>
        /// <param name="ctrlScale">控制缩放，最大超出的范围，尺寸太大的话可以对应的减小该数值</param>
        /// <param name="delay">延时</param>
        /// <returns></returns>
        public static Sequence ElasticScale(Transform trans, float duration, float scale = 1f, float ctrlScale = 0.2f, float delay = 0f)
        {
            var sequence = DOTween.Sequence().SetLink(trans.gameObject);
            float stepDur = duration / 6f;
            sequence.AppendInterval(delay);
            sequence.Append(trans.DOScale(scale + ctrlScale, stepDur * 3f).SetEase(Ease.OutCubic));
            sequence.Append(trans.DOScale(scale - ctrlScale * 0.5f, stepDur * 2f).SetEase(Ease.InOutCubic));
            sequence.Append(trans.DOScale(scale, stepDur).SetEase(Ease.OutCubic));
            return sequence;
        }

        /// <summary>
        /// X方向的弹性缩放动画
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="duration"></param>
        /// <param name="scaleX"></param>
        /// <param name="ctrlScaleX"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static Sequence ElasticScaleX(Transform trans, float duration, float scaleX = 1f, float ctrlScaleX = 0.2f, float delay = 0f)
        {
            var sequence = DOTween.Sequence().SetLink(trans.gameObject);
            float stepDur = duration / 6f;
            sequence.AppendInterval(delay);
            sequence.Append(trans.DOScaleX(scaleX + ctrlScaleX, stepDur * 3f).SetEase(Ease.OutCubic));
            sequence.Append(trans.DOScaleX(scaleX - ctrlScaleX * 0.5f, stepDur * 2f).SetEase(Ease.InOutCubic));
            sequence.Append(trans.DOScaleX(scaleX, stepDur).SetEase(Ease.OutCubic));
            return sequence;
        }

        /// <summary>
        /// 弹性的缩放加旋转，一般用于图标的出现
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="duration"></param>
        /// <param name="scale"></param>
        /// <param name="ctrlScale"></param> 
        /// <param name="rotate"></param>
        /// <param name="ctrlRotate"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static Sequence ElasticScaleAndRotate(Transform trans, float duration, float scale = 1f, float ctrlScale = 0.2f, float rotate = 0f, float ctrlRotate = 10f, float delay = 0f)
        {
            var sequence = ElasticScale(trans, duration, scale, ctrlScale);
            float stepDur = duration / 6f;
            sequence.AppendInterval(delay);
            sequence.Insert(0f, trans.DORotate(new Vector3(0f, 0f, rotate + ctrlRotate), stepDur * 3f).SetEase(Ease.OutCubic));
            sequence.Insert(stepDur * 2f, trans.DORotate(new Vector3(0f, 0f, rotate - ctrlRotate * 0.5f), stepDur * 2f).SetEase(Ease.InOutCubic));
            sequence.Insert(stepDur * 3f, trans.DORotate(new Vector3(0f, 0f, rotate), stepDur).SetEase(Ease.OutCubic));
            return sequence;
        }
    }
}