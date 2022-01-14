using UnityEngine;
using static Settings.SettingsManager;
using static Constants;

namespace StreamElementsTTS_Unity
{
    public class TalkingSprite : MonoBehaviour
    {
        public Sprite[] silent, speak;
        public new SpriteRenderer renderer;
        //public UnityEngine.UI.Image renderer;

        public bool isSpeaking;

        int speakAmount = 0;
        [SerializeField]
        internal TtsCharacter m_ActiveCharacter = null;

        private void Start()
        {
            renderer.sprite = RandomFromArray(silent);
            var utterance = FindObjectOfType<StreamElementsTtsUtterance>();
            utterance.onBeginSpeak += () => {
                m_ActiveCharacter = m_ActiveCharacter ?? _Dependencies.AllCharacters[UnityEngine.Random.Range(0, _Dependencies.AllCharacters.Count)];
                renderer.sprite = RandomFromArray(m_ActiveCharacter.silent);
                utterance.voice = m_ActiveCharacter.voice;
            };
            
            utterance.onSpeakFrame += (bool b) => {
                speakAmount++;

                if (speakAmount % 2 == 1) return;

                if (b) {
                    isSpeaking = true;
                }
                else
                    isSpeaking = false;

                if(wasSpeaking != isSpeaking)
                    renderer.sprite = RandomFromArray(isSpeaking ? m_ActiveCharacter.talk : m_ActiveCharacter.silent);

                wasSpeaking = b;
            };
        }

        bool wasSpeaking = false;

        Sprite RandomFromArray(Sprite[] ar)
        {
            return ar[Random.Range(0, ar.Length)];
        }
    }
}