using Harmony;
using System;
using TwitchChatter;
using MelonLoader;
using System.Linq;

namespace AudicaModding
{
    internal static class Hooks
    {
        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                SongRequests.menuState = state;
                if (!RequestUI.panelButtonsCreated)
                {
                    if (!RequestUI.buttonsBeingCreated && state == MenuState.State.SongPage)
                    {
                        RequestUI.CreateSongRequestFilterButton();
                    }
                    return;
                }
                if (state == MenuState.State.SongPage)
                {
                    MelonCoroutines.Start(RequestUI.SetFilterSongRequestsButtonActive(true));
                }
                else if (state == MenuState.State.LaunchPage || state == MenuState.State.MainPage)
                {
                    MelonCoroutines.Start(RequestUI.SetFilterSongRequestsButtonActive(false));
                }
                if (state == MenuState.State.Launched)
                {
                    RequestUI.requestFilterActive = false;
                }
            }
        }

        [HarmonyPatch(typeof(TwitchChatStream), "write_chat_msg", new Type[] { typeof(string) })]
        private static class PatchWriteChatMsg
        {
            private static void Prefix(string msg)
            {
                if (msg.Length > 1)
                {
                    if (msg.Substring(0, 1) == "@")
                    {
                        if (msg.Contains("tmi.twitch.tv PRIVMSG "))
                        {
                            SongRequests.ParseCommand(new ParsedTwitchMessage(msg).Message);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartupLoader), "SetState", new Type[] { typeof(StartupLoader.State) })]
        private static class StartupLoaderSetStatePatch
        {
            private static void Postfix(StartupLoader __instance, ref StartupLoader.State newState)
            {
                if (newState == StartupLoader.State.Complete)
                {
                    SongRequests.loadComplete = true;
                    SongRequests.ProcessQueue();
                }
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterAll")]
        private static class PatchFilterAll
        {
            private static void Prefix(SongListControls __instance)
            {
                if (!RequestUI.shootingFilterRequestsButton)
                {
                    RequestUI.requestFilterActive = false;
                }
                else
                {
                    RequestUI.shootingFilterRequestsButton = false;
                }
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterExtras")]
        private static class PatchFilterExtras
        {
            private static void Prefix(SongListControls __instance)
            {
                RequestUI.requestFilterActive = false;
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterMain")]
        private static class PatchFilterMain
        {
            private static void Prefix(SongListControls __instance)
            {
                RequestUI.requestFilterActive = false;
            }
        }

        [HarmonyPatch(typeof(SongSelect), "GetSongIDs", new Type[] {typeof(bool) } )]
        private static class PatchGetSongIDs
        {
            private static void Postfix(SongSelect __instance, bool extras, ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                if (RequestUI.requestFilterActive)
                {
                    __result.Clear();
                    __instance.songSelectHeaderItems.mItems[0].titleLabel.text = "Song Requests";

                    if (extras)
                    {
                        foreach (string songID in SongRequests.requestList)
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
                SongRequests.selectedSong = __instance.mSongData;
            }
        }

        [HarmonyPatch(typeof(AudioDriver), "StartPlaying")]
        private static class PatchPlay
        {
            private static void Postfix(AudioDriver __instance)
            {
                foreach(string str in SongRequests.requestList.ToList())
                {
                    if (str == SongRequests.selectedSong.songID)
                    {
                        SongRequests.requestList.Remove(str);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentLoader), "SwitchEnvironment")]
        private static class PatchSwitchEnvironment
        {
            private static void Postfix(EnvironmentLoader __instance)
            {
                RequestUI.buttonsBeingCreated = false;
                RequestUI.panelButtonsCreated = false;
            }
        }

    }
}