﻿using System;

namespace AutoSpectre
{
    /// <summary>
    /// Apply this attribute to a property to tell that a prompt should take place
    /// <example>
    ///[Ask()]
    /// public string Name {get;set;}
    /// </example>
    /// </summary>
    public class AskAttribute : Attribute
    {
        public AskAttribute()
        {
            
        }

        [Obsolete()]
        public AskAttribute(string? title = null)
        {

        }
        
        /// <summary>
        /// The title displayed. If nothing is defined a text including property name is displayed
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The type of prompting
        /// </summary>
        public AskType AskType { get; set; }
        
        /// <summary>
        /// The source to get a list of items to present to the user.
        /// Only relevant if the AskType is <see cref="AutoSpectre.AskType.Selection"/>
        /// The source pointed to must be either a property or method (with no input parameters)
        /// that returns a list of the same type as defined on the property.
        /// NOTE: If you define a source as {NameOfProperty}Source by convention that will be used
        /// as source and you don't have to define a selection source
        /// </summary>
        public string? SelectionSource { get; set; }

        /// <summary>
        /// The method that can create a string representation of the given property. The method must
        /// take the type of the property as input parameter and return a string.
        /// NOTE: If you define a converter method as {NameOfProperty}Converter you don't have to define
        /// Converter
        /// </summary>
        public string? Converter { get; set; }
    }

    public enum AskType
    {
        /// <summary>
        /// Default. The user will input the values
        /// </summary>
        Normal,
        /// <summary>
        /// Presents a selection dialog
        /// </summary>
        Selection
    }
}
