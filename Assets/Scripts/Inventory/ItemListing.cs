using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemListing : MonoBehaviour
{
    public int panelSizePx;

    private ItemImage image;
    private MovingItem movingItem;
    private Block.Type _type;

    void Start()
    {

    }

    public void Init(MovingItem movingItem, Block.Type type, string visualName, int index, Action onListingButtonClick)
    {
        image = transform.Find("Item Image").GetComponent<ItemImage>();
        image.Init(new Vector2(30, 30), OnItemButtonClick);

        this.movingItem = movingItem;
        this.type = type;

        transform.Find("Text").GetComponent<TMP_Text>().text = visualName;

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new(rectTransform.sizeDelta.x / 2, -(index + 0.5f) * rectTransform.sizeDelta.y);

        UnityAction listingAction = new(onListingButtonClick);
        transform.Find("Button").GetComponent<Button>().onClick.AddListener(listingAction);
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

    private void OnItemButtonClick()
    {
        movingItem.type = type;
    }
}
