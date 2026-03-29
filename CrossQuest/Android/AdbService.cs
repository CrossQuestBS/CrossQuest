using System.Threading.Tasks;
using CrossQuest.Android.Models;

namespace CrossQuest.Android;

public static class AdbService
{
    

    
    
    public static async Task ClearCache(AndroidTools tools)
    {

        await ProcessCaller.ProcessAsync(tools.Adb, "shell pm clear com.beatgames.beatsaber");
    }
    
    public static async Task StartGame(AndroidTools tools)
    {
        await ProcessCaller.ProcessAsync(tools.Adb, "shell am start com.beatgames.beatsaber/com.unity3d.player.UnityPlayerGameActivity");
    }
    
    public static async Task InstallAPK(AndroidTools tools, string apk)
    {
        await ProcessCaller.ProcessAsync(tools.Adb, $"install \"{apk}\"");
    }
    
    public static async Task<bool> IsDeviceConnected(AndroidTools tools)
    {
        var output = await ProcessCaller.ProcessOutputAsync(tools.Adb, "devices -l");
        
        // TODO: Figure out if this is the case for Quest 1, and Quest 2 and Quest 3s
        // Quest 3 is model:Quest_3
        return output.Contains("model:Quest");
    }
    
}
