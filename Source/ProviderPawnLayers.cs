using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderPawnLayers {
    private readonly PawnLayer accessoryLayer = new() {
        Name = "Accessory",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.Belt,
        Label = "EdB.PC.Pawn.PawnLayer.Accessory".Translate()
    };

    private Dictionary<AlienRace, List<PawnLayer>> alienPawnLayers = new();

    private readonly PawnLayer bottomClothingLayer = new() {
        Name = "BottomClothingLayer",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.OnSkin,
        Label = "EdB.PC.Pawn.PawnLayer.BottomClothingLayer".Translate()
    };

    private readonly PawnLayer eyeCoveringLayer = new() {
        Name = "EyeCovering",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.EyeCover,
        Label = "EdB.PC.Pawn.PawnLayer.EyeCovering".Translate()
    };

    private readonly PawnLayer hatLayer = new() {
        Name = "Hat",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.Overhead,
        Label = "EdB.PC.Pawn.PawnLayer.Hat".Translate()
    };

    private readonly PawnLayer middleClothingLayer = new() {
        Name = "MiddleClothingLayer",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.Middle,
        Label = "EdB.PC.Pawn.PawnLayer.MiddleClothingLayer".Translate()
    };

    private readonly PawnLayer pantsLayer = new() {
        Name = "Pants",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.OnSkin,
        Label = "EdB.PC.Pawn.PawnLayer.Pants".Translate()
    };

    private readonly Dictionary<Pair<ThingDef, Gender>, List<PawnLayer>> pawnLayerCache = new();

    private readonly PawnLayer topClothingLayer = new() {
        Name = "TopClothingLayer",
        Apparel = true,
        ApparelLayer = ApparelLayerDefOf.Shell,
        Label = "EdB.PC.Pawn.PawnLayer.TopClothingLayer".Translate()
    };

    private Dictionary<RaceProperties, List<PawnLayer>> pawnLayerLookup = new();

    public List<PawnLayer> GetLayersForPawn(CustomPawn pawn) {
        List<PawnLayer> result = null;
        if (!pawnLayerCache.TryGetValue(new Pair<ThingDef, Gender>(pawn.Pawn.def, pawn.Gender), out result)) {
            result = InitializePawnLayers(pawn.Pawn.def, pawn.Gender);
            pawnLayerCache.Add(new Pair<ThingDef, Gender>(pawn.Pawn.def, pawn.Gender), result);
        }

        return result;
    }

    public List<PawnLayer> InitializePawnLayers(ThingDef pawnDef, Gender gender) {
        var race = PrepareCarefully.Instance.Providers.AlienRaces.GetAlienRace(pawnDef);
        if (race == null) {
            return InitializeDefaultPawnLayers(pawnDef, gender);
        }

        return InitializeAlienPawnLayers(pawnDef, gender, race);
    }

    private List<PawnLayer> InitializeDefaultPawnLayers(ThingDef pawnDef, Gender gender) {
        var defaultLayers = new List<PawnLayer> {
            InitializeHairLayer(pawnDef, gender),
            InitializeBeardLayer(pawnDef, gender),
            InitializeHeadLayer(pawnDef, gender),
            InitializeBodyLayer(pawnDef, gender)
        };

        if (ModLister.IdeologyInstalled) {
            defaultLayers.Add(InitializeFaceTattooLayer(pawnDef, gender));
            defaultLayers.Add(InitializeBodyTattooLayer(pawnDef, gender));
        }

        defaultLayers.AddRange(new[] {
            pantsLayer, bottomClothingLayer, middleClothingLayer, topClothingLayer, hatLayer, accessoryLayer,
            eyeCoveringLayer
        });

        return defaultLayers;
    }

    private List<PawnLayer> InitializeAlienPawnLayers(ThingDef pawnDef, Gender gender, AlienRace race) {
        var layers = new List<PawnLayer>();
        if (race.HasHair) {
            layers.Add(InitializeHairLayer(pawnDef, gender));
        }

        if (race.HasBeards) {
            layers.Add(InitializeBeardLayer(pawnDef, gender));
        }

        layers.Add(InitializeHeadLayer(pawnDef, gender));
        layers.Add(InitializeBodyLayer(pawnDef, gender));

        if (race.Addons != null) {
            var optionsHair = PrepareCarefully.Instance.Providers.Hair.GetHairsForRace(pawnDef);
            foreach (var addon in race.Addons) {
                var layer = new PawnLayerAlienAddon();
                layer.Name = addon.Name;
                layer.Label = addon.Name;
                if (addon.Skin) {
                    layer.Skin = true;
                }
                else {
                    layer.Hair = true;
                    layer.ColorSelectorType = ColorSelectorType.RGB;
                    layer.ColorSwatches = optionsHair.Colors;
                }

                layer.AlienAddon = addon;
                layer.Options = InitializeAlienAddonOptions(race, addon);
                if (layer.Options == null || layer.Options.Count == 1) {
                    continue;
                }

                layers.Add(layer);
            }
        }

        if (ModLister.IdeologyInstalled && race.HasTattoos) {
            layers.AddRange(new[] {
                InitializeFaceTattooLayer(pawnDef, gender), InitializeBodyTattooLayer(pawnDef, gender)
            });
        }

        layers.AddRange(new[] {
            pantsLayer, bottomClothingLayer, middleClothingLayer, topClothingLayer, hatLayer, accessoryLayer,
            eyeCoveringLayer
        });
        return layers;
    }

    private PawnLayer InitializeHairLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerHair { Name = "Hair", Label = "EdB.PC.Pawn.PawnLayer.Hair".Translate() };
        result.Options = InitializeHairOptions(pawnDef, gender);
        result.ColorSwatches = PrepareCarefully.Instance.Providers.Hair.GetHairsForRace(pawnDef).Colors;
        return result;
    }

    private List<PawnLayerOption> InitializeHairOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        var hairDefs = PrepareCarefully.Instance.Providers.Hair.GetHairs(pawnDef, gender);
        foreach (var def in hairDefs) {
            var option = new PawnLayerOptionHair();
            option.HairDef = def;
            options.Add(option);
        }

        return options;
    }

    private PawnLayer InitializeBeardLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerBeard { Name = "Beard", Label = "EdB.PC.Pawn.PawnLayer.Beard".Translate() };
        result.Options = InitializeBeardOptions(pawnDef, gender);
        result.ColorSwatches = PrepareCarefully.Instance.Providers.Hair.GetHairsForRace(pawnDef).Colors;
        return result;
    }

    private List<PawnLayerOption> InitializeBeardOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        var beardDefs = PrepareCarefully.Instance.Providers.Beards.GetBeards(pawnDef, gender);
        foreach (var def in beardDefs) {
            var option = new PawnLayerOptionBeard();
            option.BeardDef = def;
            options.Add(option);
        }

        return options;
    }

    private PawnLayer InitializeFaceTattooLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerFaceTattoo {
            Name = "FaceTattoo", Label = "EdB.PC.Pawn.PawnLayer.FaceTattoo".Translate()
        };
        result.Options = InitializeFaceTattooOptions(pawnDef, gender);
        return result;
    }

    private List<PawnLayerOption> InitializeFaceTattooOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        var defs = PrepareCarefully.Instance.Providers.FaceTattoos.GetTattoos(pawnDef, gender);
        foreach (var def in defs) {
            var option = new PawnLayerOptionTattoo();
            option.TattooDef = def;
            options.Add(option);
        }

        return options;
    }

    private PawnLayer InitializeBodyTattooLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerBodyTattoo {
            Name = "BodyTattoo", Label = "EdB.PC.Pawn.PawnLayer.BodyTattoo".Translate()
        };
        result.Options = InitializeBodyTattooOptions(pawnDef, gender);
        return result;
    }

    private List<PawnLayerOption> InitializeBodyTattooOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        var defs = PrepareCarefully.Instance.Providers.BodyTattoos.GetTattoos(pawnDef, gender);
        foreach (var def in defs) {
            var option = new PawnLayerOptionTattoo();
            option.TattooDef = def;
            options.Add(option);
        }

        return options;
    }

    private PawnLayer InitializeHeadLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerHead { Name = "Head", Label = "EdB.PC.Pawn.PawnLayer.HeadType".Translate() };
        result.Options = InitializeHeadOptions(pawnDef, gender);
        return result;
    }

    private List<PawnLayerOption> InitializeHeadOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        foreach (var headType in PrepareCarefully.Instance.Providers.HeadTypes.GetHeadTypes(pawnDef, gender)) {
            var option = new PawnLayerOptionHead();
            option.HeadType = headType;
            options.Add(option);
        }

        return options;
    }

    private PawnLayer InitializeBodyLayer(ThingDef pawnDef, Gender gender) {
        PawnLayer result = new PawnLayerBody { Name = "Body", Label = "EdB.PC.Pawn.PawnLayer.BodyType".Translate() };
        result.Options = InitializeBodyOptions(pawnDef, gender);
        return result;
    }

    private List<PawnLayerOption> InitializeBodyOptions(ThingDef pawnDef, Gender gender) {
        var options = new List<PawnLayerOption>();
        foreach (var bodyType in PrepareCarefully.Instance.Providers.BodyTypes.GetBodyTypesForPawn(pawnDef, gender)) {
            var option = new PawnLayerOptionBody();
            option.BodyTypeDef = bodyType;
            options.Add(option);
        }

        return options;
    }

    private List<PawnLayerOption> InitializeAlienAddonOptions(AlienRace race, AlienRaceBodyAddon addon) {
        if (addon.OptionCount == 0) {
            return null;
        }

        var result = new List<PawnLayerOption>();
        for (var i = 0; i < addon.OptionCount; i++) {
            var option = new PawnLayerOptionAlienAddon();
            option.Label = "EdB.PC.Pawn.PawnLayer.AlienAddonOption".Translate(i + 1);
            option.Index = i;
            result.Add(option);
        }

        return result;
    }

    public PawnLayer FindLayerForApparelLayer(ApparelLayerDef layer) {
        if (layer == ApparelLayerDefOf.OnSkin) {
            return bottomClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Middle) {
            return middleClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Shell) {
            return topClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Overhead) {
            return hatLayer;
        }

        if (layer == ApparelLayerDefOf.Belt) {
            return accessoryLayer;
        }

        if (layer == ApparelLayerDefOf.EyeCover) {
            return eyeCoveringLayer;
        }

        return null;
    }

    public PawnLayer FindLayerForApparel(ThingDef def) {
        var apparelProperties = def.apparel;
        if (apparelProperties == null) {
            Logger.Warning("Trying to find an apparel layer for a non-apparel thing definition " + def.defName);
            return null;
        }

        var layer = apparelProperties.LastLayer;
        if (layer == ApparelLayerDefOf.OnSkin && apparelProperties.bodyPartGroups.Count == 1) {
            if (apparelProperties.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs)) {
                return pantsLayer;
            }

            if (apparelProperties.bodyPartGroups[0].defName == "Hands") {
                return null;
            }

            if (apparelProperties.bodyPartGroups[0].defName == "Feet") {
                return null;
            }
        }

        if (layer == ApparelLayerDefOf.OnSkin) {
            return bottomClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Middle) {
            return middleClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Shell) {
            return topClothingLayer;
        }

        if (layer == ApparelLayerDefOf.Overhead) {
            return hatLayer;
        }

        if (layer == ApparelLayerDefOf.Belt) {
            return accessoryLayer;
        }

        if (layer == ApparelLayerDefOf.EyeCover) {
            return eyeCoveringLayer;
        }

        Logger.Warning(String.Format("Cannot find matching layer for apparel: {0}.  Last layer: {1}", def.defName,
            apparelProperties.LastLayer));
        return null;
    }

    public PawnLayer FindLayerFromDeprecatedIndex(int index) {
        switch (index) {
            case 0:
                // TODO
                return null;
            case 1:
                return bottomClothingLayer;
            case 2:
                return pantsLayer;
            case 3:
                return middleClothingLayer;
            case 4:
                return topClothingLayer;
            case 5:
                // TODO
                return null;
            case 6:
                // TODO
                return null;
            case 7:
                return hatLayer;
            case 8:
                return accessoryLayer;
            default:
                return null;
        }
    }
}
