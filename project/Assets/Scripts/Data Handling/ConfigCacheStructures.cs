using UnityEngine;

[System.Serializable]
public class CharacterPlacement
{
    public string UserID;
    public Vector3 Position; 

    public CharacterPlacement(string userID, Vector3 position)
    {
        UserID = userID;
        Position = position;
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
