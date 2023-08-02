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

    float spriteHeight;
    float spritePPU;

    bool focusEnabled = true;

    public void SetTargetCharacter(CharacterAnimator newCharacter)
    {
        targetCharacter = newCharacter;
        var sprite = targetCharacter.CharacterRenderer.sprite;

        spriteHeight = sprite.rect.height;
        spritePPU = 1.0f / targetCharacter.CharacterRenderer.sprite.pixelsPerUnit;
    }

    Rect GetWorldSpriteRect(SpriteRenderer spriteRenderer, float offsetY = 0.0f)
    {
        float PPU = (1.0f / spriteRenderer.sprite.pixelsPerUnit);
        var spriteSize = spriteRenderer.sprite.rect.size * PPU;
        var targetOffset = targetCharacter.InitialPosition;
        targetOffset.x -= spriteSize.x * 0.5f;
        targetOffset.y -= spriteSize.y * 0.5f;
        targetOffset.y += offsetY;

        var spriteRect = new Rect(targetOffset, spriteSize);
        return spriteRect;
    }

    void UpdateMover()
    {
        moverObject.sprite = characterManager.SortingMode == CharacterManager.eSortingMode.Free ? moverFullSprite : moverLineSprite;

        var moverPosition = targetCharacter.InitialPosition;
        moverPosition.y += moverOffsetY;
        moverPosition.z = -0.5f;
        moverObject.transform.position = moverPosition;

        var worldPointer = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPointer.z = 0.0f;

        Rect moverRect = GetWorldSpriteRect(moverObject, moverOffsetY);
        if (moverObject.gameObject.activeSelf && moverRect.Contains(worldPointer))
        {
            if (Input.GetMouseButton(0))
            {
                targetCharacter.InitialPosition = worldPointer;
                characterManager.UpdateSorting();
            }
        }

    }
    
    public void SetUIObjectsActive(bool value)
    {
        bool sortingModeFree = characterManager.SortingMode == CharacterManager.eSortingMode.Free || characterManager.SortingMode == CharacterManager.eSortingMode.FreeLine;

        moverObject.gameObject.SetActive(value && sortingModeFree);
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
