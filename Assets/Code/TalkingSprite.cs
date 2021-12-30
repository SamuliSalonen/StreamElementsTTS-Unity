using UnityEngine;

namespace StreamElementsTTS_Unity
{
    public class TalkingSprite : MonoBehaviour
    {
        public Sprite[] silent, speak;
        public new SpriteRenderer renderer;

        public bool isSpeaking;

        int speakAmount = 0;

        private void Start()
        {
            renderer.sprite = RandomFromArray(silent);

            FindObjectOfType<StreamElementsTtsUtterance>().onSpeakFrame += (bool b) => {
                speakAmount++;

                if (speakAmount % 2 == 1) return;

                if (b) {
                    isSpeaking = true;
                }
                else
                    isSpeaking = false;

                if(wasSpeaking != isSpeaking)
                    renderer.sprite = RandomFromArray(isSpeaking ? speak : silent);

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