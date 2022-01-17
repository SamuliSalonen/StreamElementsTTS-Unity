using System;
using TwitchLib.PubSub.Events;
using static Settings.SettingsManager;
using static Constants;
using static Logger;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl
    {
        private class PubSubVerification
        {
            internal void OnPubSubServiceConnected(object sender, System.EventArgs e)
            {
                _pubSub.ListenToRewards(_Settings.GetSettingFromSecrets(OauthKeywords.CHANNEL_ID));

                _pubSub.SendTopics(_Settings.GetSettingFromSecrets(OauthKeywords.LISTEN_AUTH));
                _pubSub.ListenToChannelPoints(_Settings.GetSettingFromSecrets(OauthKeywords.CHANNEL_ID));
            }

            internal void OnPubSubServiceClosed(object sender, EventArgs e) => Log("PubSub Service Closed.");
            internal void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e) => Log("Error in PubSub Service: " + e.Exception.Message);

            internal void OnListenResponse(object sender, OnListenResponseArgs e)
            {
                if (e.Successful)
                    Log($"Successfully verified listening to topic: {e.Topic}");
                else
                    Log($"Failed to listen! Error: {e.Response.Error}");
            }

            internal void Connect() => _pubSub.Connect();
        }
    }
}
