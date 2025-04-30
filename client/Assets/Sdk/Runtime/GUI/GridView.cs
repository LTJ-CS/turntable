using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*
 * 作者: 渡鸦
 * 描述: 复用网格列表
 */

public class GridView : MonoBehaviour
{
    [SerializeField] private float _itemScale = 1;
    [SerializeField] private RectOffset _padding;
    [SerializeField] private Vector2 _spacing;

    public Action<int, MonoBehaviour> onGridItem;

    private MonoBehaviour _prefab;
    private ScrollRect _scrollRect;
    private int _itemCount = -1;
    private float _itemWidth;
    private float _itemHeight;
    private int _viewRow;
    private int _viewColumn;

    private List<MonoBehaviour> _itemPool = new List<MonoBehaviour>();

    public void Init<T>(T prefab, int count, int index = 0) where T : MonoBehaviour
    {
        _prefab = prefab;

        _scrollRect = gameObject.GetComponent<ScrollRect>();
        _scrollRect.content.anchoredPosition = Vector2.zero;

        var rt = _prefab.transform as RectTransform;
        var rect = rt.rect;
        _itemWidth = rect.width * _itemScale;
        _itemHeight = rect.height * _itemScale;

        _viewColumn = Mathf.Max(1,
                                Mathf.FloorToInt((_scrollRect.viewport.rect.width - _padding.horizontal + _spacing.x) /
                                                 (_itemWidth + _spacing.x)));
        _viewRow = Mathf.Max(1,
                             Mathf.CeilToInt((_scrollRect.viewport.rect.height + _spacing.y) /
                                             (_itemHeight + _spacing.y)));

        _scrollRect.onValueChanged.AddListener(OnValueChanged);

        CalcContentSize(count);

        if (count > 0)
        {
            index = Math.Clamp(index, 0, count - 1);
            if (count <= _viewRow)
            {
                index = 0;
            }
        }


        float posY = CalcIndexPosition(index);
        _scrollRect.content.anchoredPosition = new Vector2(0, posY);

        StartCoroutine(Init(index));
    }

    private IEnumerator Init(int index)
    {
        yield return StartCoroutine(CreateItems(index));
        OnValueChanged(default);
    }

    /// <summary>
    /// 更新数量
    /// </summary>
    public void UpdateCount(int count)
    {
        this.DOKill();
        _scrollRect.StopMovement();
        _scrollRect.content.anchoredPosition = Vector2.zero;

        if (_itemCount != count)
        {
            _itemCount = count;
            CalcContentSize(count);
        }

        ResetView();
    }

    private void CalcContentSize(int count)
    {
        float height = _padding.vertical +
                       (_itemHeight + _spacing.y) * Mathf.CeilToInt((float)count / _viewColumn) -
                       _spacing.y;
        height = Mathf.Max(height, 0);

        _itemCount = count;
        _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, height);
    }

    /// <summary>
    /// 刷新当前视窗内元素
    /// </summary>
    public void RefreshView()
    {
        foreach (var variable in _itemPool)
        {
            UpdateItem(variable);
        }
    }

    private IEnumerator CreateItems(int initIndex)
    {
        for (int row = 0; row < _viewRow + 1; row++)
        {
            for (int column = 0; column < _viewColumn; column++)
            {
                int index = row * _viewColumn + column;
                if (_itemPool.Count <= index)
                {
                    var script = Instantiate(_prefab, _scrollRect.content);
                    RectTransform rt = script.transform as RectTransform;
                    rt.localScale = new Vector3(_itemScale, _itemScale);
                    rt.anchorMin = Vector2.up;
                    rt.anchorMax = Vector2.up;

                    int i = initIndex == 0 ? row : initIndex + row;
                    float x = _padding.left + _itemWidth / 2 + (_itemWidth + _spacing.x) * column;
                    float y = -_padding.top - _itemHeight / 2 - _scrollRect.content.anchoredPosition.y -
                              (_itemHeight + _spacing.y) * i;
                    rt.anchoredPosition = new Vector2(x, y);

                    _itemPool.Add(script);
                    UpdateItem(script);

                    // 每帧创建5个
                    if ((index + 1) % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    private void UpdateItem(MonoBehaviour script)
    {
        var anchoredPosition = ((RectTransform)script.transform).anchoredPosition;
        int column = Mathf.RoundToInt((anchoredPosition.x - _padding.left - _itemWidth / 2) / (_itemWidth + _spacing.x));
        int row = Mathf.RoundToInt((-_padding.top - _itemHeight / 2 - anchoredPosition.y) / (_itemHeight + _spacing.y));
        int index = row * _viewColumn + column;
        if (index >= _itemCount)
        {
            script.gameObject.SetActive(false);
        }
        else
        {
            if (script.gameObject.activeSelf == false)
            {
                script.gameObject.SetActive(true);
            }

            onGridItem?.Invoke(index, script);
        }
    }

    private void ResetView()
    {
        for (int row = 0; row < _viewRow + 1; row++)
        {
            for (int column = 0; column < _viewColumn; column++)
            {
                int index = row * _viewColumn + column;
                if (index < _itemPool.Count)
                {
                    var script = _itemPool[index];
                    RectTransform rt = script.transform as RectTransform;
                    rt.anchorMin = Vector2.up;
                    rt.anchorMax = Vector2.up;

                    float x = _padding.left + _itemWidth / 2 + (_itemWidth + _spacing.x) * column;
                    float y = -_padding.top - _itemHeight / 2 - (_itemHeight + _spacing.y) * row -
                              _scrollRect.content.anchoredPosition.y;
                    rt.anchoredPosition = new Vector2(x, y);

                    UpdateItem(script);
                }
            }
        }
    }

    private void OnValueChanged(Vector2 value)
    {
        foreach (var variable in _itemPool)
        {
            var rt = variable.transform as RectTransform;
            float posY = _scrollRect.content.anchoredPosition.y + rt.anchoredPosition.y;
            float maxY = _itemHeight / 2;
            float minY = -((_viewRow + 1) * (_itemHeight + _spacing.y)) + _itemHeight / 2;
            if (posY > maxY)
            {
                float y = rt.anchoredPosition.y - (_viewRow + 1) * (_itemHeight + _spacing.y);
                if (y > -_scrollRect.content.rect.height)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
                    UpdateItem(variable);
                }
            }
            else if (posY < minY)
            {
                float y = rt.anchoredPosition.y + (_viewRow + 1) * (_itemHeight + _spacing.y);
                if (y < 0)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
                    UpdateItem(variable);
                }
            }
        }
    }

    private float CalcIndexPosition(int index)
    {
        float posY = _padding.top + (((RectTransform)_prefab.transform).sizeDelta.y + _spacing.y) * index;
        posY -= _spacing.y;
        float max = _scrollRect.content.sizeDelta.y - _scrollRect.viewport.rect.height;
        max = Mathf.Max(0, max);
        posY = Mathf.Clamp(posY, 0, max);
        return posY;
    }

    public void ScrollToIndex(int index, float duration = 0.5f)
    {
        float posY = CalcIndexPosition(index);
        this.DOKill();
        _scrollRect.StopMovement();
        DOTween.To(() => _scrollRect.content.anchoredPosition,
                   (value) => { _scrollRect.content.anchoredPosition = value; },
                   new Vector2(0, posY), duration)
               .SetEase(Ease.Linear)
               .SetTarget(this);
    }
}