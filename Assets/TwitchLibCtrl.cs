using System;
using System.Collections;
using TwitchLib.Api.V5.Models.Users;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using TwitchLib.Client.Events;
using TMPro;
using Newtonsoft.Json.Linq;
using static Settings.SettingsManager;
using static Constants;

namespace CoreTwitchLibSetup
{
    public class TwitchLibCtrl : MonoBehaviour
    {
        internal TtsSkipHandler ttsSkipHandler = new TtsSkipHandler();

        public static Queue<string> Messages = new Queue<string>();

        private Client _client;
        private Api _api;
        private PubSub _pubSub;
        private string _channelId;

        internal static Action OnMessageWasSkipped;

        private void Start()
        {
            OnMessageWasSkipped += () => {
                // Debug.Log("Something was skipped");
            };

            Messages = new Queue<string>();
            var secretsJson = System.IO.File.ReadAllText(_Settings.PathToAuthFile);

            var secretsParsed = JObject.Parse(secretsJson);

            Application.runInBackground = true;

            ConnectionCredentials credentials = new ConnectionCredentials(_Settings.BotName, secretsParsed[OauthKeywords.OAUTH].ToString());

            //setup irc client to connect to twitch
            _client = new Client();
            _client.Initialize(credentials, _Settings.ChannelToConnectTo);
            _client.OnConnected += OnConnected;
            _client.OnConnectionError += OnConnectionError;
            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnChatCommandReceived += OnChatCommandReceived;
            _client.Connect();

            _pubSub = new PubSub();
            _pubSub.OnPubSubServiceError += OnPubSubServiceError;
            _pubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            _pubSub.OnListenResponse += OnListenResponse;
            _pubSub.OnChannelPointsRewardRedeemed += OnChannelPointsReceived;

            _pubSub.OnPubSubServiceConnected += (object sender, EventArgs e) => 
            {
                StartCoroutine(_api.InvokeAsync(_api.Helix.Users.GetUsersAsync(logins: new List<string> { _Settings.ChannelToConnectTo }).ContinueWith(t =>
                {
                    _channelId = t.Result.Users.FirstOrDefault().Id;

                    if (_Settings.UseFallback)
                        _pubSub.ListenToRewards(_channelId); // GOOD AS A FALLBACK FOR DEBUG.
                    else
                        _pubSub.ListenToChannelPoints(_channelId);

                    _pubSub.SendTopics(/*secretsParsed["oauth"].ToString()*/);

                    Debug.Log($"Connected to channel with id: {_channelId}");
                })));
            };

            _api = new Api();
            _api.Settings.ClientId = secretsParsed[OauthKeywords.CLIENT_ID].ToString();
            _api.Settings.AccessToken = secretsParsed[OauthKeywords.CLIENT_SECRET].ToString();
            return;
            StartCoroutine(GetAccessToken(secretsParsed[OauthKeywords.CLIENT_ID].ToString(), secretsParsed[OauthKeywords.CLIENT_SECRET].ToString(), (token) =>
            {
                _api.Settings.AccessToken = token;

                // only connect once we have the token
                _pubSub.Connect();
            }));
        }

        IEnumerator GetAccessToken(string clientId, string clientSecret, Action<string> callback)
        {
            var form = new WWWForm();
            form.AddField("grant_type", "client_credentials");
            form.AddField("client_id", clientId);
            form.AddField("client_secret", clientSecret);

            using (UnityWebRequest www = UnityWebRequest.Post("https://id.twitch.tv/oauth2/token", form))
            {
                yield return www.SendWebRequest();
                var json = JObject.Parse(www.downloadHandler.text);
                callback(json["access_token"].ToString());
            }
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
            if (e.Successful)
                Debug.Log("Listening"); // Debug.Log($"Successfully verified listening to topic: {e.Topic}");
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

        public static bool TTSPaused = false;

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

                    Messages.Enqueue(e.Command.ArgumentsAsString);
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
                default:
                    break;
            }
        }

        bool SenderHasElevatedPermissions(OnChatCommandReceivedArgs e) => e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster || e.Command.ChatMessage.IsVip;
    }
}

public class Constants {
    public class Commands {
        internal const string SKIP = "skip";
        internal const string TTS = "tts";
        internal const string Pause = "pause";
        internal const string Resume = "resume";
    }

    public class OauthKeywords {
        internal const string CLIENT_ID = "clientId";
        internal const string CLIENT_SECRET = "secret";
        internal const string OAUTH = "oauth";
    }
}

public class TtsSkipHandler
{
    internal Action<ChatMessage> OnSkipMessageReceived;
    public TtsSkipHandler() => OnSkipMessageReceived += OnSkip_MessageReceived;

    List<string> Voters = new List<string>();

    int currentVoteAmount = 0;
    internal void ResetVoteAmount() {
        Voters = new List<string>();
        currentVoteAmount = 0;
    }

    void IncrementVoteAmount(string voter) {
        Voters.Add(voter);
        currentVoteAmount++;
    }

    void OnSkip_MessageReceived(ChatMessage cm)
    {
        if (cm.IsBroadcaster || cm.IsModerator)
        {
            SkipCurrentMessage();
            return;
        }

        if (Voters.Contains(cm.DisplayName))
            return;

        IncrementVoteAmount(cm.DisplayName);
    
        var amtRequired = _Settings.AllowAudienceSkipAmountOfVotesRequired;
        if (currentVoteAmount >= amtRequired)
        {
            SkipCurrentMessage();
            ResetVoteAmount();

            Output($"{currentVoteAmount}/{amtRequired}");
        }
    }

    internal void SkipCurrentMessage()
    {
        var utterance = _Dependencies.UtteranceScript;
        utterance.audioSource.Stop();

        CoreTwitchLibSetup.TwitchLibCtrl.OnMessageWasSkipped?.Invoke();

        _Dependencies.TwitchLibShite.SendMessageFromBot("Skipped!");
        _Dependencies.ShutUp.Play();
    }

    void Output(string msg) => Debug.Log($"[SkipHandler] {msg}");
}