using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject canvas, panelPrefab;
    public int squareSizePx, squareGapPx;

    public Texture2D atlas;
    public int atlasGridSize, atlasResolution;

    private readonly KeyCode[] hotbarCodes = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
        KeyCode.Minus,
        KeyCode.Equals
    };

    private GameObject[] panels;
    private int selectedPanel = 0;

    private Block.Type[] hotbarItems = new Block.Type[]
    {
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Stone,
        Block.Type.PowerSource,
        Block.Type.Wire,
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Air,
        Block.Type.Air
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panels = new GameObject[hotbarCodes.Length];
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i] = Instantiate(panelPrefab, canvas.transform);

            var panelTransform = panels[i].GetComponent<RectTransform>();
            panelTransform.anchoredPosition = new(
                (i - panels.Length / 2 + 0.5f) * squareSizePx,
                -Screen.height / 2 + squareSizePx / 2
                );
            panelTransform.sizeDelta = new(squareSizePx, squareSizePx);

            var imageObj = panels[i].transform.Find("Item Image");
            imageObj.GetComponent<RectTransform>().sizeDelta = new(squareSizePx - squareGapPx * 2, squareSizePx - squareGapPx * 2);
            imageObj.GetComponent<Image>().color = new(0f, 0f, 0f, 0f);
        }

        UpdateSelected(0);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var panel in panels)
            RepositionPanel(panel);

        for (int i = 0; i < hotbarItems.Length; i++)
            SetPanelImage(panels[i], hotbarItems[i]);

        for (int i = 0; i < hotbarCodes.Length; i++)
        {
            if (Input.GetKeyDown(hotbarCodes[i]))
            {
                UpdateSelected(i);
                break;
            }
        }
    }

    public Block.Type GetSelectedBlockType()
    {
        return hotbarItems[selectedPanel];
    }

    void SetPanelImage(GameObject panel, Block.Type type)
    {
        var image = panel.transform.Find("Item Image").GetComponent<Image>();

        if (type == Block.Type.Air)
        {
            image.color = new(0f, 0f, 0f, 0f);
        }
        else
        {
            var offset = BlockProps.textureOffsets[BlockProps.names[type]][Direction.Up.GetFaceIndex()];
            Rect rect = new(offset.x * atlasResolution, offset.y * atlasResolution, atlasResolution, atlasResolution);
            var sprite = Sprite.Create(atlas, rect, new(0.5f, 0.5f));
            image.sprite = sprite;
            image.color = new(1f, 1f, 1f, 1f);
        }
    }

    void RepositionPanel(GameObject panel)
    {
        var panelTransform = panel.GetComponent<RectTransform>();
        panelTransform.anchoredPosition = new(
            panelTransform.anchoredPosition.x,
            -Screen.height / 2 + squareSizePx / 2
            );
    }

    void UpdateSelected(int nextSelected)
    {
        panels[selectedPanel].GetComponent<Image>().color = new(1f, 1f, 1f, 0.5f);
        panels[nextSelected].GetComponent<Image>().color = new(1f, 0f, 0f, 0.5f);
        selectedPanel = nextSelected;
    }
}