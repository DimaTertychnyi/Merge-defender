using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color highlightValidColor = new Color(0.5f, 1f, 0.5f, 0.7f);
    [SerializeField] private Color highlightInvalidColor = new Color(1f, 0.5f, 0.5f, 0.7f);

    private int x;
    private int y;
    private bool isOccupied;
    private GridItem occupyingItem;

    void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
        }

        backgroundImage.color = normalColor;
    }

    public void Initialize(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
        gameObject.name = $"Cell_{gridX}_{gridY}";
    }

    public void SetOccupied(GridItem item)
    {
        isOccupied = true;
        occupyingItem = item;
    }

    public void SetFree()
    {
        isOccupied = false;
        occupyingItem = null;
    }

    public void Highlight(bool isValid)
    {
        backgroundImage.color = isValid ? highlightValidColor : highlightInvalidColor;
    }

    public void ClearHighlight()
    {
        backgroundImage.color = normalColor;
    }

    public bool IsOccupied => isOccupied;
    public GridItem OccupyingItem => occupyingItem;
    public int X => x;
    public int Y => y;
}