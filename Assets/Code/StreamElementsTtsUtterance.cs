using CoreTwitchLibSetup;
using System;
using System.Collections;
using UnityEngine;
namespace StreamElementsTTS_Unity
{
    public class StreamElementsTtsUtterance : MonoBehaviour
    {
        public AudioSource audioSource;
        public string text;
        public TtsVoices voice;
        TwitchLibCtrl m_TwitchLib = null;
        public Transform talkingContext;
        private void Start()
        {
            m_TwitchLib = FindObjectOfType<TwitchLibCtrl>();
            //    StartCoroutine(StreamElementsTTSApi.SpeakRoutine(text, voice, audioSource));
            m_ShowLocation = talkingContext.transform.position;

            talkingContext.transform.position = new Vector3(15, m_ShowLocation.y, m_ShowLocation.z);
            m_HiddenLocation = talkingContext.transform.position;

        }

        Vector3 m_ShowLocation = Vector3.zero;
        Vector3 m_HiddenLocation = Vector3.zero;

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

                if(m_InPosition)
                {

                    /*
                    if (!string.IsNullOrEmpty(text))
                    {
                        Speak();
                    }*/
                }

                if (!m_IsMoving && m_TwitchLib.GetNextMessage(out var msg))
                {
                    text = msg;
                    //if ()
                    {
                        m_LastSpeakTime = Time.time;
                        if (!m_InPosition)
                        {
                            m_IsMoving = true;

                            StartCoroutine(MoveCharacterToSpeakingPosition(true));
                        }
                    
                     
                    }
                }



                if (Time.time > m_LastSpeakTime + 3 && m_InPosition)
                {
                    if (!m_IsHiding)
                    {
                        m_IsHiding = true;
                        StartCoroutine(MoveCharacterToSpeakingPosition(false));


                    }
                }
            }
            //print(biggest);
        }


        float m_LastSpeakTime = 0;

        bool m_IsMoving = false;
        bool m_InPosition = false;
        bool m_IsHiding = false;

        IEnumerator MoveCharacterToSpeakingPosition(bool @in)
        {
            if (@in)
            {
                float elapsed = 0;
                float time = 1;
                while (elapsed < time)
                {
                    var progress = elapsed / time;
                    talkingContext.transform.position = Vector3.Lerp(m_HiddenLocation, m_ShowLocation, progress);

                    yield return new WaitForEndOfFrame();
                    elapsed += Time.deltaTime;
                }

               // m_IsMoving = false;
                m_InPosition = true;
                Speak();
            }
            else
            {
                float elapsed = 0;
                float time = 1;
                while (elapsed < time)
                {
                    var progress = elapsed / time;
                    talkingContext.transform.position = Vector3.Lerp(m_HiddenLocation, m_ShowLocation, 1f - progress);

                    yield return new WaitForEndOfFrame();
                    elapsed += Time.deltaTime;
                }

                m_IsHiding = false;
                m_InPosition = false;
                m_IsMoving = false;
            }
        }
    }
}