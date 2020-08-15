using MelonLoader;
using UnityEngine;
using Harmony;
using System;
using UnityEngine.Events;
using TMPro;
using System.Collections;

namespace AudicaModding
{
    public class AudicaMod : MelonMod
    {
        public static class BuildInfo
        {
            public const string Name = "SongRequest";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Alternity"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "0.1.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public static bool requestFilterActive = false;
        public static MenuState.State menuState;
        public static SongList.SongData selectedSong;

        public static GameObject filterMainButton = null;
        public static bool panelButtonsCreated = false;
        public static bool buttonsBeingCreated = false;
        public static bool shootingFilterRequestsButton = false;

        public static GameObject filterSongRequestsButton = null;
        public static Vector3 filterSongRequestsButtonPos = new Vector3(-22.1f, 16.5f, 14.6f);
        public static Vector3 filterSongRequestsButtonRot = new Vector3(0.0f, 307.4f, 0.0f);
        public static Vector3 filterSongRequestsButtonScale = new Vector3(2.8f, 2.8f, 2.8f);

        public static System.Collections.Generic.List<string> requestList = new System.Collections.Generic.List<string>();
        public static System.Collections.Generic.List<string> requestQueue = new System.Collections.Generic.List<string>();

        public static SongSelect songSelect = null;
        public static Il2CppSystem.Collections.Generic.List<SongSelectItem> songs = new Il2CppSystem.Collections.Generic.List<SongSelectItem>();

        public class ParsedTwitchMessage
        {
            public string badgeInfo = "";
            public string badges = "";
            public string bits = "";
            public string clientNonce = "";
            public string color = "";
            public string displayName = "";
            public string emotes = "";
            public string flags = "";
            public string id = "";
            public string mod = "";
            public string roomId = "";
            public string tmiSentTs = "";
            public string userId = "";
            public string message = "";
            public string user = "";
        }

        public static GameObject CreateButton(GameObject buttonPrefab, string label, Action onHit, Vector3 position, Vector3 eulerRotation, Vector3 scale)
        {
            GameObject buttonObject = UnityEngine.Object.Instantiate(buttonPrefab);
            buttonObject.transform.rotation = Quaternion.Euler(eulerRotation);
            buttonObject.transform.position = position;
            buttonObject.transform.localScale = scale;

            UnityEngine.Object.Destroy(buttonObject.GetComponentInChildren<Localizer>());
            TextMeshPro buttonText = buttonObject.GetComponentInChildren<TextMeshPro>();
            buttonText.text = label;
            GunButton button = buttonObject.GetComponentInChildren<GunButton>();
            button.destroyOnShot = false;
            button.disableOnShot = false;
            button.SetSelected(false);
            button.onHitEvent = new UnityEvent();
            button.onHitEvent.AddListener(onHit);

            return buttonObject.gameObject;
        }

        public static void CreateSongRequestFilterButton()
        {
            buttonsBeingCreated = true;
            filterMainButton = GameObject.FindObjectOfType<MainMenuPanel>().buttons[1];
            filterSongRequestsButton = CreateButton(filterMainButton, "Song Requests", OnFilterSongRequestsShot, filterSongRequestsButtonPos, filterSongRequestsButtonRot, filterSongRequestsButtonScale);
            panelButtonsCreated = true;
            SetFilterSongRequestsButtonnActive(true);
        }

        public static IEnumerator SetFilterSongRequestsButtonnActive(bool active)
        {
            if (active) yield return new WaitForSeconds(.65f);
            else yield return null;
            filterSongRequestsButton.SetActive(active);
        }

        public static void OnFilterSongRequestsShot()
        {
            ProcessQueue();
            requestFilterActive = true;
            SongListControls songListControls = GameObject.FindObjectOfType<SongListControls>();
            shootingFilterRequestsButton = true;
            songListControls.FilterAll();
        }

        public static ParsedTwitchMessage ParseTwitchMessage(string msg)
        {
            ParsedTwitchMessage parsedMsg = new ParsedTwitchMessage();

            string separator = ":";
            string tagSeparator = ";";

            string tags = msg.Split(separator.ToCharArray())[0];

            parsedMsg.user = msg.Split(separator.ToCharArray())[1];
            parsedMsg.message = msg.Split(separator.ToCharArray())[2];

            foreach (string str in tags.Split(tagSeparator.ToCharArray()))
            {
                if (str.Contains("badge-info="))
                {
                    parsedMsg.badgeInfo = str.Replace("badge-info=", "");
                }
                else if (str.Contains("badges="))
                {
                    parsedMsg.badges = str.Replace("badges=", "");
                }
                else if (str.Contains("bits="))
                {
                    parsedMsg.bits = str.Replace("bits=", "");
                }
                else if (str.Contains("client-nonce="))
                {
                    parsedMsg.clientNonce = str.Replace("client-nonce=", "");
                }
                else if (str.Contains("color="))
                {
                    parsedMsg.color = str.Replace("color=", "");
                }
                else if (str.Contains("display-name="))
                {
                    parsedMsg.displayName = str.Replace("display-name=", "");
                }
                else if (str.Contains("emotes="))
                {
                    parsedMsg.emotes = str.Replace("emotes=", "");
                }
                else if (str.Contains("flags="))
                {
                    parsedMsg.flags = str.Replace("flags=", "");
                }
                else if (str.Substring(0, 3) == "id=")
                {
                    parsedMsg.id = str.Replace("id=", "");
                }
                else if (str.Contains("mod="))
                {
                    parsedMsg.mod = str.Replace("mod=", "");
                }
                else if (str.Contains("room-id="))
                {
                    parsedMsg.roomId = str.Replace("room-id=", "");
                }
                else if (str.Contains("tmi-sent-ts="))
                {
                    parsedMsg.tmiSentTs = str.Replace("tmi-sent-ts=", "");
                }
                else if (str.Contains("user-id="))
                {
                    parsedMsg.userId = str.Replace("user-id=", "");
                }
            }
            return parsedMsg;
        }

        public static int GetBits(ParsedTwitchMessage msg)
        {
            if (msg.bits != "")
            {
                return 0;
            }
            else
            {
                int totalBits = 0;
                foreach (string str in msg.bits.Split(",".ToCharArray()))
                {
                    totalBits += Convert.ToInt32(str);
                }
                return totalBits;
            }
        }

        public static SongSelectItem SearchSong(string query)
        {
            songSelect = GameObject.FindObjectOfType<SongSelect>();
            SongSelectItem song = null;

            if (songSelect == null) return song;
            
            songs = songSelect.songSelectItems.mItems;

            for (int i = 0; i < songs.Count; i++)
            {
                SongSelectItem currentSong = songs[i];

                if (currentSong.mSongData.artist.ToLower().Contains(query.ToLower()) ||
                    currentSong.mSongData.title.ToLower().Contains(query.ToLower()) ||
                    currentSong.mSongData.songID.ToLower().Contains(query.ToLower()) ||
                    currentSong.mSongData.artist.ToLower().Replace(" ", "").Contains(query.ToLower()) ||
                    currentSong.mSongData.title.ToLower().Replace(" ", "").Contains(query.ToLower()))
                {
                    song = currentSong;
                    break;
                }
            }

            return song;
        }

        public static IEnumerator ProcessQueueCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            ProcessQueue();
        }

        public static void ProcessQueue()
        {
            //MelonLogger.Log(requestQueue.Count.ToString() + " in queue.");

            if (requestQueue.Count != 0)
            {
                for (int i = 0; i < requestQueue.Count; i++)
                {
                    SongSelectItem result = SearchSong(requestQueue[i]);

                    if (result != null)
                    {
                        MelonLogger.Log("Result: " + result.mSongData.songID);
                        MelonLogger.Log(result.mSongData.title);
                        if (!requestList.Contains(result.mSongData.songID))
                        {
                            requestList.Add(result.mSongData.songID);
                        }
                    }
                    else
                    {
                        MelonLogger.Log("Song not found");
                    }
                }
                requestQueue.Clear();
            }

            TextMeshPro buttonText = filterSongRequestsButton.GetComponentInChildren<TextMeshPro>();

            if (requestList.Count == 0)
            {
                if (buttonText.text.Contains("=green>"))
                {
                    buttonText.text = buttonText.text.Replace("=green>", "=red>");
                }
                else
                {
                    buttonText.text = "<color=red>" + buttonText.text + "</color>";
                }
                MelonLogger.Log("Red");
            }
            else
            {
                if (buttonText.text.Contains("=red>"))
                {
                    buttonText.text = buttonText.text.Replace("=red>", "=green>");
                }
                else
                {
                    buttonText.text = "<color=green>" + buttonText.text + "</color>";
                }
                MelonLogger.Log("Green");
            }
        }

        public static void ParseCommand(string msg)
        {
            if (msg.Substring(0, 1) == "!")
            {
                string command = msg.Replace("!", "").Split(" ".ToCharArray())[0];
                string arguments = msg.Replace("!" + command + " ", "");

                if (command == "asr")
                {
                    MelonLogger.Log("!asr requested with query \"" + arguments + "\"");

                    requestQueue.Add(arguments);

                    if (menuState == MenuState.State.SongPage)
                    {
                        ProcessQueue();
                    }
                }
                else if (command == "mine")
                {
                    //TODO add mine spawning
                }
            }
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance instance = HarmonyInstance.Create("TwitchChatEnhancer");
        }

        public override void OnUpdate()
        {
            /*
            if (Input.GetKeyDown(KeyCode.F))
            {
                SongListControls songListControls = GameObject.FindObjectOfType<SongListControls>();

                MelonLogger.Log("All Position: " + songListControls.filterAllButton.gameObject.transform.position.ToString());
                MelonLogger.Log("Main Position: " + songListControls.filterMainButton.gameObject.transform.position.ToString());
                MelonLogger.Log("Extras Position: " + songListControls.filterExtrasButton.gameObject.transform.position.ToString());
                MelonLogger.Log("Rotation: " + songListControls.filterExtrasButton.gameObject.transform.rotation.eulerAngles.ToString());
                MelonLogger.Log("Scale: " + songListControls.filterAllButton.gameObject.transform.localScale.ToString());
            }
            */
        }
    }
}



