using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CoreTwitchLibSetup;

public class TestHarnessCanvasController : MonoBehaviour
{
    [SerializeField] private DisplayMembers dm;
    [SerializeField] private StreamElementsTTS_Unity.StreamElementsTtsUtterance TTSScript; 
    
    [System.Serializable] private class DisplayMembers
    {
        [SerializeField] internal InputField SpeakText;
        [SerializeField] internal Button BtnSpeak;

        [SerializeField] internal Dropdown VoicesList;
        [SerializeField] internal Button BtnSetVoice;

        [SerializeField] internal Button BtnSkipCurrent;
        [SerializeField] internal Button BtnTogglePause;

        [SerializeField] internal Dropdown Characters;
    }

    Dictionary<string, StreamElementsTTS_Unity.TtsVoices> VoicesMap = new Dictionary<string, StreamElementsTTS_Unity.TtsVoices>();

    private void Awake()
    {
#if !UNITY_EDITOR
                gameObject.SetActive(false);
#endif

        VoicesChanger();
        CharacterChanger();

        dm.BtnSpeak.onClick.AddListener(() => {
            TwitchLibCtrl.Messages.Enqueue(dm.SpeakText.text);
        });

        dm.BtnSetVoice.gameObject.SetActive(false);

        dm.BtnSkipCurrent.onClick.AddListener(() => {
            TwitchLibCtrl.TtsSkipHandler.SkipCurrentMessage();
        });

        dm.BtnTogglePause.onClick.AddListener(() => {
            TwitchLibCtrl.TTSPaused = !TwitchLibCtrl.TTSPaused;
        });
    }

    private void VoicesChanger()
    {
        List<Dropdown.OptionData> optsForVoicesList = new List<Dropdown.OptionData>();

        var enums = System.Enum.GetValues(typeof(StreamElementsTTS_Unity.TtsVoices));
        foreach (StreamElementsTTS_Unity.TtsVoices x in enums) {
            VoicesMap.Add(x.ToString(), x);
            optsForVoicesList.Add(new Dropdown.OptionData(x.ToString()));
        }

        dm.VoicesList.AddOptions(optsForVoicesList);
        dm.VoicesList.value = Array.IndexOf(enums, TTSScript.voice);

        dm.VoicesList.onValueChanged.AddListener(o => {
            var y = dm.VoicesList.options[o].text;
            TTSScript.voice = VoicesMap[y];
        });
    }

    private void CharacterChanger()
    {
        List<Dropdown.OptionData> optsForCharactersList = new List<Dropdown.OptionData>();
        foreach (var c in Settings.SettingsManager.Instance.dependencies.AllCharacters) {
            optsForCharactersList.Add(new Dropdown.OptionData(c.name.ToString()));
        }

        dm.Characters.AddOptions(optsForCharactersList);
        dm.Characters.onValueChanged.AddListener(o =>
        {
            var y = dm.Characters.options[o].text;
            GameObject.FindObjectOfType<StreamElementsTTS_Unity.TalkingSprite>().m_ActiveCharacter
             = Settings.SettingsManager.Instance.dependencies.AllCharacters.First(t => t.name == y);
        });
    }
}
