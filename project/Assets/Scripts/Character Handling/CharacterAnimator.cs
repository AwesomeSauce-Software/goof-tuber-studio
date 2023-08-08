using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public float BobAmount
    {
        get { return bobAmount; }
        set 
        { 
            bobAmount = value;
            SetBobDistance();
        }
    }
    public float MeanVolume
    {
        get { return audioManager.MeanVolume; }
        set { audioManager.MeanVolume = value; }
    }
    public string CurrentExpressionName => currentExpressionName;
    public SpriteRenderer CharacterRenderer => characterRenderer;
    public Vector3 InitialPosition = Vector3.zero;
    public string UserID;
    public int Order;

    [Space()]
    [SerializeField] AudioCache audioManager;
    [SerializeField] SpriteCache spriteManager;
    [SerializeField] SpriteRenderer expressionRenderer;
    [Header("Audio Settings")]
    [Range(0.0f, 0.005f)] public float CutOff;
    [SerializeField] float bobTime;
    [SerializeField] float bobSpeed;
    
    SpriteRenderer characterRenderer;
    Vector3 referenceSpriteSize;
    float referenceSpriteHeight;

    string currentExpressionName;
    Sprite[] talkingSprites;

    Vector3 bobMaxDistance;
    float bobAmount = 0.5f;
    float bobTimer;

    public void SetSpriteSize(float width, float height)
    {
        if (referenceSpriteSize.x > 0.0f && referenceSpriteSize.y > 0.0f)
            transform.localScale = new Vector3(width / referenceSpriteSize.x, height / referenceSpriteSize.y);
    }

    public void LoadAvatarPayload(AvatarPayload avatarPayload)
    {
        spriteManager.LoadAvatarPayload(avatarPayload);
        SetupSprites();
    }

    void UpdateExpression()
    {
        //if (Input.GetKeyDown(KeyCode.Keypad0))
        //{
        //   
        //}
        // to-do:
        //  replace with a keybind implementation
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
        var bob = Vector3.Lerp(InitialPosition, InitialPosition + bobMaxDistance, t);
        bob.z = Order * 0.05f;

        characterRenderer.sprite = talkingSprites[isTalking ? 1 : 0];
        transform.position = bob;
    }
    
    void SetBobDistance()
    {
        bobMaxDistance = Vector3.down * referenceSpriteHeight * BobAmount;
    }

    void SetupSprites()
    {
        expressionRenderer.enabled = false;
        talkingSprites[0] = spriteManager.GetSprite("NonTalking.png");
        talkingSprites[1] = spriteManager.GetSprite("Talking.png");

        referenceSpriteSize = talkingSprites[0].rect.size;
        referenceSpriteHeight = referenceSpriteSize.y * (1.0f / talkingSprites[0].pixelsPerUnit);
    }

    void Update()
    {
        UpdateExpression();
        AnimateCharacter();    
    }

    void Start()
    {
        SetupSprites();
        SetBobDistance();
    }

    void Awake()
    {
        audioManager = GetComponent<AudioCache>();
        characterRenderer = GetComponent<SpriteRenderer>();
        talkingSprites = new Sprite[2];
    }
}
