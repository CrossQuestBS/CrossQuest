using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CrossQuestUI.Services
{
    public static class FileService
    {
        public static async Task<IStorageFile?> OpenFileAsync()
        {
            var files = await App.Current.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open File",
                AllowMultiple = false
            });

            return files.Count >= 1 ? files[0] : null;
        }

        public static async Task<IStorageFolder?> OpenFolderAsync()
        {
            var folders = await App.Current.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Open Folder",
                AllowMultiple = false
            });

            return folders.Count >= 1 ? folders[0] : null;
        }
    }
}