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
        List<Dropdown.OptionData> opts = new List<Dropdown.OptionData>();

        foreach (StreamElementsTTS_Unity.TtsVoices x in System.Enum.GetValues(typeof(StreamElementsTTS_Unity.TtsVoices))) {
            kvp.Add(x.ToString(), x);
            opts.Add(new Dropdown.OptionData(x.ToString()));
        }

        dm.VoicesList.AddOptions(opts);

        dm.BtnSpeak.onClick.AddListener(() => { 
            TTSScript.text = dm.SpeakText.text;
            TTSScript.Speak();
        });

        dm.BtnSetVoice.onClick.AddListener(() => {
            var y = dm.VoicesList.options[dm.VoicesList.value].text;
            Debug.Log(y);
            TTSScript.voice = kvp[y];
        });
    }
}
