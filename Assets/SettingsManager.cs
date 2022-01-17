using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    public class SettingsManager : ManagerBase<SettingsManager>
    {
        [SerializeField] internal AppSettings settings;
        [SerializeField] internal SceneDependencies dependencies;

        private void Awake()
        {
            ApplySettings();
        }

        internal static AppSettings _Settings { get => Instance.settings; }
        internal static SceneDependencies _Dependencies { get => Instance.dependencies; }

        void ReadExistingSettings()
        {
            settings.PathToAuthFile = PlayerPrefs.GetString("PathToAuthFile");
            settings.Mode = (AppSettings.ScreenMode)PlayerPrefs.GetInt("ScreenMode");
            settings.AllowAudienceSkip = PlayerPrefs.GetInt("AllowAudienceSkip") == 0 ? false : true;
            settings.AllowAudienceSkipAmountOfVotesRequired = PlayerPrefs.GetInt("AllowAudienceSkipAmountOfVotesRequired");
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString("PathToAuthFile", settings.PathToAuthFile);
            PlayerPrefs.SetInt("ScreenMode", (int)settings.Mode);
            PlayerPrefs.SetInt("AllowAudienceSkip", settings.AllowAudienceSkip == true ? 1 : 0);
            PlayerPrefs.SetInt("AllowAudienceSkipAmountOfVotesRequired", settings.AllowAudienceSkipAmountOfVotesRequired);
        }

        public void ApplySettings()
        {
            ReadExistingSettings();

            switch(settings.Mode)
            {
                case AppSettings.ScreenMode.Transparent:
                    dependencies.TransparentScreen.enabled = true;
                    break;
            }

            // settings.AllowAudienceSkip == true ? 1 : 0
            // settings.AllowAudienceSkipAmountOfVotesRequired
        }

        [System.Serializable] public class AppSettings {
            /* contents of c:/temp/chatbot.txt
             {
	            "secret":"________",
	            "clientId":"________",
	            "oauth":"________",
	            "accessToken":"________",
	            "refreshToken":"________",
	            "listenAuth": "________",
	            "channelId": "________",
	            "ChannelToConnectTo": "________",
	            "BotName": "________"
            }
             */

            [SerializeField] internal ScreenMode Mode;

            [Header("Auth")]
            [SerializeField] internal string PathToAuthFile = "";
            [SerializeField] internal bool UseFallback = false;

            [Header("Bot")]
            [SerializeField] internal bool SendConnectedMessage = false;

            [Header("Skipping")]
            [SerializeField] internal int AllowAudienceSkipAmountOfVotesRequired = 3;
            [SerializeField] internal bool AllowAudienceSkip = false;

            [Header("Features")]
            [SerializeField] internal bool AllowPauseResume = true;
            [SerializeField] internal bool ReplyToButtsbot = true;
            [SerializeField] internal bool BardakifyHis = true;

            [SerializeField] internal bool AntiBitGameyMode = true;

            [SerializeField] internal bool ReplaceStrings = false;
            [SerializeField] internal string StringToReplace = "john";
            [SerializeField] internal string StringToReplaceWith = "me";

            [SerializeField] internal bool TtsForEveryChatMessage = false;

            [SerializeField] internal List<string> IgnoreUserList = new List<string>() {
                "StreamElements", "Streamlabs", "Nightbot", "NightBot"
            };

            public enum ScreenMode : byte { Transparent }

            private static JObject SecretsParsed;
            internal string GetSettingFromSecrets(string name)
            {
                if (SecretsParsed == null) SecretsParsed = JObject.Parse(System.IO.File.ReadAllText(_Settings.PathToAuthFile));

                return SecretsParsed[name].ToString();
            }
        }

        [System.Serializable]
        public class SceneDependencies
        {
            [SerializeField] public TransparentWindow TransparentScreen;
            [SerializeField] public UnityEngine.UI.Image GreenScreen;
            [SerializeField] public StreamElementsTTS_Unity.StreamElementsTtsUtterance UtteranceScript;
            // [SerializeField] public StreamElementsTTS_Unity.StreamElementsTTSApi ApiScript;
            [SerializeField] public CoreTwitchLibSetup.TwitchLibCtrl TwitchLibShite;
            [SerializeField] public StreamElementsTTS_Unity.TalkingSprite TalkingSprite;
            [SerializeField] public AudioSource ShutUp;
            [SerializeField] public List<StreamElementsTTS_Unity.TtsCharacter> AllCharacters;
        }
    }

    public class ManagerBase<T> : MonoBehaviour where T : Component
    {
        static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();

                return _instance;
            }
        }
    }
}