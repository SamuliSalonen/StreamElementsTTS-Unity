﻿using UnityEngine;
namespace StreamElementsTTS_Unity
{
    public class StreamElementsTtsUtterance : MonoBehaviour
    {
        public AudioSource audioSource;
        public string text;
        public TtsVoices voice;

        private void Start()
        {
            StartCoroutine(StreamElementsTTSApi.SpeakRoutine(text, voice, audioSource));
        }

        internal void Speak()
        {
            Debug.Log("Sock over it?");
            StreamElementsTTSApi.SpeakRoutine(text, voice, audioSource);
            audioSource.Play();
        }
    }
}