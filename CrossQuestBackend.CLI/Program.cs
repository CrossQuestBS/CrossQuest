using CrossQuestBackend;
using CrossQuestBackend.Game;
using CrossQuestBackend.Unity;

var games = await ResourceDownloader.Games();
var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version, version.UnityVersion.Version);

await instance.SetupInstance("", unityInstance);

// Mods here (?)

await instance.RunPreIL2CPP(unityInstance);
await instance.RunIL2CPP(unityInstance, "/Users/maribell/QPM-RS/ndk/29.0.14206865+preview-0");



// TODO: Need a way to get boot.config
// TODO: use apktool to unextract downloaded apk
// TODO: Patch apk with compiled files
// TODO: use apktool to extract apk
// TODO: use apk sign to new apk
// TODO: Uninstall game if needed
// TODO: Clear game cache
// TODO: Install game
// TODO: Fix permissions
