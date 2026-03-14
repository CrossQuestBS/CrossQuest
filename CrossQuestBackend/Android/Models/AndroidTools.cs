namespace CrossQuestBackend.Android.Models;

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
}