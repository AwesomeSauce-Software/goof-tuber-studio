using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpriteCache : MonoBehaviour
{
    public abstract Sprite GetSprite(string spriteName);
    public abstract Sprite GetExpression(string expressionCategory, int categoryIndex = 0);
}
