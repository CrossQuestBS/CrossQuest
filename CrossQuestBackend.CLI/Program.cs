using CrossQuestBackend;
using CrossQuestBackend.Android;
using CrossQuestBackend.Android.Models;
using CrossQuestBackend.Game;
using CrossQuestBackend.Unity;

var games = await ResourceDownloader.Games();
var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version);
await instance.SetupInstance("", unityInstance);

// TODO: Let users decide to download or use their own!
var apkSignerPath = await AndroidToolsDownloader.DownloadApkSigner();
var adb = await AndroidToolsDownloader.DownloadADB();
var apktoolJar = await AndroidToolsDownloader.DownloadApktool();
var ndkPath = await AndroidToolsDownloader.DownloadNDK();

var androidTools = new AndroidTools(ndkPath, apkSignerPath, adb, apktoolJar);

// Mods here (?)

await instance.RunPreIL2CPP(unityInstance);
await instance.RunIL2CPP(unityInstance, androidTools.NDK);

// TODO: Need a way to get boot.config
// TODO: use apktool to unextract downloaded apk
// TODO: Patch apk with compiled files
// TODO: use apktool to extract apk
// TODO: use apk sign to new apk
// TODO: Uninstall game if needed
// TODO: Clear game cache
// TODO: Install game
// TODO: Fix permissions
