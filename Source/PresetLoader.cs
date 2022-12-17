using System;
using Verse;
using PostLoadIniter = EdB.PrepareCarefully.Reflection.PostLoadIniter;

namespace EdB.PrepareCarefully;

public class PresetLoader {
    public static bool LoadFromFile(PrepareCarefully loadout, string presetName) {
        var version = "";
        try {
            Scribe.loader.InitLoading(PresetFiles.FilePathForSavedPreset(presetName));
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
            "4" => new PresetLoaderV5().Load(loadout, presetName),
            "5" => new PresetLoaderV5().Load(loadout, presetName),
            _ => throw new Exception("Invalid preset version")
        };
    }

    public static void ClearSaveablesAndCrossRefs() {
        // I don't fully understand how these cross-references and saveables are resolved, but
        // if we don't clear them out, we get null pointer exceptions.
        PostLoadIniter.ClearSaveablesToPostLoad(Scribe.loader.initer);
        if (Scribe.loader.crossRefs.crossReferencingExposables != null) {
            Scribe.loader.crossRefs.crossReferencingExposables.Clear();
        }

        Scribe.loader.FinalizeLoading();
    }
}
