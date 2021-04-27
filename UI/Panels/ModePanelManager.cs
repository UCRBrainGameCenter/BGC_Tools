using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.UI.Panels
{
    public enum ShowPanelMode
    {
        Hierarchy = 0,
        Push,
        Pop,
        PushClone,
        PopClone,
        Immediate,
        MAX
    }

    public class ModePanelManager : MonoBehaviour
    {
        [SerializeField]
        private ModePanel initialPanel = null;

        [SerializeField]
        private ModePanel[] orderedListOfPanels = null;
        [SerializeField]
        private Axis panelAxis = Axis.XAxis;
        [SerializeField]
        private float flipTime = 0.3f;
        [SerializeField]
        private CopyPanel copyPanel = null;

        private Dictionary<ModePanel, int> animationControlOrderMap;

        private ModePanel lastActivePanel = null;

        private void Awake()
        {
            lastActivePanel = initialPanel;
        }

        void Start()
        {
            Debug.Assert(initialPanel != null, "InitialPanel must be set");

            animationControlOrderMap = new Dictionary<ModePanel, int>(orderedListOfPanels.Length);

            int i = 0;
            foreach (ModePanel modePanel in orderedListOfPanels)
            {
                animationControlOrderMap.Add(modePanel, i++);

                bool visible = modePanel == initialPanel;

                modePanel.ImmediateStateSet(visible);
                modePanel.gameObject.SetActive(visible);
            }

            if (copyPanel != null)
            {
                copyPanel.gameObject.SetActive(true);
            }

            initialPanel.FocusAcquired();
        }

        public void ImmediatePanelSet(ModePanel newPanel)
        {
            if (lastActivePanel == newPanel)
            {
                //Cleanup panel
                newPanel.FocusLost();
                //Reprepare panel
                newPanel.FocusAcquired();
                return;
            }
            
            if (!animationControlOrderMap.ContainsKey(newPanel))
            {
                Debug.LogError("Error: Panel Not included in initialization array.");
                return;
            }

            newPanel.gameObject.SetActive(true);

            lastActivePanel.FocusLost();
            lastActivePanel.ImmediateStateSet(false);

            newPanel.FocusAcquired();
            newPanel.ImmediateStateSet(true);

            if (lastActivePanel != copyPanel)
            {
                lastActivePanel.gameObject.SetActive(false);
            }

            lastActivePanel = newPanel;
        }

        public void SetPanelActive(ModePanel newPanel, ShowPanelMode mode = ShowPanelMode.Hierarchy)
        {
            bool newPanelInferior = false;

            switch (mode)
            {
                case ShowPanelMode.Hierarchy:
                    newPanelInferior = animationControlOrderMap[lastActivePanel] < animationControlOrderMap[newPanel];
                    break;

                case ShowPanelMode.Push:
                case ShowPanelMode.PushClone:
                    newPanelInferior = true;
                    break;

                case ShowPanelMode.Pop:
                case ShowPanelMode.PopClone:
                    newPanelInferior = false;
                    break;

                case ShowPanelMode.Immediate:
                    //No setup necessary
                    break;

                default:
                    newPanelInferior = true;
                    Debug.LogError($"Unrecognized ShowPanelMode: {mode}");
                    break;
            }

            switch (mode)
            {
                case ShowPanelMode.Hierarchy:
                case ShowPanelMode.Push:
                case ShowPanelMode.Pop:
                    SetPanelActive(newPanel, newPanelInferior);
                    break;

                case ShowPanelMode.PushClone:
                case ShowPanelMode.PopClone:
                    SpecialSetPanelActive(newPanel, newPanelInferior);
                    break;

                case ShowPanelMode.Immediate:
                    ImmediatePanelSet(newPanel);
                    break;

                default:
                    Debug.LogError($"Unrecognized ShowPanelMode: {mode}");
                    break;
            }
        }

        protected void SetPanelActive(ModePanel newPanel, bool newPanelInferior)
        {
            if (lastActivePanel == newPanel)
            {
                //Cleanup panel
                newPanel.FocusLost();
                //Reprepare panel
                newPanel.FocusAcquired();
                return;
            }
            
            if (!animationControlOrderMap.ContainsKey(newPanel))
            {
                Debug.LogError("Error: Panel Not included in initialization array.");
                return;
            }

            newPanel.gameObject.SetActive(true);
            lastActivePanel.FocusLost();

            newPanel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Show,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Inferior : Orientation.Superior));

            lastActivePanel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Hide,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Superior : Orientation.Inferior),
                finishedCallback: DisableModePanel);

            newPanel.FocusAcquired();

            lastActivePanel = newPanel;
        }

        protected void SpecialSetPanelActive(ModePanel newPanel, bool newPanelInferior)
        {
            copyPanel.TakeSnapshot();

            lastActivePanel.FocusLost();
            lastActivePanel.ImmediateStateSet(false);

            if (lastActivePanel != newPanel)
            {
                if (lastActivePanel != copyPanel)
                {
                    lastActivePanel.gameObject.SetActive(false);
                }
                newPanel.gameObject.SetActive(true);
            }

            newPanel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Show,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Inferior : Orientation.Superior));

            copyPanel.FocusAcquired();
            copyPanel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Hide,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Superior : Orientation.Inferior),
                finishedCallback: DisableModePanel);

            newPanel.FocusAcquired();

            lastActivePanel = newPanel;
        }

        public void SimulatePanelSwipe(
            ModePanel panel,
            Action betweenSwipeAction = null,
            Action<ModePanel> afterSwipeAction = null,
            bool newPanelInferior = true)
        {
            if (panel != lastActivePanel)
            {
                Debug.LogError("Cannot simulate panel swipe on the non-active panel.");
                return;
            }

            copyPanel.TakeSnapshot();
            betweenSwipeAction?.Invoke();

            panel.ImmediateStateSet(false);

            panel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Show,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Inferior : Orientation.Superior),
                finishedCallback: afterSwipeAction);

            copyPanel.FocusAcquired();
            copyPanel.LerpHandler.Activate(
                duration: flipTime,
                lerpAction: new ModePanelTranslator(
                    direction: Direction.Hide,
                    axis: panelAxis,
                    orientation: newPanelInferior ? Orientation.Superior : Orientation.Inferior),
                finishedCallback: DisableModePanel);
        }

        private void DisableModePanel(ModePanel panel)
        {
            if (panel != copyPanel)
            {
                panel.gameObject.SetActive(false);
            }
            else if (copyPanel != null)
            {
                copyPanel.FocusLost();
            }
        }
    }
}
