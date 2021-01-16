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
            public const string Version = "0.2.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public static bool loadComplete             = false;
        public static bool hasCompatibleSongBrowser = false;
        public static SongList.SongData selectedSong;

        public static System.Collections.Generic.List<string> requestList = new System.Collections.Generic.List<string>();
        public static System.Collections.Generic.List<string> requestQueue = new System.Collections.Generic.List<string>();

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(HasCompatibleSongBrowser))
            {
                hasCompatibleSongBrowser = true;
                MelonLogger.Log("Song Browser is installed. Enabling integration");
                RequestUI.Register();
            }
        }

        private bool HasCompatibleSongBrowser(MelonMod mod)
        { 
            if (mod.Info.SystemType.Name == nameof(SongBrowser))
            {
                string[] versionInfo = mod.Info.Version.Split('.');
                int major = int.Parse(versionInfo[0]);
                int minor = int.Parse(versionInfo[1]);
                if (major > 2 || (major == 2 && minor >= 3))
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
                        MelonLogger.Log(result.title);
                        if (!requestList.Contains(result.songID))
                        {
                            requestList.Add(result.songID);
                            addedAny = true;
                        }
                    }
                    else
                    {
                        MelonLogger.Log("Song not found");
                    }
                    
                }
                requestQueue.Clear();
            }

            if (addedAny && MenuState.GetState() == MenuState.State.SongPage)
                RequestUI.UpdateFilter();

            RequestUI.UpdateButtonText();
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



