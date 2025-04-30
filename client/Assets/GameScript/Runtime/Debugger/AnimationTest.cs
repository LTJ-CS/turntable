using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private RectTransform m_Head1;

    private Vector3 _head1Scale = new Vector3();
    private Vector2 _head1V2    = new Vector2();

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Test1()
    {
        // 停止上一个动画
        DOTween.Kill(m_Head1.gameObject);
        // 初始化属性
        SetHeadScaleRotation(new Vector4(0, 0));
        // 动画
        var durScale = 1f;

        DOTween.Sequence()
               .SetLink(m_Head1.gameObject)
               //
               .Append(m_Head1.DOScale(1.5f, 0.2f * durScale))
               .Join(m_Head1.DORotate(new Vector3(0, 0, -60f), 0.2f * durScale))
               //
               .Append(m_Head1.DOScale(1f, 0.2f * durScale))
               .Join(m_Head1.DORotate(new Vector3(0, 0, 30f), 0.2f * durScale))
               //
               .Append(m_Head1.DOScale(1.2f, 0.2f * durScale))
               .Join(m_Head1.DORotate(new Vector3(0, 0, -20f), 0.2f * durScale))
               //
               .Append(m_Head1.DOScale(1f, 0.2f * durScale))
               .Join(m_Head1.DORotate(new Vector3(0, 0, 0f), 0.2f * durScale))
               // 
               .Play();
        // DOTween.Sequence()
        //        .SetLink(m_Head1.gameObject)
        //        .Append(
        //            DOTween
        //                .To(
        //                    GetHeadScaleRotation,
        //                    SetHeadScaleRotation,
        //                    new Vector2(1.5f, -40f),
        //                    0.2f * durScale
        //                )
        //                .SetEase(Ease.Linear)
        //                .SetLink(m_Head1.gameObject)
        //        )
        //        .Append(
        //            DOTween
        //                .To(
        //                    GetHeadScaleRotation,
        //                    SetHeadScaleRotation,
        //                    new Vector2(1f, 30f),
        //                    0.2f * durScale
        //                )
        //                .SetEase(Ease.Linear)
        //                .SetLink(m_Head1.gameObject)
        //        )
        //        .Append(
        //            DOTween
        //                .To(
        //                    GetHeadScaleRotation,
        //                    SetHeadScaleRotation,
        //                    new Vector2(1.2f, -20f),
        //                    0.2f * durScale
        //                )
        //                .SetEase(Ease.Linear)
        //                .SetLink(m_Head1.gameObject)
        //        )
        //        .Append(
        //            DOTween
        //                .To(
        //                    GetHeadScaleRotation,
        //                    SetHeadScaleRotation,
        //                    new Vector2(1f, 0f),
        //                    0.2f * durScale
        //                )
        //                .SetEase(Ease.Linear)
        //                .SetLink(m_Head1.gameObject)
        //        )
        //        .Play();
    }

    public void Test2()
    {
        // 停止上一个动画
        DOTween.Kill(m_Head1.gameObject);
        // 初始化属性
        SetHeadScaleRotation(new Vector2(1, 0));
        // 动画
        var durScale = 1f;
        DOTween.Sequence()
               .SetLink(m_Head1.gameObject)
               .Append(m_Head1.DOScale(0f, 0.5f * durScale))
               .Join(m_Head1.DORotate(new Vector3(0, 0, 270f), 0.5f * durScale, RotateMode.WorldAxisAdd))
               .Play();
        // DOTween.Sequence()
        //        .SetLink(m_Head1.gameObject)
        //        .Append(
        //            DOTween
        //                .To(
        //                    GetHeadScaleRotation,
        //                    SetHeadScaleRotation,
        //                    new Vector2(0f, 270f),
        //                    0.5f * durScale
        //                )
        //                .SetEase(Ease.Linear)
        //                .SetLink(m_Head1.gameObject)
        //        )
        //        .Play();
    }

    private Vector2 GetHeadScaleRotation()
    {
        _head1V2.x = m_Head1.localScale.x;
        _head1V2.y = m_Head1.localRotation.eulerAngles.z;
        return _head1V2;
    }

    private void SetHeadScaleRotation(Vector2 v2)
    {
        _head1Scale.x = v2.x;
        _head1Scale.y = v2.x;
        _head1Scale.z = v2.x;
        m_Head1.localScale = _head1Scale;
        m_Head1.rotation = Quaternion.Euler(0, 0, v2.y);
    }
}