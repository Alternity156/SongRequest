using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace AudicaModding
{
    internal static class RequestUI
    {
        public static bool requestFilterActive = false;

        private static bool additionalGUIActive = false;

        private static GameObject  filterSongRequestsButton       = null;
        private static TextMeshPro filterButtonText               = null;
        private static GameObject  requestButtonSelectedIndicator = null;
        private static GameObject  skipSongRequestsButton         = null;
        private static GameObject  downloadMissingButton          = null;
        private static TextMeshPro downloadButtonText             = null;
        private static GunButton   downloadGunButton              = null;

        private static Vector3     filterSongRequestsButtonPos   = new Vector3(0.0f, 10.5f, 0.0f);
        private static Vector3     filterSongRequestsButtonScale = new Vector3(2.8f, 2.8f, 2.8f);
                                   
        private static Vector3     skipButtonPos                 = new Vector3(0.0f, 17.1f, 0.0f);
        private static Vector3     skipButtonScale               = new Vector3(1.0f, 1.0f, 1.0f);

        private static Vector3     downloadButtonPos             = new Vector3(0.0f, 15.1f, 0.0f);
        private static Vector3     downloadButtonScale           = new Vector3(1.0f, 1.0f, 1.0f);

        private static SongSelect       songSelect       = null;
        private static SongListControls songListControls = null;

        private static System.Func<object> getFilter = null; // for use with song browser integration, actually of FilterPanel.Filter

        private static int queuedDownloadCount = 0;

        // if compatible version of song browser is available, use song browser's filter panel
        public static void Register()
        {
            getFilter = FilterPanel.RegisterFilter("requests", true, "Song Requests",
                                                   ShowAdditionalGUI, HideAdditonalGUI,
                                                   ApplyFilter);
        }

        public static void Initialize()
        {
            if (songListControls == null)
            {
                songSelect       = GameObject.FindObjectOfType<SongSelect>();
                songListControls = GameObject.FindObjectOfType<SongListControls>();

                if (!SongRequests.hasCompatibleSongBrowser) // song browser integration does this automatically
                {
                    CreateSongRequestFilterButton();

                    // move that button down, since the download button doesn't exist
                    skipButtonPos   = downloadButtonPos;
                    skipButtonScale = downloadButtonScale;
                }
                CreateSongRequestSkipButton();
                CreateDownloadMissingButton();
            }
        }

        public static void DisableFilter()
        {
            requestFilterActive = false;
            requestButtonSelectedIndicator.SetActive(false);
            HideAdditonalGUI();
        }

        public static void UpdateButtonText(bool processing = false)
        {
            TextMeshPro buttonText = null;
            if (SongRequests.hasCompatibleSongBrowser)
            {
                buttonText = GetSongBrowserFilterButtonText();
            }
            else
            {
                if (filterSongRequestsButton == null)
                    return;
                if (filterButtonText == null)
                    filterButtonText = filterSongRequestsButton.GetComponentInChildren<TextMeshPro>();

                buttonText = filterButtonText;
            }

            if (buttonText == null)
                return;

            if (SongRequests.requestList.Count == 0 && SongRequests.missingSongs.Count == 0)
            {
                if (buttonText.text.Contains("=green>"))
                {
                    buttonText.text = buttonText.text.Replace("=green>", "=red>");
                }
                else if (!buttonText.text.Contains("=red>"))
                {
                    buttonText.text = "<color=red>" + buttonText.text + "</color>";
                }
            }
            else
            {
                if (buttonText.text.Contains("=red>"))
                {
                    buttonText.text = buttonText.text.Replace("=red>", "=green>");
                }
                else if (!buttonText.text.Contains("=green>"))
                {
                    buttonText.text = "<color=green>" + buttonText.text + "</color>";
                }
            }

            // update 
            if (SongRequests.hasCompatibleSongBrowser && downloadMissingButton != null)
            {
                if (SongRequests.activeWebSearchCount > 0)
                {
                    downloadButtonText.text = "Processing...";
                    downloadGunButton.SetInteractable(false);
                }
                else if (SongRequests.missingSongs.Count > 0)
                {
                    downloadButtonText.text = "<color=green>Download missing</color>";
                    downloadGunButton.SetInteractable(true);
                }
                else
                {
                    downloadButtonText.text = "Download missing";
                    downloadGunButton.SetInteractable(false);
                }
            }
        }
        private static TextMeshPro GetSongBrowserFilterButtonText()
        {
            return ((FilterPanel.Filter)getFilter())?.ButtonText;
        }

        public static void ShowAdditionalGUI()
        {
            additionalGUIActive = true;
            skipSongRequestsButton?.SetActive(true);
            downloadMissingButton?.SetActive(true);
        }
        public static void HideAdditonalGUI()
        {
            additionalGUIActive = false;
            skipSongRequestsButton?.SetActive(false);
            downloadMissingButton?.SetActive(false);
        }

        public static void UpdateFilter()
        {
            if ((SongRequests.hasCompatibleSongBrowser && IsSongBrowserFilterActive()) || requestFilterActive)
                songSelect?.ShowSongList();
        }
        private static bool IsSongBrowserFilterActive()
        {
            return ((FilterPanel.Filter)getFilter()).IsActive;
        }

        private static GameObject CreateButton(GameObject buttonPrefab, string label, System.Action onHit, Vector3 position, Vector3 scale)
        {
            GameObject buttonObject = Object.Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            buttonObject.transform.localPosition    = position;
            buttonObject.transform.localScale       = scale;
            buttonObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);

            Object.Destroy(buttonObject.GetComponentInChildren<Localizer>());
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

        private static void CreateSongRequestFilterButton()
        {
            if (filterSongRequestsButton != null)
            {
                filterSongRequestsButton.SetActive(true);
                return;
            }

            GameObject filterMainButton = GameObject.Find("menu/ShellPage_Song/page/ShellPanel_Left/FilterExtras");
            if (filterMainButton == null)
                return;

            filterSongRequestsButton = CreateButton(filterMainButton, "Song Requests", OnFilterSongRequestsShot, 
                                                    filterSongRequestsButtonPos, filterSongRequestsButtonScale);

            requestButtonSelectedIndicator = filterSongRequestsButton.transform.GetChild(3).gameObject;
            requestButtonSelectedIndicator.SetActive(requestFilterActive);

            filterMainButton.GetComponentInChildren<GunButton>().onHitEvent.AddListener(new System.Action(() =>
            {
                DisableFilter();
                songSelect.ShowSongList();
            }));

            UpdateButtonText();
        }

        private static void CreateSongRequestSkipButton()
        {
            if (skipSongRequestsButton != null)
            {
                skipSongRequestsButton.SetActive(true);
                return;
            }

            GameObject backButton = GameObject.Find("menu/ShellPage_Song/page/backParent/back");
            if (backButton == null)
                return;

            skipSongRequestsButton = CreateButton(backButton, "Skip Next", OnSkipSongRequestShot, 
                                                  skipButtonPos, skipButtonScale);

            skipSongRequestsButton.SetActive(additionalGUIActive);
        }

        private static void CreateDownloadMissingButton()
        {
            if (!SongRequests.hasCompatibleSongBrowser)
                return;

            if (downloadMissingButton != null)
            {
                downloadMissingButton.SetActive(true);
                return;
            }

            GameObject backButton = GameObject.Find("menu/ShellPage_Song/page/backParent/back");
            if (backButton == null)
                return;

            downloadMissingButton = CreateButton(backButton, "Download missing", OnDownloadMissingShot,
                                                  downloadButtonPos, downloadButtonScale);

            downloadMissingButton.SetActive(additionalGUIActive);

            downloadGunButton  = downloadMissingButton.GetComponentInChildren<GunButton>();
            downloadButtonText = downloadMissingButton.GetComponentInChildren<TextMeshPro>();

            UpdateButtonText();
        }

        private static bool ApplyFilter(Il2CppSystem.Collections.Generic.List<string> result)
        {
            result.Clear();

            foreach (string songID in SongRequests.requestList)
            {
                result.Add(songID);
            }
            return true;
        }

        private static void OnFilterSongRequestsShot()
        {
            songListControls.FilterExtras(); // this seems to fix duplicated songs;
            if (!requestFilterActive)
            {
                requestFilterActive = true;
                requestButtonSelectedIndicator.SetActive(true);
                ShowAdditionalGUI();
            }
            else
            {
                DisableFilter();
            }
            songSelect.ShowSongList();
        }

        private static void OnDownloadMissingShot()
        {
            MenuState.I.GoToMainPage();
            KataConfig.I.CreateDebugText("Downloading missing songs...", new Vector3(0f, -1f, 5f), 5f, null, false, 0.2f);
            MelonLoader.MelonCoroutines.Start(DownloadMissing());
        }

        private static System.Collections.IEnumerator DownloadMissing()
        {
            yield return new WaitForSeconds(0.5f);
            List<string> ids = new List<string>(SongRequests.missingSongs.Keys);
            foreach (string id in ids)
            {
                queuedDownloadCount++;
                MelonLoader.MelonCoroutines.Start(SongDownloader.DownloadSong(((Song)SongRequests.missingSongs[id]).download_url, OnDownloadComplete));
                SongRequests.requestQueue.Add(((Song)SongRequests.missingSongs[id]).title);
                SongRequests.missingSongs.Remove(id);
                yield return null;
            }
        }

        private static void OnDownloadComplete()
        {
            queuedDownloadCount--;
            if (queuedDownloadCount == 0) // only refresh once all downloads are done
            {
                SongBrowser.ReloadSongList();
            }
        }

        private static void OnSkipSongRequestShot()
        {
            if (SongRequests.requestList.Count > 0 && songSelect != null && songSelect.songSelectItems != null && songSelect.songSelectItems.mItems != null)
            {
                string id = songSelect.songSelectItems.mItems[0].mSongData.songID;
                SongRequests.requestList.Remove(id);
                UpdateButtonText();
                songSelect.ShowSongList();
            }
        }
    }
}



