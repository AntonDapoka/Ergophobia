using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenteredGridLayoutScript : MonoBehaviour
{
    [SerializeField] private int columns = 3;
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);

    public int Columns
    {
        get => columns;
        set { columns = Mathf.Max(1, value); UpdateLayout(); }
    }

    public Vector2 Spacing
    {
        get => spacing;
        set { spacing = value; UpdateLayout(); }
    }

    private void OnTransformChildrenChanged()
    {
        UpdateLayout();
    }

    private void OnValidate()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        if (columns < 1) columns = 1;
        int childCount = transform.childCount;
        if (childCount == 0) return;

        // Получаем размеры дочерних объектов
        var childSizes = new Vector2[childCount];
        for (int i = 0; i < childCount; i++)
        {
            RectTransform rt = transform.GetChild(i) as RectTransform;
            if (rt != null)
                childSizes[i] = rt.sizeDelta;
            else
                childSizes[i] = Vector2.zero;
        }

        // Рассчитываем количество строк
        int rows = Mathf.CeilToInt((float)childCount / columns);

        // Находим максимальные размеры ячеек
        float maxCellWidth = 0f;
        float maxCellHeight = 0f;
        for (int i = 0; i < childCount; i++)
        {
            maxCellWidth = Mathf.Max(maxCellWidth, childSizes[i].x);
            maxCellHeight = Mathf.Max(maxCellHeight, childSizes[i].y);
        }

        // Размер всей сетки
        float totalWidth = columns * maxCellWidth + (columns - 1) * spacing.x;
        float totalHeight = rows * maxCellHeight + (rows - 1) * spacing.y;

        // Смещение, чтобы сетка была центрирована
        Vector2 startOffset = new Vector2(-totalWidth / 2f + maxCellWidth / 2f,
                                          totalHeight / 2f - maxCellHeight / 2f);

        for (int i = 0; i < childCount; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector2 pos = new Vector2(
                startOffset.x + col * (maxCellWidth + spacing.x),
                startOffset.y - row * (maxCellHeight + spacing.y)
            );

            var child = transform.GetChild(i);
            if (child is RectTransform rect)
                rect.anchoredPosition = pos;
            else
                child.localPosition = pos;
        }
    }
}