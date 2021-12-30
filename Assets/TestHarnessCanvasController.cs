using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    Dictionary<string, StreamElementsTTS_Unity.TtsVoices> kvp = new Dictionary<string, StreamElementsTTS_Unity.TtsVoices>();

    private void Awake()
    {
#if !UNITY_EDITOR

        gameObject.SetActive(false);
#endif
        List<Dropdown.OptionData> opts = new List<Dropdown.OptionData>();

        var enums = System.Enum.GetValues(typeof(StreamElementsTTS_Unity.TtsVoices));
        foreach (StreamElementsTTS_Unity.TtsVoices x in enums) {
            kvp.Add(x.ToString(), x);
            opts.Add(new Dropdown.OptionData(x.ToString()));
        }

        dm.VoicesList.AddOptions(opts);
        dm.VoicesList.value = Array.IndexOf(enums, TTSScript.voice);

        dm.VoicesList.onValueChanged.AddListener(o =>
        {
            var y = dm.VoicesList.options[o].text;
            TTSScript.voice = kvp[y];
        });

        dm.BtnSpeak.onClick.AddListener(() => {
            CoreTwitchLibSetup.TwitchLibCtrl.Messages.Enqueue(dm.SpeakText.text);
            //TTSScript.text = dm.SpeakText.text;
          //  TTSScript.Speak();
        });

        dm.BtnSetVoice.gameObject.SetActive(false);
    }
}
