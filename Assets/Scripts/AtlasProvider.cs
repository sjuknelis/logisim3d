using System.IO;
using UnityEngine;

public class AtlasProvider : MonoBehaviour
{
    public static Texture2D texture;
    public static Material material;
    public static int tileSizePx;

    public string imagePath;
    public int tileSizePxP;

    void Awake()
    {
        tileSizePx = tileSizePxP;

        byte[] atlasBytes = File.ReadAllBytes(imagePath);
        texture = new(2, 2);
        texture.LoadImage(atlasBytes);

        material = new(Shader.Find("Unlit/Texture"))
        {
            mainTexture = texture
        };
    }
}
