namespace CrossQuestBackend.Oculus.Models;

public record QuestDownloadConfig(
    string AppId,
    string Version,
    string BinaryId
);