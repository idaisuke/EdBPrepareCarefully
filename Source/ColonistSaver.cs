using System;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully;

public static class ColonistSaver {
    //
    // Static Methods
    //
    public static void SaveToFile(CustomPawn customPawn, string colonistName) {
        var pawn = new SaveRecordPawnV5(customPawn);
        try {
            Scribe.saver.InitSaving(ColonistFiles.FilePathForSavedColonist(colonistName), "character");
            var versionStringFull = "5";
            Scribe_Values.Look<string>(ref versionStringFull, "version");
            var modString = LoadedModManager.RunningMods
                .Select<ModContentPack, string>((Func<ModContentPack, string>)(mod => mod.Name)).ToCommaList(true);
            Scribe_Values.Look<string>(ref modString, "mods");

            Scribe_Deep.Look(ref pawn, "pawn");
        }
        catch (Exception e) {
            Logger.Error("Failed to save preset file");
            throw e;
        }
        finally {
            Scribe.saver.FinalizeSaving();
            Scribe.mode = LoadSaveMode.Inactive;
        }
    }
}
