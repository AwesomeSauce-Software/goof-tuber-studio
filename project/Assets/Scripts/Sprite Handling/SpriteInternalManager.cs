using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteInternalManager : SpriteCache
{
    public List<string> CachedSpritePaths => cachedSpritePaths;

    [SerializeField] string spriteFolder;
    [SerializeField] string defaultExpressionCategory;

    List<string> cachedSpritePaths;
    Dictionary<string, Sprite> cachedSprites;
    Dictionary<string, List<Sprite>> expressionCategories;
    List<string> expressionCategoryNames;
    string spritePath;

    public override Sprite GetSprite(string spriteName)
    {
        return cachedSprites[spriteName];
    }

    public override Sprite GetExpression(string expressionCategory, int categoryIndex = 0)
    {
        return expressionCategories[expressionCategory][categoryIndex];
    }

    void AttemptLoadSprites()
    {
        cachedSprites = new Dictionary<string, Sprite>();
        cachedSpritePaths = new List<string>();
        expressionCategories = new Dictionary<string, List<Sprite>>();
        expressionCategoryNames = new List<string>();

        cachedSprites.Add("NonTalking.png", DataSystem.LoadSprite("NonTalking.png", spritePath));
        cachedSprites.Add("Talking.png", DataSystem.LoadSprite("Talking.png", spritePath));
        cachedSpritePaths.Add(spritePath + "NonTalking.png");
        cachedSpritePaths.Add(spritePath + "Talking.png");

        var expressions = DataSystem.GetFilesWithName("Expression_*", spritePath);
        foreach (var expressionPath in expressions)
        {
            var expressionSprite = DataSystem.LoadSpriteFromPath(expressionPath);
            if (expressionSprite != null)
            {
                var expressionName = expressionPath.Substring(spritePath.Length);
                var nameSegments = expressionName.Split('_');

                expressionSprite.name = expressionName;
                cachedSpritePaths.Add(expressionPath);

                string category = defaultExpressionCategory;
                if (nameSegments.Length == 3)
                    category = nameSegments[1].ToLower();

                if (!expressionCategories.ContainsKey(category))
                {
                    expressionCategories.Add(category, new List<Sprite>());
                    expressionCategoryNames.Add(category);
                }
                expressionCategories[category].Add(expressionSprite);

                cachedSprites.Add(expressionName, expressionSprite);
            }
        }
    }

    void SetupFolders()
    {
        spritePath = DataSystem.CreateSpace(spriteFolder);
    }

    void Awake()
    {
        SetupFolders();
        AttemptLoadSprites();
    }
}
