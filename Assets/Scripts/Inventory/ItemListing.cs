using TMPro;
using UnityEngine;

public class ItemListing : MonoBehaviour
{
    public int panelSizePx;

    private ItemImage image;
    private MovingItem movingItem;
    private Block.Type _type;

    void Start()
    {
        
    }

    public void Init(MovingItem movingItem, Block.Type type, string visualName, int index)
    {
        image = transform.Find("Item Image").GetComponent<ItemImage>();
        image.Init(new Vector2(30, 30), OnButtonClick);

        this.movingItem = movingItem;
        this.type = type;

        transform.Find("Text").GetComponent<TMP_Text>().text = visualName;

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new(rectTransform.sizeDelta.x / 2, -(index + 0.5f) * rectTransform.sizeDelta.y);
    }

    void Update()
    {

    }

    public Block.Type type
    {
        get => _type;
        set
        {
            _type = value;
            image.type = value;
        }
    }
    
    private void OnButtonClick()
    {
        movingItem.type = type;
    }
}
