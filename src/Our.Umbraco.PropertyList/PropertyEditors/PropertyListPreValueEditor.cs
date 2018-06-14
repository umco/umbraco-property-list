using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

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
                Key = "dataType",
                Name = "Data Type",
                View = IOHelper.ResolveUrl("~/App_Plugins/PropertyList/views/propertylist.datatypepicker.html"),
                Description = "Select a data type."
            });

            // The rest of the fields can be added at the bottom.
            Fields.AddRange(new[]
            {
                new PreValueField
                {
                    Key = "minItems",
                    Name = "Min Items",
                    View = "number",
                    Description = "Set the minimum number of items allowed."
                },
                new PreValueField
                {
                    Key = "maxItems",
                    Name = "Max Items",
                    View = "number",
                    Description = "Set the maximum number of items allowed."
                },
                new PreValueField
                {
                    Key = "hideLabel",
                    Name = "Hide Label",
                    View = "boolean",
                    Description = "Set whether to hide the editor label and have the list take up the full width of the editor window."
                }
            });
        }
    }
}