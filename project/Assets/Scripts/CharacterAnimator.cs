using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public float MeanVolume { get; set; }
    public string CurrentExpressionName => currentExpressionName;
    public AudioCache AudioManager => audioManager;

    [SerializeField] AudioInternalHandler audioManager;
    [SerializeField] SpriteCache spriteManager;
    [SerializeField] Image expressionImage;
    [Header("Audio Settings")]
    [Range(0.0f, 0.005f)] public float CutOff;
    [SerializeField] float bobTime;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobDistance;

    Image characterImage;

    AudioSource audioSource;

    int currentExpression = -1;
    string currentExpressionName;
    Sprite[] talkingSprites;
    Vector3 initialPosition;

    float bobTimer;

    public void SetSpriteManager(SpriteCache newSpriteManager)
    {
        spriteManager = newSpriteManager;
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
        
        float t = bobTimer / bobTime;
        var bob = Vector3.Lerp(initialPosition, initialPosition + (Vector3.up * bobDistance), t);
    
        characterImage.sprite = talkingSprites[isTalking ? 1 : 0];
        characterImage.transform.position = bob;
        expressionImage.transform.position = bob;
    }
    
    void SetupSprites()
    {
        talkingSprites[0] = spriteManager.GetSprite("NonTalking.png");
        talkingSprites[1] = spriteManager.GetSprite("Talking.png");
    }

    void Update()
    {
        if (audioManager != null)
            MeanVolume = audioManager.MeanVolume;
        UpdateExpression();
        AnimateCharacter();    
    }
    
    void Start()
    {
        expressionImage.enabled = false;
        SetupSprites();
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        characterImage = GetComponent<Image>();
        initialPosition = characterImage.transform.position;

        talkingSprites = new Sprite[2];
    }
}
