using System;

namespace AutoSpectre
{
    public class AskAttribute : Attribute
    {
        public AskAttribute()
        {
            
        }

        [Obsolete()]
        public AskAttribute(string? title = null)
        {

        }

        //public AskAttribute(string? title = null, AskType askType = AutoSpectre.AskType.Normal, string? selectionSource = null)
        //{
        //    Title = title;
        //    AskType = askType;
        //    SelectionSource = selectionSource;
        //}

        /// <summary>
        /// The title displayed. If nothing is defined a text including property name is displayed
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The type used to ask with
        /// </summary>
        public AskType AskType { get; set; }
        public string? SelectionSource { get; set; }

        public string? Converter { get; set; }
    }

    public enum AskType
    {
        /// <summary>
        /// Default. 
        /// </summary>
        Normal,
        /// <summary>
        /// Presents a selection dialog
        /// </summary>
        Selection
    }
}
