using System;
using System.Threading.Tasks;

public interface ILoadingOperation
{
    string Description { get; }
    
    Task Load(Action<float> onProgress);
} 