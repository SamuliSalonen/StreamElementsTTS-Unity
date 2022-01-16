using System;
using System.Collections;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using TwitchLib.Client.Events;
using Newtonsoft.Json.Linq;
using static Settings.SettingsManager;
using static Constants;
using static Logger;

namespace CoreTwitchLibSetup
{
    public class TwitchLibCtrl : MonoBehaviour
    {
        internal TtsSkipHandler ttsSkipHandler = new TtsSkipHandler();

        // private TwitchAdminCommandReceiver adminCommandReceiver;
        // private TwitchCommandReceiver commandReceiver;
        // private TwitchIRCReceiver ircReceiver;

        public static Queue<string> Messages = new Queue<string>();
        public static bool TTSPaused = false;

        private Client _client;
        private Api _api;
        private PubSub _pubSub;

        private static JObject SecretsParsed;
        string GetSetting(string name) => SecretsParsed[name].ToString();

        private void Start()
        {
            Application.runInBackground = true;

            var secretsJson = System.IO.File.ReadAllText(_Settings.PathToAuthFile);
            SecretsParsed = JObject.Parse(secretsJson);
            SetupMessageHandler();

            SetupIRC();
            SetupPubSub();
            SetupAPI();
        }

        private static void SetupMessageHandler()
        {
            Messages = new Queue<string>();
        }

        private void SetupAPI()
        {
            _api = new Api();
            _api.Settings.ClientId = GetSetting(OauthKeywords.CLIENT_ID);
        }

        private void SetupPubSub()
        {
            _pubSub = new PubSub();
            _pubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            _pubSub.OnPubSubServiceError += OnPubSubServiceError;
            _pubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            _pubSub.OnListenResponse += OnListenResponse;
            _pubSub.OnChannelPointsRewardRedeemed += OnChannelPointsReceived;
            _pubSub.OnRewardRedeemed += OnRewardRedeemed;
            _pubSub.Connect();
        }

        private void SetupIRC()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(_Settings.BotName, GetSetting(OauthKeywords.ACCESS_TOKEN));
            _client = new Client();
            _client.Initialize(credentials, _Settings.ChannelToConnectTo);
            _client.OnConnected += OnConnected;
            _client.OnConnectionError += OnConnectionError;
            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnChatCommandReceived += OnChatCommandReceived;
            _client.Connect();
        }

        public bool GetNextMessage(out string msg)
        {
            if (TTSPaused)
            {
                msg = null;
                return false;
            }

            if (Messages.Count > 0)
            {
                msg = Messages.Dequeue();
                ttsSkipHandler.ResetVoteAmount();
                return true;
            }

            msg = null;
            return false;
        }



        void OnPubSubServiceConnected(object sender, System.EventArgs e)
        {
            _pubSub.ListenToRewards(GetSetting(OauthKeywords.CHANNEL_ID));
            _pubSub.SendTopics(GetSetting(OauthKeywords.LISTEN_AUTH));
            _pubSub.ListenToChannelPoints(GetSetting(OauthKeywords.CHANNEL_ID));
        }

        private void OnPubSubServiceClosed(object sender, EventArgs e)
        {
            Debug.Log("CLOSED");
        }

        private void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Debug.Log("ERROR");
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            Debug.Log($"Reward Redeemed listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardTitle}");

            if (_Settings.UseFallback)
                OnChannelPointRedemption(e.RewardTitle, e.Message);
        }

        private void OnChannelPointsReceived(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            Debug.Log($"Channel Point received listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardRedeemed.Redemption.Reward.Title}");

            if (!_Settings.UseFallback)
                OnChannelPointRedemption(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.Reward.Prompt);
        }

        void OnChannelPointRedemption(string rewardTitle, string ttsText)
        {
            Log("Redemption detected.. " + rewardTitle);
            if (rewardTitle.Equals("Text to Speech!"))
                Messages.Enqueue(ttsText);
        }

        private void OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            print("connection error :" + e.Error.Message);
        }

        private void OnWhisper(object sender, OnWhisperArgs e) => Debug.Log($"{e.Whisper.Data}");

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            Debug.Log("Onlistenresp");
            if (e.Successful)
                Debug.Log($"Successfully verified listening to topic: {e.Topic}");
            else
                Debug.Log($"Failed to listen! Error: {e.Response.Error}");
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            if (_Settings.SendConnectedMessage)
                _client.SendMessage(_Settings.ChannelToConnectTo, "connected");
        }

        internal void SendMessageFromBot(string msg) => _client.SendMessage(_Settings.ChannelToConnectTo, msg);

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Debug.Log("joined");
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.DisplayName.StartsWith("buttsbot") && e.ChatMessage.Message != ":D")
            {
                Messages.Enqueue(e.ChatMessage.Message.ToLower());

                if(_Settings.ReplyToButtsbot)
                    _client.SendMessage(_Settings.ChannelToConnectTo, "buttsbot yes");
            }

            if (_Settings.IgnoreUserList.Contains(e.ChatMessage.DisplayName)) return;

            if (_Settings.BardakifyHis && (e.ChatMessage.Message.ToLower().StartsWith("im") || e.ChatMessage.Message.ToLower().StartsWith("i'm"))) {
                string[] x = e.ChatMessage.Message.Split(' ');
                if(x.Length > 1)
                    _client.SendMessage(_Settings.ChannelToConnectTo, $"Hi {x[1]}, I'm Bardaky.");
            }

            if (_Settings.TtsForEveryChatMessage)
                if (_Settings.ReplaceStrings)
                    Messages.Enqueue(e.ChatMessage.Message.ToLower().Replace(_Settings.StringToReplace, _Settings.StringToReplaceWith));
                else
                    Messages.Enqueue(e.ChatMessage.Message);
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            //switch statement based on command text
            if (_Settings.IgnoreUserList.Contains(e.Command.ChatMessage.DisplayName)) return;

            switch (e.Command.CommandText.ToLower())
            {
                case Commands.TTS:
                    if (_Settings.AntiBitGameyMode && e.Command.ChatMessage.Message.Contains("777")) {
                        // Anti-BitGamey
                        Messages.Enqueue("Feck off with your sevens BitGamey!");
                        return;
                    }

                    if(_Dependencies.TalkingSprite != null && _Dependencies.TalkingSprite.m_ActiveCharacter != null && _Dependencies.TalkingSprite.m_ActiveCharacter.name == "frog_daky")
                    {
                        string dakified = "";

                        foreach (var word in e.Command.ArgumentsAsString.Split(' '))
                        {
                            if(UnityEngine.Random.Range(0, 50) > 30)
                                dakified += word + "daky" + ' ';
                            else
                                dakified += word + ' ';
                        }

                        Messages.Enqueue(dakified);
                    } else
                    {
                        Messages.Enqueue(e.Command.ArgumentsAsString);
                    }

                    break;
                case Commands.SKIP:
                    if (_Settings.AllowAudienceSkip)
                        ttsSkipHandler.OnSkipMessageReceived?.Invoke(e.Command.ChatMessage);
                    break;
                case Commands.Pause:
                    if(_Settings.AllowPauseResume && SenderHasElevatedPermissions(e)) {
                        TTSPaused = true;
                        _Dependencies.UtteranceScript.audioSource.Pause();
                    }
                    break;
                case Commands.Resume:
                    if(_Settings.AllowPauseResume && SenderHasElevatedPermissions(e)) {
                        TTSPaused = false;
                        _Dependencies.UtteranceScript.audioSource.UnPause();
                        _Dependencies.TalkingSprite.isSpeaking = true;
                    }
                    break;
                case Commands.Character:
                    if(SenderHasElevatedPermissions(e))
                    {
                        var character = _Dependencies.AllCharacters.FirstOrDefault(o => o.name == e.Command.ArgumentsAsString);
                        if(character != null)
                            _Dependencies.TalkingSprite.m_ActiveCharacter = character;
                        else
                            _client.SendMessage(_Settings.ChannelToConnectTo, $"No character called [{e.Command.ArgumentsAsString}] found.");
                    }
                    break;
                default:
                    break;
            }
        }

        bool SenderHasElevatedPermissions(OnChatCommandReceivedArgs e) => e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster || e.Command.ChatMessage.IsVip;
    }
}
