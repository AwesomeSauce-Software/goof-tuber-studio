using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFocus : MonoBehaviour
{
    [SerializeField] GameObject editCharacterUI;
    [SerializeField] SpaceManager spaceManager;
    [SerializeField] Camera mainCamera;
    [Space()]
    [SerializeField] CharacterAnimator targetCharacter;
    [Header("Mover Element")]
    [SerializeField] SpriteRenderer moverObject;
    [SerializeField] Sprite moverFullSprite;
    [SerializeField] Sprite moverLineSprite;
    [SerializeField] float moverOffsetY;

    CharacterManager characterManager;

    bool focusEnabled = true;
    bool movingMover = false;

    public void SetTargetCharacter(CharacterAnimator newCharacter)
    {
        targetCharacter = newCharacter;
    }

    public void SetUIObjectsActive(bool value)
    {
        bool sortingModeFree = characterManager.SortingMode == CharacterManager.eSortingMode.Free || characterManager.SortingMode == CharacterManager.eSortingMode.FreeLine;

        moverObject.gameObject.SetActive(value && sortingModeFree);
    }

    bool IsPointInSpriteBounds(SpriteRenderer spriteRenderer, Vector3 point, float boundsMultiplier = 1.0f)
    {
        return IsPointInSpriteBounds(spriteRenderer, point, Vector3.zero, boundsMultiplier);
    }

    bool IsPointInSpriteBounds(SpriteRenderer spriteRenderer, Vector3 point, Vector3 offset, float boundsMultiplier = 1.0f)
    {
        var spritePosition = spriteRenderer.gameObject.transform.position + offset;
        var sprite = spriteRenderer.sprite;
        var spritePPU = 1.0f / sprite.pixelsPerUnit;
        var spriteSize = sprite.rect.size * spriteRenderer.transform.localScale * spritePPU * 0.5f * boundsMultiplier;

        var left = spritePosition.x - spriteSize.x;
        var right = spritePosition.x + spriteSize.x;
        var down = spritePosition.y - spriteSize.y;
        var up = spritePosition.y + spriteSize.y;

        return (point.x > left && point.x < right && point.y > down && point.y < up);
    }

    Vector3 GetWorldPointer()
    {
        var worldPointer = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPointer.z = 0.0f;

        return worldPointer;
    }

    void UpdateMover()
    {
        moverObject.sprite = characterManager.SortingMode == CharacterManager.eSortingMode.Free ? moverFullSprite : moverLineSprite;

        bool moverActive = moverObject.gameObject.activeSelf;

        var moverPosition = targetCharacter.InitialPosition;
        moverPosition.y += moverOffsetY;
        moverPosition.z = -0.5f;
        moverObject.transform.position = moverPosition;

        var worldPointer = GetWorldPointer();

        if (moverActive && !movingMover && Input.GetMouseButton(0))
        {
            movingMover = IsPointInSpriteBounds(moverObject, worldPointer);
        }
        else if (!moverActive || Input.GetMouseButtonUp(0))
        {
            movingMover = false;
        }

        if (movingMover)
        {
            targetCharacter.InitialPosition = worldPointer - Vector3.up * moverOffsetY;
            characterManager.UpdateSorting();
        }

    }

    void UpdateTargetCharacter()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var characterList = characterManager.Characters;
            var worldPointer = GetWorldPointer();

            characterList.Sort((a, b) => a.transform.position.z > b.transform.position.z ? 1 : -1);

            foreach (var character in characterList)
            {
                var sprite = character.CharacterRenderer.sprite;
                var spritePPU = 1.0f / sprite.pixelsPerUnit;
                var halfHeight = Vector3.up * sprite.rect.height * spritePPU * 0.5f;

                if (IsPointInSpriteBounds(character.CharacterRenderer, worldPointer, halfHeight))
                {
                    SetTargetCharacter(character);
                    break;
                }
            }
        }
    }

    void UpdateControls()
    {
        if (focusEnabled ^ editCharacterUI.activeSelf)
        {
            focusEnabled = editCharacterUI.activeSelf;
            SetUIObjectsActive(focusEnabled);
        }

        if (focusEnabled)
        {
            UpdateTargetCharacter();
            UpdateMover();
        }
    }

    void Update()
    {
        UpdateControls();    
    }

    private void LateUpdate()
    {
        UpdateControls();
    }

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
        SetTargetCharacter(targetCharacter);
    }
}
