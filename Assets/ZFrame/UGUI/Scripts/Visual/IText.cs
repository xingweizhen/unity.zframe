using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
    public delegate void TextChanged(string newText);

    public interface IText
    {
        string text { get; set; }
    }

    public interface ILabel : IText
    {
        event TextChanged onTextChanged;
        RectTransform rectTransform { get; }
        Color color { get; set; }
        string rawText { get; }
        bool localized { get; set; }
        LinkInfo FindLink(Vector3 screenPos, Camera camera);
    }
}
