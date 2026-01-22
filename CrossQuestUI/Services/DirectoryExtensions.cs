using System.IO;

namespace CrossQuestUI.Services
{
    public static class DirectoryExtensions
    {
        public static void CopyFolder(string fromPath, string toPath)
        {
            if (!Directory.Exists( toPath ))
                Directory.CreateDirectory( toPath );
            var files = Directory.GetFiles( fromPath );
            foreach (var file in files)
            {
                var name = Path.GetFileName( file );
                var dest = Path.Combine( toPath, name );
                File.Copy( file, dest );
            }
            var folders = Directory.GetDirectories( fromPath );
            foreach (var folder in folders)
            {
                var name = Path.GetFileName( folder );
                var dest = Path.Combine( toPath, name );
                CopyFolder( folder, dest );
            }
        }
        
        public static void MoveZippedToParent(string folder)
        {
            var subDirectories = Directory.GetDirectories(folder);
            
            if (subDirectories.Length == 1)
            {
                CopyFolder(subDirectories[0], folder);
            }
            
            Directory.Delete(subDirectories[0], true);
        }
    }
}