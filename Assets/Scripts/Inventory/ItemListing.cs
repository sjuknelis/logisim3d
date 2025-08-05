using UnityEngine;

public class ItemListing : MonoBehaviour
{
    public MovingItem movingItem;
    public int panelSizePx;

    private Block.Type type = Block.Type.PowerSource;

    void Start()
    {
        var image = transform.Find("Item Image").GetComponent<ItemImage>();
        image.Init(new Vector2(30, 30), OnButtonClick);
        image.type = type;
    }

    void Update()
    {

    }
    
    private void OnButtonClick()
    {
        movingItem.type = type;
    }
}
