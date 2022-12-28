using System;

namespace EdB.PrepareCarefully;

public static class PawnLoaderV1400 {
    public static CustomPawn Load(string name) {
        throw new NotImplementedException("Not implemented yet");
    }

    public static CustomPawn ConvertSaveRecordToPawn(SaveRecordPawnV1400 record) {
        var pawnKindDef = record.PawnKindDef;

        var pawnThingDef = record.ThingDef;

        throw new NotImplementedException();
    }
}
