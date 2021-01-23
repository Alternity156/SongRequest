# SongRequest
!asr command for song requests in twitch chat.
Allows viewers to request songs from the streamer's local library and, when used with SongBrowser version 2.3.2 or newer, all published custom songs.

## Use
* Find a song you want to request using one of these:
  * [BeastSaber Audica Song List](https://bsaber.com/category/audica/)
  * [Audica Wiki](http://www.audica.wiki/audicawiki/index.php/Custom_Songs)
* Type !asr *song title* into the twitch chat
  * Title can be complete or partial
    * Unless there are multiple songs with the same title, a search for the full title should always give the correct result
	* The search is not case sensitive, so searching for *Monster* and *MONSTER* gives the same result
	* Partial searches may give unexpected results
  * Advanced search (especially useful if there are multiple songs with the same song title, or if the same song has been mapped multiple times): 
    * Add -artist *artist* to the end to limit the search to that artist (may be partial, e.g. *Dua* instead of *Dua Lipa*)
    * Add -mapper *mapper* to the end to limit the search to that mapper (may be partial, e.g. *SugarBear* instead of *SugarBear125*)
	* Note that these search tags are case sensitive, so -Artist and -Mapper do **not** work

Examples: 
* !asr Those Who Fight Further 
  * Full song title
* !asr Counting Bodies Like Sheep
  * Partial song title
* !asr MONSTER -artist REOL
  * Song title (MONSTER) plus artist (REOL)
* !asr Monster -mapper octo
  * Song title (Monster) plus mapper (octo)
* !asr Man -artist Rihanna -mapper DeadDraco
  * Partial song title (Man) plus artist (Rihanna) plus mapper (DeadDraco)

## Installation
* Download latest release from [here](https://github.com/Alternity156/SongRequest/releases)
* Save the .dll file to [YourAudicaFolder]\Mods
* Enable the in-game twitch chat
  * Go to Settings - Spectator Cam
  * Scroll to the *Twitch Chat View* section
  * Enable *Twitch Chat View*
  * Enter the name of your channel in *Twitch Channel:*
* Optionally: Install the [SongBrowser mod](https://github.com/octoberU/SongBrowser) version 2.3.2 or newer to enable requests for custom songs that haven't been downloaded yet
