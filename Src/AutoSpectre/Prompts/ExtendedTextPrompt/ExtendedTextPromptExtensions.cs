using System;
using System.Collections.Generic;
using AutoSpectre.Prompts.ExtendedTextPrompt;
using Spectre.Console;

namespace AutoSpectre.Prompts;

public static class ExtendedTextPromptExtensions
{
    /// <summary>
    /// Allow empty input.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> AllowEmpty<T>(this ExtendedTextPrompt<T> obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.AllowEmpty = true;
        return obj;
    }

    /// <summary>
    /// Sets the prompt style.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="style">The prompt style.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> PromptStyle<T>(this ExtendedTextPrompt<T> obj, Style style)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (style is null)
        {
            throw new ArgumentNullException(nameof(style));
        }

        obj.PromptStyle = style;
        return obj;
    }

    public static ExtendedTextPrompt<T> WithFromStringConverter<T>(this ExtendedTextPrompt<T> obj,
        TryParseFromStringDelegate<T> converter)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (converter == null) throw new ArgumentNullException(nameof(converter));

        obj.FromStringConverter = converter;
        return obj;
    }

    public static ExtendedTextPrompt<T> WithHelp<T>(this ExtendedTextPrompt<T> obj, string helpText)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (helpText == null) throw new ArgumentNullException(nameof(helpText));

        obj.HelpText = helpText;
        return obj;
    }

    /// <summary>
    /// Show or hide choices.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="show">Whether or not choices should be visible.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ShowChoices<T>(this ExtendedTextPrompt<T> obj, bool show)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.ShowChoices = show;
        return obj;
    }

    /// <summary>
    /// Shows choices.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ShowChoices<T>(this ExtendedTextPrompt<T> obj)
    {
        return ShowChoices(obj, true);
    }

    /// <summary>
    /// Hides choices.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> HideChoices<T>(this ExtendedTextPrompt<T> obj)
    {
        return ShowChoices(obj, false);
    }

    /// <summary>
    /// Show or hide the default value.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="show">Whether or not the default value should be visible.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ShowDefaultValue<T>(this ExtendedTextPrompt<T> obj, bool show)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.ShowDefaultValue = show;
        return obj;
    }

    /// <summary>
    /// Shows the default value.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ShowDefaultValue<T>(this ExtendedTextPrompt<T> obj)
    {
        return ShowDefaultValue(obj, true);
    }

    /// <summary>
    /// Hides the default value.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> HideDefaultValue<T>(this ExtendedTextPrompt<T> obj)
    {
        return ShowDefaultValue(obj, false);
    }

    /// <summary>
    /// Sets the validation error message for the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ValidationErrorMessage<T>(this ExtendedTextPrompt<T> obj, string message)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.ValidationErrorMessage = message;
        return obj;
    }

    /// <summary>
    /// Sets the "invalid choice" message for the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="message">The "invalid choice" message.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> InvalidChoiceMessage<T>(this ExtendedTextPrompt<T> obj, string message)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.InvalidChoiceMessage = message;
        return obj;
    }

    /// <summary>
    /// Sets the default value of the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="value">The default value.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> DefaultValue<T>(this ExtendedTextPrompt<T> obj, T value)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.DefaultValue = new DefaultPromptValue<T>(value);
        return obj;
    }

    /// <summary>
    /// Sets the validation criteria for the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="validator">The validation criteria.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> Validate<T>(this ExtendedTextPrompt<T> obj, Func<T, bool> validator,
        string? message = null)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.Validator = result =>
        {
            if (validator(result))
            {
                return ValidationResult.Success();
            }

            return ValidationResult.Error(message);
        };

        return obj;
    }

    /// <summary>
    /// Sets the validation criteria for the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="validator">The validation criteria.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> Validate<T>(this ExtendedTextPrompt<T> obj, Func<T, ValidationResult> validator)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.Validator = validator;

        return obj;
    }

    /// <summary>
    /// Adds a choice to the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="choice">The choice to add.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> AddChoice<T>(this ExtendedTextPrompt<T> obj, T choice)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.Choices.Add(choice);
        return obj;
    }

    /// <summary>
    /// Adds multiple choices to the prompt.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="choices">The choices to add.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> AddChoices<T>(this ExtendedTextPrompt<T> obj, IEnumerable<T> choices)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (choices is null)
        {
            throw new ArgumentNullException(nameof(choices));
        }

        foreach (var choice in choices)
        {
            obj.Choices.Add(choice);
        }

        return obj;
    }

    /// <summary>
    /// Replaces prompt user input with asterisks in the console.
    /// </summary>
    /// <typeparam name="T">The prompt type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> Secret<T>(this ExtendedTextPrompt<T> obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.IsSecret = true;
        return obj;
    }

    /// <summary>
    /// Replaces prompt user input with mask in the console.
    /// </summary>
    /// <typeparam name="T">The prompt type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="mask">The masking character to use for the secret.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> Secret<T>(this ExtendedTextPrompt<T> obj, char? mask)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.IsSecret = true;
        obj.Mask = mask;
        return obj;
    }

    /// <summary>
    /// Sets the function to create a display string for a given choice.
    /// </summary>
    /// <typeparam name="T">The prompt type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="displaySelector">The function to get a display string for a given choice.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> WithConverter<T>(this ExtendedTextPrompt<T> obj,
        Func<T, string>? displaySelector)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.Converter = displaySelector;
        return obj;
    }

    /// <summary>
    /// Sets the style in which the default value is displayed.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="style">The default value style.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> DefaultValueStyle<T>(this ExtendedTextPrompt<T> obj, Style style)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (style is null)
        {
            throw new ArgumentNullException(nameof(style));
        }

        obj.DefaultValueStyle = style;
        return obj;
    }

    /// <summary>
    /// Sets the style in which the list of choices is displayed.
    /// </summary>
    /// <typeparam name="T">The prompt result type.</typeparam>
    /// <param name="obj">The prompt.</param>
    /// <param name="style">The style to use for displaying the choices.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static ExtendedTextPrompt<T> ChoicesStyle<T>(this ExtendedTextPrompt<T> obj, Style style)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (style is null)
        {
            throw new ArgumentNullException(nameof(style));
        }

        obj.ChoicesStyle = style;
        return obj;
    }
}