using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelRelationshipsParentChild : PanelBase {
    public delegate void AddChildToGroupHandler(ParentChildGroup group, CustomPawn pawn);

    public delegate void AddGroupHandler(ParentChildGroup group);

    public delegate void AddParentToGroupHandler(ParentChildGroup group, CustomPawn pawn);

    public delegate void RemoveChildFromGroupHandler(ParentChildGroup group, CustomPawn pawn);

    public delegate void RemoveGroupHandler(ParentChildGroup group);

    public delegate void RemoveParentFromGroupHandler(ParentChildGroup group, CustomPawn pawn);

    private readonly List<PawnGroupPair> childrenToRemove = new();
    private readonly Color ColorChild = new(22f / 255f, 22f / 255f, 23f / 255f);
    private readonly Color ColorChildEmpty = new(22f / 255f, 22f / 255f, 23f / 255f, 0.40f);

    private readonly Color ColorParent = new(69f / 255f, 70f / 255f, 72f / 255f);
    private readonly Color ColorParentEmpty = new(69f / 255f, 70f / 255f, 72f / 255f, 0.25f);
    private readonly float PaddingAddButton = 4;
    private readonly float PaddingBox = 12;
    private readonly float PaddingDeleteButton = 6;
    private readonly List<PawnGroupPair> parentsToRemove = new();

    private readonly ScrollViewHorizontal scrollView = new();
    private readonly Vector2 SizeAddButton = new(16, 16);
    private readonly Vector2 SizeArrowBase = new(12, 10);
    private readonly Vector2 SizeArrowHead = new(20, 10);
    private readonly float SizeChildOffset = 24;
    private readonly Vector2 SizeDeleteButton = new(12, 12);
    private readonly Vector2 SizeGender = new(48, 48);
    private readonly Vector2 SizePawn = new(90, 90);
    private readonly float SpacingArrow = 12;
    private readonly float SpacingGender = 9;
    private readonly float SpacingGroup = 16;
    private readonly float SpacingPawn = 12;

    private List<ParentChildGroup> groupsToRemove = new();
    private Rect RectScrollView;
    protected List<WidgetTable<CustomPawn>.RowGroup> rowGroups = new();

    public override string PanelHeader => "EdB.PC.Panel.RelationshipsParentChild.Title".Translate();

    public event AddParentToGroupHandler ParentAddedToGroup;
    public event RemoveParentFromGroupHandler ParentRemovedFromGroup;
    public event AddChildToGroupHandler ChildAddedToGroup;
    public event RemoveChildFromGroupHandler ChildRemovedFromGroup;
    public event AddGroupHandler GroupAdded;

    public override void Resize(Rect rect) {
        base.Resize(rect);
        var padding = Style.SizePanelPadding;
        RectScrollView = new Rect(padding.x, BodyRect.y, rect.width - (padding.x * 2),
            rect.height - BodyRect.y - padding.y);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);
        float cursor = 0;

        GUI.color = Color.white;

        GUI.BeginGroup(RectScrollView);
        scrollView.Begin(new Rect(Vector2.zero, RectScrollView.size));
        try {
            foreach (var group in PrepareCarefully.Instance.RelationshipManager.ParentChildGroups) {
                cursor = DrawGroup(cursor, group);
            }

            cursor = DrawNextGroup(cursor);
        }
        finally {
            scrollView.End(cursor);
            GUI.EndGroup();
        }

        // Remove any parents or children that were marked for removal.
        if (parentsToRemove.Count > 0) {
            foreach (var pair in parentsToRemove) {
                ParentRemovedFromGroup(pair.Group, pair.Pawn);
            }

            parentsToRemove.Clear();
        }

        if (childrenToRemove.Count > 0) {
            foreach (var pair in childrenToRemove) {
                ChildRemovedFromGroup(pair.Group, pair.Pawn);
            }

            childrenToRemove.Clear();
        }
    }

    protected float DrawGroup(float cursor, ParentChildGroup group) {
        var parentBoxCount = group.Parents.Count + 1;
        var childBoxCount = group.Children.Count + 1;
        var widthOfParents = (parentBoxCount * SizePawn.x) + (SpacingPawn * (parentBoxCount - 1)) + (PaddingBox * 2);
        var widthOfChildren = SizeChildOffset + (childBoxCount * SizePawn.x) + (SpacingPawn * (childBoxCount - 1)) +
                              (PaddingBox * 2);
        var width = Mathf.Max(widthOfChildren, widthOfParents);
        var height = (PaddingBox * 2) + (SpacingArrow * 2) + SizeArrowBase.y + SizeArrowHead.y + (SizePawn.y * 2);
        var rect = new Rect(cursor, 0, width, height);

        GUI.color = Style.ColorPanelBackgroundItem;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);

        GUI.BeginGroup(rect);
        try {
            var arrowColor = group.Parents.Count > 0 ? ColorParent : ColorParentEmpty;
            var firstParentPawnRect = new Rect(PaddingBox, PaddingBox, SizePawn.x, SizePawn.y);
            var parentPawnRect = firstParentPawnRect;
            float arrowBaseRightParent = 0;
            foreach (var parent in group.Parents) {
                GUI.color = ColorParent;
                GUI.DrawTexture(parentPawnRect, BaseContent.WhiteTex);

                var arrowBaseRect = new Rect(parentPawnRect.MiddleX() - SizeArrowBase.HalfX(), parentPawnRect.yMax,
                    SizeArrowBase.x, SpacingArrow);
                arrowBaseRightParent = arrowBaseRect.xMax;
                GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);

                DrawPortrait(parent, parentPawnRect);

                if (parentPawnRect.Contains(Event.current.mousePosition)) {
                    var deleteRect = new Rect(parentPawnRect.xMax - PaddingDeleteButton - SizeDeleteButton.x,
                        parentPawnRect.y + PaddingDeleteButton, SizeDeleteButton.x, SizeDeleteButton.y);
                    Style.SetGUIColorForButton(deleteRect);
                    GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
                    if (Widgets.ButtonInvisible(deleteRect)) {
                        parentsToRemove.Add(new PawnGroupPair(parent, group));
                    }
                }

                parentPawnRect.x += SizePawn.x + SpacingPawn;
            }

            // If there's no parent, we still want to draw the arrow that points from the first parent box to the first child.
            // Normally, we would have drawn it when we drew the parent, but since there's aren't any parents, we have to call
            // it out here and draw it separately.
            if (group.Parents.Count == 0) {
                GUI.color = arrowColor;
                var arrowBaseRect = new Rect(parentPawnRect.MiddleX() - SizeArrowBase.HalfX(), parentPawnRect.yMax,
                    SizeArrowBase.x, SpacingArrow);
                GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);
                arrowBaseRightParent = arrowBaseRect.x;
            }

            GUI.color = ColorParentEmpty;
            GUI.DrawTexture(parentPawnRect, BaseContent.WhiteTex);
            var addParentRect = new Rect(parentPawnRect.xMax - PaddingAddButton - SizeAddButton.x,
                parentPawnRect.y + PaddingAddButton, SizeAddButton.x, SizeAddButton.y);
            Style.SetGUIColorForButton(parentPawnRect);
            GUI.DrawTexture(addParentRect, Textures.TextureButtonAdd);
            if (Widgets.ButtonInvisible(parentPawnRect)) {
                ShowParentDialogForGroup(group, null, pawn => {
                    ParentAddedToGroup(group, pawn);
                });
            }

            var childPawnRect = new Rect(PaddingBox + SizeChildOffset,
                PaddingBox + SizePawn.y + SizeArrowBase.y + SizeArrowHead.y + (SpacingArrow * 2), SizePawn.x,
                SizePawn.y);
            var arrowBaseLeft = firstParentPawnRect.MiddleX() - SizeArrowBase.HalfX();
            var arrowBaseRightChild = arrowBaseLeft + SizeArrowBase.x;
            foreach (var child in group.Children) {
                GUI.color = ColorChild;
                GUI.DrawTexture(childPawnRect, BaseContent.WhiteTex);

                GUI.color = arrowColor;
                var arrowBaseRect = new Rect(childPawnRect.MiddleX() - SizeArrowBase.HalfX(),
                    childPawnRect.y - SizeArrowHead.y - SpacingArrow, SizeArrowBase.x, SpacingArrow);
                GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);

                var arrowHeadRect = new Rect(childPawnRect.MiddleX() - SizeArrowHead.HalfX(),
                    childPawnRect.y - SizeArrowHead.y, SizeArrowHead.x, SizeArrowHead.y);
                GUI.DrawTexture(arrowHeadRect, Textures.TextureArrowDown);

                DrawPortrait(child, childPawnRect);

                if (childPawnRect.Contains(Event.current.mousePosition)) {
                    var deleteRect = new Rect(childPawnRect.xMax - PaddingDeleteButton - SizeDeleteButton.x,
                        childPawnRect.y + PaddingDeleteButton, SizeDeleteButton.x, SizeDeleteButton.y);
                    Style.SetGUIColorForButton(deleteRect);
                    GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
                    if (Widgets.ButtonInvisible(deleteRect)) {
                        childrenToRemove.Add(new PawnGroupPair(child, group));
                    }
                }

                childPawnRect.x += SizePawn.x + SpacingPawn;
                arrowBaseRightChild = arrowBaseRect.xMax;
            }

            // If there's no children, we still want to draw the arrow that points to the first child.
            // Normally, we would have drawn it when we drew the children, but since there's aren't any children,
            // we have to call it out here and draw it separately.
            if (group.Children.Count == 0) {
                GUI.color = arrowColor;
                var arrowBaseRect = new Rect(childPawnRect.MiddleX() - SizeArrowBase.HalfX(),
                    childPawnRect.y - SizeArrowHead.y - SpacingArrow, SizeArrowBase.x, SpacingArrow);
                GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);
                var arrowHeadRect = new Rect(childPawnRect.MiddleX() - SizeArrowHead.HalfX(),
                    childPawnRect.y - SizeArrowHead.y, SizeArrowHead.x, SizeArrowHead.y);
                GUI.DrawTexture(arrowHeadRect, Textures.TextureArrowDown);
                arrowBaseRightChild = arrowBaseRect.xMax;
            }

            GUI.color = ColorChildEmpty;
            GUI.DrawTexture(childPawnRect, BaseContent.WhiteTex);
            var addChildRect = new Rect(childPawnRect.xMax - PaddingAddButton - SizeAddButton.x,
                childPawnRect.y + PaddingAddButton, SizeAddButton.x, SizeAddButton.y);
            Style.SetGUIColorForButton(childPawnRect);
            GUI.DrawTexture(addChildRect, Textures.TextureButtonAdd);
            if (Widgets.ButtonInvisible(childPawnRect)) {
                ShowChildDialogForGroup(group, null, pawn => {
                    ChildAddedToGroup(group, pawn);
                });
            }

            var arrowBaseRight = Mathf.Max(arrowBaseRightChild, arrowBaseRightParent);
            var arrowBaseLineRect = new Rect(arrowBaseLeft, PaddingBox + SizePawn.y + SpacingArrow,
                arrowBaseRight - arrowBaseLeft, SizeArrowBase.y);
            GUI.color = arrowColor;
            GUI.DrawTexture(arrowBaseLineRect, BaseContent.WhiteTex);
        }
        finally {
            GUI.EndGroup();
        }

        GUI.color = Color.white;
        cursor += width + SpacingGroup;

        return cursor;
    }

    protected void DrawPortrait(CustomPawn pawn, Rect rect) {
        var parentNameRect = new Rect(rect.x, rect.yMax - 34, rect.width, 26);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperCenter;
        GUI.color = Style.ColorText;
        Widgets.Label(parentNameRect, pawn.ShortName);
        GUI.color = Color.white;

        var parentProfessionRect = new Rect(rect.x, rect.yMax - 18, rect.width, 18);
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.LowerCenter;
        GUI.color = Style.ColorText;
        Widgets.Label(parentProfessionRect, GetProfessionLabel(pawn));
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;

        var hidden = pawn.Hidden;
        if (!hidden) {
            var parentPortraitRect = rect.InsetBy(6);
            parentPortraitRect.y -= 8;
            var parentPortraitTexture = pawn.GetPortrait(parentPortraitRect.size);
            GUI.DrawTexture(parentPortraitRect.OffsetBy(0, -4), parentPortraitTexture);
        }
        else {
            GUI.color = Style.ColorButton;
            var parentPortraitRect = new Rect(rect.MiddleX() - SizeGender.HalfX(), rect.y + SpacingGender, SizeGender.x,
                SizeGender.y);
            if (pawn.Gender == Gender.Female) {
                GUI.DrawTexture(parentPortraitRect, Textures.TextureGenderFemaleLarge);
            }
            else if (pawn.Gender == Gender.Male) {
                GUI.DrawTexture(parentPortraitRect, Textures.TextureGenderMaleLarge);
            }
            else {
                GUI.DrawTexture(parentPortraitRect, Textures.TextureGenderlessLarge);
            }
        }

        TooltipHandler.TipRegion(rect, GetTooltipText(pawn));
    }

    protected string GetTooltipText(CustomPawn pawn) {
        string description;
        var hidden = pawn.Hidden;
        if (!hidden) {
            string age = pawn.BiologicalAge != pawn.ChronologicalAge
                ? "EdB.PC.Pawn.AgeWithChronological".Translate(pawn.BiologicalAge, pawn.ChronologicalAge)
                : "EdB.PC.Pawn.AgeWithoutChronological".Translate(pawn.BiologicalAge);
            description = pawn.Gender != Gender.None
                ? "EdB.PC.Pawn.PawnDescriptionWithGender".Translate(pawn.ProfessionLabel, pawn.Gender.GetLabel(), age)
                : "EdB.PC.Pawn.PawnDescriptionNoGender".Translate(pawn.ProfessionLabel, age);
        }
        else {
            string profession = "EdB.PC.Pawn.HiddenPawnProfession".Translate();
            description = pawn.Gender != Gender.None
                ? "EdB.PC.Pawn.HiddenPawnDescriptionWithGender".Translate(profession, pawn.Gender.GetLabel())
                : "EdB.PC.Pawn.HiddenPawnDescriptionNoGender".Translate(profession);
        }

        return pawn.FullName + "\n" + description;
    }

    protected void ShowParentDialogForGroup(ParentChildGroup group, CustomPawn selected, Action<CustomPawn> action) {
        var selectedPawn = selected;
        var disabled = new HashSet<CustomPawn>();
        if (group != null) {
            disabled.AddRange(group.Parents);
            disabled.AddRange(group.Children);
        }

        rowGroups.Clear();
        rowGroups.Add(new WidgetTable<CustomPawn>.RowGroup(
            "<b>" + "EdB.PC.AddParentChild.Header.SelectColonist".Translate() + "</b>",
            PrepareCarefully.Instance.RelationshipManager.AvailableColonyPawns));
        var sortedHiddenPawns = PrepareCarefully.Instance.RelationshipManager.HiddenParentChildPawns.ToList();
        sortedHiddenPawns.Sort((a, b) => {
            if (a.Type != b.Type) {
                return a.Type == CustomPawnType.Hidden ? -1 : 1;
            }

            var aInt = a.Index == null ? 0 : a.Index.Value;
            var bInt = b.Index == null ? 0 : b.Index.Value;
            return aInt.CompareTo(bInt);
        });
        rowGroups.Add(new WidgetTable<CustomPawn>.RowGroup(
            "<b>" + "EdB.PC.AddParentChild.Header.SelectWorldPawn".Translate() + "</b>",
            PrepareCarefully.Instance.RelationshipManager.AvailableWorldPawns));
        var newPawnGroup = new WidgetTable<CustomPawn>.RowGroup(
            "<b>" + "EdB.PC.AddParentChild.Header.CreateTemporaryPawn".Translate() + "</b>",
            PrepareCarefully.Instance.RelationshipManager.TemporaryPawns);
        rowGroups.Add(newPawnGroup);
        var pawnDialog = new DialogSelectParentChildPawn {
            HeaderLabel = "EdB.PC.AddParentChild.Header.AddParent".Translate(),
            SelectAction = pawn => { selectedPawn = pawn; },
            RowGroups = rowGroups,
            DisabledPawns = disabled,
            ConfirmValidation = () => {
                if (selectedPawn == null) {
                    return "EdB.PC.AddParentChild.Error.ParentRequired";
                }

                return null;
            },
            CloseAction = () => {
                // If the user selected a new pawn, replace the pawn in the new pawn list with another one.
                var index = newPawnGroup.Rows.FirstIndexOf(p => {
                    return p == selectedPawn;
                });
                if (index > -1 && index < PrepareCarefully.Instance.RelationshipManager.TemporaryPawns.Count) {
                    selectedPawn = PrepareCarefully.Instance.RelationshipManager.ReplaceNewTemporaryCharacter(index);
                }

                action(selectedPawn);
            }
        };
        Find.WindowStack.Add(pawnDialog);
    }

    protected void ShowChildDialogForGroup(ParentChildGroup group, CustomPawn selected, Action<CustomPawn> action) {
        var selectedPawn = selected;
        var disabled = new HashSet<CustomPawn>();
        if (group != null) {
            disabled.AddRange(group.Parents);
            disabled.AddRange(group.Children);
        }

        rowGroups.Clear();
        rowGroups.Add(new WidgetTable<CustomPawn>.RowGroup(
            "<b>" + "EdB.PC.AddParentChild.Header.SelectColonist".Translate() + "</b>",
            PrepareCarefully.Instance.RelationshipManager.ColonyAndWorldPawnsForRelationships.Where(pawn => {
                return pawn.Type == CustomPawnType.Colonist;
            })));
        var sortedHiddenPawns = PrepareCarefully.Instance.RelationshipManager.HiddenParentChildPawns.ToList();
        sortedHiddenPawns.Sort((a, b) => {
            if (a.Type != b.Type) {
                return a.Type == CustomPawnType.Hidden ? -1 : 1;
            }

            var aInt = a.Index == null ? 0 : a.Index.Value;
            var bInt = b.Index == null ? 0 : b.Index.Value;
            return aInt.CompareTo(bInt);
        });
        rowGroups.Add(new WidgetTable<CustomPawn>.RowGroup(
            "<b>" + "EdB.PC.AddParentChild.Header.SelectWorldPawn".Translate() + "</b>",
            PrepareCarefully.Instance.RelationshipManager.AvailableWorldPawns.Concat(sortedHiddenPawns)));
        var newPawnGroup = new WidgetTable<CustomPawn>.RowGroup(
            "EdB.PC.AddParentChild.Header.CreateTemporaryPawn".Translate(),
            PrepareCarefully.Instance.RelationshipManager.TemporaryPawns);
        rowGroups.Add(newPawnGroup);
        var pawnDialog = new DialogSelectParentChildPawn {
            HeaderLabel = "EdB.PC.AddParentChild.Header.AddChild".Translate(),
            SelectAction = pawn => { selectedPawn = pawn; },
            RowGroups = rowGroups,
            DisabledPawns = disabled,
            ConfirmValidation = () => {
                if (selectedPawn == null) {
                    return "EdB.PC.AddParentChild.Error.ChildRequired";
                }

                return null;
            },
            CloseAction = () => {
                // If the user selected a new pawn, replace the pawn in the new pawn list with another one.
                var index = newPawnGroup.Rows.FirstIndexOf(p => {
                    return p == selectedPawn;
                });
                if (index > -1 && index < PrepareCarefully.Instance.RelationshipManager.TemporaryPawns.Count) {
                    selectedPawn = PrepareCarefully.Instance.RelationshipManager.ReplaceNewTemporaryCharacter(index);
                }

                action(selectedPawn);
            }
        };
        Find.WindowStack.Add(pawnDialog);
    }

    protected float DrawNextGroup(float cursor) {
        var parentBoxCount = 1;
        var childBoxCount = 1;
        var widthOfParents = (parentBoxCount * SizePawn.x) + (SpacingPawn * (parentBoxCount - 1)) + (PaddingBox * 2);
        var widthOfChildren = SizeChildOffset + (childBoxCount * SizePawn.x) + (SpacingPawn * (childBoxCount - 1)) +
                              (PaddingBox * 2);
        var width = Mathf.Max(widthOfChildren, widthOfParents);
        var height = (PaddingBox * 2) + (SpacingArrow * 2) + SizeArrowBase.y + SizeArrowHead.y + (SizePawn.y * 2);
        var rect = new Rect(cursor, 0, width, height);

        GUI.color = Style.ColorPanelBackgroundItem;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);

        GUI.BeginGroup(rect);
        try {
            var parentPawnRect = new Rect(PaddingBox, PaddingBox, SizePawn.x, SizePawn.y);

            GUI.color = ColorParentEmpty;
            GUI.DrawTexture(parentPawnRect, BaseContent.WhiteTex);
            var addParentRect = new Rect(parentPawnRect.xMax - PaddingAddButton - SizeAddButton.x,
                parentPawnRect.y + PaddingAddButton, SizeAddButton.x, SizeAddButton.y);
            Style.SetGUIColorForButton(parentPawnRect);
            GUI.DrawTexture(addParentRect, Textures.TextureButtonAdd);
            if (Widgets.ButtonInvisible(parentPawnRect)) {
                ShowParentDialogForGroup(null, null, pawn => {
                    var group = new ParentChildGroup();
                    group.Parents.Add(pawn);
                    GroupAdded(group);
                });
            }

            GUI.color = ColorParentEmpty;
            var arrowBaseRect = new Rect(parentPawnRect.MiddleX() - SizeArrowBase.HalfX(), parentPawnRect.yMax,
                SizeArrowBase.x, SpacingArrow);
            GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);

            GUI.color = ColorChildEmpty;
            var childPawnRect = new Rect(PaddingBox + SizeChildOffset,
                PaddingBox + SizePawn.y + SizeArrowBase.y + SizeArrowHead.y + (SpacingArrow * 2), SizePawn.x,
                SizePawn.y);
            var arrowBaseLeft = parentPawnRect.MiddleX() - SizeArrowBase.HalfX();
            GUI.DrawTexture(childPawnRect, BaseContent.WhiteTex);
            var addChildRect = new Rect(childPawnRect.xMax - PaddingAddButton - SizeAddButton.x,
                childPawnRect.y + PaddingAddButton, SizeAddButton.x, SizeAddButton.y);
            Style.SetGUIColorForButton(childPawnRect);
            GUI.DrawTexture(addChildRect, Textures.TextureButtonAdd);
            if (Widgets.ButtonInvisible(childPawnRect)) {
                ShowChildDialogForGroup(null, null, pawn => {
                    var group = new ParentChildGroup();
                    group.Children.Add(pawn);
                    GroupAdded(group);
                });
            }

            GUI.color = ColorParentEmpty;
            arrowBaseRect = new Rect(childPawnRect.MiddleX() - SizeArrowBase.HalfX(),
                childPawnRect.y - SizeArrowHead.y - SpacingArrow, SizeArrowBase.x, SpacingArrow);
            GUI.DrawTexture(arrowBaseRect, BaseContent.WhiteTex);
            var arrowBaseRightChild = arrowBaseRect.xMax;

            var arrowHeadRect = new Rect(childPawnRect.MiddleX() - SizeArrowHead.HalfX(),
                childPawnRect.y - SizeArrowHead.y, SizeArrowHead.x, SizeArrowHead.y);
            GUI.DrawTexture(arrowHeadRect, Textures.TextureArrowDown);

            var arrowBaseLineRect = new Rect(arrowBaseLeft, PaddingBox + SizePawn.y + SpacingArrow,
                arrowBaseRightChild - arrowBaseLeft, SizeArrowBase.y);
            GUI.DrawTexture(arrowBaseLineRect, BaseContent.WhiteTex);
        }
        finally {
            GUI.EndGroup();
        }

        GUI.color = Color.white;
        cursor += width + SpacingGroup;

        return cursor;
    }

    private string GetProfessionLabel(CustomPawn pawn) {
        var hidden = pawn.Hidden;
        if (!hidden) {
            return pawn.Type == CustomPawnType.Colonist
                ? "EdB.PC.AddParentChild.Colony".Translate()
                : "EdB.PC.AddParentChild.World".Translate();
        }

        return pawn.Type == CustomPawnType.Temporary
            ? "EdB.PC.AddParentChild.Temporary".Translate()
            : "EdB.PC.AddParentChild.World".Translate();
    }

    protected struct PawnGroupPair {
        public CustomPawn Pawn;
        public ParentChildGroup Group;

        public PawnGroupPair(CustomPawn Pawn, ParentChildGroup Group) {
            this.Pawn = Pawn;
            this.Group = Group;
        }
    }
}
