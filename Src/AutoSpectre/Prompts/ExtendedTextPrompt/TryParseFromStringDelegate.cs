using System.Globalization;

namespace AutoSpectre.Prompts.ExtendedTextPrompt;

/// <summary>
/// A delegate for taking a <see cref="string"/> value and a CultureInfo (that can be null) and try
/// to convert it the given <see cref="T"/> type. 
/// </summary>
/// <typeparam name="T"></typeparam>
public delegate bool TryParseFromStringDelegate<T>(string value, CultureInfo? culture, out T? result);