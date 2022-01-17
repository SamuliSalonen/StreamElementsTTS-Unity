using TwitchLib.PubSub.Events;
using static Settings.SettingsManager;
using static Logger;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl
    {
        public class PubSubCommandHandler
        {
            internal void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
            {
                Log($"Reward Redeemed listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardTitle}");
                if (_Settings.UseFallback) OnChannelPointRedemption(e.RewardTitle, e.Message);
            }

            internal void OnChannelPointsReceived(object sender, OnChannelPointsRewardRedeemedArgs e)
            {
                Log($"Channel Point received listener triggered.. Use Fallback: { _Settings.UseFallback } || {e.RewardRedeemed.Redemption.Reward.Title}");
                if (!_Settings.UseFallback) OnChannelPointRedemption(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.Reward.Prompt);
            }

            private void OnChannelPointRedemption(string rewardTitle, string ttsText)
            {
                Log("Redemption detected.. " + rewardTitle);
                if (rewardTitle.Equals("Text to Speech!")) Messages.Enqueue(ttsText);
            }

        }
    }
}
