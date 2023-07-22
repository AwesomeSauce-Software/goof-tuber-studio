using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] Image expressionImage;
    [Header("Audio Settings")]
    [Range(0.0f, 0.005f)] public float CutOff;
    [SerializeField] float bobTime;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobDistance;
    [Header("Sprites")]
    [SerializeField] Sprite nonTalkingSprite;
    [SerializeField] Sprite talkingSprite;
    [SerializeField] List<Sprite> expressions;
    [Header("Sprite File Names")]
    [SerializeField] string nonTalkingFileName;
    [SerializeField] string talkingFileName;
    [SerializeField] List<string> expressionsFileNames;

    AudioSource audioSource;
    Image characterImage;

    string currentDevice = "";
    float noiseLevel;

    int currentExpression = -1;
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

        audioSource.clip = Microphone.Start(deviceName, true, 4, 44100);
        currentDevice = deviceName;
        while (!(Microphone.GetPosition(deviceName) > 0)) { }
        audioSource.Play();
    }
    
    void SetupDefaultMicrophone()
    {
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
#if UNITY_EDITOR
        Debug.Log($"Noise level set to: {noiseLevel}");
#endif

        yield return null;
    }

    void UpdateExpression()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            currentExpression++;
            if (currentExpression >= expressions.Count)
                currentExpression = -1;

            if (currentExpression > -1)
            {
                expressionImage.enabled = true;
                expressionImage.sprite = expressions[currentExpression];
            }
            else
            {
                expressionImage.enabled = false;
            }
        }
    }

    void AnimateCharacter()
    {
        float meanVolume = GetMeanVolume();
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

    void AttemptLoadSprites()
    {
        var newNonTalking = DataSystem.LoadSprite(nonTalkingFileName);
        talkingSprites[0] = newNonTalking == null ? nonTalkingSprite : newNonTalking;

        var newTalking = DataSystem.LoadSprite(talkingFileName);
        talkingSprites[1] = newTalking == null ? talkingSprite : newTalking;

        for (int i = 0; i < expressions.Count; ++i)
        {
            var newExpression = DataSystem.LoadSprite(expressionsFileNames[i]);
            if (newExpression != null)
                expressions[i] = newExpression;
        }
    }

    void Update()
    {
        UpdateExpression();
        AnimateCharacter();    
    }

    void Start()
    {
        expressionImage.enabled = false;
        AttemptLoadSprites();
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
        talkingSprites[0] = nonTalkingSprite;
        talkingSprites[1] = talkingSprite;
    }
}
