using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpriteCache : MonoBehaviour
{
    public int NonTalkingIndex { get; } = 0;
    public int TalkingIndex { get; } = 1;

    public int ExpressionIndex { get; } = 2;
    public int ExpressionCount { get; protected set; } = 0;

    public abstract Sprite GetSprite(int index);
    public abstract Sprite GetExpression(string expressionCategory, int categoryIndex = 0);
    protected abstract void AttemptLoadSprites();
}

public class SpriteManager : SpriteCache
{
    [SerializeField] string spriteFolder;
    [SerializeField] string defaultExpressionCategory;


    List<Sprite> cachedSprites;
    Dictionary<string, List<Sprite>> expressionCategories;
    List<string> expressionCategoryNames;
    string spritePath;

    public override Sprite GetSprite(int index)
    {
        return cachedSprites[index];
    }

    public override Sprite GetExpression(string expressionCategory, int categoryIndex = 0)
    {
        return expressionCategories[expressionCategory][categoryIndex];
    }

    protected override void AttemptLoadSprites()
    {
        cachedSprites = new List<Sprite>();
        expressionCategories = new Dictionary<string, List<Sprite>>();
        expressionCategoryNames = new List<string>();

        cachedSprites.Add(DataSystem.LoadSprite("NonTalking.png", spritePath));
        cachedSprites.Add(DataSystem.LoadSprite("Talking.png", spritePath));

        var expressions = DataSystem.GetFilesWithName("Expression_*", spritePath);
        foreach (var expressionPath in expressions)
        {
            var expressionSprite = DataSystem.LoadSpriteFromPath(expressionPath);
            if (expressionSprite != null)
            {
                var expressionName = expressionPath.Substring(spritePath.Length).Split('_');

                string category = defaultExpressionCategory;
                if (expressionName.Length == 3)
                    category = expressionName[1].ToLower();

                if (!expressionCategories.ContainsKey(category))
                {
                    expressionCategories.Add(category, new List<Sprite>());
                    expressionCategoryNames.Add(category);
                }
                expressionCategories[category].Add(expressionSprite);

                cachedSprites.Add(expressionSprite);
                ExpressionCount++;
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
