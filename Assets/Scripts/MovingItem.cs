using UnityEngine;

public class MovingItem : MonoBehaviour
{
    public int imageSizePx;

    private Block.Type item = Block.Type.Air;
    private ItemImage image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<ItemImage>();
        image.Init(new(imageSizePx, imageSizePx), () => {});
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
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
}
