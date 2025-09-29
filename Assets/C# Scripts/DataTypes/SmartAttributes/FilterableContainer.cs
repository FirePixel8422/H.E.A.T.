using UnityEngine;



[System.Serializable]
public struct FilterableContainer<T>
{
    [Header("If true this data will not be used for current attachment")]
    [SerializeField] private bool ignoreData;
    [SerializeField] private T data;

    public void ApplyToStat(ref T data)
    {
        if (ignoreData) return;

        data = this.data;
    }


    public FilterableContainer(T data, bool ignoreData)
    {
        this.ignoreData = ignoreData;
        this.data = data;
    }
    public FilterableContainer(bool ignoreData)
    {
        this.ignoreData = ignoreData;
        this.data = default;
    }
}