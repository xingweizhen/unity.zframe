using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace ZFrame
{
	public class LoopListView : TreeView
	{
		public delegate void DrawHeaderDelegate(Rect rect);
		
		public delegate void DrawRowDelegate(int index, Rect rect, bool selected);

		public delegate void AfterDrawRowDelegate();
		
		public delegate void SelectItemDelegate(LoopListView list, int index);

		public delegate int InsertItemDelegate(LoopListView list, int index);

		public delegate int DeleteItemDelegate(LoopListView list, int index);

		public DrawHeaderDelegate onDrawHeader;
		public DrawRowDelegate onDrawRow;
		public event SelectItemDelegate onSelectItem;
		public event AfterDrawRowDelegate onAfterDrawRow;
		public InsertItemDelegate onInsertItem;
		public DeleteItemDelegate onDeleteItem;
		
		private int totalRow;
		
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

		public LoopListView(TreeViewState state, float rowHeight) : base(state)
		{
			this.rowHeight = rowHeight;
		}

		// public LoopListView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) { }

		protected override TreeViewItem BuildRoot()
		{
			// 创建根节点, depth=-1时表示不可见
			var root = new TreeViewItem {id = -1, depth = -1, displayName = "root"};
			var rows = new List<TreeViewItem>();
			for (int i = 0; i < totalRow; i++) {
				rows.Add(new TreeViewItem {id = i, depth = 0, displayName = "#" + i});
			}
			SetupParentsAndChildrenFromDepths(root, rows);
			
			return root;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			if (onDrawRow != null) {
				onDrawRow.Invoke(args.row, args.rowRect, args.selected);
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

		public override void OnGUI(Rect rect)
		{
//			Rect srect = rect;
//			srect.height = 18f;
//			searchString = _searchField.OnGUI(rect, searchString);
//
//			rect.y += 18f;
			
			var headerRect = new Rect(rect.x, rect.y, rect.width, 20);
			EditorGUI.DrawRect(new Rect(rect.x, headerRect.height - 2, rect.width, 1), Color.black);
			
			rect.y += headerRect.height;
			rect.height -= headerRect.height;
			
			var buttonHeight = 20;
			rect.height -= buttonHeight + 4;
			base.OnGUI(rect);

			if (onDrawHeader != null) {
				if (showingVerticalScrollBar) headerRect.width -= 15;
				onDrawHeader(headerRect);
			}
			
			if (totalRow == 0) {
				GUI.Label(rect, "List is Empty");
			}
			GUILayout.BeginArea(new Rect(rect.x, rect.y + rect.height, rect.width, buttonHeight));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("添加")) {
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
			if (GUILayout.Button("移除")) {
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
