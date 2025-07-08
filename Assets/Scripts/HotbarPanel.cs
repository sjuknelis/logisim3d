using UnityEngine;
using UnityEngine.UI;

public class HotbarPanel : MonoBehaviour
{
    private MovingItem movingItem;
    private int panelSizePx;

    private ItemImage image;
    private RectTransform rectTransform;

    private Block.Type item;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Init(int xPosPx, Block.Type defaultItem, MovingItem movingItem, int panelSizePx, int itemImageSizePx)
    {
        this.movingItem = movingItem;
        this.panelSizePx = panelSizePx;

        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new(
            xPosPx,
            -Screen.height / 2 + panelSizePx / 2
            );
        rectTransform.sizeDelta = new(panelSizePx, panelSizePx);

        image = rectTransform.Find("Item Image").GetComponent<ItemImage>();
        image.Init(new(itemImageSizePx, itemImageSizePx), OnButtonClick);

        SetItem(defaultItem);
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.anchoredPosition = new(
            rectTransform.anchoredPosition.x,
            -Screen.height / 2 + panelSizePx / 2
            );
    }

    public void SetItem(Block.Type item)
    {
        image.SetItem(item);
        this.item = item;
    }

    public Block.Type GetItem()
    {
        return item;
    }

    public void SetSelected(bool isSelected)
    {
        Color color = isSelected ? new(1f, 0f, 0f, 0.5f) : new(1f, 1f, 1f, 0.5f);
        GetComponent<Image>().color = color;
    }

    void OnButtonClick()
    {
        if (movingItem.GetItem() == Block.Type.Air)
        {
            movingItem.SetItem(item);
            SetItem(Block.Type.Air);
        }
        else
        {
            SetItem(movingItem.GetItem());
            movingItem.SetItem(Block.Type.Air);
        }
    }
}
