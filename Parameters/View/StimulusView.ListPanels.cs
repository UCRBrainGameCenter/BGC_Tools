using UnityEngine;
using UnityEngine.UI;

namespace BGC.Parameters.View
{
    /// <summary>
    /// Shared layout for the list editors (StimulusView.ListFields.cs): a panel, lighter than the
    /// group around it, with a dark header bar across its top and a padded body below; and bars,
    /// rows whose children are laid out left to right at sizes given by <see cref="SizeListWidget"/>.
    /// </summary>
    public abstract partial class StimulusView
    {
        protected virtual Color ListPanelBG { get; } = new Color32(0xA8, 0xA8, 0xA8, 0xFF);
        protected virtual Color ListHeaderBG { get; } = new Color32(0x50, 0x50, 0x50, 0xFF);
        protected virtual Color ListHeaderText { get; } = Color.white;
        protected virtual Color ListHeaderErrorText { get; } = new Color32(0xFF, 0x9A, 0x9A, 0xFF);

        // Bar widgets are as tall as the editor's other rows (60): shorter, and the widgets' 35pt
        // captions no longer fit and are not drawn
        private const float ListWidgetHeight = 60f;
        private const int ListBarVerticalPadding = 5;
        private const float ListBarHeight = ListWidgetHeight + 2 * ListBarVerticalPadding;

        // A header bar's side padding clears the panel's rounded corners
        private static RectOffset ListHeaderPadding => new RectOffset(20, 20, ListBarVerticalPadding, ListBarVerticalPadding);
        private const float ListAddButtonWidth = 200f;

        /// <summary>A panel in parent; returns its header bar and its body (where its rows go)</summary>
        private void SpawnListPanel(Transform parent, out GameObject headerBar, out GameObject body)
        {
            GameObject panel = SpawnListFrame("ListPanel", parent, new RectOffset(0, 0, 0, 0));
            SetListBackground(panel, ListPanelBG);

            headerBar = SpawnListBar("ListHeader", panel.transform, ListHeaderPadding);
            SetListBackground(headerBar, ListHeaderBG);

            body = SpawnListFrame("ListBody", panel.transform, new RectOffset(20, 10, 5, 10));
        }

        /// <summary>A column that stacks its children, laid out like a property frame</summary>
        private static GameObject SpawnListFrame(string name, Transform parent, RectOffset padding)
        {
            GameObject frame = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(parent, false);

            VerticalLayoutGroup layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return frame;
        }

        /// <summary>A row that lays out its children left to right at the sizes SizeListWidget gives them</summary>
        private static GameObject SpawnListBar(string name, Transform parent, RectOffset padding)
        {
            GameObject bar = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            bar.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            LayoutElement layoutElement = bar.AddComponent<LayoutElement>();
            layoutElement.minHeight = ListBarHeight;
            layoutElement.preferredHeight = ListBarHeight;

            return bar;
        }

        /// <summary>Fix a bar child's width (or let it take the spare width) and height</summary>
        private static GameObject SizeListWidget(GameObject widget, float width, bool flexible = false)
        {
            if (!widget.TryGetComponent(out LayoutElement layoutElement))
            {
                layoutElement = widget.AddComponent<LayoutElement>();
            }

            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.flexibleWidth = flexible ? 1f : 0f;
            layoutElement.minHeight = ListWidgetHeight;
            layoutElement.preferredHeight = ListWidgetHeight;

            return widget;
        }

        /// <summary>A flexible empty bar child, which pushes the children after it to the right</summary>
        private static void SpawnListSpacer(GameObject bar) =>
            SizeListWidget(new GameObject("Spacer", typeof(RectTransform)), 0f, flexible: true)
                .transform.SetParent(bar.transform, false);

        /// <summary>The panel sprite (rounded, outlined), tinted; a plain rectangle if the scene sets none</summary>
        private void SetListBackground(GameObject target, Color color)
        {
            Image image = target.AddComponent<Image>();
            image.sprite = widgetFactory.PanelSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }
    }
}
