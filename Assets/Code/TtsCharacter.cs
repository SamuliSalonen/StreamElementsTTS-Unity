using UnityEngine;

namespace StreamElementsTTS_Unity
{
    [CreateAssetMenu(fileName = "tts character", menuName ="tts/character")]
    public class TtsCharacter : ScriptableObject
    {
        public TtsVoices voice = TtsVoices.Brian;
        public Sprite[] silent;

        public Sprite[] talk;
    }
}