using System;
using System.Collections;
using TwitchLib.Api.V5.Models.Users;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using TwitchLib.Client.Events;
using TMPro;
using Newtonsoft.Json.Linq;

namespace CoreTwitchLibSetup
{
    public class TwitchLibCtrl : MonoBehaviour
    {
        [SerializeField]
        private string _channelToConnectTo = "clayman666";

        [SerializeField]
        string botName = "irishjerngames";

        [SerializeField]
        bool m_SendConnectedMessage = false;

        [SerializeField]
        string stringToReplace = "john";

        [SerializeField]
        string stringToReplaceWith = "me";

        [SerializeField]
        bool replaceStrings = false;

        [SerializeField]
        bool ttsForEveryChatMessage = false;

        [SerializeField]
        bool useFallback = false;

        public static Queue<string> Messages = new Queue<string>();

        private Client _client;

        private Api _api;

        private PubSub _pubSub;

        private string _channelId;

        private void Start()
        {
            Messages = new Queue<string>();
            var secretsJson = System.IO.File.ReadAllText("D:/Unity/Chatbot.txt");
            var secretsParsed = JObject.Parse(secretsJson);

            Application.runInBackground = true;

            ConnectionCredentials credentials = new ConnectionCredentials(botName, secretsParsed["oauth"].ToString());

            //setup irc client to connect to twitch
            _client = new Client();
            _client.Initialize(credentials, _channelToConnectTo);
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
                StartCoroutine(_api.InvokeAsync(_api.Helix.Users.GetUsersAsync(logins: new List<string> { _channelToConnectTo }).ContinueWith(t =>
                {
                    _channelId = t.Result.Users.FirstOrDefault().Id;

                    if (useFallback)
                        _pubSub.ListenToRewards(_channelId); // GOOD AS A FALLBACK FOR DEBUG.
                    else
                        _pubSub.ListenToChannelPoints(_channelId);

                    _pubSub.SendTopics(/*secretsParsed["oauth"].ToString()*/);

                    Debug.Log($"Connected to channel with id: {_channelId}");
                })));
            };

            _api = new Api();
            _api.Settings.ClientId = secretsParsed["clientId"].ToString();
            _api.Settings.AccessToken = secretsParsed["clientSecret"].ToString();
            StartCoroutine(GetAccessToken(secretsParsed["clientId"].ToString(), secretsParsed["clientSecret"].ToString(), (token) =>
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
            Debug.Log($"Reward Redeemed listener triggered.. Use Fallback: { useFallback } || {e.RewardTitle}");

            if (useFallback)
                OnChannelPointRedemption(e.RewardTitle, e.Message);
        }

        private void OnChannelPointsReceived(object sender, OnChannelPointsRewardRedeemedArgs e)
        {
            Debug.Log($"Channel Point received listener triggered.. Use Fallback: { useFallback } || {e.RewardRedeemed.Redemption.Reward.Title}");

            if (!useFallback)
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
            if (m_SendConnectedMessage)
                _client.SendMessage(_channelToConnectTo, "connected");
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Debug.Log("joined");

        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (ttsForEveryChatMessage)
                if (replaceStrings)
                    Messages.Enqueue(e.ChatMessage.Message.ToLower().Replace(stringToReplace, stringToReplaceWith));
                else
                    Messages.Enqueue(e.ChatMessage.Message);

            /*
			MessagesReceivedIRC.Add(new MessageCache()
			{
				index = MessagesReceivedIRC.Count,
				captain = e.ChatMessage.Username,
				shipName = e.ChatMessage.Message
			});

			bufferTime = Time.time + BUFFER_TIME_INCREMENT;
			*/
        }

        public bool GetNextMessage(out string msg)
        {
            if (Messages.Count > 0)
            {
                msg = Messages.Dequeue();
                return true;
            }

            msg = null;
            return false;
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            //switch statement based on command text
            switch (e.Command.CommandText)
            {
                //in case typing !join
                case "tts":
                    Messages.Enqueue(e.Command.ArgumentsAsString);
                    break;
                default:
                    break;
            }
        }
    }
}
