using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public float MeanVolume => meanVolume;
    public string CurrentExpressionName => currentExpressionName;

    [SerializeField] SpriteCache spriteManager;
    [SerializeField] Image expressionImage;
    [Header("Audio Settings")]
    [Range(0.0f, 0.005f)] public float CutOff;
    [SerializeField] float bobTime;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobDistance;

    AudioSource audioSource;
    Image characterImage;

    string currentDevice = "";
    float noiseLevel;
    float meanVolume;

    int currentExpression = -1;
    string currentExpressionName;
    Sprite[] talkingSprites;
    Vector3 initialPosition;

    float bobTimer;

    public void SetupMicrophone(string deviceName)
    {
        if (Microphone.devices.Length <= 0)
            return;

        audioSource.Stop();
        if (currentDevice.Length > 0)
            Microphone.End(currentDevice);

        audioSource.clip = Microphone.Start(deviceName, true, 1, 44100);
        currentDevice = deviceName;
        while (!(Microphone.GetPosition(deviceName) > 0)) { }
        audioSource.Play();
    }
    
    void SetupDefaultMicrophone()
    {
        Application.runInBackground = true;
        string device = "";
        if (Microphone.devices.Length > 0)
            device = Microphone.devices[0];
        SetupMicrophone(device);
    }

    float GetMeanVolume()
    {
        float[] audioSamples = new float[1024];
        audioSource.GetSpectrumData(audioSamples, 0, FFTWindow.Rectangular);
        float meanVolume = audioSamples.Average();

        return meanVolume;
    }

    IEnumerator SampleNoise()
    {
        float meanedNoise = 0.0f;
        for (int i = 0; i < 32; ++i)
        {
            meanedNoise += GetMeanVolume();

            yield return new WaitForSeconds(0.005f);
        }
        if (meanedNoise > 0.0f)
            meanedNoise /= 32.0f;

        noiseLevel = meanedNoise;

        yield return null;
    }

    void UpdateExpression()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            currentExpression++;
            if (currentExpression >= spriteManager.ExpressionCount)
                currentExpression = -1;
    
            if (currentExpression > -1)
            {
                expressionImage.enabled = true;
                expressionImage.sprite = spriteManager.GetSprite(spriteManager.ExpressionIndex + currentExpression);
                currentExpressionName = expressionImage.sprite.name;
            }
            else
            {
                currentExpressionName = "";
                expressionImage.enabled = false;
            }
        }
    }
    
    void AnimateCharacter()
    {
        meanVolume = GetMeanVolume();
        bool isTalking = meanVolume > CutOff + noiseLevel;
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
        talkingSprites[0] = spriteManager.GetSprite(spriteManager.NonTalkingIndex);
        talkingSprites[1] = spriteManager.GetSprite(spriteManager.TalkingIndex);
    }

    void Update()
    {
        UpdateExpression();
        AnimateCharacter();    
    }
    
    void Start()
    {
        expressionImage.enabled = false;
        SetupSprites();
        SetupDefaultMicrophone();
        StartCoroutine(SampleNoise());
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
