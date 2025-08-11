using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject itemListingPrefab;
    public MovingItem movingItem;

    private readonly (Block.Type type, string visualName, string description)[] typeEntries = new[]
    {
        (Block.Type.Stone, "Stone", "A basic building block."),
        (Block.Type.PowerSource, "Power Source", "Generates power for circuits."),
        (Block.Type.Wire, "Wire", "Conducts power between components.")
    };

    private CanvasGroup canvasGroup;
    private bool _open;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        open = false;

        for (int i = 0; i < typeEntries.Length; i++)
        {
            var (type, visualName, description) = typeEntries[i];
            var itemListing = Instantiate(itemListingPrefab, transform);
            itemListing.GetComponent<ItemListing>().Init(movingItem, type, visualName, i);
        }
    }

    void Update()
    {

    }
    
    public bool open
    {
        get => _open;
        set
        {
            _open = value;
            canvasGroup.alpha = value ? 1f : 0f;
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
    }
}
