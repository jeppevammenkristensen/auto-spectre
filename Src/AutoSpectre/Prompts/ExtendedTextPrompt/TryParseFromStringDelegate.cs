using System.Globalization;

namespace AutoSpectre.Prompts.ExtendedTextPrompt;


/// <summary>
/// Represents a delegate that tries to parse a string value into a specified type, considering culture information.
/// </summary>
/// <param name="value">The string value to parse.</param>
/// <param name="culture">The culture to use for parsing.</param>
/// <param name="result">The parsed result, if successful.</param>
/// <typeparam name="T"></typeparam>
public delegate bool TryParseFromStringDelegate<T>(string value, CultureInfo? culture, out T? result);