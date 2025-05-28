using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            GetExtraCharacters();
        }

        private void GetExtraCharacters()
        {
            string extraCharacterPath = Path.Combine(Application.persistentDataPath, "Characters");
            if (!Directory.Exists(extraCharacterPath)) return;

            foreach (var d in Directory.EnumerateDirectories(extraCharacterPath))
            {
                DirectoryInfo di = new DirectoryInfo(d);

                var OpenM = di.GetDirectories().SingleOrDefault(o => o.Name == "Open");
                var ClosedM = di.GetDirectories().SingleOrDefault(o => o.Name == "Closed");

                var opens = FileUtils.GetSpritesFromDirectory(OpenM);
                var closeds = FileUtils.GetSpritesFromDirectory(ClosedM);
                var character = ScriptableObject.CreateInstance<StreamElementsTTS_Unity.TtsCharacter>();

                character.name = di.Name;

                character.silent = closeds.ToArray();
                character.talk = opens.ToArray();

                dependencies.AllCharacters.Add(character);
            }
        }

        internal static AppSettings _Settings { get => Instance.settings; }
        internal static SceneDependencies _Dependencies { get => Instance.dependencies; }

        void ReadExistingSettings()
        {
<<<<<<< HEAD
            settings.PathToAuthFile = "C:/temp/chatbot.txt"; //PlayerPrefs.GetString("PathToAuthFile");
            settings.Mode = AppSettings.ScreenMode.Transparent;//(AppSettings.ScreenMode)PlayerPrefs.GetInt("ScreenMode");
            settings.AllowAudienceSkip = true;//PlayerPrefs.GetInt("AllowAudienceSkip") == 0 ? false : true;
            settings.AllowAudienceSkipAmountOfVotesRequired = 1;//PlayerPrefs.GetInt("AllowAudienceSkipAmountOfVotesRequired");
=======
            var pathToAuth = PlayerPrefs.GetString("PathToAuthFile");
            if (string.IsNullOrEmpty(pathToAuth))
                SaveSettings();

            settings.PathToAuthFile = PlayerPrefs.GetString("PathToAuthFile");
            settings.Mode = (AppSettings.ScreenMode)PlayerPrefs.GetInt("ScreenMode");
            settings.AllowAudienceSkip = PlayerPrefs.GetInt("AllowAudienceSkip") == 0 ? false : true;
            settings.AllowAudienceSkipAmountOfVotesRequired = PlayerPrefs.GetInt("AllowAudienceSkipAmountOfVotesRequired");
            settings.PathToCommandsList = PlayerPrefs.GetString("PathToCommandsList");
            settings.UseCommandsList = PlayerPrefs.GetInt("UseCommandsList") == 0 ? false : true;
>>>>>>> 4b5706d (Implemented ChatCommands via the bot from a file; Implemented read out commands for JokeDaky and things like Inspiread)
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString("PathToAuthFile", settings.PathToAuthFile);
            PlayerPrefs.SetInt("ScreenMode", (int)settings.Mode);
            PlayerPrefs.SetInt("AllowAudienceSkip", settings.AllowAudienceSkip == true ? 1 : 0);
            PlayerPrefs.SetInt("AllowAudienceSkipAmountOfVotesRequired", settings.AllowAudienceSkipAmountOfVotesRequired);
            PlayerPrefs.SetString("PathToCommandsList", settings.PathToCommandsList);
            PlayerPrefs.SetInt("UseCommandsList", settings.UseCommandsList == true ? 1 : 0);
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

            [SerializeField] internal List<string> ReadOutCommands = new List<string>();
            [SerializeField] internal List<string> ReadOutCommandFilePaths = new List<string>();

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

            [SerializeField] internal bool TTSViaChatCommand = false;

            [SerializeField] internal bool ReplaceStrings = false;
            [SerializeField] internal string StringToReplace = "john";
            [SerializeField] internal string StringToReplaceWith = "me";

            [SerializeField] internal bool TtsForEveryChatMessage = false;

            [SerializeField] internal List<string> IgnoreUserList = new List<string>() {
                "StreamElements", "Streamlabs", "Nightbot", "NightBot"
            };

            [SerializeField] internal bool UseCommandsList = false;
            [SerializeField] internal string PathToCommandsList = "";

            public enum ScreenMode : byte { Transparent }

            private static JObject SecretsParsed;
            internal string GetSettingFromSecrets(string name)
            {
                if (SecretsParsed == null) SecretsParsed = JObject.Parse(System.IO.File.ReadAllText(_Settings.PathToAuthFile));

                return SecretsParsed[name].ToString();
            }

            [SerializeField] internal string ExtraCharactersPath = "";
        }

        [System.Serializable]
        public class SceneDependencies
        {
            [SerializeField] public TransparentWindow TransparentScreen;
            [SerializeField] public UnityEngine.UI.Image GreenScreen;
            [SerializeField] public StreamElementsTTS_Unity.StreamElementsTtsUtterance UtteranceScript;
            [SerializeField] public CoreTwitchLibSetup.TwitchLibCtrl TwitchLibShite;
            [SerializeField] public StreamElementsTTS_Unity.TalkingSprite TalkingSprite;
            [SerializeField] public AudioSource ShutUp;
            [SerializeField] public List<StreamElementsTTS_Unity.TtsCharacter> AllCharacters;
        }

        [System.Serializable]
        public class ChatCommand
        {
            // data structure for the chat commands file
            /*
            [
              {
                "Name": "lurk",
                "Response": "lurkresponse",
	            "RequireElevatedPermission": "false"
              },
              {
                "Name": "socials",
                "Response": "socialsresponse",
	            "RequireElevatedPermission": "true"
              }
            ]
            */

            // available parameters for the response:
            // $User$           -> User that sent the message, e.g. "$User$ is now lurking"
            // $Message$        -> Original message after the command, e.g. "$User$ will be back in $Message$ time"
            // $Channel$        -> Creates a twitch link to the users channel that sent the command, e.g. "Check out $Message$ over at $Channel$"

            public string Name;
            public string Response;
            public bool RequireElevatedPermission;
        }
    }
}