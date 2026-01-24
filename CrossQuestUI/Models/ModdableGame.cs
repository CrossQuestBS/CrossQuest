using System.Collections.Generic;

namespace CrossQuestUI.Models
{
    public record struct ModdableGame
    {
        public string PackageId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public ModInfo[] Mods { get; set; }
        public string UnityVersion { get; set; }
        public string BaseProjectUrl { get; set; }
        
    }
}