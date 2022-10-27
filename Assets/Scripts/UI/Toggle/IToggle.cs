using System;

public interface IToggle<T>
{
    event Action<T> ToggleOnEvent;

    T EventArgument { get; set; }
}
