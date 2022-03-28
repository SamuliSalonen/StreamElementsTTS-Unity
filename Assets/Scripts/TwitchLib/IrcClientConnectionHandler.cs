using TwitchLib.Client.Events;
using static Settings.SettingsManager;
using static Constants;
using static Logger;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl
    {
        private class IrcClientConnectionHandler
        {
            internal void OnConnected(object sender, OnConnectedArgs e) {
             //   if (_Settings.SendConnectedMessage) _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), "connected");
            }

            internal void OnConnectionError(object sender, OnConnectionErrorArgs e) => Log("Connection Error: " + e.Error.Message);
            internal void OnJoinedChannel(object sender, OnJoinedChannelArgs e) => Log("Bot joined Channel.");

            internal void Connect() => _client.Connect();
        }
    }
}
