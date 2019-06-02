using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ZFrame.Editors
{
	public class GUIStyleViewer : EditorWindow
	{
		private LoopListView m_TreeView;

		[MenuItem("Tools/GUIStyleViewer...")]
		public static void Open()
		{
			GetWindow(typeof(GUIStyleViewer));
		}

		private List<GUIStyle> m_Styles;

		private void OnEnable()
		{
			m_TreeView = new LoopListView(new TreeViewState(), 60) {
				onMatchItem = (item, search) => {
					if (string.IsNullOrEmpty(search)) return true;
						
					var style = m_Styles[item.id];
					return style != null && style.name.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
				},
				onDrawRow = (rect, item, row, col, selected) => {
					var style = m_Styles[item.id];
					DrawStyleItem(rect, style);
				},
			};
		}

		private void OnGUI()
		{
			if (m_TreeView.GetRows() == null) {
				m_Styles = new List<GUIStyle>();
				foreach (var style in GUI.skin) m_Styles.Add((GUIStyle)style);
				
				m_TreeView.BuildRows(m_Styles.Count);
			}
			m_TreeView.OnGUI(new Rect(0, 0, position.width, position.height));
		}

		void DrawStyleItem(Rect rect, GUIStyle style)
		{
			const float btnWidth = 60;
			const float styleGap = 40;
			
			rect.y += 2;
			rect.height /= 2;
			rect.width = (rect.width - btnWidth - styleGap * 2) / 3f;
			GUI.Label(rect, style.name);

			rect.x += rect.width;
			GUI.Label(rect, style.name, style);
			
			rect.x += rect.width + styleGap;
			GUI.Label(rect, "", style);

			rect.x += rect.width + styleGap;
			rect.width = btnWidth;
			if (GUI.Button(rect, "复制名称")) {
				TextEditor textEditor = new TextEditor();
				textEditor.text = style.name;
				textEditor.OnFocus();
				textEditor.Copy();
			}
		}
	}
}