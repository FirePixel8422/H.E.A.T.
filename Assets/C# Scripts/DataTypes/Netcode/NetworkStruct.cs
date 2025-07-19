using System;
using Unity.Netcode;
using UnityEngine;



/// <summary>
/// Netcode compatible wrapper for structs. Adds an OnValueChanged event that can be used to notify when the value changes.
/// </summary>
[System.Serializable]
public class NetworkStruct<T> where T : INetworkSerializable 
{
    [SerializeField] private T value;

    public Action<T> OnValueChanged;

    public T Value     
    {
        get => value;
        set
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }
    }

    public NetworkStruct(T initialValue = default(T))
    {
        value = initialValue;
    }

    public void SetDirty()
    {
        OnValueChanged?.Invoke(value);
    }
}