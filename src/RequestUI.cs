using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace AudicaModding
{
    internal static class RequestUI
    {
        public static bool requestFilterActive = false;
        public static bool firstInit = true;

        private static GameObject filterSongRequestsButton         = null;
        private static GameObject requestButtonSelectedIndicator = null;

        private static Vector3    filterSongRequestsButtonPos   = new Vector3(-22.1f, 16.5f, 14.6f);
        private static Vector3    filterSongRequestsButtonScale = new Vector3(2.8f, 2.8f, 2.8f);
        
        private static SongSelect       songSelect       = null;
        private static SongListControls songListControls = null;

        public static void Initialize()
        {
            if (firstInit)
            {
                firstInit = false;

                songSelect       = GameObject.FindObjectOfType<SongSelect>();
                songListControls = GameObject.FindObjectOfType<SongListControls>();

                CreateSongRequestFilterButton();
            }
        }

        public static void DisableFilter()
        {
            requestFilterActive = false;
            requestButtonSelectedIndicator.SetActive(false);
        }

        public static void UpdateButtonText()
        {
            if (filterSongRequestsButton == null)
                return;

            TextMeshPro buttonText = filterSongRequestsButton.GetComponentInChildren<TextMeshPro>();

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

        private static GameObject CreateButton(GameObject buttonPrefab, string label, System.Action onHit, Vector3 position, Vector3 scale)
        {
            GameObject buttonObject = Object.Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            buttonObject.transform.position = position;
            buttonObject.transform.localScale = scale;

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

        private static void OnFilterSongRequestsShot()
        {
            songListControls.FilterExtras(); // this seems to fix duplicated songs;
            if (!requestFilterActive)
            {
                requestFilterActive = true;
                requestButtonSelectedIndicator.SetActive(true);
            }
            else
            {
                DisableFilter();
            }
            songSelect.ShowSongList();
        }
    }
}



