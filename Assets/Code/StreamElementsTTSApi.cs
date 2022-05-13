
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace StreamElementsTTS_Unity
{
    public static class StreamElementsTTSApi
    {
        public static string TtsVoiceToString(this TtsVoices voice)
        {
            return voice.ToString().Replace("_", "-");
        }

        static void AudioSourceSampler(AudioSource source)
        {
            //   source.GetSpectrumData();
            //    source.GetSpectrumData();
        }

        internal static IEnumerator SpeakRoutine(string text, TtsVoices voice, AudioSource audioSource)
        {
            if (!IsTextValid(text))
            {
                throw new ArgumentException("text");
            }

            string remove = "";
            float pitchValue = 1;
            var splitText = text.Split(' ');
            if (splitText[0] == "pitch")
            {
                if (splitText[1] == "random")
                {
                    pitchValue = UnityEngine.Random.Range(.5f, 2f);
                }

                if (float.TryParse(splitText[1], out var pitch))
                {
                    pitchValue = pitch;
                }

                remove = splitText[0] + " " + splitText[1];
                text = text.Replace(remove, "");
                //   splitText[1]
            }

            //string.split(" ")[0].split("=")[0] == "pitch":

            var uri = BuildRequestUri(text, voice);
            return GetAudioClipRoutine(uri, (clip) =>
            {
                /*
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
           */
                audioSource.pitch = pitchValue;//UnityEngine.Random.Range(.5f, 2f);
                audioSource.PlayOneShot(clip);

            });
        }

        static bool IsTextValid(string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static string BuildRequestUri(string ttsText, TtsVoices voice)
        {

            return $"https://api.streamelements.com/kappa/v2/speech?voice={voice.TtsVoiceToString()}&text={ttsText}";
        }

        public static IEnumerator GetAudioClipRoutine(string fullPath, Action<AudioClip> callback)
        {

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    callback?.Invoke(null);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    callback?.Invoke(clip);

                }
            }
        }
    }

}