using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
	public class LoopListView : TreeView
	{
		public delegate void DrawHeaderDelegate(Rect rect);
		
		public delegate void DrawRowDelegate(Rect rect, TreeViewItem item, int row, int col, bool selected);

		public delegate void AfterDrawRowDelegate();
		
		public delegate void SelectItemDelegate(LoopListView list, int index);

		public delegate int InsertItemDelegate(LoopListView list, int index);

		public delegate int DeleteItemDelegate(LoopListView list, int index);

		public delegate bool ItemMatchDelegate(TreeViewItem item, string search);

		public DrawHeaderDelegate onDrawHeader;
		public DrawRowDelegate onDrawRow;
		public event SelectItemDelegate onSelectItem;
		public event AfterDrawRowDelegate onAfterDrawRow;
		public InsertItemDelegate onInsertItem;
		public DeleteItemDelegate onDeleteItem;
		public ItemMatchDelegate onMatchItem;
        public System.Action<TreeViewItem> onCreateItem;
		
		private int totalRow;
        public bool allowAdd, allowDelete;
		
		private List<int> m_Selected = new List<int>(1);

		public int index {
			set {
				if (value < 0) {
					m_Selected.Clear();
				} else {
					if (m_Selected.Count > 0) {
						m_Selected[0] = value;
					} else {
						m_Selected.Add(value);
					}
				}

				SetSelection(m_Selected);
			}
			get { return state.selectedIDs.Count > 0 ? state.selectedIDs[0] : -1; }
		}

		// 搜索控件
		//SearchField _searchField = new SearchField();

		public LoopListView(TreeViewState state, float rowHeight, bool showBorder = true, bool showAlternatingRow = true) : base(state)
		{
			this.rowHeight = rowHeight;
            this.showBorder = showBorder;
            this.showAlternatingRowBackgrounds = showAlternatingRow;
        }

		public LoopListView(TreeViewState state, MultiColumnHeader multiColumnHeader, float rowHeight,
            bool showBorder = true, bool showAlternatingRow = true) : base(state, multiColumnHeader) {
            this.rowHeight = rowHeight;
            this.showBorder = showBorder;
            this.showAlternatingRowBackgrounds = showAlternatingRow;
        }

		protected override TreeViewItem BuildRoot()
		{
			// 创建根节点, depth=-1时表示不可见
			var root = new TreeViewItem {id = -1, depth = -1, displayName = "root"};
			var rows = new List<TreeViewItem>();
			for (int i = 0; i < totalRow; i++) {
                var item = new TreeViewItem { id = i, depth = 0, displayName = "#" + i };
                if (onCreateItem != null) onCreateItem.Invoke(item);
                rows.Add(item);
			}
			SetupParentsAndChildrenFromDepths(root, rows);
			
			return root;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			if (onDrawRow != null) {
                if (multiColumnHeader == null) {
                    onDrawRow.Invoke(args.rowRect, args.item, args.row, 0, args.selected);
                } else {
                    for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
                        onDrawRow(args.GetCellRect(i), args.item, args.row, args.GetColumn(i), args.selected);
                    }
                }
			} else {
				base.RowGUI(args);
			}
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		protected override void SelectionChanged(IList<int> selectedIds)
		{
			if (onSelectItem != null) {
				onSelectItem.Invoke(this, index);
			}
		}

//		protected override void DoubleClickedItem(int id)
//		{
//			if (onSelectItem != null) onSelectItem.Invoke(id);
//		}
//
//		protected override void ContextClickedItem(int id)
//		{
//			if (onSelectItem != null) onSelectItem.Invoke(id);
//		}

		protected override void SearchChanged(string newSearch)
		{
			base.SearchChanged(newSearch);
		}

		protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
		{
			return onMatchItem != null ? onMatchItem.Invoke(item, search) : base.DoesItemMatchSearch(item, search);
		}

		public override void OnGUI(Rect rect)
		{
//			Rect srect = rect;
//			srect.height = 18f;
//			searchString = _searchField.OnGUI(rect, searchString);
//
//			rect.y += 18f;
			
			var headerRect = new Rect(rect.x, rect.y, rect.width, 20);

            if (onDrawHeader != null) {
                EditorGUI.DrawRect(new Rect(rect.x, headerRect.height - 2, rect.width, 1), Color.black);
                rect.y += headerRect.height;
                rect.height -= headerRect.height;
            }

			if (onMatchItem != null) {
				var searchRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
				GUILayout.BeginArea(searchRect);
				searchString = EditorAPI.SearchField(searchString);
				GUILayout.EndArea();
				rect.y += searchRect.height;
				rect.height -= searchRect.height;
			}
			
            var buttonHeight = allowAdd || allowDelete ? 20 : 0;
			rect.height -= buttonHeight + 4;
			
			base.OnGUI(rect);

			if (onDrawHeader != null) {
				if (showingVerticalScrollBar) headerRect.width -= 15;
				onDrawHeader(headerRect);
			}
			
			if (totalRow == 0) {
				GUI.Label(rect, "List is Empty");
			}

            if (buttonHeight > 0) {
                GUILayout.BeginArea(new Rect(rect.x, rect.y + rect.height, rect.width, buttonHeight));
                GUILayout.BeginHorizontal();
                if (allowAdd && GUILayout.Button("添加")) {
                    if (onInsertItem != null) {
                        var newIdx = onInsertItem.Invoke(this, index);
                        BuildRows(totalRow + 1);
                        index = newIdx;
                        if (onSelectItem != null) {
                            onSelectItem.Invoke(this, newIdx);
                        }
                    }
                }

                EditorGUI.BeginDisabledGroup(index < 0);
                if (allowDelete && GUILayout.Button("移除")) {
                    if (onDeleteItem != null) {
                        var newIdx = onDeleteItem.Invoke(this, index);
                        BuildRows(totalRow - 1);
                        index = newIdx;
                        if (onSelectItem != null) {
                            onSelectItem.Invoke(this, newIdx);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

		protected override void AfterRowsGUI()
		{
			if (onAfterDrawRow != null) onAfterDrawRow.Invoke();
		}

		public void BuildRows(int totalRow)
		{
			this.totalRow = totalRow;
			
			Reload();
		}

		public void Release()
		{
			onDrawRow = null;
			onSelectItem = null;
		}
	}
}
