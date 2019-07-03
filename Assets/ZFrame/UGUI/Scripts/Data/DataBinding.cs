using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    public interface IDataContext
    {
        IDataContext this[string scope] { get; }
    }

    /// <summary>
    /// 可视元素 (Text/Image/...)
    /// </summary>
    public interface IViewElement
    {
        string dataScope { get; }
        void UpdateView(IDataContext context);
    }

    public class DataTransfer : Singleton<DataTransfer>
    {
        private List<IViewElement> m_Elements = new List<IViewElement>();

        public void OnDataChanged(string scope, IDataContext context)
        {
            for (var i = 0; i < m_Elements.Count; ++i) {
                if (m_Elements[i].dataScope.StartsWith(scope)) {
                    m_Elements[i].UpdateView(context[m_Elements[i].dataScope]);
                }
            }
        }
    }

    public abstract class ViewElement<T> : IViewElement where T : Object
    {
        public T target { get; private set; }
        public string dataScope { get; private set; }
        public abstract void UpdateView(IDataContext context);
    }

    public class TextDataView : ViewElement<Text>
    {
        public override void UpdateView(IDataContext context)
        {
            target.text = context.ToString();
        }
    }

    public class ImageDataView : ViewElement<Image>
    {
        public override void UpdateView(IDataContext context)
        {

        }
    }
}
