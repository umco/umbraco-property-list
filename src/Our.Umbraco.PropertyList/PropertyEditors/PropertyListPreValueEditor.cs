using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using static Our.Umbraco.PropertyList.PropertyListConstants;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    internal class PropertyListPreValueEditor : PreValueEditor
    {
        public PropertyListPreValueEditor()
            : base()
        {
            // In terms of inheritance, we'd like the "dataType" field to always be at the top.
            Fields.Insert(0, new PreValueField
            {
                Key = PreValueKeys.DataType,
                Name = "Data Type",
                View = IOHelper.ResolveUrl("~/App_Plugins/PropertyList/views/propertylist.datatypepicker.html"),
                Description = "Select a data type."
            });

            // The rest of the fields can be added at the bottom.
            Fields.AddRange(new[]
            {
                new PreValueField
                {
                    Key = PreValueKeys.MinItems,
                    Name = "Min Items",
                    View = "number",
                    Description = "Set the minimum number of items allowed."
                },
                new PreValueField
                {
                    Key = PreValueKeys.MaxItems,
                    Name = "Max Items",
                    View = "number",
                    Description = "Set the maximum number of items allowed."
                },
                new PreValueField
                {
                    Key = PreValueKeys.HideLabel,
                    Name = "Hide Label",
                    View = "boolean",
                    Description = "Set whether to hide the editor label and have the list take up the full width of the editor window."
                }
            });
        }
    }
}