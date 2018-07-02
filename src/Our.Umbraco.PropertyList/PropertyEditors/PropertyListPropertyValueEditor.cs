using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.PropertyList.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
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
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            // We'd like this to be hot-swappable with "Repeatable Textstrings" property-editor.
            // We make the assumption that if the value isn't JSON, then it could be newline-delimited list
            // let's split/join the values and get it into a proxy JSON string.
            if (propertyValue.DetectIsJson() == false)
            {
                var items = string.Join("', '", propertyValue.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                propertyValue = $"{{ dtd: '{PropertyListPropertyEditor.DefaultTextstringPropertyEditorGuid}', values: [ '{items}' ] }}";
            }

            var model = JsonConvert.DeserializeObject<PropertyListValue>(propertyValue);
            if (model == null || model.DataTypeGuid.Equals(Guid.Empty) || model.Values == null)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            // Get the associated datatype definition
            var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
            if (dtd == null)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            // Lookup the property editor
            var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
            if (propEditor == null)
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);

            var propType = new PropertyType(dtd, propertyType.Alias);

            for (var i = 0; i < model.Values.Count; i++)
            {
                var obj = model.Values[i];

                var prop = new Property(propType, obj?.ToString());
                var newValue = propEditor.ValueEditor.ConvertDbToEditor(prop, propType, dataTypeService);

                model.Values[i] = (newValue == null) ? null : JToken.FromObject(newValue);
            }

            // Return the strongly-typed object, Umbraco will handle the JSON serializing/parsing, then Angular can handle it directly
            return model;
        }

        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var propertyValue = property?.Value?.ToString();
            if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue))
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            var model = JsonConvert.DeserializeObject<PropertyListValue>(propertyValue);
            if (model == null || model.DataTypeGuid.Equals(Guid.Empty) || model.Values == null)
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
            if (dtd == null)
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
            if (propEditor == null)
                return base.ConvertDbToString(property, propertyType, dataTypeService);

            var propType = new PropertyType(dtd, propertyType.Alias);

            for (var i = 0; i < model.Values.Count; i++)
            {
                var obj = model.Values[i];
                var prop = new Property(propType, obj?.ToString());
                var newValue = propEditor.ValueEditor.ConvertDbToString(prop, propType, dataTypeService);

                model.Values[i] = newValue;
            }

            return JsonConvert.SerializeObject(model);
        }

        public override XNode ConvertDbToXml(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var propertyValue = property?.Value?.ToString();
            if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue))
                return base.ConvertDbToXml(property, propertyType, dataTypeService);

            var model = JsonConvert.DeserializeObject<PropertyListValue>(propertyValue);
            if (model == null || model.DataTypeGuid.Equals(Guid.Empty) || model.Values == null)
                return base.ConvertDbToXml(property, propertyType, dataTypeService);

            var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
            if (dtd == null)
                return base.ConvertDbToXml(property, propertyType, dataTypeService);

            var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
            if (propEditor == null)
                return base.ConvertDbToXml(property, propertyType, dataTypeService);

            var propType = new PropertyType(dtd, propertyType.Alias);

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

        public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            var value = editorValue?.Value?.ToString();
            if (value == null || string.IsNullOrWhiteSpace(value))
                return base.ConvertEditorToDb(editorValue, currentValue);

            var model = JsonConvert.DeserializeObject<PropertyListValue>(value);
            if (model == null || model.DataTypeGuid.Equals(Guid.Empty) || model.Values == null)
                return base.ConvertEditorToDb(editorValue, currentValue);

            var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            var dtd = dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);
            if (dtd == null)
                return base.ConvertEditorToDb(editorValue, currentValue);

            var preValues = dataTypeService.GetPreValuesCollectionByDataTypeId(dtd.Id);
            if (preValues == null)
                return base.ConvertEditorToDb(editorValue, currentValue);

            var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
            if (propEditor == null)
                return base.ConvertEditorToDb(editorValue, currentValue);

            for (var i = 0; i < model.Values.Count; i++)
            {
                var obj = model.Values[i];

                var propData = new ContentPropertyData(obj, preValues, new Dictionary<string, object>());
                var newValue = propEditor.ValueEditor.ConvertEditorToDb(propData, obj);
                model.Values[i] = (newValue == null) ? null : JToken.FromObject(newValue);
            }

            return JsonConvert.SerializeObject(model);
        }
    }
}