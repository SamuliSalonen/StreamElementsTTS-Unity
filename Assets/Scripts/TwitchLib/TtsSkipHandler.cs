using System;
using TwitchLib.Client.Models;
using UnityEngine;
using System.Collections.Generic;
using static Settings.SettingsManager;

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

        _Dependencies.TwitchLibShite.ircClientChatHandler.SendMessageFromBot("Skipped!");
        _Dependencies.ShutUp.Play();
    }

    void Output(string msg) => Debug.Log($"[SkipHandler] {msg}");
}