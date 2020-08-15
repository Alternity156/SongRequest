using Harmony;
using System;
using TwitchChatter;
using MelonLoader;

namespace AudicaModding
{
    internal static class Hooks
    {
        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                AudicaMod.menuState = state;
                if (!AudicaMod.panelButtonsCreated)
                {
                    if (!AudicaMod.buttonsBeingCreated && state == MenuState.State.SongPage) AudicaMod.CreateSongRequestFilterButton();
                    return;
                }
                if (state == MenuState.State.SongPage)
                {
                    MelonCoroutines.Start(AudicaMod.SetFilterSongRequestsButtonnActive(true));
                    MelonCoroutines.Start(AudicaMod.ProcessQueueCoroutine());
                }
                else if (state == MenuState.State.LaunchPage || state == MenuState.State.MainPage) MelonCoroutines.Start(AudicaMod.SetFilterSongRequestsButtonnActive(false));
            }
        }

        [HarmonyPatch(typeof(TwitchChatStream), "write_chat_msg", new Type[] { typeof(string) })]
        private static class PatchWriteChatMsg
        {
            private static void Prefix(TwitchChatStream __instance, string msg)
            {
                //MelonLogger.Log("TwitchChatStream: " + msg);
                if (msg.Substring(0, 1) == "@")
                {
                    if (msg.Contains("tmi.twitch.tv PRIVMSG "))
                    {
                        AudicaMod.ParsedTwitchMessage parsedMsg = AudicaMod.ParseTwitchMessage(msg);
                        AudicaMod.ParseCommand(parsedMsg.message);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterAll")]
        private static class PatchFilterAll
        {
            private static void Prefix(SongListControls __instance)
            {
                if (!AudicaMod.shootingFilterRequestsButton)
                {
                    AudicaMod.requestFilterActive = false;
                }
                else
                {
                    AudicaMod.shootingFilterRequestsButton = false;
                }
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterExtras")]
        private static class PatchFilterExtras
        {
            private static void Prefix(SongListControls __instance)
            {
                AudicaMod.requestFilterActive = false;
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterMain")]
        private static class PatchFilterMain
        {
            private static void Prefix(SongListControls __instance)
            {
                AudicaMod.requestFilterActive = false;
            }
        }

        [HarmonyPatch(typeof(SongSelect), "GetSongIDs", new Type[] {typeof(bool) } )]
        private static class PatchGetSongIDs
        {
            private static void Postfix(SongSelect __instance, bool extras, ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                if (AudicaMod.requestFilterActive)
                {
                    __result.Clear();
                    __instance.songSelectHeaderItems.mItems[0].titleLabel.text = "Song Requests";

                    if (extras)
                    {
                        foreach (string songID in AudicaMod.requestList)
                        {
                            __result.Add(songID);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SongSelectItem), "OnSelect")]
        private static class PatchOnSelect
        {
            private static void Postfix(SongSelectItem __instance)
            {
                AudicaMod.selectedSong = __instance.mSongData;
            }
        }

        [HarmonyPatch(typeof(AudioDriver), "StartPlaying")]
        private static class PatchPlay
        {
            private static void Postfix(AudioDriver __instance)
            {
                for (int i = 0; i < AudicaMod.requestList.Count; i++)
                {
                    if (AudicaMod.requestList[i] == AudicaMod.selectedSong.songID)
                    {
                        AudicaMod.requestList.RemoveAt(i);
                    }
                }
            }
        }
    }
}