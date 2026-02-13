using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 3;
    [SerializeField] private int gridHeight = 3;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float spacing = 5f;

    [Header("Grid Size Limits")]
    [SerializeField] private int minGridSize = 1;
    [SerializeField] private int maxGridWidth = 10;
    [SerializeField] private int maxGridHeight = 10;

    [Header("References")]
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private GameObject cellPrefab;

    private GridCell[,] gridCells;
    private List<GridItem> placedItems = new List<GridItem>();

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        gridCells = new GridCell[gridWidth, gridHeight];

        // Отримуємо pivot батьківського контейнера для правильного anchor дітей
        Vector2 parentPivot = gridContainer.pivot;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();

                // Anchor дочірнього елемента має відповідати pivot батька
                cellRect.anchorMin = parentPivot;
                cellRect.anchorMax = parentPivot;
                cellRect.pivot = new Vector2(0, 1); // Pivot клітинки завжди лівий верхній кут

                // Позиціонування клітинки відносно anchor point
                float posX = x * (cellSize + spacing);
                float posY = -y * (cellSize + spacing);
                cellRect.anchoredPosition = new Vector2(posX, posY);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);

                GridCell cell = cellObj.GetComponent<GridCell>();
                if (cell == null)
                {
                    cell = cellObj.AddComponent<GridCell>();
                }

                cell.Initialize(x, y);
                gridCells[x, y] = cell;
            }
        }

        // Налаштування розміру контейнера сітки
        float totalWidth = gridWidth * cellSize + (gridWidth - 1) * spacing;
        float totalHeight = gridHeight * cellSize + (gridHeight - 1) * spacing;
        gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    // Перевірка чи можна розмістити предмет
    public bool CanPlaceItem(int startX, int startY, int width, int height, GridItem itemToIgnore = null)
    {
        // Перевірка меж сітки
        if (startX < 0 || startY < 0 || startX + width > gridWidth || startY + height > gridHeight)
        {
            return false;
        }

        // Перевірка чи клітинки вільні
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (gridCells[x, y].IsOccupied && gridCells[x, y].OccupyingItem != itemToIgnore)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Розміщення предмета на сітці
    public bool PlaceItem(GridItem item, int startX, int startY)
    {
        if (!CanPlaceItem(startX, startY, item.Width, item.Height, item))
        {
            return false;
        }

        // Видалення попереднього розміщення
        RemoveItem(item);

        // Заняття клітинок
        for (int x = startX; x < startX + item.Width; x++)
        {
            for (int y = startY; y < startY + item.Height; y++)
            {
                gridCells[x, y].SetOccupied(item);
            }
        }

        // Позиціонування предмета
        RectTransform itemRect = item.GetComponent<RectTransform>();

        // Anchor предмета має відповідати pivot GridContainer
        Vector2 parentPivot = gridContainer.pivot;
        itemRect.anchorMin = parentPivot;
        itemRect.anchorMax = parentPivot;
        itemRect.pivot = new Vector2(0, 1); // Pivot предмета завжди лівий верхній кут

        float posX = startX * (cellSize + spacing);
        float posY = -startY * (cellSize + spacing);
        itemRect.anchoredPosition = new Vector2(posX, posY);

        item.SetGridPosition(startX, startY);

        if (!placedItems.Contains(item))
        {
            placedItems.Add(item);
        }

        return true;
    }

    // Видалення предмета з сітки
    public void RemoveItem(GridItem item)
    {
        if (item.GridX == -1 || item.GridY == -1)
        {
            return;
        }

        for (int x = item.GridX; x < item.GridX + item.Width; x++)
        {
            for (int y = item.GridY; y < item.GridY + item.Height; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    gridCells[x, y].SetFree();
                }
            }
        }

        item.SetGridPosition(-1, -1);
    }

    // Отримання позиції клітинки за світовими координатами
    public bool GetGridPosition(Vector2 worldPosition, out int gridX, out int gridY)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainer, worldPosition, null, out localPos);

        gridX = Mathf.FloorToInt(localPos.x / (cellSize + spacing));
        gridY = Mathf.FloorToInt(-localPos.y / (cellSize + spacing));

        return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
    }

    // Підсвітка клітинок при наведенні
    public void HighlightCells(int startX, int startY, int width, int height, bool canPlace)
    {
        ClearHighlight();

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    gridCells[x, y].Highlight(canPlace);
                }
            }
        }
    }

    public void ClearHighlight()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridCells[x, y].ClearHighlight();
            }
        }
    }

    public float CellSize => cellSize;
    public float Spacing => spacing;
    public RectTransform GridContainer => gridContainer;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    // === МЕТОДИ РОЗШИРЕННЯ СІТКИ ===

    /// <summary>
    /// Додає один стовпець справа
    /// </summary>
    public bool AddColumn()
    {
        return ResizeGrid(gridWidth + 1, gridHeight);
    }

    /// <summary>
    /// Додає один рядок знизу
    /// </summary>
    public bool AddRow()
    {
        return ResizeGrid(gridWidth, gridHeight + 1);
    }

    /// <summary>
    /// Видаляє один стовпець справа (якщо він порожній)
    /// </summary>
    public bool RemoveColumn()
    {
        if (gridWidth <= minGridSize) return false;

        // Перевірка чи останній стовпець порожній
        for (int y = 0; y < gridHeight; y++)
        {
            if (gridCells[gridWidth - 1, y].IsOccupied)
            {
                Debug.LogWarning("Неможливо видалити стовпець - він зайнятий предметами");
                return false;
            }
        }

        return ResizeGrid(gridWidth - 1, gridHeight);
    }

    /// <summary>
    /// Видаляє один рядок знизу (якщо він порожній)
    /// </summary>
    public bool RemoveRow()
    {
        if (gridHeight <= minGridSize) return false;

        // Перевірка чи останній рядок порожній
        for (int x = 0; x < gridWidth; x++)
        {
            if (gridCells[x, gridHeight - 1].IsOccupied)
            {
                Debug.LogWarning("Неможливо видалити рядок - він зайнятий предметами");
                return false;
            }
        }

        return ResizeGrid(gridWidth, gridHeight - 1);
    }

    /// <summary>
    /// Змінює розмір сітки до вказаних розмірів
    /// </summary>
    public bool ResizeGrid(int newWidth, int newHeight)
    {
        // Перевірка обмежень
        if (newWidth < minGridSize || newHeight < minGridSize)
        {
            Debug.LogWarning($"Розмір сітки не може бути меншим за {minGridSize}");
            return false;
        }

        if (newWidth > maxGridWidth || newHeight > maxGridHeight)
        {
            Debug.LogWarning($"Розмір сітки не може перевищувати {maxGridWidth}x{maxGridHeight}");
            return false;
        }

        // Якщо зменшуємо - перевіряємо чи немає предметів в областях що видаляються
        if (newWidth < gridWidth || newHeight < gridHeight)
        {
            for (int x = newWidth; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y].IsOccupied)
                    {
                        Debug.LogWarning("Неможливо зменшити сітку - є предмети в області що видаляється");
                        return false;
                    }
                }
            }

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = newHeight; y < gridHeight; y++)
                {
                    if (gridCells[x, y].IsOccupied)
                    {
                        Debug.LogWarning("Неможливо зменшити сітку - є предмети в області що видаляється");
                        return false;
                    }
                }
            }
        }

        int oldWidth = gridWidth;
        int oldHeight = gridHeight;

        gridWidth = newWidth;
        gridHeight = newHeight;

        // Створення нового масиву клітинок
        GridCell[,] newGridCells = new GridCell[gridWidth, gridHeight];

        // Копіювання старих клітинок
        for (int x = 0; x < Mathf.Min(oldWidth, newWidth); x++)
        {
            for (int y = 0; y < Mathf.Min(oldHeight, newHeight); y++)
            {
                newGridCells[x, y] = gridCells[x, y];
            }
        }

        // Видалення клітинок що більше не потрібні
        if (newWidth < oldWidth || newHeight < oldHeight)
        {
            for (int x = 0; x < oldWidth; x++)
            {
                for (int y = 0; y < oldHeight; y++)
                {
                    if (x >= newWidth || y >= newHeight)
                    {
                        if (gridCells[x, y] != null)
                        {
                            Destroy(gridCells[x, y].gameObject);
                        }
                    }
                }
            }
        }

        // Створення нових клітинок
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (newGridCells[x, y] == null)
                {
                    GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                    RectTransform cellRect = cellObj.GetComponent<RectTransform>();

                    // Anchor дочірнього елемента має відповідати pivot батька
                    Vector2 parentPivot = gridContainer.pivot;
                    cellRect.anchorMin = parentPivot;
                    cellRect.anchorMax = parentPivot;
                    cellRect.pivot = new Vector2(0, 1);

                    float posX = x * (cellSize + spacing);
                    float posY = -y * (cellSize + spacing);
                    cellRect.anchoredPosition = new Vector2(posX, posY);
                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);

                    GridCell cell = cellObj.GetComponent<GridCell>();
                    if (cell == null)
                    {
                        cell = cellObj.AddComponent<GridCell>();
                    }

                    cell.Initialize(x, y);
                    newGridCells[x, y] = cell;
                }
            }
        }

        gridCells = newGridCells;

        // Оновлення розміру контейнера
        float totalWidth = gridWidth * cellSize + (gridWidth - 1) * spacing;
        float totalHeight = gridHeight * cellSize + (gridHeight - 1) * spacing;
        gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

        return true;
    }

    /// <summary>
    /// Додає кілька стовпців справа
    /// </summary>
    public bool AddColumns(int count)
    {
        return ResizeGrid(gridWidth + count, gridHeight);
    }

    /// <summary>
    /// Додає кілька рядків знизу
    /// </summary>
    public bool AddRows(int count)
    {
        return ResizeGrid(gridWidth, gridHeight + count);
    }

    /// <summary>
    /// Встановлює конкретний розмір сітки
    /// </summary>
    public bool SetGridSize(int width, int height)
    {
        return ResizeGrid(width, height);
    }

    /// <summary>
    /// Очищає всю сітку від предметів
    /// </summary>
    public void ClearGrid()
    {
        List<GridItem> itemsToRemove = new List<GridItem>(placedItems);
        foreach (var item in itemsToRemove)
        {
            RemoveItem(item);
        }
        placedItems.Clear();
    }
}