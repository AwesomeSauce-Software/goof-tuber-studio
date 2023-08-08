using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterFocus : MonoBehaviour
{
    [SerializeField] GameObject editCharacterUI;
    [SerializeField] Image characterImage;
    [SerializeField] SpaceManager spaceManager;
    [SerializeField] Camera mainCamera;
    [Space()]
    [SerializeField] CharacterAnimator targetCharacter;
    [Header("Mover Element")]
    [SerializeField] SpriteRenderer moverObject;
    [SerializeField] Sprite moverFullSprite;
    [SerializeField] Sprite moverLineSprite;
    [SerializeField] float moverOffsetY;
    [Header("Scaler Element")]
    [SerializeField] InputField widthInput;
    [SerializeField] InputField heightInput;
    [Header("Bobber Element")]
    [SerializeField] Slider bobSlider;
    [Header("Ordering Element")]
    [SerializeField] Button sendToFrontButton;
    [SerializeField] Button sendToBackButton;
    [SerializeField] Button orderUpButton;
    [SerializeField] Button orderDownButton;

    CharacterManager characterManager;

    bool focusEnabled = true;
    bool movingMover = false;

    public void SetTargetCharacter(CharacterAnimator newCharacter)
    {
        targetCharacter = newCharacter;

        UpdateCharacterUI();
    }

    public void SetCharacterSpriteSize()
    {
        float width, height;
        float.TryParse(widthInput.text, out width);
        float.TryParse(heightInput.text, out height);

        if (width > 0.0f && height > 0.0f)
            targetCharacter.SetSpriteSize(width, height);
    }

    public void GetNextCharacter()
    {
        SetCharacterFromList(1);
    }

    public void GetPreviousCharacter()
    {
        SetCharacterFromList(-1);
    }

    public void SetCharacterToFront()
    {
        characterManager.SwapCharacterOrdering(targetCharacter, 0);
        UpdateOrderButtons();
    }    

    public void SetCharacterToBack()
    {
        characterManager.SetCharacterOrderToBack(targetCharacter);
        UpdateOrderButtons();
    }

    public void IncrementCharacterOrder()
    {
        characterManager.SwapCharacterOrdering(targetCharacter, targetCharacter.Order + 1);
        UpdateOrderButtons();
    }

    public void DecrementCharacterOrder()
    {
        characterManager.SwapCharacterOrdering(targetCharacter, targetCharacter.Order - 1);
        UpdateOrderButtons();
    }

    public void SetCharacterBobAmount(float value)
    {
        targetCharacter.BobAmount = value;
    }

    public void SetUIObjectsActive(bool value)
    {
        bool sortingModeFree = characterManager.SortingMode == CharacterManager.eSortingMode.Free || characterManager.SortingMode == CharacterManager.eSortingMode.FreeLine;

        moverObject.gameObject.SetActive(value && sortingModeFree);

        if (value)
        {
            UpdateCharacterUI();
        }
    }

    void SetCharacterFromList(int direction)
    {
        var characterList = characterManager.Characters;

        int index = characterList.FindIndex(c => c == targetCharacter);
        if (index >= 0)
        {
            index += direction;
            if (index < 0)
                index = characterList.Count - 1;
            else if (index >= characterList.Count)
                index = 0;

            SetTargetCharacter(characterList[index]);
        }
    }

    void UpdateOrderButtons()
    {
        int characterCount = characterManager.Characters.Count;
        bool isFront = targetCharacter.Order == 0;
        bool isBack = targetCharacter.Order == characterCount - 1;
        sendToFrontButton.interactable = !isFront;
        sendToBackButton.interactable = !isBack;
        orderUpButton.interactable = !isFront;
        orderDownButton.interactable = !isBack;
    }

    void UpdateCharacterUI()
    {
        characterImage.sprite = targetCharacter.CharacterRenderer.sprite;
        bobSlider.value = targetCharacter.BobAmount;
        UpdateOrderButtons();
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

            foreach (var character in characterList)
            {
                var sprite = character.CharacterRenderer.sprite;
                var spritePPU = 1.0f / sprite.pixelsPerUnit;
                var halfHeight = Vector3.up * sprite.rect.height * character.transform.localScale.y * spritePPU * 0.5f;

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

    private void Start()
    {
        SetTargetCharacter(targetCharacter);
    }

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }
}
