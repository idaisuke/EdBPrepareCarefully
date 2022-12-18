using System;
using Verse;

namespace EdB.PrepareCarefully;

public static class ColonistLoader {
    public static CustomPawn LoadFromFile(string name) {
        var version = "";
        try {
            Scribe.loader.InitLoading(ColonistFiles.FilePathForSavedColonist(name));
            Scribe_Values.Look<string>(ref version, "version", "unknown");
        }
        catch (Exception) {
            Logger.Error("Failed to load preset file");
            throw;
        }
        finally {
            Scribe.mode = LoadSaveMode.Inactive;
        }

        return version switch {
            "4" => new PawnLoaderV5().Load(name),
            "5" => new PawnLoaderV5().Load(name),
            _ => throw new Exception("Invalid preset version")
        };
    }
}
