using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CoreTwitchLibSetup;

using System.IO;

public class TestHarnessCanvasController : MonoBehaviour
{
    [SerializeField] private DisplayMembers dm;
    [SerializeField] private StreamElementsTTS_Unity.StreamElementsTtsUtterance TTSScript; 
    
    [System.Serializable] private class DisplayMembers
    {
        [SerializeField] internal Image TestSprite;

        [SerializeField] internal InputField SpeakText;
        [SerializeField] internal Button BtnSpeak;

        [SerializeField] internal Dropdown VoicesList;
        [SerializeField] internal Button BtnSetVoice;

        [SerializeField] internal Button BtnSkipCurrent;
        [SerializeField] internal Button BtnTogglePause;

        [SerializeField] internal Dropdown Characters;

        [SerializeField] internal Button BtnLoadExtras;
    }

    Dictionary<string, StreamElementsTTS_Unity.TtsVoices> VoicesMap = new Dictionary<string, StreamElementsTTS_Unity.TtsVoices>();

    private void Start()
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

        dm.BtnLoadExtras.onClick.AddListener(() =>
        {
            foreach (var d in Directory.EnumerateDirectories(Path.Combine(Application.persistentDataPath, "Characters")))
            {
                DirectoryInfo di = new DirectoryInfo(d);

                var OpenM = di.GetDirectories().SingleOrDefault(o => o.Name == "Open");
                var ClosedM = di.GetDirectories().SingleOrDefault(o => o.Name == "Closed");

                var opens = GetSpritesFromDirectory(OpenM);
                var closeds = GetSpritesFromDirectory(ClosedM);
                var character = ScriptableObject.CreateInstance<StreamElementsTTS_Unity.TtsCharacter>();

                character.name = di.Name;

                character.silent = closeds.ToArray();
                character.talk = opens.ToArray();

                Settings.SettingsManager.Instance.dependencies.AllCharacters.Add(character);
            }

            CharacterChanger();
        });
    }

    private List<Sprite> GetSpritesFromDirectory(DirectoryInfo OpenM)
    {
        List<Sprite> rtn = new List<Sprite>();
        if (OpenM != null)
            foreach (var file in OpenM.EnumerateFiles().Where(o => o.Name.EndsWith("png")))
            {
                var tex = LoadPNG(file.FullName);
                rtn.Add(Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f));
            }
        return rtn;
    }

    Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
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
        dm.Characters.ClearOptions();

        List<Dropdown.OptionData> optsForCharactersList = new List<Dropdown.OptionData>();
        foreach (var c in Settings.SettingsManager.Instance.dependencies.AllCharacters) {
            optsForCharactersList.Add(new Dropdown.OptionData(c.name.ToString()));
        }

        dm.Characters.AddOptions(optsForCharactersList);
        dm.Characters.onValueChanged.AddListener(o => {
            var y = dm.Characters.options[o].text;
            GameObject.FindObjectOfType<StreamElementsTTS_Unity.TalkingSprite>().m_ActiveCharacter
             = Settings.SettingsManager.Instance.dependencies.AllCharacters.First(t => t.name == y);
        });
    }
}
