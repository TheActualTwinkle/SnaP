[System.Serializable]
public struct SoundDto : ISaveLoadData
{
    public bool MusicEnabled { get; private set; }
    public bool SfxEnabled { get; private set; }

    public SoundDto(bool musicEnabled, bool sfxEnabled)
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