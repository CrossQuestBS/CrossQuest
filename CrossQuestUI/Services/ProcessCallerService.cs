using System.Diagnostics;
using System.Threading.Tasks;
using CrossQuestUI.Models;

namespace CrossQuestUI.Services
{
    public static class ProcessCallerService
    {
        public static async Task<bool> ProcessAsync(string fileName, string arguments, bool useShellExecute = false)
        {
            
            var startInfo = new ProcessStartInfo() { FileName = fileName, Arguments = arguments, CreateNoWindow = true, UseShellExecute = useShellExecute}; 
            using var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();

            await proc.WaitForExitAsync();

            return proc.ExitCode == 0;

        }

        public static async Task<bool> ProcessAsync(string fileName, string arguments, string expectedOutputText)
        {
            var startInfo = new ProcessStartInfo() { FileName = fileName, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
            using var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();

            var result = await proc.StandardOutput.ReadToEndAsync();
            
            await proc.WaitForExitAsync();

            return result.Contains(expectedOutputText);
        }

        public static async Task<string> ProcessOutputAsync(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo() { FileName = fileName, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
            using var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();

            var result = await proc.StandardOutput.ReadToEndAsync();
            
            await proc.WaitForExitAsync();

            return result;
        }
    }
}