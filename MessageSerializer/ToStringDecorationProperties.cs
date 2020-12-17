using System;
using System.Text;

namespace MessageSerializer
{
    public class ToStringDecorationProperties
    {
        public ToStringDecorationProperties()
        {
            IndentString = "    ";
            SeparateLine = true;
            Indent = true;
            NameValueSeparator = ": ";
            Prefix = "";
            Suffix = "";
            Separator = ", ";
        }

        /// <summary>
        /// The string to be used for each indent level (e.g. "    " or "\t")
        /// Default: "    "
        /// </summary>
        public string IndentString { get; set; }

        /// <summary>
        /// Indicates whether each item should be on a separate line
        /// Default: true
        /// </summary>
        public bool SeparateLine { get; set; }

        /// <summary>
        /// Indicates whether each item after the first should be indented from the first item's indent level
        /// Default: true
        /// </summary>
        public bool Indent { get; set; }

        /// <summary>
        /// What string should go between the name and the value of each item
        /// Default: ": "
        /// </summary>
        public string NameValueSeparator { get; set; }

        /// <summary>
        /// What string should go before each item
        /// Default: ""
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// What string should go after each item
        /// Default: ""
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// What string should be used between items
        /// Default: ", "
        /// </summary>
        public string Separator { get; set; }

        public string GetSeparator(bool exclude)
        {
            return exclude ? "" : Separator;
        }

        public int GetNewIndentLevel(int currentIndentLevel, bool preventIncrement)
        {
            if (!preventIncrement && SeparateLine && Indent)
                ++currentIndentLevel;

            return currentIndentLevel;
        }

        public string GetIndentString(int indentLevel, bool exclude)
        {
            if (exclude || !SeparateLine)
                return "";

            return Environment.NewLine + new StringBuilder().Insert(0, IndentString, indentLevel);
        }
    }
}
