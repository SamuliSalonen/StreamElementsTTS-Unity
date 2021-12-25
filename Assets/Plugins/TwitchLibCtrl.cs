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

namespace CoreTwitchLibSetup
{
    public class TwitchLibCtrl : MonoBehaviour
    {


        [SerializeField]
        private string _channelToConnectTo = "clayman6666";

        private Client _client;

        private Api _api;


        string botName = "claybot6";

        string client_id = "b1v0wdn4lwndiqp3jlr22qvaeza22b";

        string apiKey = "oauth:ff086d8s6a3nagrz2l7t0rwsgqd37g";

        private void Start()
        {
            Application.runInBackground = true;

            ConnectionCredentials credentials = new ConnectionCredentials(botName, apiKey);
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
            _api.Settings.ClientId = client_id;//auth.client_id;
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
            if (m_Messages.Count > 0)
            {
                msg = m_Messages.Dequeue();
                return true;
            }

            msg = null;
            return false;
        }
        Queue<string> m_Messages = new Queue<string>();
        private void OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
        {


            //switch statement based on command text
            switch (e.Command.CommandText)
            {
                //in case typing !join
                case "tts":


                    m_Messages.Enqueue(e.Command.ArgumentsAsString);
                    break;
                default:
                    break;
            }
        }


    }
}