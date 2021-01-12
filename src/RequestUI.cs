using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

namespace AudicaModding
{
    public static class RequestUI
    {
        public static bool requestFilterActive = false;
        public static MenuState.State menuState;

        public static GameObject filterMainButton = null;
        public static bool panelButtonsCreated = false;
        public static bool buttonsBeingCreated = false;
        public static bool shootingFilterRequestsButton = false;

        public static GameObject filterSongRequestsButton = null;
        public static Vector3 filterSongRequestsButtonPos = new Vector3(-22.1f, 16.5f, 14.6f);
        public static Vector3 filterSongRequestsButtonRot = new Vector3(0.0f, 307.4f, 0.0f);
        public static Vector3 filterSongRequestsButtonScale = new Vector3(2.8f, 2.8f, 2.8f);

        public static GameObject CreateButton(GameObject buttonPrefab, string label, System.Action onHit, Vector3 position, Vector3 eulerRotation, Vector3 scale)
        {
            GameObject buttonObject = Object.Instantiate(buttonPrefab);
            buttonObject.transform.rotation = Quaternion.Euler(eulerRotation);
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

        public static void CreateSongRequestFilterButton()
        {
            buttonsBeingCreated = true;
            filterMainButton = GameObject.FindObjectOfType<MainMenuPanel>().buttons[1];
            filterSongRequestsButton = CreateButton(filterMainButton, "Song Requests", OnFilterSongRequestsShot, filterSongRequestsButtonPos, filterSongRequestsButtonRot, filterSongRequestsButtonScale);
            panelButtonsCreated = true;
            SetFilterSongRequestsButtonActive(true);
        }

        public static IEnumerator SetFilterSongRequestsButtonActive(bool active)
        {
            if (active) yield return new WaitForSeconds(.65f);
            //else yield return null;
            filterSongRequestsButton.SetActive(active);
        }

        public static void OnFilterSongRequestsShot()
        {
            if (!SongRequests.processQueueRunning)
            {
                requestFilterActive = true;
                SongListControls songListControls = GameObject.FindObjectOfType<SongListControls>();
                shootingFilterRequestsButton = true;
                songListControls.FilterAll();
            }
        }

        public static void UpdateButtonText()
        {
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

        public static void UpdateFilter()
        {
            if (requestFilterActive)
            {
                SongListControls songListControls = GameObject.FindObjectOfType<SongListControls>();
                shootingFilterRequestsButton = true;
                songListControls.FilterAll();
            }
        }
    }
}



