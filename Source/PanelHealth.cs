using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelHealth : PanelModule {
    public delegate void AddImplantHandler(Implant implant);

    public delegate void AddInjuryHandler(Injury injury);

    public delegate void RemoveHediffHandler(Hediff hediff);

    protected static readonly string HediffTypeInjury = "HediffTypeInjury";
    protected static readonly string HediffTypeImplant = "HediffTypeImplant";
    protected HashSet<BodyPartRecord> disabledBodyParts = new();
    protected HashSet<RecipeDef> disabledImplantRecipes = new();
    protected HashSet<InjuryOption> disabledInjuryOptions = new();
    protected float FieldPadding = 6;

    protected Rect FieldRect;
    protected List<Field> fields = new();
    protected List<Hediff> hediffRemovalList = new();
    protected LabelTrimmer labelTrimmer = new();
    protected List<InjurySeverity> permanentInjurySeverities = new();
    protected Rect RectButtonDelete;
    protected string selectedHediffType = HediffTypeImplant;
    protected List<InjurySeverity> severityOptions = new();

    public PanelHealth() {
        permanentInjurySeverities.Add(new InjurySeverity(2));
        permanentInjurySeverities.Add(new InjurySeverity(3));
        permanentInjurySeverities.Add(new InjurySeverity(4));
        permanentInjurySeverities.Add(new InjurySeverity(5));
        permanentInjurySeverities.Add(new InjurySeverity(6));
    }

    public event AddInjuryHandler InjuryAdded;
    public event AddImplantHandler ImplantAdded;
    public event RemoveHediffHandler HediffRemoved;

    public override void Resize(float width) {
        base.Resize(width);
        var buttonSize = new Vector2(12, 12);
        float buttonPadding = 8;
        FieldRect = new Rect(FieldPadding, 0, width - (FieldPadding * 2), Style.FieldHeight);

        RectButtonDelete = new Rect(FieldRect.xMax - buttonPadding - buttonSize.x,
            (FieldRect.height * 0.5f) - (buttonSize.y * 0.5f),
            buttonSize.x, buttonSize.y);

        labelTrimmer.Width = FieldRect.width - ((FieldRect.xMax - RectButtonDelete.xMin) * 2) - 10;
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "Health".Translate().Resolve());

        var currentPawn = state.CurrentPawn;
        var index = 0;
        var groupedHediffs = ReflectionUtil.Method(typeof(HealthCardUtility), "VisibleHediffGroupsInOrder")
            .Invoke(null, new object[] { currentPawn.Pawn, false }) as IEnumerable<IGrouping<BodyPartRecord, Hediff>>;
        foreach (var group in groupedHediffs) {
            foreach (var hediff in group) {
                if (index >= fields.Count) {
                    fields.Add(new Field());
                }

                if (index != 0) {
                    y += FieldPadding;
                }

                y += DrawHediff(currentPawn, hediff, fields[index], y, Width);

                index++;
            }
        }

        // If the index is still zero, then the pawn has no hediffs.  Draw the "none" label.
        if (index == 0) {
            GUI.color = Style.ColorText;
            Widgets.Label(FieldRect.InsetBy(6, 0).OffsetBy(0, y - 4), "EdB.PC.Panel.Health.None".Translate());
            y += FieldRect.height - 4;
        }

        if (hediffRemovalList.Count > 0) {
            foreach (var x in hediffRemovalList) {
                HediffRemoved(x);
            }

            hediffRemovalList.Clear();
        }

        DrawAddButton(top, Width);

        return y - top;
    }

    protected string GetTooltipForPart(Pawn pawn, BodyPartRecord part) {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(part.LabelCap + ": ");
        stringBuilder.AppendLine(" " + pawn.health.hediffSet.GetPartHealth(part) + " / " + part.def.GetMaxHealth(pawn));
        var num = PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, part);
        if (num != 1f) {
            stringBuilder.AppendLine("Efficiency".Translate() + ": " + num.ToStringPercent());
        }

        return stringBuilder.ToString();
    }

    public float DrawHediff(CustomPawn currentPawn, Hediff hediff, Field field, float y, float width) {
        var top = y;
        var pawn = currentPawn.Pawn;
        var part = hediff.Part;
        var labelWidth = part != null
            ? Text.CalcHeight(part.LabelCap, width)
            : Text.CalcHeight("WholeBody".Translate(), width);
        var partColor = HealthUtility.RedColor;
        if (part != null) {
            partColor = HealthUtility.GetPartConditionLabel(pawn, part).Second;
        }

        var partLabel = part != null ? part.LabelCap : "WholeBody".Translate().Resolve();
        var hediffLabel = hediff.LabelCap;
        var changeColor = hediff.LabelColor;

        var fieldRect = FieldRect.OffsetBy(0, y);
        field.Rect = fieldRect;
        var fieldClickRect = fieldRect;
        fieldClickRect.width -= 36;
        field.ClickRect = fieldClickRect;

        if (part != null) {
            field.Label =
                labelTrimmer.TrimLabelIfNeeded(new HealthPanelLabelProvider(partLabel, hediffLabel, partColor,
                    changeColor));
        }
        else {
            var trimmedLabel = labelTrimmer.TrimLabelIfNeeded(hediffLabel);
            field.Label = "<color=#" + ColorUtility.ToHtmlStringRGBA(changeColor) + ">" + trimmedLabel + "</color>";
        }

        field.TipAction = rect => {
            TooltipHandler.TipRegion(rect,
                new TipSignal(() => hediff.GetTooltip(pawn, false), (int)y + 127857, TooltipPriority.Default));
            if (part != null) {
                TooltipHandler.TipRegion(rect,
                    new TipSignal(() => GetTooltipForPart(pawn, part), (int)y + 127858, TooltipPriority.Pawn));
            }
        };

        field.Draw();

        var deleteRect = RectButtonDelete.OffsetBy(0, y);
        if (deleteRect.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
        if (Widgets.ButtonInvisible(deleteRect, false)) {
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            hediffRemovalList.Add(hediff);
        }

        GUI.color = Color.white;

        y += FieldRect.height;

        return y - top;
    }

    public void DrawAddButton(float y, float width) {
        var rect = new Rect(width - 27, y + 12, 16, 16);
        if (rect.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(rect, Textures.TextureButtonAdd);

        // Add button.
        if (Widgets.ButtonInvisible(rect, false)) {
            var customPawn = PrepareCarefully.Instance.State.CurrentPawn;

            var addEntryAction = () => { };

            var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(customPawn);
            var selectedHediffType = this.selectedHediffType;
            RecipeDef selectedRecipe = null;
            InjuryOption selectedInjury = null;
            BodyPartRecord selectedBodyPart = null;
            var bodyPartSelectionRequired = true;
            InjurySeverity selectedSeverity = null;

            Dialog_Options<InjurySeverity> severityDialog;
            Dialog_Options<BodyPartRecord> bodyPartDialog;
            Dialog_Options<InjuryOption> injuryOptionDialog;
            DialogManageImplants manageImplantsDialog;
            Dialog_Options<string> hediffTypeDialog;

            ResetDisabledInjuryOptions(customPawn);

            var addInjuryAction = () => {
                if (bodyPartSelectionRequired) {
                    AddInjuryToPawn(selectedInjury, selectedSeverity, selectedBodyPart);
                }
                else {
                    if (selectedInjury.ValidParts != null && selectedInjury.ValidParts.Count > 0) {
                        foreach (var p in selectedInjury.ValidParts) {
                            var part = healthOptions.FindBodyPartsForDef(p).FirstOrDefault();
                            if (part != null) {
                                AddInjuryToPawn(selectedInjury, selectedSeverity, part.Record);
                            }
                            else {
                                Logger.Warning("Could not find body part record for definition: " + p.defName);
                            }
                        }
                    }
                    else {
                        AddInjuryToPawn(selectedInjury, selectedSeverity, null);
                    }
                }
            };

            severityDialog = new Dialog_Options<InjurySeverity>(severityOptions) {
                ConfirmButtonLabel = "EdB.PC.Common.Add".Translate(),
                CancelButtonLabel = "EdB.PC.Common.Cancel".Translate(),
                HeaderLabel = "EdB.PC.Panel.Health.SelectSeverity".Translate(),
                NameFunc = option => {
                    if (!string.IsNullOrEmpty(option.Label)) {
                        return option.Label;
                    }

                    return selectedInjury.HediffDef.LabelCap;
                },
                SelectedFunc = option => {
                    return option == selectedSeverity;
                },
                SelectAction = option => {
                    selectedSeverity = option;
                },
                ConfirmValidation = () => {
                    if (selectedSeverity == null) {
                        return "EdB.PC.Panel.Health.Error.MustSelectSeverity";
                    }

                    return null;
                },
                CloseAction = () => {
                    addInjuryAction();
                }
            };

            bodyPartDialog = new Dialog_Options<BodyPartRecord>(null) {
                ConfirmButtonLabel = "EdB.PC.Common.Add".Translate(),
                CancelButtonLabel = "EdB.PC.Common.Cancel".Translate(),
                HeaderLabel = "EdB.PC.Dialog.BodyPart.Header".Translate(),
                NameFunc = option => {
                    return option.LabelCap;
                },
                SelectedFunc = option => {
                    return option == selectedBodyPart;
                },
                SelectAction = option => {
                    selectedBodyPart = option;
                },
                EnabledFunc = option => {
                    return !disabledBodyParts.Contains(option);
                },
                ConfirmValidation = () => {
                    if (selectedBodyPart == null) {
                        return "EdB.PC.Dialog.BodyPart.Error.Required";
                    }

                    return null;
                },
                CloseAction = () => {
                    if (selectedHediffType == HediffTypeInjury) {
                        if (severityOptions.Count > 1) {
                            Find.WindowStack.Add(severityDialog);
                        }
                        else {
                            if (severityOptions.Count > 0) {
                                selectedSeverity = severityOptions[0];
                            }

                            addInjuryAction();
                        }
                    }
                    else if (selectedHediffType == HediffTypeImplant) {
                        ImplantAdded(new Implant(selectedBodyPart, selectedRecipe));
                    }
                }
            };

            injuryOptionDialog = new Dialog_Options<InjuryOption>(healthOptions.InjuryOptions) {
                ConfirmButtonLabel = "EdB.PC.Common.Next".Translate(),
                CancelButtonLabel = "EdB.PC.Common.Cancel".Translate(),
                HeaderLabel = "EdB.PC.Dialog.Injury.Header".Translate(),
                NameFunc = option => {
                    return option.Label;
                },
                DescriptionFunc = option => {
                    return option.HediffDef?.description;
                },
                SelectedFunc = option => {
                    return selectedInjury == option;
                },
                SelectAction = option => {
                    selectedInjury = option;
                    if (option.ValidParts == null && !option.WholeBody) {
                        bodyPartSelectionRequired = true;
                    }
                    else if (option.ValidParts != null && option.ValidParts.Count > 0) {
                        bodyPartSelectionRequired = true;
                    }
                    else {
                        bodyPartSelectionRequired = false;
                    }
                },
                EnabledFunc = option => {
                    return !disabledInjuryOptions.Contains(option);
                },
                ConfirmValidation = () => {
                    if (selectedInjury == null) {
                        return "EdB.PC.Dialog.Injury.Error.Required";
                    }

                    return null;
                },
                CloseAction = () => {
                    ResetSeverityOptions(selectedInjury);
                    if (bodyPartSelectionRequired) {
                        bodyPartDialog.Options = healthOptions.BodyPartsForInjury(selectedInjury);
                        var count = bodyPartDialog.Options.Count();
                        if (count > 1) {
                            ResetDisabledBodyParts(bodyPartDialog.Options, customPawn);
                            Find.WindowStack.Add(bodyPartDialog);
                            return;
                        }

                        if (count == 1) {
                            selectedBodyPart = bodyPartDialog.Options.First();
                        }
                    }

                    if (severityOptions.Count > 1) {
                        Find.WindowStack.Add(severityDialog);
                    }
                    else {
                        if (severityOptions.Count > 0) {
                            selectedSeverity = severityOptions[0];
                        }

                        addInjuryAction();
                    }
                }
            };

            hediffTypeDialog = new Dialog_Options<string>(new[] { HediffTypeInjury, HediffTypeImplant }) {
                ConfirmButtonLabel = "EdB.PC.Common.Next".Translate(),
                CancelButtonLabel = "EdB.PC.Common.Cancel".Translate(),
                NameFunc = type => {
                    return ("EdB.PC.Panel.Health." + type).Translate();
                },
                SelectedFunc = type => {
                    return selectedHediffType == type;
                },
                SelectAction = type => {
                    selectedHediffType = type;
                },
                ConfirmValidation = () => {
                    if (selectedHediffType == null) {
                        return "EdB.PC.Panel.Health.Error.MustSelectOption";
                    }

                    return null;
                },
                CloseAction = () => {
                    this.selectedHediffType = selectedHediffType;
                    if (selectedHediffType == HediffTypeInjury) {
                        Find.WindowStack.Add(injuryOptionDialog);
                    }
                    else {
                        ResetDisabledImplantRecipes(customPawn);
                        manageImplantsDialog = new DialogManageImplants(customPawn) {
                            HeaderLabel = "EdB.PC.Dialog.Implant.Header".Translate(),
                            CloseAction = implants => {
                                ApplyImplantsToPawn(customPawn, implants);
                            }
                        };
                        Find.WindowStack.Add(manageImplantsDialog);
                    }
                }
            };
            Find.WindowStack.Add(hediffTypeDialog);
        }
    }

    protected void ApplyImplantsToPawn(CustomPawn pawn, List<Implant> implants) {
        //Logger.Debug("Updated implants");
        //foreach (var i in implants) {
        //    Logger.Debug("  " + i.recipe.LabelCap + ", " + i.PartName + (i.ReplacesPart ? ", replaces part" : ""));
        //}
        pawn.UpdateImplants(implants);
    }

    protected void AddInjuryToPawn(InjuryOption option, InjurySeverity severity, BodyPartRecord bodyPart) {
        var injury = new Injury();
        injury.BodyPartRecord = bodyPart;
        injury.Option = option;
        if (severity != null) {
            injury.Severity = severity.Value;
        }
        else {
            injury.Severity = option.HediffDef.initialSeverity;
        }

        InjuryAdded(injury);
    }

    protected void ResetDisabledInjuryOptions(CustomPawn pawn) {
        disabledInjuryOptions.Clear();
        var optionsHealth = PrepareCarefully.Instance.Providers.Health.GetOptions(pawn);
        foreach (var injuryOption in optionsHealth.InjuryOptions) {
            var option = injuryOption;
            if (option.IsOldInjury) {
                continue;
            }

            if (option.ValidParts != null && option.ValidParts.Count > 0) {
                var records = new HashSet<BodyPartRecord>(optionsHealth.BodyPartsForInjury(injuryOption));
                var recordCount = records.Count;
                var injuryCountForThatPart = pawn.Injuries.Where(i => {
                    return i.Option == option;
                }).Count();
                if (injuryCountForThatPart >= recordCount) {
                    disabledInjuryOptions.Add(injuryOption);
                }
            }
            else {
                var injury = pawn.Injuries.FirstOrDefault(i => {
                    return i.Option == option;
                });
                if (injury != null) {
                    disabledInjuryOptions.Add(injuryOption);
                }
            }
        }
    }

    protected void ResetDisabledBodyParts(IEnumerable<BodyPartRecord> parts, CustomPawn pawn) {
        disabledBodyParts.Clear();
        var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(pawn);
        foreach (var part in parts) {
            var uniquePart = healthOptions.FindBodyPartsForRecord(part);
            if (pawn.HasPartBeenReplaced(part) ||
                pawn.HasAtLeastOnePartBeenReplaced(uniquePart.Ancestors.Select(p => { return p.Record; }))) {
                disabledBodyParts.Add(part);
            }
            else {
                var injury = pawn.Injuries.FirstOrDefault(i => {
                    return i.BodyPartRecord == part;
                });
                if (injury != null) {
                    disabledBodyParts.Add(part);
                }
            }
        }
    }

    protected void ResetDisabledImplantRecipes(CustomPawn pawn) {
        disabledImplantRecipes.Clear();
        var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(pawn);
        foreach (var recipeDef in healthOptions.ImplantRecipes) {
            if (recipeDef.appliedOnFixedBodyParts != null) {
                if (recipeDef.appliedOnFixedBodyParts.Count == 1) {
                    foreach (var uniquePart in
                             healthOptions.FindBodyPartsForDef(recipeDef.appliedOnFixedBodyParts[0])) {
                        if (pawn.HasSameImplant(uniquePart.Record, recipeDef)) {
                            disabledImplantRecipes.Add(recipeDef);
                            break;
                        }

                        if (pawn.HasPartBeenReplaced(uniquePart.Record)) {
                            disabledImplantRecipes.Add(recipeDef);
                            break;
                        }
                    }
                }
            }
        }
    }

    protected void ResetSeverityOptions(InjuryOption injuryOption) {
        severityOptions.Clear();
        if (injuryOption.SeverityOptions().Any(s => s != null)) {
            severityOptions.AddRange(injuryOption.SeverityOptions());
            //Logger.Debug("{" + injuryOption.Label + "} has severity options: " + string.Join(", ", severityOptions.Select(o => o.Label)));
        }
        //Logger.Debug("{" + injuryOption.Label + "} has no severity options");
    }

    // Custom label provider for health diffs that properly maintains the rich text/html tags while trimming.
    public struct HealthPanelLabelProvider : LabelTrimmer.LabelProvider {
        private static readonly int PART_NAME = 0;
        private static readonly int CHANGE_NAME = 1;
        private int elementToTrim;
        private string partName;
        private string changeName;
        private readonly string partColor;
        private readonly string changeColor;

        public HealthPanelLabelProvider(string partName, string changeName, Color partColor, Color changeColor) {
            this.partName = partName;
            this.changeName = changeName;
            this.partColor = ColorUtility.ToHtmlStringRGBA(partColor);
            this.changeColor = ColorUtility.ToHtmlStringRGBA(changeColor);
            elementToTrim = CHANGE_NAME;
        }

        public string Current => "<color=#" + partColor + ">" + partName + "</color>: <color=#" + changeColor + ">" +
                                 changeName + "</color>";

        public string CurrentWithSuffix(string suffix) {
            return "<color=#" + partColor + ">" + partName + "</color>: <color=#" + changeColor + ">" + changeName +
                   suffix + "</color>";
        }

        public string Trim() {
            if (elementToTrim == CHANGE_NAME) {
                if (!TrimChangeName()) {
                    elementToTrim = PART_NAME;
                }
            }
            else {
                TrimPartName();
            }

            return Current;
        }

        private bool TrimString(ref string value) {
            var length = value.Length;
            if (length == 0) {
                return false;
            }

            value = value.Substring(0, length - 1).TrimEnd();
            if (length == 0) {
                return false;
            }

            return true;
        }

        private bool TrimChangeName() {
            return TrimString(ref changeName);
        }

        private bool TrimPartName() {
            return TrimString(ref partName);
        }
    }
}
