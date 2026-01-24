using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrossQuestUI.Models;

namespace CrossQuestUI.Services
{
    public static class UnityHubService
    {
        private static string HubPath => "/Applications/Unity Hub.app/Contents/MacOS/Unity Hub";

        public static async Task<UnityEditor[]> GetEditors()
        {
            var output = await ProcessCallerService.ProcessOutputAsync(HubPath, "-- --headless editors -i");
            
            List<UnityEditor> editors = new List<UnityEditor>();
            

            foreach (var line in output.GetLines())
            {
                editors.Add(new UnityEditor(line.Split("installed at")[0].Split("(")[0].Trim(), line.Split("installed at")[1].Trim()));
            }
            
            return editors.ToArray();
        }
    }
}