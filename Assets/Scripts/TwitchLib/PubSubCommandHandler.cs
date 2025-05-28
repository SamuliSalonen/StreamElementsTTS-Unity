using TwitchLib.PubSub.Events;
using static Settings.SettingsManager;
using static Logger;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Linq;
using static Constants;
using TwitchLib.Unity;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl
    {
        public class PubSubCommandHandler
        {
            private Client _client;

            public PubSubCommandHandler(Client client)
            {
                _client = client;
            }

            internal void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
            {
                Log($"Reward Redeemed listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardTitle}");
                if (_Settings.UseFallback && !e.Status.Equals("ACTION_TAKEN")) // don't tts again when the redeem was accepted or rejected
                    OnChannelPointRedemption(e.RewardTitle, e.Message, e.DisplayName);
            }

            internal void OnChannelPointsReceived(object sender, OnChannelPointsRewardRedeemedArgs e)
            {
                Log($"Channel Point received listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardRedeemed.Redemption.Reward.Title}");
                if (!_Settings.UseFallback)
                    OnChannelPointRedemption(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.Reward.Prompt, e.RewardRedeemed.Redemption.User.DisplayName);
            }

            private void OnChannelPointRedemption(string rewardTitle, string ttsText, string user)
            {
                Log("Redemption detected.. " + rewardTitle);
                if (rewardTitle.Equals("Text to Speech!"))
                {
                    if (user.Equals(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO)))
                        Messages.Enqueue(ttsText);
                    else
                    {
                        if (_Settings.AntiBitGameyMode && ttsText.Contains("777"))
                        {
                            // Anti-BitGamey
                            Messages.Enqueue($"Feck off with your sevens {user}!");
                            return;
                        }
                        else
                        {
                            Messages.Enqueue(ttsText);
                        }
                    }
                }
                else
                {
                    // check if there are readout commands and if the title matches a command
                    for (int i = 0; i < _Settings.ReadOutCommands.Count; i++)
                        if (rewardTitle.Equals(_Settings.ReadOutCommands[i]))
                            if (File.Exists(_Settings.ReadOutCommandFilePaths[i]))
                            {
                                // read out and send a random line
                                string[] lines = File.ReadAllLines(_Settings.ReadOutCommandFilePaths[i]);
                                Random rand = new Random();
                                var line = lines[rand.Next(lines.Length)];
                                Messages.Enqueue(line);
                                _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), line);
                            }
                            else
                                Messages.Enqueue("Hey, that file doesn't exist!");
                }
            }

        }
    }
}
