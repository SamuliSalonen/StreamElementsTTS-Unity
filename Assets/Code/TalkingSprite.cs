using UnityEngine;

namespace StreamElementsTTS_Unity
{
    public class TalkingSprite : MonoBehaviour
    {
        public Sprite silent, speak;
        public new SpriteRenderer renderer;

        public bool isSpeaking;

        private void Start()
        {
            FindObjectOfType<StreamElementsTtsUtterance>().onSpeakFrame += (bool b) => isSpeaking = b;
        }

        private void Update()
        {
            if(isSpeaking)
            {
                renderer.sprite = speak;
            }
            else
            {
                renderer.sprite = silent;
            }
        }
    }
}