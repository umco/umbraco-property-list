using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    internal class PropertyListPreValueEditor : PreValueEditor
    {
        [PreValueField("dataType", "Data Type", "~/App_Plugins/PropertyList/views/propertylist.datatypepicker.html", Description = "Select a data type.")]
        public string DataType { get; set; }

        [PreValueField("minItems", "Min Items", "number", Description = "Set the minimum number of items allowed.")]
        public string MinItems { get; set; }

        [PreValueField("maxItems", "Max Items", "number", Description = "Set the maximum number of items allowed.")]
        public string MaxItems { get; set; }

        [PreValueField("hideLabel", "Hide Label", "boolean", Description = "Set whether to hide the editor label and have the list take up the full width of the editor window.")]
        public string HideLabel { get; set; }
    }
}