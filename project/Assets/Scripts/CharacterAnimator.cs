using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Audio Settings")]
    public float MovementSensitivity;
    [Range(0.0f, 0.005f)] public float CutOff;
    [Header("Sprites")]
    [SerializeField] Sprite nonTalkingSprite;
    [SerializeField] Sprite talkingSprite;

    AudioSource audioSource;
    Image characterImage;

    string currentDevice = "";
    float noiseLevel;

    Sprite[] sprites;
    Vector3 initialPosition;

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

    void AnimateCharacter()
    {
        float meanVolume = GetMeanVolume();

        bool isTalking = meanVolume > CutOff + noiseLevel;
        characterImage.sprite = sprites[isTalking ? 1 : 0];
    }


    void Update()
    {
        AnimateCharacter();    
    }

    void Start()
    {
        SetupDefaultMicrophone();
        StartCoroutine(SampleNoise());
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        characterImage = GetComponent<Image>();
        initialPosition = characterImage.transform.position;

        sprites = new Sprite[2];
        sprites[0] = nonTalkingSprite;
        sprites[1] = talkingSprite;
    }
}
