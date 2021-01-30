# SongRequest
!asr command for song requests in twitch chat.
Allows viewers to request songs from the streamer's local library and, when used with SongBrowser version 2.3.2 or newer, all published custom songs.

## Use
Find a song you want to request using one of these:
* [BeastSaber Audica Song List](https://bsaber.com/category/audica/)
* [Audica Wiki](http://www.audica.wiki/audicawiki/index.php/Custom_Songs)

Then use a variation of the !asr command in twitch chat:

| Command                                       | Example                              | Result                                          |
|-----------------------------------------------|--------------------------------------|-------------------------------------------------|
|!asr *title*                                   |!asr Those Who Fight                  | Request Those Who Fight Further                 | 
|!asr *title* -mapper *mapper*                  |!asr Monster -mapper Octo             | Request Monster mapped by Octo                  |
|!asr *title* -artist *artist*                  |!asr MONSTER -artist REOL             | Request MONSTER by REOL                         |
|!asr *title* - artist *artist* -mapper *mapper*|!asr Man -artist Rihanna -mapper Draco| Request Man Down by Rihanna, mapped by DeadDraco|

Note that search is not case sensitive, so `!asr Monster` and `!asr MONSTER` give the same result unless the mapper or artist is explicitly included in the command (the way it is done in the examples above).

## Installation
* Download latest release from [here](https://github.com/Alternity156/SongRequest/releases)
* Save the .dll file to [YourAudicaFolder]\Mods
* Enable the in-game twitch chat
  * Go to Settings - Spectator Cam
  * Scroll to the *Twitch Chat View* section
  * Enable *Twitch Chat View*
  * Enter the name of your channel in *Twitch Channel:*
* Optionally: Install the [SongBrowser mod](https://github.com/octoberU/SongBrowser) version 2.4.0 or newer to enable requests for custom songs that haven't been downloaded yet
