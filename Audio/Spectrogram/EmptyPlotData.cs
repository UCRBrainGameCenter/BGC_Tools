using UnityEngine;

namespace BGC.Audio.Visualization
{
    public class EmptyPlotData : PlotData
    {
        public override void PopulateWidget(GameObject parent)
        {
            CreateTextureWidget("Empty Plot Widget", parent, plot, new Vector2(0f, 0f), new Vector2(1f, 1f));
        }
    }

}
