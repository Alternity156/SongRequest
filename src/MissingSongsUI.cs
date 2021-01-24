using System.Collections;
using System;
using TMPro;
using UnityEngine;
using MelonLoader;
using UnityEngine.Events;
using System.Collections.Generic;

namespace AudicaModding
{
	internal static class MissingSongsUI
	{
		public static bool lookingAtMissingSongs = false;
		public static bool needsSongListRefresh  = false;

		private static OptionsMenu songItemMenu;
		private static GunButton   backButton;
		private static GameObject  downloadAllButton;

		private static List<string> missingSongsIDs = null;
		private static int downloadCount = 0;

		public static void SetMenu(OptionsMenu optionsMenu)
		{
			songItemMenu = optionsMenu;
		}

		public static void GoToMissingSongsPage()
		{
			needsSongListRefresh = false;

			songItemMenu.ShowPage(OptionsMenu.Page.Customization);

			if (backButton == null)
            {
				var button = GameObject.Find("menu/ShellPage_Settings/page/backParent/back");
				backButton = button.GetComponentInChildren<GunButton>();

				// set up "download all" button
				downloadAllButton						     = UnityEngine.Object.Instantiate(button, button.transform.parent);
				downloadAllButton.transform.localPosition    = new Vector3(-0.8f, 2.0f, 0.0f);
				downloadAllButton.transform.localScale       = new Vector3(1.5f, 1.5f, 1.5f);
				downloadAllButton.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);

				UnityEngine.Object.Destroy(downloadAllButton.GetComponentInChildren<Localizer>());
				TextMeshPro buttonText = downloadAllButton.GetComponentInChildren<TextMeshPro>();
				buttonText.text        = "Download all";

				Action    onHit   = new Action(() => { OnDownloadAll(); });
				GunButton btn     = downloadAllButton.GetComponentInChildren<GunButton>();
				btn.destroyOnShot = false;
				btn.disableOnShot = false;
				btn.SetSelected(false);
				btn.onHitEvent = new UnityEvent();
				btn.onHitEvent.AddListener(onHit);
			}
			else
            {
				downloadAllButton.SetActive(true);
            }

			missingSongsIDs = new List<string>(SongRequests.missingSongs.Keys);
			SetupList();
			AddSongItems(songItemMenu);
		}

		public static void Cancel()
        {
			lookingAtMissingSongs = false;
			downloadAllButton.SetActive(false);

			if (needsSongListRefresh)
			{
				MenuState.I.GoToMainPage();
				SongBrowser.ReloadSongList();
			}
			else
			{
				MenuState.I.GoToSongPage();
				RequestUI.UpdateButtonText();
			}
		}

		public static void ResetScrollPosition()
		{
			songItemMenu?.scrollable.SnapTo(0);
		}

		private static void SetupList()
		{
			songItemMenu.ShowPage(OptionsMenu.Page.Customization);
			CleanUpPage(songItemMenu);
		}

		public static void AddSongItems(OptionsMenu optionsMenu)
		{
			CleanUpPage(optionsMenu);
			songItemMenu.screenTitle.text = "Missing " + missingSongsIDs.Count + " songs";

			foreach (string key in missingSongsIDs)
			{
				Song s = (Song)SongRequests.missingSongs[key];
				CreateSongItem(s, optionsMenu);
			}
		}

		private static void CreateSongItem(Song song, OptionsMenu optionsMenu)
		{
			var row = new Il2CppSystem.Collections.Generic.List<GameObject>();

			var textBlock   = optionsMenu.AddTextBlock(0, song.title + " - " + song.artist + " (mapped by " + song.author + ")");
			var TMP         = textBlock.transform.GetChild(0).GetComponent<TextMeshPro>();
			TMP.fontSizeMax = 32;
			TMP.fontSizeMin = 8;
			optionsMenu.scrollable.AddRow(textBlock.gameObject);

			// Skip button
			bool   destroyOnShot = true;
			Action onHit         = new Action(() => {
				missingSongsIDs.Remove(song.song_id); // remove from local copy
				SongRequests.missingSongs.Remove(song.song_id); // remove from main list
				AddSongItems(optionsMenu); // refresh list
			});

			var skipButton = optionsMenu.AddButton(1,
				"Skip",
				onHit,
				null,
				null);
			skipButton.button.destroyOnShot   = destroyOnShot;
			skipButton.button.doMeshExplosion = destroyOnShot;

			// Download button
			Action onHit2 = new Action(() => {
				StartDownload(song.song_id, song.download_url, TMP);
			});

			var downloadButton = optionsMenu.AddButton(0,
				"Download",
				onHit2,
				null,
				null);
			downloadButton.button.destroyOnShot   = destroyOnShot;
			downloadButton.button.doMeshExplosion = destroyOnShot;

			// Preview button
			var previewButton = optionsMenu.AddButton(0,
				"Preview",
				new Action(() => { MelonCoroutines.Start(SongDownloader.StreamPreviewSong(song.preview_url)); }),
				null,
				null);

			optionsMenu.scrollable.AddRow(previewButton.gameObject);
			row.Add(downloadButton.gameObject);
			row.Add(skipButton.gameObject);

			optionsMenu.scrollable.AddRow(row);
		}

		private static void StartDownload(string songID, string downloadURL, TextMeshPro tmp)
        {
			downloadCount++;
			missingSongsIDs.Remove(songID); // remove from local list so we don't queue it up again if Download All is used
			AddSongItems(songItemMenu); // refresh list
			MelonCoroutines.Start(SongDownloader.DownloadSong(downloadURL, OnDownloadDone));
		}

		private static void OnDownloadDone()
        {
			downloadCount--;
			needsSongListRefresh = true;
		}

		private static void OnDownloadAll()
		{
			lookingAtMissingSongs = false;
			needsSongListRefresh  = false;
			downloadAllButton.SetActive(false);
			MenuState.I.GoToMainPage();
			KataConfig.I.CreateDebugText("Downloading missing songs...", new Vector3(0f, -1f, 5f), 5f, null, false, 0.2f);
			MelonCoroutines.Start(DownloadAll());
		}

		private static IEnumerator DownloadAll()
		{
			yield return new WaitForSeconds(0.5f);
			foreach (string id in missingSongsIDs)
			{
				downloadCount++;
				MelonCoroutines.Start(SongDownloader.DownloadSong(((Song)SongRequests.missingSongs[id]).download_url, OnDownloadAllComplete));
				yield return null;
			}
		}

		private static void OnDownloadAllComplete()
        {
			downloadCount--;
			if (downloadCount == 0)
			{
				SongBrowser.ReloadSongList();
			}
		}

		private static void CleanUpPage(OptionsMenu optionsMenu)
		{
			Transform optionsTransform = optionsMenu.transform;
			for (int i = 0; i < optionsTransform.childCount; i++)
			{
				Transform child = optionsTransform.GetChild(i);
				if (child.gameObject.name.Contains("(Clone)"))
				{
					GameObject.Destroy(child.gameObject);
				}
			}
			optionsMenu.mRows.Clear();
			optionsMenu.scrollable.ClearRows();
			optionsMenu.scrollable.mRows.Clear();
		}
	}
}