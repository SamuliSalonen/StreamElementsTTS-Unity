﻿using System.Linq;
using TwitchLib.Client.Events;
using static Settings.SettingsManager;
using static Constants;

namespace CoreTwitchLibSetup
{
    public partial class TwitchLibCtrl
    {
        internal class IrcClientChatHandler
        {
            internal void SendMessageFromBot(string msg) => _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), msg);

            bool SenderHasElevatedPermissions(OnChatCommandReceivedArgs e) => e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster || e.Command.ChatMessage.IsVip;

            internal void OnMessageReceived(object sender, OnMessageReceivedArgs e)
            {
                if (e.ChatMessage.DisplayName.StartsWith("buttsbot") && e.ChatMessage.Message != ":D")
                {
                    Messages.Enqueue(e.ChatMessage.Message.ToLower());

                    if (_Settings.ReplyToButtsbot)
                        _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), "buttsbot yes");
                }

                if (_Settings.IgnoreUserList.Contains(e.ChatMessage.DisplayName)) return;

                if (_Settings.BardakifyHis && (e.ChatMessage.Message.ToLower().StartsWith("im") || e.ChatMessage.Message.ToLower().StartsWith("i'm")))
                {
                    string[] x = e.ChatMessage.Message.Split(' ');
                    if (x.Length > 1)
                        _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), $"Hi {x[1]}, I'm Bardaky.");
                }

                if (_Settings.TtsForEveryChatMessage)
                    if (_Settings.ReplaceStrings)
                        Messages.Enqueue(e.ChatMessage.Message.ToLower().Replace(_Settings.StringToReplace, _Settings.StringToReplaceWith));
                    else
                        Messages.Enqueue(e.ChatMessage.Message);
            }

            internal void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
            {
                //switch statement based on command text
                if (_Settings.IgnoreUserList.Contains(e.Command.ChatMessage.DisplayName)) return;

                switch (e.Command.CommandText.ToLower())
                {
                    case Commands.TTS:
                        if (_Settings.AntiBitGameyMode && e.Command.ChatMessage.Message.Contains("777"))
                        {
                            // Anti-BitGamey
                            Messages.Enqueue("Feck off with your sevens BitGamey!");
                            return;
                        }

                        if (_Dependencies.TalkingSprite != null && _Dependencies.TalkingSprite.m_ActiveCharacter != null && _Dependencies.TalkingSprite.m_ActiveCharacter.name == "frog_daky")
                        {
                            string dakified = "";

                            foreach (var word in e.Command.ArgumentsAsString.Split(' '))
                            {
                                if (UnityEngine.Random.Range(0, 50) > 30)
                                    dakified += word + "daky" + ' ';
                                else
                                    dakified += word + ' ';
                            }

                            Messages.Enqueue(dakified);
                        }
                        else
                        {
                            Messages.Enqueue(e.Command.ArgumentsAsString);
                        }

                        break;
                    case Commands.SKIP:
                        if (_Settings.AllowAudienceSkip)
                            TtsSkipHandler.OnSkipMessageReceived?.Invoke(e.Command.ChatMessage);
                        break;
                    case Commands.Pause:
                        if (_Settings.AllowPauseResume && SenderHasElevatedPermissions(e))
                        {
                            TTSPaused = true;
                            _Dependencies.UtteranceScript.audioSource.Pause();
                        }
                        break;
                    case Commands.Resume:
                        if (_Settings.AllowPauseResume && SenderHasElevatedPermissions(e))
                        {
                            TTSPaused = false;
                            _Dependencies.UtteranceScript.audioSource.UnPause();
                            _Dependencies.TalkingSprite.isSpeaking = true;
                        }
                        break;
                    case Commands.Character:
                        if (SenderHasElevatedPermissions(e))
                        {
                            var character = _Dependencies.AllCharacters.FirstOrDefault(o => o.name == e.Command.ArgumentsAsString);
                            if (character != null)
                                _Dependencies.TalkingSprite.m_ActiveCharacter = character;
                            else
                                _client.SendMessage(_Settings.GetSettingFromSecrets(SettingsFromJson.CHANNEL_TO_CONNECT_TO), $"No character called [{e.Command.ArgumentsAsString}] found.");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
