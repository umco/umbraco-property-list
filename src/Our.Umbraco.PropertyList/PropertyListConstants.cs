namespace Our.Umbraco.PropertyList
{
    /// <remarks>
    /// "Oh, magic strings when a developer first knows a typo in compiled code."
    /// </remarks>
    internal static class PropertyListConstants
    {
        public static class PreValueKeys
        {
            public const string DataType = "dataType";

            public const string MinItems = "minItems";

            public const string MaxItems = "maxItems";

            public const string HideLabel = "hideLabel";
        }

        public static class PropertyEditorKeys
        {
            public const string Alias = "Our.Umbraco.PropertyList";

            public const string Name = "Property List";

            public const string Group = "Lists";

            public const string Icon = "icon-bulleted-list";

            public const string ViewPath = "~/App_Plugins/PropertyList/views/propertylist.html";

            public const string JavaScriptPath = "~/App_Plugins/PropertyList/js/propertylist.js";
        }

        internal static class DataTypeGuids
        {
            public const string DefaultTextstring = "0cc0eba1-9960-42c9-bf9b-60e150b429ae";
        }

        internal static class ValueConverterKeys
        {
            public const string CacheKeyFormat = "Our.Umbraco.PropertyList.PropertyListValueConverter.GetInnerPublishedPropertyType_{0}_{1}";
        }

        internal static class PropertyValidatorKeys
        {
            public const string CacheKeyFormat = "Our.Umbraco.PropertyList.PropertyListValidator.GetPropertyValidators_{0}";
        }
    }
}