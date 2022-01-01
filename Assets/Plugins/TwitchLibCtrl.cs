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
using System.Threading.Tasks;

namespace CoreTwitchLibSetup
{
    public class TwitchLibCtrl : MonoBehaviour
    {
        [SerializeField]
        private string _channelToConnectTo = "clayman666";

        private Client _client;

        private Api _api;

        [SerializeField]
        string botName = "irishjerngames";

        private void Start()
        {
            Messages = new Queue<string>();
            var secretsJson = System.IO.File.ReadAllText("C:/Temp/chatbot.txt");
            var secretsParsed = JObject.Parse(secretsJson);

            Application.runInBackground = true;

            ConnectionCredentials credentials = new ConnectionCredentials(botName, secretsParsed["secret"].ToString());

            //setup irc client to connect to twitch
            _client = new Client();
            _client.Initialize(credentials, _channelToConnectTo);
            _client.OnConnected += OnConnected;
            _client.OnConnectionError += OnConnectionError;
            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnChatCommandReceived += OnChatCommandReceived;
            _client.Connect();

            _api = new Api();
            _api.Settings.ClientId = secretsParsed["clientId"].ToString();//auth.client_id;
          //  _api.Settings.AccessToken = secretsParsed["secret"].ToString();
            StartCoroutine(GetAccessToken(secretsParsed["clientId"].ToString(), secretsParsed["clientSecret"].ToString(), (token)=> {
                _api.Settings.AccessToken = token;
                GetUser();
            }));
           // 
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
                // print(www.downloadHandler.text);
                
            }
        }

        async void GetUser()
        {
            var usersResult = await _api.Helix.Users.GetUsersAsync(null, new List<string>() { "clayman666" });
            //    var userResult = await _api.V5.Users.GetUserByNameAsync("clayman666");
            foreach (var item in usersResult.Users)
            {
                print(item.Id);
            }

        }

        private void OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            print("connection error :" + e.Error.Message);
        }

        private void OnWhisper(object sender, OnWhisperArgs e) => Debug.Log($"{e.Whisper.Data}");


        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e.Successful) Debug.Log("Listening"); // Debug.Log($"Successfully verified listening to topic: {e.Topic}");
            else Debug.Log($"Failed to listen! Error: {e.Response.Error}");
        }
        [SerializeField]
        bool m_SendConnectedMessage = false;
        private void OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            if (m_SendConnectedMessage)
                _client.SendMessage(_channelToConnectTo, "connected");
        }

        private void OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            Debug.Log("joined");

        }

        private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {

            var message = e.ChatMessage.Message.ToLower().Replace("john", "me");
            Messages.Enqueue(message);

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
        public static Queue<string> Messages = new Queue<string>();
        private void OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
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