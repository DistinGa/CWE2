using System.Collections;
using System.Collections.Generic;
using Assets.SimpleLocalization;

/// <summary>
/// Класс для хранения локализованных строковых списков.
/// </summary>
public class LocalizedDictionary<T>
{
    Dictionary<T, string> Dictionary;  //Key - ключ в контексте приложения; Value - ключ в ассете SimpleLocalization.

    public LocalizedDictionary(Dictionary<T, string> dictionary)
    {
        Dictionary = dictionary;
    }

    /// <summary>
    /// Получить название на текущем языке.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetLocalizedString(T key)
    {
        return LocalizationManager.Localize(Dictionary[key]);
    }

    public string this[T index]
    {
        get { return GetLocalizedString(index); }
        set { Dictionary[index] = value; }
    }
}
