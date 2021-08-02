namespace BGC.IO
{
    public enum SaveDataResult
    {
        SavedNew = 0,
        Overwritten,
        OverwriteLocked,
        OverwriteQuery,
        None
    }
}
