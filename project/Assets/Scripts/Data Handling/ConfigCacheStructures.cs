using UnityEngine;

[System.Serializable]
public class CharacterPlacement
{
    public string UserID;
    public int Order;
    public float BobAmount;
    public Vector3 Position;
    public Vector3 Scale;

    public CharacterPlacement(string userID, Vector3 position, Vector3 scale, int order, float bobAmount)
    {
        UserID = userID;
        Order = order;
        BobAmount = bobAmount;
        Position = position;
        Scale = scale;
    }
}

[System.Serializable]
public class CharactersConfig
{
    public CharacterManager.eSortingMode SortingMode;
    public int CharactersVisible;
    public float LineSortRight;
    public float LineSortLeft;
    public CharacterPlacement[] CharacterPlacements; 
}
