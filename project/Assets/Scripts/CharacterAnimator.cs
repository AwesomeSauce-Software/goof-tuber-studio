using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public float MeanVolume
    {
        get { return audioManager.MeanVolume; }
        set { audioManager.MeanVolume = value; }
    }
    public string CurrentExpressionName => currentExpressionName;
    public SpriteRenderer CharacterRenderer => characterRenderer;
    public Vector3 InitialPosition;

    [SerializeField] AudioCache audioManager;
    [SerializeField] SpriteCache spriteManager;
    [SerializeField] SpriteRenderer expressionRenderer;
    [Header("Audio Settings")]
    [Range(0.0f, 0.005f)] public float CutOff;
    [SerializeField] float bobTime;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobDistance;

    SpriteRenderer characterRenderer;

    //int currentExpression = -1;
    string currentExpressionName;
    Sprite[] talkingSprites;

    float bobTimer;

    public void LoadAvatarPayload(AvatarPayload avatarPayload)
    {
        spriteManager.LoadAvatarPayload(avatarPayload);
        SetupSprites();
    }

    void UpdateExpression()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            //currentExpression++;
            //if (currentExpression >= spriteManager.ExpressionCount)
            //    currentExpression = -1;
            //
            //if (currentExpression > -1)
            //{
            //    expressionImage.enabled = true;
            //    expressionImage.sprite = spriteManager.GetSprite(spriteManager.ExpressionIndex + currentExpression);
            //    currentExpressionName = expressionImage.sprite.name;
            //    expressionImage.SetNativeSize();
            //}
            //else
            //{
            //    currentExpressionName = "";
            //    expressionImage.enabled = false;
            //}
        }
    }
    
    void AnimateCharacter()
    {
        bool isTalking = MeanVolume > CutOff;
        float direction = isTalking ? 1.0f : -1.0f;
    
        bobTimer += direction * bobSpeed * Time.deltaTime;
        if (bobTimer > bobTime)
        {
            bobTimer = bobTime;
        }
        else if (bobTimer < 0.0f)
        {
            bobTimer = 0.0f;
        }
        
        float t = Mathf.Clamp01(bobTimer / bobTime);
        var bob = Vector3.Lerp(InitialPosition, InitialPosition + (Vector3.up * bobDistance), t);

        characterRenderer.sprite = talkingSprites[isTalking ? 1 : 0];
        transform.position = bob;
    }
    
    void SetupSprites()
    {
        expressionRenderer.enabled = false;
        talkingSprites[0] = spriteManager.GetSprite("NonTalking.png");
        talkingSprites[1] = spriteManager.GetSprite("Talking.png");
    }

    void SetupInitialPosition()
    {
        InitialPosition = transform.position;
    }

    void Update()
    {
        UpdateExpression();
        AnimateCharacter();    
    }

    void Start()
    {
        SetupSprites();
        SetupInitialPosition();
    }

    void Awake()
    {
        audioManager = GetComponent<AudioCache>();
        characterRenderer = GetComponent<SpriteRenderer>();
        talkingSprites = new Sprite[2];
    }
}
