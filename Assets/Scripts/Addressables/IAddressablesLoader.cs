using System.Threading.Tasks;

public interface IAddressablesLoader
{
    public uint LoadedCount { get; }
    
    public uint AssetsCount { get; }
    
    Task LoadContent();
    
    void UnloadContent();
}
