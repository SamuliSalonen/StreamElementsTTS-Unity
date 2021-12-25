using CoreTwitchLibSetup;
using System;
using UnityEngine;
namespace StreamElementsTTS_Unity
{
    public class StreamElementsTtsUtterance : MonoBehaviour
    {
        public AudioSource audioSource;
        public string text;
        public TtsVoices voice;
        TwitchLibCtrl m_TwitchLib = null;
        private void Start()
        {
            m_TwitchLib = FindObjectOfType<TwitchLibCtrl>();
            StartCoroutine(StreamElementsTTSApi.SpeakRoutine(text, voice, audioSource));
        }

        internal void Speak()
        {
            Debug.Log("Sock over it?");
            StartCoroutine(StreamElementsTTSApi.SpeakRoutine(text, voice, audioSource));
            //audioSource.Play();
        }

        float[] spectrum = new float[256];

        public event Action<bool> onSpeakFrame;
        public float biggestView;
        public bool isTalking => audioSource.isPlaying;
        private void Update()
        {

            if (isTalking)
            {
                audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
                float biggest = -1;
                for (int i = 1; i < spectrum.Length - 1; i++)
                {
                    Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
                    Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
                    Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
                    Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
                    if (biggest < spectrum[i - 1])
                    {
                        biggest = spectrum[i - 1];
                    }
                }

                biggestView = biggest;
                onSpeakFrame?.Invoke(biggest > 0.05f);
            
            }
            else
            {
                onSpeakFrame?.Invoke(false);
                if (m_TwitchLib.GetNextMessage(out var msg))
                {
                    text = msg;
                    Speak();
                }
            }
            //print(biggest);
        }
    }
}