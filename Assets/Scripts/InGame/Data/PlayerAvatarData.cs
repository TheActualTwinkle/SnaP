using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct PlayerAvatarData : ISaveLoadData, INetworkSerializable
{
    public const int MaxBytesPerRpc = 65000; // Max RPC parameter size. todo: Try it with many images.

    public byte[] CodedValue => _codedValue;
    private byte[] _codedValue;
    
    public PlayerAvatarData(byte[] codedValue)
    {
        _codedValue = codedValue.ToArray();
    }

    public void SetDefaultValues()
    {
        _codedValue = TextureConverter.GetRawTexture(Resources.Load<Sprite>("Sprites/ava").texture);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        _codedValue ??= Array.Empty<byte>();

        var length = 0;
        if (serializer.IsReader == false)
        {
            length = _codedValue.Length;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            _codedValue = new byte[length];
        }

        for (var n = 0; n < length; ++n)
        {
            serializer.SerializeValue(ref _codedValue[n]);
        }
    }
}