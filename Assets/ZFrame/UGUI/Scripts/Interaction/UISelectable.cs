using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace ZFrame.UGUI
{
    public abstract class UISelectable : UIBehaviour
    {
        [SerializeField]
        protected Selectable m_Selectable;

        private bool m_GroupsAllowInteraction;

        private bool m_Interactable = true;
        protected bool IsInteractable()
        {
            return m_Selectable ? m_Selectable.IsInteractable() : m_GroupsAllowInteraction && m_Interactable;
        }

        protected bool IsForwardable()
        {
            return m_Selectable && !m_Selectable.gameObject.Equals(gameObject)
                && m_Selectable.IsInteractable();
        }

        public bool interactable {
            get { return IsInteractable(); }
            set {
                if (IsInteractable() == value) return;

                m_Interactable = value;
                if (m_Selectable) {
                    m_Selectable.interactable = value;
                }

                var state = value ? SelectingState.Normal : SelectingState.Disabled;
                this.TryStateTransition(state, true);
            }
        }

        protected override void OnCanvasGroupChanged()
        {
            // Figure out if parent groups allow interaction
            // If no interaction is alowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            var cvGrps = ListPool<Component>.Get();
            while (t != null) {
                t.GetComponents(typeof(CanvasGroup), cvGrps);
                bool shouldBreak = false;
                for (var i = 0; i < cvGrps.Count; i++) {
                    // if the parent group does not allow interaction
                    // we need to break
                    var cvGrp = (CanvasGroup)cvGrps[i];
                    if (!cvGrp.interactable) {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (cvGrp.ignoreParentGroups)
                        shouldBreak = true;
                }
                if (shouldBreak)
                    break;

                t = t.parent;
            }
            ListPool<Component>.Release(cvGrps);

            if (groupAllowInteraction != m_GroupsAllowInteraction) {
                m_GroupsAllowInteraction = groupAllowInteraction;
                var state = interactable ? SelectingState.Normal : SelectingState.Disabled;
                this.TryStateTransition(state, true);
            }
        }

        protected override void Start()
        {
            if (!m_Selectable) m_Selectable = GetComponent<Selectable>();
        }
    }
}
