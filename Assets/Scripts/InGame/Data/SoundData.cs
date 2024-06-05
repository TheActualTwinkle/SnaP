[System.Serializable]
public struct SoundData : ISaveLoadData
{
    public bool MusicEnabled { get; private set; }
    public bool SfxEnabled { get; private set; }

    public SoundData(bool musicEnabled, bool sfxEnabled)
    {
        MusicEnabled = musicEnabled;
        SfxEnabled = sfxEnabled;
    }

    public void SetDefaultValues()
    {
        MusicEnabled = true;
        SfxEnabled = true;
    }
}