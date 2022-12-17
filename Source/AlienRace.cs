using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully; 

public class AlienRace {
    public List<AlienRaceBodyAddon> addons = new();

    public ThingDef ThingDef {
        get;
        set;
    }

    public bool UseMelaninLevels {
        get;
        set;
    }

    public bool HasSecondaryColor {
        get;
        set;
    }

    public bool ChangeableColor {
        get;
        set;
    }

    public List<Color> PrimaryColors {
        get;
        set;
    }

    public List<Color> SecondaryColors {
        get;
        set;
    }

    public List<Color> HairColors {
        get;
        set;
    }

    public List<BodyTypeDef> BodyTypes {
        get;
        set;
    }

    public List<string> CrownTypes {
        get;
        set;
    }

    public bool GenderSpecificHeads {
        get;
        set;
    }

    public string GraphicsPathForHeads {
        get;
        set;
    }

    public string GraphicsPathForBodyTypes {
        get;
        set;
    }

    public bool HasHair {
        get;
        set;
    }

    public bool HasBeards {
        get;
        set;
    }

    public bool HasTattoos {
        get;
        set;
    }

    public HashSet<string> HairTags {
        get;
        set;
    }

    public bool RaceSpecificApparelOnly {
        get;
        set;
    }

    public HashSet<string> RaceSpecificApparel {
        get;
        set;
    }

    public HashSet<string> AllowedApparel {
        get;
        set;
    }

    public HashSet<string> DisallowedApparel {
        get;
        set;
    }

    public List<AlienRaceBodyAddon> Addons {
        get => addons;
        set => addons = value;
    }

    public float MinAgeForAdulthood { get; set; }
}
