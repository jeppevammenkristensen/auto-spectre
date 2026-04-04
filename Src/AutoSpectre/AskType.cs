using System;

namespace AutoSpectre
{
    /// <summary>
    /// Specifies the type of prompt to present to the user.
    /// </summary>
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
