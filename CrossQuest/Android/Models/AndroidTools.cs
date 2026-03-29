using System.IO;
using System.Threading.Tasks;
using CrossQuest.Unity;
using Newtonsoft.Json;

namespace CrossQuest.Android.Models;

public class AndroidTools
{
    public string NDK { get; set; }
    
    public string ApktoolJar { get; set; }
    public string Apksigner { get; set; }
    public string Adb { get; set; }

    
    public AndroidTools(string ndk, string apksigner, string adb, string apktoolJar)
    {
        NDK = ndk;
        ApktoolJar = apktoolJar;
        Apksigner = apksigner;
        Adb = adb;
    }

    public async Task Save(UnityInstance instance)
    {
        var serializeObject = JsonConvert.SerializeObject(this);
        await File.WriteAllTextAsync(Path.Join(instance.InstancePath, "AndroidTools.json"), serializeObject);
    }
}