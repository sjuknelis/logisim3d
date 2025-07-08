using UnityEngine;

public class AtlasProvider : MonoBehaviour
{
    public static Texture2D atlasTexture;
    public static int atlasGridSize, atlasResolution;

    public Texture2D atlasTextureP;
    public int atlasGridSizeP, atlasResolutionP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        atlasTexture = atlasTextureP;
        atlasGridSize = atlasGridSizeP;
        atlasResolution = atlasResolutionP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
