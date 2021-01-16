using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace AudicaModding
{
    internal static class RequestUI
    {
        public static bool requestFilterActive = false;
        public static bool firstInit = true;

        private static bool isSkipButtonActive = false;

        private static GameObject filterSongRequestsButton       = null;
        private static GameObject requestButtonSelectedIndicator = null;
        private static GameObject skipSongRequestsButton         = null;

        private static Vector3    filterSongRequestsButtonPos   = new Vector3(0.0f, 10.5f, 0.0f);
        private static Vector3    filterSongRequestsButtonScale = new Vector3(2.8f, 2.8f, 2.8f);

        private static Vector3    skipButtonPos                 = new Vector3(0.0f, 15.1f, 0.0f);
        private static Vector3    skipButtonScale               = new Vector3(1.0f, 1.0f, 1.0f);

        private static SongSelect       songSelect       = null;
        private static SongListControls songListControls = null;

        private static System.Func<FilterPanel.Filter> getFilter = null; // for use with song browser integration

        // if compatible version of song browser is available, use song browser's filter panel
        public static void Register()
        {
            getFilter = FilterPanel.RegisterFilter("requests", "Song Requests",
                                                   ShowSkipButton, HideSkipButton,
                                                   ApplyFilter);
        }

        public static void Initialize()
        {
            if (firstInit)
            {
                firstInit = false;

                songSelect       = GameObject.FindObjectOfType<SongSelect>();
                songListControls = GameObject.FindObjectOfType<SongListControls>();

                if (!SongRequests.hasCompatibleSongBrowser) // song browser integration does this automatically
                    CreateSongRequestFilterButton();
                CreateSongRequestSkipButton();
            }
        }

        public static void DisableFilter()
        {
            requestFilterActive = false;
            requestButtonSelectedIndicator.SetActive(false);
            HideSkipButton();
        }

        public static void UpdateButtonText()
        {
            TextMeshPro buttonText = null;
            if (SongRequests.hasCompatibleSongBrowser)
            {
                buttonText = getFilter()?.ButtonText;
                if (buttonText == null)
                    return;
            }
            else
            {
                if (filterSongRequestsButton == null)
                    return;
                buttonText = filterSongRequestsButton.GetComponentInChildren<TextMeshPro>();
            }

            if (SongRequests.requestList.Count == 0)
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
        }

        public static void ShowSkipButton()
        {
            isSkipButtonActive = true;
            skipSongRequestsButton?.SetActive(true);
        }
        public static void HideSkipButton()
        {
            isSkipButtonActive = false;
            skipSongRequestsButton?.SetActive(false);
        }

        public static void UpdateFilter()
        {
            if ((SongRequests.hasCompatibleSongBrowser && getFilter().IsActive) || requestFilterActive)
                songSelect?.ShowSongList();
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

            skipSongRequestsButton.SetActive(isSkipButtonActive);
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
                ShowSkipButton();
            }
            else
            {
                DisableFilter();
            }
            songSelect.ShowSongList();
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



