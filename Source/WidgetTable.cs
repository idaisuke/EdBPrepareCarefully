using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class WidgetTable<T> where T : class {
    protected static Vector2 SizeSortIndicator = new(8, 4);
    protected List<float> columnHeights = new();
    protected List<Column> columns = new();
    protected Action<T> doubleClickAction;
    protected Func<T, bool> enabledFunc = T => { return true; };
    protected T scrollTo;
    protected ScrollViewVertical scrollView = new();
    protected Action<T> selectedAction;
    protected int sortDirection = 1;
    protected Column sortedColumn;
    protected Rect tableRect;

    public WidgetTable() {
        SupportSelection = false;
    }

    public Rect Rect {
        get => tableRect;
        set => tableRect = value;
    }

    public bool ShowHeader {
        get;
        set;
    }

    public Color BackgroundColor {
        get;
        set;
    }

    public Color RowColor {
        get;
        set;
    }

    public Color AlternateRowColor {
        get;
        set;
    }

    public Color SelectedRowColor {
        get;
        set;
    }

    public List<T> Items {
        get;
        set;
    }

    public float RowHeight {
        get;
        set;
    }

    public float RowGroupHeaderHeight {
        get;
        set;
    }

    public bool SupportSelection {
        get;
        set;
    }

    public T Selected {
        get;
        set;
    }

    public Action<Column, int> SortAction {
        get;
        set;
    }

    public Action<RowGroup, int> DrawRowGroupHeaderAction {
        get;
        set;
    }

    public Func<RowGroup, int, float> MeasureRowGroupHeaderAction {
        get;
        set;
    }

    public ScrollViewVertical ScrollView => scrollView;

    public Action<T> DoubleClickAction {
        get => doubleClickAction;
        set => doubleClickAction = value;
    }

    public Action<T> SelectedAction {
        get => selectedAction;
        set => selectedAction = value;
    }

    public Func<T, bool> RowEnabledFunc {
        get => enabledFunc;
        set => enabledFunc = value;
    }

    public void ScrollTo(T row) {
        scrollTo = row;
    }

    public void SetSortState(string name, int direction) {
        sortDirection = direction;
        var column = columns.FirstOrDefault(arg => { return arg.Name == name; });
        sortedColumn = column;
    }

    public void Sort(int direction) {
        if (direction == -1 || direction == 1) {
            if (direction != sortDirection) {
                sortDirection = direction;
                if (sortedColumn != null) {
                    DoSortAction();
                }
            }
        }
    }

    public void Sort(Column column, int direction) {
        if (column != sortedColumn || direction != sortDirection) {
            sortedColumn = column;
            sortDirection = direction;
            DoSortAction();
        }
    }

    private void DoSortAction() {
        if (SortAction != null) {
            SortAction(sortedColumn, sortDirection);
        }
    }

    public void AddColumn(Column column) {
        columns.Add(column);
    }

    public void Draw(IEnumerable<T> rows) {
        var tableRect = this.tableRect;
        if (ShowHeader) {
            DrawHeader(new Rect(tableRect.x, tableRect.y, tableRect.width, 20));
            tableRect = tableRect.InsetBy(0, 20, 0, 0);
        }

        GUI.color = BackgroundColor;
        GUI.DrawTexture(tableRect, BaseContent.WhiteTex);
        GUI.color = Color.white;

        float cursor = 0;
        float? scrollToCursorTop = null;
        float? scrollToCursorBottom = null;
        GUI.BeginGroup(tableRect);
        scrollView.Begin(new Rect(0, 0, tableRect.width, tableRect.height));
        var index = 0;
        try {
            foreach (var row in rows) {
                if (scrollTo != null && row == scrollTo) {
                    scrollToCursorTop = cursor;
                }

                cursor = DrawRow(cursor, row, index);
                if (scrollTo != null && row == scrollTo) {
                    scrollToCursorBottom = cursor;
                }

                index++;
            }
        }
        finally {
            scrollView.End(cursor);
            GUI.EndGroup();
        }

        // Scroll to the specific row, if any.  Need to do this after all of the rows have been drawn.
        if (scrollTo != null) {
            ScrollTo(scrollToCursorTop.Value, scrollToCursorBottom.Value);
            scrollTo = null;
        }
    }

    public void Draw(IEnumerable<RowGroup> rowGroups) {
        var tableRect = this.tableRect;
        if (ShowHeader) {
            DrawHeader(new Rect(tableRect.x, tableRect.y, tableRect.width, 20));
            tableRect = tableRect.InsetBy(0, 20, 0, 0);
        }

        GUI.color = BackgroundColor;
        GUI.DrawTexture(tableRect, BaseContent.WhiteTex);
        GUI.color = Color.white;

        float cursor = 0;
        float? scrollToCursorTop = null;
        float? scrollToCursorBottom = null;
        GUI.BeginGroup(tableRect);
        scrollView.Begin(new Rect(0, 0, tableRect.width, tableRect.height));
        var index = 0;
        try {
            foreach (var group in rowGroups) {
                if (group.Rows.DefaultIfEmpty() == null) {
                    continue;
                }

                if (group.Label != null) {
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.LowerLeft;
                    var headerRect = new Rect(tableRect.x + 1, cursor - 2, tableRect.width - 4, RowGroupHeaderHeight);
                    if (scrollView.ScrollbarsVisible) {
                        headerRect.width -= 16;
                    }

                    var labelHeight = Text.CalcHeight(group.Label, headerRect.width) + 16;
                    labelHeight = Mathf.Max(labelHeight, RowGroupHeaderHeight);
                    headerRect.height = labelHeight;
                    Widgets.Label(headerRect, group.Label);
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;
                    cursor += headerRect.height;
                    index = 0;
                }

                foreach (var row in group.Rows) {
                    if (scrollTo != null && row == scrollTo) {
                        scrollToCursorTop = cursor;
                    }

                    cursor = DrawRow(cursor, row, index);
                    if (scrollTo != null && row == scrollTo) {
                        scrollToCursorBottom = cursor;
                    }

                    index++;
                }
            }
        }
        finally {
            scrollView.End(cursor);
            GUI.EndGroup();
        }

        // Scroll to the specific row, if any.  Need to do this after all of the rows have been drawn.
        if (scrollTo != null) {
            ScrollTo(scrollToCursorTop.Value, scrollToCursorBottom.Value);
            scrollTo = null;
        }
    }

    protected void ScrollTo(float top, float bottom) {
        var contentHeight = bottom - top;
        var pos = top - (Mathf.Ceil(scrollView.ViewHeight * 0.25f) - Mathf.Floor(contentHeight * 0.5f));
        if (pos < scrollView.Position.y) {
            pos = scrollView.Position.y;
        }

        scrollView.ScrollTo(pos);
    }

    protected void ResizeColumnHeights() {
        // If the number of column heights don't match the number of columns, add enough to match.
        if (columnHeights.Count < columns.Count) {
            var diff = columns.Count - columnHeights.Count;
            for (var i = 0; i < diff; i++) {
                columnHeights.Add(0);
            }
        }
    }

    protected float MeasureRow(T row, int index) {
        float rowHeight = 0;
        var columnIndex = 0;
        foreach (var column in columns) {
            var columnHeight = column.MeasureAction == null
                ? RowHeight
                : column.MeasureAction(row, column.Width, new Metadata(0, index, columnIndex));
            columnHeights[columnIndex] = columnHeight;
            rowHeight = Mathf.Max(rowHeight, columnHeight);
            columnIndex++;
        }

        return rowHeight;
    }

    protected float DrawRow(float cursor, T row, int index) {
        // Measure the columns and get the row height from the maximum column height.
        ResizeColumnHeights();
        var rowHeight = MeasureRow(row, index);

        // Set the row rectangle using the row height that we previously calculated.
        var rowRect = new Rect(0, cursor, tableRect.width, rowHeight);

        // Only draw the row if it's within the bounds of the content rect.
        if (cursor + rowRect.height >= scrollView.Position.y
            && cursor <= scrollView.Position.y + scrollView.ViewHeight) {
            GUI.color = index % 2 == 0 ? RowColor : AlternateRowColor;
            if (row == Selected && SelectedRowColor.a != 0) {
                GUI.color = SelectedRowColor;
            }

            if (GUI.color.a != 0) {
                GUI.DrawTexture(rowRect, BaseContent.WhiteTex);
            }

            GUI.color = Color.white;

            float columnCursor = 0;
            var columnIndex = 0;
            foreach (var column in columns) {
                var columnRect = new Rect(columnCursor, rowRect.y, column.Width, rowRect.height);
                if (column.AdjustForScrollbars && scrollView.ScrollbarsVisible) {
                    columnRect.width = columnRect.width - 16;
                }

                column.DrawAction?.Invoke(row, columnRect, new Metadata(0, index, columnIndex));
                columnCursor += columnRect.width;
                columnIndex++;
            }

            if (SupportSelection) {
                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition)) {
                    if (Event.current.button == 0) {
                        if (Event.current.clickCount == 1) {
                            Selected = row;
                            selectedAction?.Invoke(row);
                        }
                        else if (Event.current.clickCount == 2) {
                            doubleClickAction?.Invoke(row);
                        }
                    }
                }
            }
        }

        cursor += rowHeight;
        return cursor;
    }

    protected void DoScroll(IEnumerable<T> rows, T scrollTo) {
        ResizeColumnHeights();
        // Iterate the rows to try to find the one we're looking for and to determine the
        // row top and bottom positions.
        var index = -1;
        float rowTop = 0;
        float rowBottom = 0;
        var foundRow = false;
        foreach (var row in rows) {
            index++;
            rowBottom = rowTop + MeasureRow(row, index);
            if (Equals(row, scrollTo)) {
                foundRow = true;
                break;
            }

            rowTop = rowBottom;
        }

        if (index < 0 || !foundRow) {
            return;
        }

        var min = ScrollView.Position.y;
        var max = min + Rect.height;
        var pos = index * RowHeight;
        if (rowTop < min) {
            var amount = min - rowTop;
            ScrollView.Position = new Vector2(ScrollView.Position.x, ScrollView.Position.y - amount);
        }
        else if (rowBottom > max) {
            var amount = rowBottom - max;
            ScrollView.Position = new Vector2(ScrollView.Position.x, ScrollView.Position.y + amount);
        }
    }

    public void DrawHeader(Rect rect) {
        Column clickedColumn = null;
        GUI.color = Style.ColorTableHeader;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);
        GUI.color = Style.ColorTableHeaderBorder;
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), BaseContent.WhiteTex);

        var cursor = rect.x;
        GUI.color = Style.ColorText;
        Text.Font = GameFont.Tiny;
        foreach (var column in columns) {
            if (column.Label != null) {
                Text.Anchor = column.Alignment;
                var labelRect = new Rect(cursor, rect.y, column.Width, rect.height);
                if (column.AdjustForScrollbars && scrollView.ScrollbarsVisible) {
                    labelRect.width -= 16;
                }

                if (column.AllowSorting) {
                    var columnWidth = labelRect.width;
                    var textSize = Text.CalcSize(column.Label);
                    Rect textRect;
                    Rect sortRect;
                    if (column.Alignment == TextAnchor.LowerLeft) {
                        textRect = new Rect(labelRect.x, labelRect.y, textSize.x, textSize.y);
                        sortRect = new Rect(labelRect.x + textSize.x + 2, labelRect.yMax - 11, SizeSortIndicator.x,
                            SizeSortIndicator.y);
                    }
                    else {
                        textRect = new Rect(labelRect.xMax - textSize.x - SizeSortIndicator.x - 2,
                            labelRect.yMax - textSize.y, textSize.x, textSize.y);
                        sortRect = new Rect(labelRect.xMax - SizeSortIndicator.x, labelRect.yMax - 11,
                            SizeSortIndicator.x, SizeSortIndicator.y);
                        labelRect = labelRect.InsetBy(0, 0, SizeSortIndicator.x + 2, 0);
                    }

                    var highlightRect = textRect.Combined(sortRect);
                    Style.SetGUIColorForButton(highlightRect);
                    if (Widgets.ButtonInvisible(highlightRect, false)) {
                        clickedColumn = column;
                    }

                    if (sortedColumn == column) {
                        if (sortDirection == 1) {
                            GUI.DrawTexture(sortRect, Textures.TextureSortAscending);
                        }
                        else {
                            GUI.DrawTexture(sortRect, Textures.TextureSortDescending);
                        }
                    }

                    Widgets.Label(labelRect, column.Label);
                    GUI.color = Style.ColorText;
                    cursor += columnWidth;
                }
                else {
                    Widgets.Label(labelRect, column.Label);
                    cursor += labelRect.width;
                }
            }
            else {
                cursor += column.Width;
            }
        }

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        if (clickedColumn != null) {
            if (sortedColumn != clickedColumn) {
                Sort(clickedColumn, 1);
            }
            else {
                Sort(-sortDirection);
            }
        }
    }

    public class RowGroup {
        public string Label;
        public IEnumerable<T> Rows;

        public RowGroup() {
        }

        public RowGroup(string label, IEnumerable<T> rows) {
            Label = label;
            Rows = rows;
        }
    }

    public struct Metadata {
        public int groupIndex;
        public int rowIndex;
        public int columnIndex;

        public Metadata(int groupIndex, int rowIndex, int columnIndex) {
            this.groupIndex = groupIndex;
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
        }
    }

    public class Column {
        public bool AdjustForScrollbars = false;
        public TextAnchor Alignment = TextAnchor.LowerLeft;
        public bool AllowSorting = false;
        public Action<T, Rect, Metadata> DrawAction = (T, Rect, Metadata) => { };
        public string Label;
        public Func<T, float, Metadata, float> MeasureAction = null;
        public string Name;
        public float Width;
    }
}
