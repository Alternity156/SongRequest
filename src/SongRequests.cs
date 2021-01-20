using System.Collections.Generic;
using MelonLoader;
using System.Linq;
[assembly: MelonOptionalDependencies("SongBrowser")]

namespace AudicaModding
{
    public class SongRequests : MelonMod
    {
        public static class BuildInfo
        {
            public const string Name = "SongRequest";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Alternity"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.1.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public static bool loadComplete             = false;
        public static bool hasCompatibleSongBrowser = false;
        public static int  activeWebSearchCount     = 0;
        public static SongList.SongData selectedSong;

        public static List<string>               requestList  = new List<string>();
        // actually Dictionary<string, Song>, but can't use that type in case SongBrowser is not available
        public static Dictionary<string, object> missingSongs = new Dictionary<string, object>();
        public static List<string>               requestQueue = new List<string>();

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(HasCompatibleSongBrowser))
            {
                InitSongBrowserIntegration();
            }
        }

        private void InitSongBrowserIntegration()
        {
            hasCompatibleSongBrowser = true;
            MelonLogger.Log("Song Browser is installed. Enabling integration");
            RequestUI.Register();

            // make sure queue is processed after song list reload
            // to show requested maps that were missing and just
            // got downloaded
            SongBrowser.RegisterSongListPostProcessing(ProcessQueue);
        }

        private bool HasCompatibleSongBrowser(MelonMod mod)
        { 
            if (mod.Info.SystemType.Name == nameof(SongBrowser))
            {
                string[] versionInfo = mod.Info.Version.Split('.');
                int major = int.Parse(versionInfo[0]);
                int minor = int.Parse(versionInfo[1]);
                int patch = int.Parse(versionInfo[2]);
                if (major > 2 || (major == 2 && minor >= 3 && patch >= 1))
                    return true;
            }
            return false;
        }

        public static int GetBits(ParsedTwitchMessage msg)
        {
            if (msg.Bits != "")
            {
                return 0;
            }
            else
            {
                int totalBits = 0;
                foreach (string str in msg.Bits.Split(",".ToCharArray()))
                {
                    totalBits += System.Convert.ToInt32(str);
                }
                return totalBits;
            }
        }

        public static SongList.SongData SearchSong(string query)
        {
            SongList.SongData song = null;
            
            for (int i = 0; i < SongList.I.songs.Count - 1; i++)
            {
                SongList.SongData currentSong = SongList.I.songs[i];
                if (currentSong.artist.ToLower().Contains(query.ToLower()) ||
                    currentSong.title.ToLower().Contains(query.ToLower()) ||
                    currentSong.songID.ToLower().Contains(query.ToLower()) ||
                    currentSong.artist.ToLower().Replace(" ", "").Contains(query.ToLower()) ||
                    currentSong.title.ToLower().Replace(" ", "").Contains(query.ToLower()))
                {
                    song = currentSong;
                    break;
                }
            }
            return song;
        }

        public static void ProcessQueue()
        {
            bool addedAny = false;
            MelonLogger.Log(requestQueue.Count.ToString() + " in queue.");
            
            if (requestQueue.Count != 0)
            {
                foreach (string str in requestQueue.ToList())
                {
                    SongList.SongData result = SearchSong(str);
            
                    if (result != null)
                    {
                        MelonLogger.Log("Result: " + result.songID);
                        if (!requestList.Contains(result.songID))
                        {
                            requestList.Add(result.songID);
                            addedAny = true;
                        }
                    }
                    else if (hasCompatibleSongBrowser)
                    {
                        StartWebSearch(str);
                    }
                    else
                    {
                        MelonLogger.Log($"Found no match for \"{str}\"");
                    }
                }
                requestQueue.Clear();
            }
            
            if (addedAny && MenuState.GetState() == MenuState.State.SongPage)
                RequestUI.UpdateFilter();
            
            RequestUI.UpdateButtonText();
        }
        private static void StartWebSearch(string query)
        {
            activeWebSearchCount++;
            MelonCoroutines.Start(SongDownloader.DoSongWebSearch(query, ProcessWebSearchResult, DifficultyFilter.All));
        }
        private static void ProcessWebSearchResult(string query, APISongList response)
        {
            activeWebSearchCount--;
            if (response.song_count > 0)
            {
                if (!missingSongs.ContainsKey(response.songs[0].song_id)) // for now just use the first match, TODO: prefer exact match
                {
                    missingSongs.Add(response.songs[0].song_id, response.songs[0]);
                }
                MelonLogger.Log("Result (missing): " + response.songs[0].song_id);
            }
            else
            {
                MelonLogger.Log($"Found no match for \"{query}\"");
            }
            if (activeWebSearchCount == 0)
            {
                RequestUI.UpdateButtonText();
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

                    if (loadComplete)
                    {
                        ProcessQueue();
                    }
                }
            }
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



