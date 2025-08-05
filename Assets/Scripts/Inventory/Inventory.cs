using UnityEngine;

public class Inventory : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool _open;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        open = false;
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
