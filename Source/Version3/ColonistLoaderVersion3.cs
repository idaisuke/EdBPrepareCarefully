using System;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully; 

public class ColonistLoaderVersion3 {
    public CustomPawn Load(PrepareCarefully loadout, string name) {
        var pawnRecord = new SaveRecordPawnV3();
        var modString = "";
        var version = "";
        try {
            Scribe.loader.InitLoading(ColonistFiles.FilePathForSavedColonist(name));
            Scribe_Values.Look<string>(ref version, "version", "unknown");
            Scribe_Values.Look<string>(ref modString, "mods", "");

            try {
                Scribe_Deep.Look(ref pawnRecord, "colonist", null);
            }
            catch (Exception e) {
                Messages.Message(modString, MessageTypeDefOf.SilentInput);
                Messages.Message("EdB.PC.Dialog.PawnPreset.Error.Failed".Translate(), MessageTypeDefOf.RejectInput);
                Logger.Warning("Failed to load preset", e);
                Logger.Warning("Colonist was created with the following mods: " + modString);
                return null;
            }
        }
        catch (Exception e) {
            Logger.Error("Failed to load preset file");
            throw e;
        }
        finally {
            PresetLoader.ClearSaveablesAndCrossRefs();
        }

        if (pawnRecord == null) {
            Messages.Message(modString, MessageTypeDefOf.SilentInput);
            Messages.Message("EdB.PC.Dialog.PawnPreset.Error.Failed".Translate(), MessageTypeDefOf.RejectInput);
            Logger.Warning("Colonist was created with the following mods: " + modString);
            return null;
        }

        var loader = new PresetLoaderVersion3();
        var loadedPawn = loader.LoadPawn(pawnRecord);
        if (loadedPawn != null) {
            var idConflictPawn = PrepareCarefully.Instance.Pawns.FirstOrDefault(p => {
                return p.Id == loadedPawn.Id;
            });
            if (idConflictPawn != null) {
                loadedPawn.GenerateId();
            }

            return loadedPawn;
        }

        loadout.State.AddError(loader.ModString);
        loadout.State.AddError("EdB.PC.Dialog.Preset.Error.NoCharacter".Translate());
        Logger.Warning("Preset was created with the following mods: " + modString);
        return null;
    }
}
