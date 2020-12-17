namespace MessageSerializer
{
    public class ToStringFormatProperties
    {
        static ToStringFormatProperties()
        {
            Default = new ToStringFormatProperties();
        }

        public ToStringFormatProperties()
        {
            Fields = new ToStringDecorationProperties();
            ListItemHeaders = new ToStringDecorationProperties();
            ListItems = new ToStringDecorationProperties();

            // When there is a ListItemHeader (which there is by default)
            // you don't really want a ListItems separator because you 
            // will get something like:
            // Index 0:
            //     Value0
            // Index 1: ,
            //     Value1
            // That comma at the end of the Index 1 line is actually the 
            // separator for Value1 since it's not the 1st item
            ListItems.Separator = "";

            // Similar to the ListItems.Separator we don't really want a separator
            // for the ListItemHeader either as it actually goes at 
            // the end of the previous ListItem.
            ListItemHeaders.Separator = "";

            NumberListItems = true;
            ListItemName = "Index ";
            UseOneBasedListIndex = false;
        }

        public static ToStringFormatProperties Default { get; set; }

        /// <summary>
        /// Settings for how to decorate fields
        /// </summary>
        public ToStringDecorationProperties Fields { get; set; }

        /// <summary>
        /// Settings for how to decorate list item headers
        /// </summary>
        public ToStringDecorationProperties ListItemHeaders { get; set; }

        /// <summary>
        /// Settings for how to decorate individual list items
        /// </summary>
        public ToStringDecorationProperties ListItems { get; set; }

        /// <summary>
        /// If true each item in a list will be labeled as ListItemName Index (e.g. Item 1)
        /// See ListItemName and UseOneBasedListIndex
        /// Default: true
        /// </summary>
        public bool NumberListItems { get; set; }

        /// <summary>
        /// String to label each item in a list (e.g. "Index", "Item Number").
        /// Will be appended with the index of the item (see UseOneBasedListIndex)
        /// Default: "Index "
        /// </summary>
        public string ListItemName { get; set; }

        /// <summary>
        /// If true list item numbering will start at 1, otherwise they will start at 0
        /// Default: false
        /// </summary>
        public bool UseOneBasedListIndex { get; set; }
    }
}
