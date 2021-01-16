using Harmony;
using System;
using TwitchChatter;
using MelonLoader;
using System.Linq;

namespace AudicaModding
{
    internal static class Hooks
    {
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

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class MenuStateSetStatePatch
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.SongPage)
                {
                    RequestUI.UpdateButtonText();
                }
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterAll")]
        private static class PatchFilterAll
        {
            private static void Prefix(SongListControls __instance)
            {
                if (!SongRequests.hasCompatibleSongBrowser)
                    RequestUI.DisableFilter();
            }
        }

        [HarmonyPatch(typeof(SongListControls), "FilterMain")]
        private static class PatchFilterMain
        {
            private static void Prefix(SongListControls __instance)
            {
                if (!SongRequests.hasCompatibleSongBrowser)
                    RequestUI.DisableFilter();
            }
        }

        [HarmonyPatch(typeof(SongSelect), "GetSongIDs", new Type[] {typeof(bool) } )]
        private static class PatchGetSongIDs
        {
            private static void Postfix(SongSelect __instance, ref bool extras, ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                if (!SongRequests.hasCompatibleSongBrowser && RequestUI.requestFilterActive)
                {
                    extras = true;
                    __result.Clear();
                    __instance.songSelectHeaderItems.mItems[0].titleLabel.text = "Song Requests";

                    foreach (string songID in SongRequests.requestList)
                    {
                        __result.Add(songID);
                    }
                    __instance.scroller.SnapTo(0);
                }
            }
        }

        [HarmonyPatch(typeof(SongSelect), "OnEnable", new Type[0])]
        private static class AdjustSongSelect
        {
            private static void Postfix(SongSelect __instance)
            {
                RequestUI.Initialize();
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

        [HarmonyPatch(typeof(LaunchPanel), "Play", new Type[0])]
        private static class ResetFilterPanel
        {
            private static void Prefix(SongListControls __instance)
            {
                RequestUI.firstInit = true;
            }
        }
    }
}