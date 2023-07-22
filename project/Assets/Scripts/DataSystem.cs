using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSystem : MonoBehaviour
{
    [SerializeField] string spaceName;

    string dataPath;
    string fullPath;

    static DataSystem instance;

    public static Sprite LoadSprite(string name)
    {
        Texture2D texture = LoadTexture(name);

        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector3.one * 0.5f);
            return sprite;
        }
        return null;
    }

    public static Texture2D LoadTexture(string name)
    {
        string pathName = instance.fullPath + name;
        if (File.Exists(pathName))
        {
            int tempWidth = 128, tempHeight = 128;
            byte[] imageBytes = File.ReadAllBytes(pathName);
            Texture2D texture = new Texture2D(tempWidth, tempHeight, TextureFormat.RGBA32, false);
            texture.LoadImage(imageBytes);
            texture.filterMode = FilterMode.Point;
            return texture;
        }
        return null;
    }

    void SetupSpace()
    {
        dataPath = Application.persistentDataPath + Path.DirectorySeparatorChar;
        fullPath = dataPath + spaceName;
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        fullPath += Path.DirectorySeparatorChar;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            SetupSpace();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
