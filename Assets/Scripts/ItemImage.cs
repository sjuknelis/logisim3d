using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemImage : MonoBehaviour
{
    public bool useButton;

    private Image imageComp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Init(Vector2 size, Action onButtonClick)
    {
        GetComponent<RectTransform>().sizeDelta = size;

        imageComp = GetComponent<Image>();
        imageComp.color = new(0f, 0f, 0f, 0f);
        
        if (useButton)
        {
            var buttonObj = transform.Find("Button").gameObject;

            buttonObj.GetComponent<RectTransform>().sizeDelta = size;

            UnityAction action = new(onButtonClick);
            buttonObj.GetComponent<Button>().onClick.AddListener(action);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetItem(Block.Type item)
    {
        if (item == Block.Type.Air)
        {
            imageComp.color = new(0f, 0f, 0f, 0f);
        }
        else
        {
            var offset = BlockProps.textureOffsets[BlockProps.names[item]][Direction.Up.GetFaceIndex()];
            var resolution = AtlasProvider.atlasResolution;
            Rect rect = new(offset.x * resolution, offset.y * resolution, resolution, resolution);
            var sprite = Sprite.Create(AtlasProvider.atlasTexture, rect, new(0.5f, 0.5f));
            imageComp.sprite = sprite;
            imageComp.color = new(1f, 1f, 1f, 1f);
        }
    }
}
