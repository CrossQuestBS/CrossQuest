using System.Collections.Generic;

namespace CrossQuestBackend.Oculus.Models;

public record RiftDownloadConfig(
    string AppId,
    string Version,
    string BinaryId,
    List<string> FilesToDownload
);