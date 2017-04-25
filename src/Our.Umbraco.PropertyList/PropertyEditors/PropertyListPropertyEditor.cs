using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClientDependency.Core;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    [PropertyEditor(
        PropertyListPropertyEditor.PropertyEditorAlias,
        "Property List",
        "JSON",
        "/App_Plugins/PropertyList/Views/propertylist.html",
        Group = "Lists",
        Icon = "",
        IsParameterEditor = false)]
    [PropertyEditorAsset(ClientDependencyType.Css, "~/App_Plugins/PropertyList/css/propertylist.css")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.controllers.js")]
    public class PropertyListPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.PropertyList";

        private IDictionary<string, object> _defaultPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreValues; }
            set { _defaultPreValues = value; }
        }

        public PropertyListPropertyEditor()
        {
            _defaultPreValues = new Dictionary<string, object>
            {
                { "dataType", "-88" },
                { "minItems", 0 },
                { "maxItems", 0 }
            };
        }

        #region Pre Value Editor

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new PropertyListPreValueEditor();
        }

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

        #endregion

        #region Value Editor
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PropertyListPropertyValueEditor(base.CreateValueEditor());
        }

        internal class PropertyListPropertyValueEditor : PropertyValueEditorWrapper
        {
            public PropertyListPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
                Validators.Add(new PropertyListValidator());
            }

            public override void ConfigureForDisplay(PreValueCollection preValues)
            {
                base.ConfigureForDisplay(preValues);

                if (preValues.PreValuesAsDictionary.ContainsKey("hideLabel"))
                {
                    var boolAttempt = preValues.PreValuesAsDictionary["hideLabel"].Value.TryConvertTo<bool>();
                    if (boolAttempt.Success)
                    {
                        HideLabel = boolAttempt.Result;
                    }
                }
            }

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);
            }

            public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                return base.ConvertDbToString(property, propertyType, dataTypeService);
            }

            public override XNode ConvertDbToXml(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                return base.ConvertDbToXml(property, propertyType, dataTypeService);
            }

            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                return base.ConvertEditorToDb(editorValue, currentValue);
            }
        }

        internal class PropertyListValidator : IPropertyValidator
        {
            public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
            {
                return Enumerable.Empty<ValidationResult>();
            }
        }

        #endregion
    }
}