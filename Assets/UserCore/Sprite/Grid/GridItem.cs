using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;
    [SerializeField] private Image itemImage;

    [Header("Drag Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private float dragAlpha = 0.6f;

    private GridSystem gridSystem;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private int gridX = -1;
    private int gridY = -1;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
            if (itemImage == null)
            {
                itemImage = gameObject.AddComponent<Image>();
            }
        }

        // Пошук Canvas у батьків
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void Initialize(GridSystem grid, int itemWidth, int itemHeight)
    {
        gridSystem = grid;
        width = itemWidth;
        height = itemHeight;

        // КРИТИЧНО: Anchor має відповідати pivot батьківського GridContainer
        Vector2 parentPivot = gridSystem.GridContainer.pivot;
        rectTransform.anchorMin = parentPivot;
        rectTransform.anchorMax = parentPivot;
        rectTransform.pivot = new Vector2(0, 1); // Pivot предмета завжди лівий верхній кут

        // Встановлення розміру предмета
        // Формула: (кількість клітинок * розмір клітинки) + (кількість проміжків * spacing)
        // Для предмета 2x2: (2 * cellSize) + (1 * spacing) = покриває 2 клітинки + 1 проміжок між ними
        float totalWidth = (width * gridSystem.CellSize) + ((width - 1) * gridSystem.Spacing);
        float totalHeight = (height * gridSystem.CellSize) + ((height - 1) * gridSystem.Spacing);

        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        // Переконуємось що Image компонент правильно налаштований
        if (itemImage != null)
        {
            itemImage.type = Image.Type.Simple;
        }

        Debug.Log($"GridItem ініціалізовано: {width}x{height}, розмір: {totalWidth}x{totalHeight}, anchor: {parentPivot}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Видалення з сітки під час перетягування
        if (gridSystem != null)
        {
            gridSystem.RemoveItem(this);
        }

        // Переміщення на верхній рівень для правильного відображення
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Переміщення предмета за курсором
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Показ підказки куди можна розмістити
        if (gridSystem != null)
        {
            int gridX, gridY;
            if (gridSystem.GetGridPosition(eventData.position, out gridX, out gridY))
            {
                bool canPlace = gridSystem.CanPlaceItem(gridX, gridY, width, height);
                gridSystem.HighlightCells(gridX, gridY, width, height, canPlace);
            }
            else
            {
                gridSystem.ClearHighlight();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (gridSystem != null)
        {
            gridSystem.ClearHighlight();

            // Спроба розмістити предмет
            int gridX, gridY;
            if (gridSystem.GetGridPosition(eventData.position, out gridX, out gridY))
            {
                if (gridSystem.PlaceItem(this, gridX, gridY))
                {
                    // Успішно розміщено
                    transform.SetParent(gridSystem.GridContainer);
                    return;
                }
            }

            // Повернення на початкову позицію якщо не вдалося розмістити
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;

            // Спроба повернути на попередню позицію в сітці
            if (this.gridX != -1 && this.gridY != -1)
            {
                gridSystem.PlaceItem(this, this.gridX, this.gridY);
            }
        }
    }

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public void SetColor(Color color)
    {
        if (itemImage != null)
        {
            itemImage.color = color;
        }
    }

    public int Width => width;
    public int Height => height;
    public int GridX => gridX;
    public int GridY => gridY;
}