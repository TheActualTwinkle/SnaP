using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddressablesLoaderMetadata
{
    public IEnumerable<string> OperableSceneNames { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// If true, LoadedCount will be equal to 0 after scene change.
    /// </summary>
    public bool IsExclusive { get; set; } = false;
}

/// <summary>
/// Factory for creating Addressables loaders.
/// </summary>
public static class AddressablesLoaderFactory
{
    private class ImportInfo
    {
        [ImportMany]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<Lazy<IAddressablesLoader, AddressablesLoaderMetadata>> AddressablesLoaders { get; set; } =
            Enumerable.Empty<Lazy<IAddressablesLoader, AddressablesLoaderMetadata>>();
    }
    
    private static readonly ImportInfo Info = new();
    
    static AddressablesLoaderFactory()
    {
        Assembly[] assemblies = { typeof(IAddressablesLoader).Assembly };
        ContainerConfiguration configuration = new();
        try
        {
            configuration = configuration.WithAssemblies(assemblies);
        }
        catch (Exception)
        {
            Logger.Log("Failed to load AddressablesLoaderFactory", Logger.LogLevel.Error, Logger.LogSource.Addressables);
            throw;
        }
        
        using CompositionHost container = configuration.CreateContainer();
        container.SatisfyImports(Info);
    }

    /// <summary>
    /// Returns all loaders that are operable for the given scene.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns>IEnumerable of Loaders.</returns>
    public static IEnumerable<IAddressablesLoader> GetAllForScene(string sceneName)
    {
        return Info.AddressablesLoaders
            .Where(loader => loader.Metadata.OperableSceneNames.Contains(sceneName))
            .Select(loader => loader.Value);
    }
    
    /// <summary>
    /// Returns all loaders that are exclusive for the given scene.
    /// </summary>
    /// <param name="sceneName">Scene name.</param>
    /// <returns>IEnumerable of Loaders.</returns>
    public static IEnumerable<IAddressablesLoader> GetExclusiveForScene(string sceneName)
    {
        return Info.AddressablesLoaders
            .Where(loader => (loader.Metadata.OperableSceneNames.Contains(sceneName) && loader.Metadata.OperableSceneNames.Count() == 1) ||
                             loader.Metadata.IsExclusive is true)
            .Select(loader => loader.Value);
    }
    
    /// <summary>
    /// Returns loader by generic type.
    /// </summary>
    /// <typeparam name="T">IAddressablesLoader.</typeparam>
    /// <returns>Specific loader.</returns>
    public static T Get<T>() where T: IAddressablesLoader
    {
        var firstOrDefault = Info.AddressablesLoaders.Select(t => t.Value).OfType<T>().FirstOrDefault();
        return firstOrDefault;
    }
}