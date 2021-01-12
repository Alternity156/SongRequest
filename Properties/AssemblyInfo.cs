using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using AudicaModding;

[assembly: AssemblyTitle(SongRequests.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(SongRequests.BuildInfo.Company)]
[assembly: AssemblyProduct(SongRequests.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + SongRequests.BuildInfo.Author)]
[assembly: AssemblyTrademark(SongRequests.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(SongRequests.BuildInfo.Version)]
[assembly: AssemblyFileVersion(SongRequests.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(SongRequests), SongRequests.BuildInfo.Name, SongRequests.BuildInfo.Version, SongRequests.BuildInfo.Author, SongRequests.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]