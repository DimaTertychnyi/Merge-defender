using UnityEngine;
using UnityEngine.UI;

public class GridTestExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemsContainer;

    [Header("Test Items")]
    [SerializeField]
    private Color[] itemColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    void Start()
    {
        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
        }

        // Створення тестових предметів
        CreateTestItem(1, 1, itemColors[0]); // 1x1
        CreateTestItem(2, 1, itemColors[1]); // 2x1
        CreateTestItem(1, 2, itemColors[2]); // 1x2
        CreateTestItem(2, 2, itemColors[3]); // 2x2
    }

    void CreateTestItem(int width, int height, Color color)
    {
        GameObject itemObj = Instantiate(itemPrefab, itemsContainer);
        GridItem item = itemObj.GetComponent<GridItem>();

        if (item == null)
        {
            item = itemObj.AddComponent<GridItem>();
        }

        item.Initialize(gridSystem, width, height);
        item.SetColor(color);

        // Додавання тексту з розміром
        Text sizeText = itemObj.GetComponentInChildren<Text>();
        if (sizeText != null)
        {
            sizeText.text = $"{width}x{height}";
        }
    }
}