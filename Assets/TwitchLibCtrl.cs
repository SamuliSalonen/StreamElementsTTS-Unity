using System.Collections;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using static Settings.SettingsManager;
using static Constants;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl : MonoBehaviour
    {

        public static Queue<string> Messages = new Queue<string>();
        public static bool TTSPaused = false;

        internal static Client _client;
        internal static Api _api;
        internal static PubSub _pubSub;

        internal static readonly TtsSkipHandler TtsSkipHandler = new TtsSkipHandler();

        private readonly PubSubVerification pubSubConnectionHandler = new PubSubVerification();
        private readonly PubSubCommandHandler pubSubCommandHandler = new PubSubCommandHandler();

        private readonly IrcClientConnectionHandler ircClientConnectionHandler = new IrcClientConnectionHandler();
        internal readonly IrcClientChatHandler ircClientChatHandler = new IrcClientChatHandler();

        private void Start()
        {
            Application.runInBackground = true;

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
            _api.Settings.ClientId = _Settings.GetSettingFromSecrets(OauthKeywords.CLIENT_ID);
        }

        private void SetupPubSub()
        {
            _pubSub = new PubSub();
            _pubSub.OnPubSubServiceConnected += pubSubConnectionHandler.OnPubSubServiceConnected;
            _pubSub.OnPubSubServiceError += pubSubConnectionHandler.OnPubSubServiceError;
            _pubSub.OnPubSubServiceClosed += pubSubConnectionHandler.OnPubSubServiceClosed;
            _pubSub.OnListenResponse += pubSubConnectionHandler.OnListenResponse;

            _pubSub.OnChannelPointsRewardRedeemed += pubSubCommandHandler.OnChannelPointsReceived;
            _pubSub.OnRewardRedeemed += pubSubCommandHandler.OnRewardRedeemed;
            pubSubConnectionHandler.Connect();
        }

        private void SetupIRC()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(_Settings.GetSettingFromSecrets(SettingsFromJson.BOT_NAME), _Settings.GetSettingFromSecrets(OauthKeywords.ACCESS_TOKEN));
            _client = new Client();
            _client.Initialize(credentials, _Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO));
            _client.OnConnected += ircClientConnectionHandler.OnConnected;
            _client.OnConnectionError += ircClientConnectionHandler.OnConnectionError;
            _client.OnJoinedChannel += ircClientConnectionHandler.OnJoinedChannel;

            _client.OnMessageReceived += ircClientChatHandler.OnMessageReceived;
            _client.OnChatCommandReceived += ircClientChatHandler.OnChatCommandReceived;
            ircClientConnectionHandler.Connect();
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
                TtsSkipHandler.ResetVoteAmount();
                return true;
            }

            msg = null;
            return false;
        }

    }
}
