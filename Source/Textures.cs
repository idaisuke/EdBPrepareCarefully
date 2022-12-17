using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

[StaticConstructorOnStartup]
public static class Textures {
    public static Texture2D TexturePassionMajor;
    public static Texture2D TexturePassionMinor;
    public static Texture2D TextureFieldAtlas;
    public static Texture2D TexturePortraitBackground;
    public static Texture2D TextureButtonPrevious;
    public static Texture2D TextureButtonNext;
    public static Texture2D TextureButtonRandom;
    public static Texture2D TextureButtonRandomLarge;
    public static Texture2D TexturePassionNone;
    public static Texture2D TextureButtonDelete;
    public static Texture2D TextureButtonDeleteTab;
    public static Texture2D TextureButtonDeleteTabHighlight;
    public static Texture2D TextureButtonEdit;
    public static Texture2D TextureButtonGenderFemale;
    public static Texture2D TextureButtonGenderMale;
    public static Texture2D TextureButtonReset;
    public static Texture2D TextureButtonClearSkills;
    public static Texture2D TextureDropdownIndicator;
    public static Texture2D TextureAlert;
    public static Texture2D TextureAlertSmall;
    public static Texture2D TextureDerivedRelationship;
    public static Texture2D TextureButtonAdd;
    public static Texture2D TextureRadioButtonOff;
    public static Texture2D TextureDeleteX;
    public static Texture2D TextureAlternateRow;
    public static Texture2D TextureSkillBarFill;
    public static Texture2D TextureSortAscending;
    public static Texture2D TextureSortDescending;
    public static Texture2D TextureTabAtlas;
    public static Texture2D TextureButtonBGAtlas;
    public static Texture2D TextureButtonBGAtlasMouseover;
    public static Texture2D TextureButtonBGAtlasClick;
    public static Texture2D TextureArrowLeft;
    public static Texture2D TextureArrowRight;
    public static Texture2D TextureArrowDown;
    public static Texture2D TextureGenderFemaleLarge;
    public static Texture2D TextureGenderMaleLarge;
    public static Texture2D TextureGenderlessLarge;
    public static Texture2D TextureCheckbox;
    public static Texture2D TextureCheckboxSelected;
    public static Texture2D TextureCheckboxPartiallySelected;
    public static Texture2D TextureDottedLine;
    public static Texture2D TextureMaximizeUp;
    public static Texture2D TextureMaximizeDown;
    public static Texture2D TextureButtonWorldPawn;
    public static Texture2D TextureButtonColonyPawn;
    public static Texture2D TextureFilterAtlas1;
    public static Texture2D TextureFilterAtlas2;
    public static Texture2D TextureButtonCloseSmall;

    static Textures() {
        LoadTextures();
    }

    public static Texture2D TextureWhite => BaseContent.WhiteTex;

    public static Texture2D TextureButtonInfo => TexButton.Info;

    public static bool Loaded { get; private set; }

    public static void Reset() {
        LongEventHandler.ExecuteWhenFinished(() => {
            LoadTextures();
        });
    }

    private static void LoadTextures() {
        Loaded = false;
        TexturePassionMajor = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");
        TexturePassionMinor = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
        TextureRadioButtonOff = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff");
        TexturePortraitBackground = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/CharMakerPortraitBG");
        TextureFieldAtlas = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/FieldAtlas");
        TextureButtonPrevious = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonPrevious");
        TextureButtonNext = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonNext");
        TextureButtonRandom = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonRandom");
        TextureButtonRandomLarge = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonRandomLarge");
        TexturePassionNone = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/NoPassion");
        TextureButtonClearSkills = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonClear");
        TextureButtonCloseSmall = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonCloseSmall");
        TextureButtonDelete = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonDelete");
        TextureButtonDeleteTab = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonDeleteTab");
        TextureButtonDeleteTabHighlight = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonDeleteTabHighlight");
        TextureButtonEdit = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonEdit");
        TextureButtonGenderFemale = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonGenderFemale");
        TextureButtonGenderMale = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonGenderMale");
        TextureButtonReset = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonReset");
        TextureDropdownIndicator = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/DropdownIndicator");
        TextureAlert = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/Alert");
        TextureAlertSmall = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/AlertSmall");
        TextureDerivedRelationship = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/DerivedRelationship");
        TextureButtonAdd = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonAdd");
        TextureDeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
        TextureSortAscending = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/SortAscending");
        TextureSortDescending = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/SortDescending");
        TextureArrowLeft = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ArrowLeft");
        TextureArrowRight = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ArrowRight");
        TextureArrowDown = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ArrowDown");
        TextureGenderFemaleLarge = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/GenderFemaleLarge");
        TextureGenderMaleLarge = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/GenderMaleLarge");
        TextureGenderlessLarge = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/GenderlessLarge");
        TextureCheckbox = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/Checkbox");
        TextureCheckboxSelected = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/CheckboxSelected");
        TextureCheckboxPartiallySelected =
            ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/CheckboxPartiallySelected");
        TextureDottedLine = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/DottedLine");
        TextureMaximizeUp = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/MaximizeUp");
        TextureMaximizeDown = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/MaximizeDown");
        TextureButtonWorldPawn = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonWorldPawn");
        TextureButtonColonyPawn = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/ButtonColonyPawn");
        TextureFilterAtlas1 = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/FilterAtlas1");
        TextureFilterAtlas2 = ContentFinder<Texture2D>.Get("EdB/PrepareCarefully/FilterAtlas2");

        TextureTabAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TabAtlas");

        TextureButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");
        TextureButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");
        TextureButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");

        TextureAlternateRow = SolidColorMaterials.NewSolidColorTexture(new Color(1, 1, 1, 0.05f));
        TextureSkillBarFill = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        Loaded = true;
    }
}
