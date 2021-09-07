using System;

namespace AutoSpectre
{
    public class AskAttribute : Attribute
    {
        public AskAttribute(string? title = null)
        {
            Title = title;
        }

        /// <summary>
        /// The title displayed. If nothing is defined a text including property name is displayed
        /// </summary>
        public string? Title { get; set; }

    }
}
