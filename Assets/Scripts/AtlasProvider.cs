using UnityEngine;

public class AtlasProvider : MonoBehaviour
{
    public static Texture2D atlasTexture;
    public static int atlasGridSize, atlasResolution;
    public static Material atlasMaterial;

    public Texture2D atlasTextureP;
    public int atlasGridSizeP, atlasResolutionP;

    void Awake()
    {
        atlasTexture = atlasTextureP;
        atlasGridSize = atlasGridSizeP;
        atlasResolution = atlasResolutionP;

        atlasMaterial = new(Shader.Find("Unlit/Texture"))
        {
            mainTexture = atlasTexture
        };
    }
}
