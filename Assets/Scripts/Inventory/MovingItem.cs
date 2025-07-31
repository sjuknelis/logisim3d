using UnityEngine;

public class MovingItem : MonoBehaviour
{
    public int imageSizePx;

    private Block.Type _type = Block.Type.Air;
    private ItemImage image;

    void Start()
    {
        image = GetComponent<ItemImage>();
        image.Init(new(imageSizePx, imageSizePx), () => {});
    }

    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
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
}
