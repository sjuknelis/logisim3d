using UnityEngine;

public class Hotbar : MonoBehaviour
{
    private readonly int panelCount = 12;

    private readonly KeyCode[] hotbarCodes = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
        KeyCode.Alpha0, KeyCode.Minus, KeyCode.Equals
    };

    private readonly Block.Type[] defaultHotbarItems = new Block.Type[]
    {
        Block.Type.Air, Block.Type.Air, Block.Type.Air,
        Block.Type.Stone, Block.Type.PowerSource, Block.Type.Wire,
        Block.Type.Air, Block.Type.Air, Block.Type.Air,
        Block.Type.Air, Block.Type.Air, Block.Type.Air
    };

    public GameObject panelPrefab, movingItemObj;
    public int panelSizePx, itemImageSizePx;

    private HotbarPanel[] panels;
    private int selectedPanelIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var movingItem = movingItemObj.GetComponent<MovingItem>();

        panels = new HotbarPanel[hotbarCodes.Length];
        for (int i = 0; i < panelCount; i++)
        {
            panels[i] = Instantiate(panelPrefab, transform).GetComponent<HotbarPanel>();
            panels[i].Init(
                (int)((i - panels.Length / 2 + 0.5f) * panelSizePx),
                defaultHotbarItems[i],
                movingItem,
                panelSizePx,
                itemImageSizePx
                );
        }

        UpdateSelectedPanel(0);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < hotbarCodes.Length; i++)
        {
            if (Input.GetKeyDown(hotbarCodes[i]))
            {
                UpdateSelectedPanel(i);
                break;
            }
        }
    }

    public Block.Type GetSelectedBlockType()
    {
        return panels[selectedPanelIndex].GetItem();
    }

    void UpdateSelectedPanel(int nextIndex)
    {
        panels[selectedPanelIndex].SetSelected(false);
        panels[nextIndex].SetSelected(true);
        selectedPanelIndex = nextIndex;
    }
}