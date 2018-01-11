using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClientDependency.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.PropertyList.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
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
        Icon = "icon-bulleted-list",
        IsParameterEditor = false)]
    [PropertyEditorAsset(ClientDependencyType.Css, "~/App_Plugins/PropertyList/css/propertylist.css")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.controllers.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.resources.js")]
    public class PropertyListPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.PropertyList";

        private const string DefaultTextstringPropertyEditorGuid = "0cc0eba1-9960-42c9-bf9b-60e150b429ae"; // Guid for default Textstring

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
                { "dataType", DefaultTextstringPropertyEditorGuid },
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
                var propertyValue = property?.Value?.ToString();

                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue))
                    return string.Empty;

                // We'd like this to be hot-swappable with "Repeatable Textstrings" property-editor.
                // We make the assumption that if the value isn't JSON, then it could be newline-delimited list
                // let's split/join the values and get it into a proxy JSON string.
                if (propertyValue.DetectIsJson() == false)
                {
                    var items = string.Join("', '", propertyValue.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                    propertyValue = $"{{ dtd: '{DefaultTextstringPropertyEditorGuid}', values: [ '{items}' ] }}";
                }

                var model = JsonConvert.DeserializeObject<PropertyListValue>(propertyValue);
                if (model == null || model.DataTypeGuid.Equals(Guid.Empty) || model.Values == null)
                    return string.Empty;

                // Get the associated datatype definition
                var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);

                // Something weird is happening in core whereby ConvertDbToString is getting
                // called loads of times on publish, forcing the property value to get converted
                // again, which in tern screws up the values. To get round it, we create a
                // dummy property copying the original properties value, this way not overwriting
                // the original property value allowing it to be re-converted again later.
                //
                // NB: Credit to Vorto for noticing this issue.
                // https://github.com/umco/umbraco-vorto/blob/master/src/Our.Umbraco.Vorto/Web/PropertyEditors/VortoPropertyEditor.cs
                var prop2 = new Property(propertyType, property.Value);

                // Lookup the property editor
                var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
                var propType = new PropertyType(dtd);

                for (var i = 0; i < model.Values.Count; i++)
                {
                    var obj = model.Values[i];

                    var prop = new Property(propType, obj?.ToString());
                    var newValue = propEditor.ValueEditor.ConvertDbToEditor(prop, propType, dataTypeService);
                    model.Values[i] = (newValue == null) ? null : JToken.FromObject(newValue);
                }

                prop2.Value = JsonConvert.SerializeObject(model);

                return base.ConvertDbToEditor(prop2, propertyType, dataTypeService);
            }

            public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null || property.Value.ToString().IsNullOrWhiteSpace())
                    return string.Empty;

                // Something weird is happening in core whereby ConvertDbToString is getting
                // called loads of times on publish, forcing the property value to get converted
                // again, which in tern screws up the values. To get round it, we create a
                // dummy property copying the original properties value, this way not overwriting
                // the original property value allowing it to be re-converted again later.
                //
                // NB: Credit to Vorto for noticing this issue.
                // https://github.com/umco/umbraco-vorto/blob/master/src/Our.Umbraco.Vorto/Web/PropertyEditors/VortoPropertyEditor.cs
                var prop2 = new Property(propertyType, property.Value);

                try
                {
                    var model = JsonConvert.DeserializeObject<PropertyListValue>(property.Value.ToString());
                    if (model != null && model.DataTypeGuid.Equals(Guid.Empty) == false && model.Values != null)
                    {
                        var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
                        var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
                        var propType = new PropertyType(dtd);

                        for (var i = 0; i < model.Values.Count; i++)
                        {
                            var obj = model.Values[i];

                            var prop = new Property(propType, obj == null ? null : obj.ToString());
                            var newValue = propEditor.ValueEditor.ConvertDbToString(prop, propType, dataTypeService);
                            model.Values[i] = newValue;
                        }

                        prop2.Value = JsonConvert.SerializeObject(model);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<PropertyListPropertyValueEditor>("Error converting DB value to String", ex);
                }

                return base.ConvertDbToString(prop2, propertyType, dataTypeService);
            }

            public override XNode ConvertDbToXml(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value != null && string.IsNullOrWhiteSpace(property.Value.ToString()) == false)
                {
                    try
                    {
                        var model = JsonConvert.DeserializeObject<PropertyListValue>(property.Value.ToString());
                        if (model != null && model.DataTypeGuid.Equals(Guid.Empty) == false && model.Values != null)
                        {
                            var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
                            var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
                            var propType = new PropertyType(dtd);

                            for (var i = 0; i < model.Values.Count; i++)
                            {
                                var obj = model.Values[i];
                                var prop = new Property(propType, obj?.ToString());
                                var newValue = propEditor.ValueEditor.ConvertDbToXml(prop, propType, dataTypeService);
                                model.Values[i] = newValue;
                            }

                            return new XElement("values",
                                new XAttribute("dtd", model.DataTypeGuid),
                                model.Values.Select(x => new XElement("value", x)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<PropertyListPropertyValueEditor>("Error converting DB value to String", ex);
                    }
                }

                return base.ConvertDbToXml(property, propertyType, dataTypeService);
            }

            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return string.Empty;

                try
                {
                    var model = JsonConvert.DeserializeObject<PropertyListValue>(editorValue.Value.ToString());
                    if (model.Values != null)
                    {
                        var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                        var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
                        var preValues = dataTypeService.GetPreValuesCollectionByDataTypeId(dtd.Id);
                        var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);

                        for (var i = 0; i < model.Values.Count; i++)
                        {
                            var obj = model.Values[i];

                            var propData = new ContentPropertyData(obj, preValues, new Dictionary<string, object>());
                            var newValue = propEditor.ValueEditor.ConvertEditorToDb(propData, obj);
                            model.Values[i] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }
                    }

                    return JsonConvert.SerializeObject(model);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<PropertyListPropertyValueEditor>("Error converting DB value to Editor", ex);
                }

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