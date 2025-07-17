using System;
using Unity.Collections;
using UnityEngine;



/// <summary>
/// Netcode compatible wrapper for values (ints, floats, etc). Adds an OnValueChanged event that can be used to notify when the value changes.
/// </summary>
[System.Serializable]
public class NetworkValue<T> where T : struct
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

    public void SetDirty()
    {
        OnValueChanged?.Invoke(value);
    }



    /// <summary>
    /// Filter for what types are supported by NetworkValue.
    /// </summary>
    public NetworkValue()
    {
        Type t = typeof(T);
        if (IsSupportedType(t) == false)
        {
            throw new InvalidOperationException($"{t.Name} is not supported by NetworkValue");
        }
    }
    private static bool IsSupportedType(Type t)
    {
        return t == typeof(byte) || t == typeof(sbyte) ||
               t == typeof(short) || t == typeof(ushort) ||
               t == typeof(int) || t == typeof(uint) ||
               t == typeof(long) || t == typeof(ulong) ||
               t == typeof(float) || t == typeof(double) ||
               t == typeof(decimal) || t == typeof(bool) ||
               t == typeof(FixedString32Bytes) ||
               t == typeof(FixedString64Bytes) ||
               t == typeof(FixedString128Bytes) ||
               t == typeof(FixedString512Bytes) ||
               t == typeof(FixedString4096Bytes);
    }
}