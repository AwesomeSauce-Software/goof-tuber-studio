using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public static class DataSystem
{
    public static Sprite LoadSprite(string name, string path)
    {
        string pathName = path + name;
        return LoadSpriteFromPath(pathName);
    }

    public static Sprite LoadSpriteFromPath(string path)
    {
        Texture2D texture = LoadTextureFromPath(path);

        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(0.5f, 0.0f));
            return sprite;
        }
        return null;
    }

    public static Texture2D LoadTexture(string name, string path)
    {
        string pathName = path + name;
        return LoadTextureFromPath(pathName);
    }

    public static Texture2D LoadTextureFromPath(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(imageBytes);
            texture.filterMode = FilterMode.Point;
            return texture;
        }
        return null;
    }

    public static Sprite LoadSpriteFromBase64(string base64)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageBytes);
        texture.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(0.5f, 0.0f));
        return sprite;
    }

    public static string[] GetFilesWithName(string name, string path)
    {
        return Directory.GetFiles(path, name);
    }

    public static string CreateSpace(string folder)
    {
        var dataPath = Application.persistentDataPath + Path.DirectorySeparatorChar;
        var fullPath = dataPath + folder;
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        fullPath += Path.DirectorySeparatorChar;

        return fullPath;
    }
}
