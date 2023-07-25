using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioInternalHandler : AudioExternalHandler
{
    public AudioSource AudioSource;
    string currentDevice = "";
    float noiseLevel;

    public void SetupMicrophone(string deviceName)
    {
        if (Microphone.devices.Length <= 0)
            return;

        AudioSource.Stop();
        if (currentDevice.Length > 0)
            Microphone.End(currentDevice);

        AudioSource.clip = Microphone.Start(deviceName, true, 1, 44100);
        currentDevice = deviceName;
        while (!(Microphone.GetPosition(deviceName) > 0)) { }
        AudioSource.Play();
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
        AudioSource.GetSpectrumData(audioSamples, 0, FFTWindow.Rectangular);
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

    void Update()
    {
        MeanVolume = GetMeanVolume();
    }

    void Start()
    {
        SetupDefaultMicrophone();
    }

    void Awake()
    {
        StartCoroutine(SampleNoise());
    }
}
