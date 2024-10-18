namespace Droomploeg.DreamOps.WebApp.Common.Extensions;

public static class DictionaryExtensions
{
    public static void AddNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? value)
        where TKey : notnull
    {
        if (value != null)
        {
            dictionary.Add(key, value);
        }
    }
}
