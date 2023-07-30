using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceManager : MonoBehaviour
{
    public Vector3 LeftMostPoint => leftMostSpace.position;
    public Vector3 RightMostPoint => rightMostSpace.position;

    [SerializeField] Camera mainCamera;
    [SerializeField] RectTransform leftMostSpace;
    [SerializeField] RectTransform rightMostSpace;
}
