namespace BGC.Audio.Visualization
{
    public struct PlotPoint
    {
        public float parameterValue;
        public bool responseCorrect;

        public PlotPoint(float parameterValue, bool responseCorrect)
        {
            this.parameterValue = parameterValue;
            this.responseCorrect = responseCorrect;
        }
    }

}
