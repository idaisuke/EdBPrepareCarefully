using System;
using RimWorld;
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

        if ("2".Equals(version)) {
            Messages.Message("EdB.PC.Dialog.PawnPreset.Error.PreAlpha13NotSupported".Translate(),
                MessageTypeDefOf.ThreatBig);
            return null;
        }

        if ("3".Equals(version)) {
            return new ColonistLoaderVersion3().Load(loadout, name);
        }

        if ("4".Equals(version)) {
            return new PawnLoaderV5().Load(loadout, name);
        }

        if ("5".Equals(version)) {
            return new PawnLoaderV5().Load(loadout, name);
        }

        throw new Exception("Invalid preset version");
    }
}
