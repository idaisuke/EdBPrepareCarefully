using System;
using Verse;

namespace EdB.PrepareCarefully;

public class ColonistLoader {
    public static CustomPawn LoadFromFile(PrepareCarefully loadout, string name) {
        var version = "";
        try {
            Scribe.loader.InitLoading(ColonistFiles.FilePathForSavedColonist(name));
            Scribe_Values.Look<string>(ref version, "version", "unknown");
        }
        catch (Exception e) {
            Logger.Error("Failed to load preset file");
            throw e;
        }
        finally {
            Scribe.mode = LoadSaveMode.Inactive;
        }

        return version switch {
            "4" => new PawnLoaderV5().Load(loadout, name),
            "5" => new PawnLoaderV5().Load(loadout, name),
            _ => throw new Exception("Invalid preset version")
        };
    }
}
