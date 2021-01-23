using System.Collections.Generic;
using MelonLoader;
using System.Linq;
using System.Text.RegularExpressions;

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
        public static SongList.SongData selectedSong;

        public static List<string>               requestList  = new List<string>();
        // actually Dictionary<string, Song>, but can't use that type in case SongBrowser is not available
        public static Dictionary<string, object> missingSongs = new Dictionary<string, object>();
        public static List<string>               requestQueue = new List<string>();

        private static Dictionary<string, QueryData> webSearchQueryData = new Dictionary<string, QueryData>();

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(HasCompatibleSongBrowser))
            {
                InitSongBrowserIntegration();
            }
        }

        public static int GetActiveWebSearchCount()
        {
            return webSearchQueryData.Count;
        }

        private void InitSongBrowserIntegration()
        {
            hasCompatibleSongBrowser = true;
            MelonLogger.Log("Song Browser is installed. Enabling integration");
            RequestUI.Register();

            // make sure queue is processed after song list reload
            // to show requested maps that were missing and just
            // got downloaded
            SongBrowser.RegisterSongListPostProcessing(ProcessQueueAfterRefresh);
        }

        private bool HasCompatibleSongBrowser(MelonMod mod)
        { 
            if (mod.Info.SystemType.Name == nameof(SongBrowser))
            {
                string[] versionInfo = mod.Info.Version.Split('.');
                int major = int.Parse(versionInfo[0]);
                int minor = int.Parse(versionInfo[1]);
                int patch = int.Parse(versionInfo[2]);
                if (major > 2 || (major == 2 && minor >= 3 && patch >= 2))
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

        private static SongList.SongData SearchSong(QueryData data, out bool foundExactMatch)
        {
            SongList.SongData song        = null;
            bool              foundAny    = false;
            bool              foundBetter = false;
            foundExactMatch               = false;

            for (int i = 0; i < SongList.I.songs.Count - 1; i++)
            {
                SongList.SongData currentSong = SongList.I.songs[i];
                if ((data.Artist == null || currentSong.artist.ToLowerInvariant().Replace(" ", "").Contains(data.Artist)) &&
                    (data.Mapper == null || currentSong.author.ToLowerInvariant().Replace(" ", "").Contains(data.Mapper)) &&
                    (currentSong.title.ToLowerInvariant().Contains(data.Title) ||
                     currentSong.songID.ToLowerInvariant().Contains(data.Title.Replace(" ", ""))))
                { 
                    if (LookForMatch(data.Title, currentSong.title, ref foundAny, ref foundBetter, ref foundExactMatch))
                    {
                        song = currentSong;
                        if (foundExactMatch)
                            break;
                    }
                }
            }
            return song;
        }
        private static bool LookForMatch(string querySongTitle, string matchSongTitle, 
                                         ref bool foundAny, ref bool foundBetter, ref bool foundExact)
        {
            bool newBestMatch = false;
            // keep first partial match as result unless we find an exact match
            if (!foundAny)
            {
                foundAny      = true;
                newBestMatch  = true;
            }

            // prefer songs that actually start with the first word of the query
            // over random partial matches (e.g. !asr Bang should find Bang! and not
            // My New Sneakers Could Never Replace My Multi-Colored Bangalores 
            if (!foundBetter)
            {
                if (matchSongTitle.ToLowerInvariant().StartsWith(querySongTitle))
                {
                    foundBetter  = true;
                    newBestMatch = true;
                }
            }

            // exact matches are best
            if (matchSongTitle.ToLowerInvariant().Equals(querySongTitle))
            {
                foundExact   = true;
                newBestMatch = true;
            }

            return newBestMatch;
        }

        public static void ProcessQueue()
        {
            bool addedAny = false;
            MelonLogger.Log(requestQueue.Count + " in queue.");
            
            if (requestQueue.Count != 0)
            {
                foreach (string str in requestQueue)
                {
                    QueryData         data   = new QueryData(str);
                    SongList.SongData result = SearchSong(data, out bool foundExactMatch);

                    if ((!hasCompatibleSongBrowser || foundExactMatch) && result != null)
                    {
                        // if we have web search we want to make sure we prioritize exact matches
                        // over partial local ones
                        MelonLogger.Log("Result: " + result.songID);
                        if (!requestList.Contains(result.songID))
                        {
                            requestList.Add(result.songID);
                            addedAny = true;
                        }
                    }
                    else if (hasCompatibleSongBrowser)
                    {
                        StartWebSearch(data);
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
        private static void StartWebSearch(QueryData data)
        {
            webSearchQueryData.Add(data.Title, data);
            MelonCoroutines.Start(SongDownloader.DoSongWebSearch(data.Title, ProcessWebSearchResult, DifficultyFilter.All));
        }
        private static void ProcessWebSearchResult(string query, APISongList response)
        {
            QueryData data = webSearchQueryData[query];
            bool addedLocalMatch = false;
            if (response.song_count > 0)
            {
                Song bestMatch   = null;
                bool foundAny    = false;
                bool foundBetter = false;
                bool foundExact  = false;
                foreach (Song s in response.songs)
                {
                    if ((data.Artist == null || s.artist.ToLowerInvariant().Replace(" ", "").Contains(data.Artist)) &&
                        (data.Mapper == null || s.author.ToLowerInvariant().Replace(" ", "").Contains(data.Mapper)) &&
                        (s.title.ToLowerInvariant().Contains(data.Title) ||
                         s.song_id.ToLowerInvariant().Contains(data.Title.Replace(" ", ""))))
                    {
                        if (LookForMatch(data.Title, s.title, ref foundAny, ref foundBetter, ref foundExact))
                        {
                            bestMatch = s;
                            if (foundExact)
                                break;
                        }
                    }
                }
                if (bestMatch != null)
                {
                    // check if we already have that file downloaded
                    QueryData matchData = new QueryData($"{bestMatch.title} -artist {bestMatch.artist} -mapper {bestMatch.author}");
                    SongList.SongData s = SearchSong(matchData, out bool isExactMatch);
                    if (isExactMatch)
                    {
                        MelonLogger.Log("Result: " + s.songID);
                        if (!requestList.Contains(s.songID))
                        {
                            requestList.Add(s.songID);
                            addedLocalMatch = true;
                        }
                    }
                    else if (!missingSongs.ContainsKey(bestMatch.song_id))
                    {
                        missingSongs.Add(bestMatch.song_id, bestMatch);
                        MelonLogger.Log("Result (missing): " + bestMatch.song_id);
                    }
                }
                else
                {
                    MelonLogger.Log($"Found no match for \"{data.FullQuery}\"");
                }
            }
            else
            {
                // check if we have a local match (can happen if
                // this particular map hasn't been uploaded or was taken down)
                SongList.SongData s = SearchSong(data, out bool _);
                if (s != null)
                {
                    MelonLogger.Log("Result: " + s.songID);
                    if (!requestList.Contains(s.songID))
                    {
                        requestList.Add(s.songID);
                        addedLocalMatch = true;
                    }
                }
                else
                {
                    MelonLogger.Log($"Found no match for \"{data.FullQuery}\"");
                }
            }

            if (addedLocalMatch && MenuState.GetState() == MenuState.State.SongPage)
                RequestUI.UpdateFilter();

            webSearchQueryData.Remove(query);
            if (GetActiveWebSearchCount() == 0)
            {
                RequestUI.UpdateButtonText();
            }
        }
        private static void ProcessQueueAfterRefresh()
        {
            // put all missing songs into the queue to make sure
            // we catch it if they just got downloaded
            foreach (string s in missingSongs.Keys)
            {
                Song addedInfo = (Song)missingSongs[s];
                requestQueue.Add($"{addedInfo.title} -artist {addedInfo.artist} -mapper {addedInfo.author}");
            }
            missingSongs.Clear();

            ProcessQueue();
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

        private class QueryData
        {
            public QueryData(string query)
            {
                Artist    = null;
                Mapper    = null;
                FullQuery = query;

                string modifiedQuery = query + "-endQuery";
                if (query.Contains("-artist"))
                {
                    // match everything from -artist to the next occurrence of -mapper, -artist or -endQuery
                    Match m = Regex.Match(modifiedQuery, "-artist.*?(?=-mapper|-artist|-endQuery)");
                    query = query.Replace(m.Value, ""); // remove artist part from song title
                    Artist = m.Value.Replace("-artist", "").Trim().ToLowerInvariant();
                    Artist = Artist.Replace(" ", "");
                }
                if (query.Contains("-mapper"))
                {
                    // match everything from -mapper to the next occurrence of -mapper, -artist or -endQuery
                    Match m = Regex.Match(modifiedQuery, "-mapper.*?(?=-mapper|-artist|-endQuery)");
                    query = query.Replace(m.Value, ""); // remove mapper part from song title
                    Mapper = m.Value.Replace("-mapper", "").Trim().ToLowerInvariant();
                    Mapper = Mapper.Replace(" ", "");
                }
                Title = query.Trim().ToLowerInvariant();
            }

            public string Title { get; private set; }
            public string Artist { get; private set; }
            public string Mapper { get; private set; }
            public string FullQuery { get; private set; }
        }
    }
}



