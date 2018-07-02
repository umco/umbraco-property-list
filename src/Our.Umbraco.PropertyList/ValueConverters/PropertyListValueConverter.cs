using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Our.Umbraco.PropertyList.Models;
using Our.Umbraco.PropertyList.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.PropertyList.ValueConverters
{
    public class PropertyListValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(PropertyListPropertyEditor.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var data = source?.ToString();
            if (string.IsNullOrWhiteSpace(data))
                return null;

            var innerPropertyType = this.GetInnerPublishedPropertyType(propertyType);

            var items = new List<object>();

            // Detect whether the value is in JSON or XML format
            //
            // NOTE: We can't be sure which format the data is in.
            // With "nested property-editors", (e.g. Nested Content, Stacked Content),
            // they don't convert the call `ConvertDbToXml`.
            if (data.DetectIsJson())
            {
                var model = JsonConvert.DeserializeObject<PropertyListValue>(data);
                if (model != null)
                    items.AddRange(model.Values);
            }
            else
            {
                // otherwise we assume it's XML
                var elements = XElement.Parse(data);
                if (elements != null && elements.HasElements)
                    items.AddRange(elements.XPathSelectElements("value").Select(x => x.Value));
            }

            var values = new List<object>();

            foreach (var valueData in items)
            {
                var valueSource = innerPropertyType.ConvertDataToSource(valueData, preview);

                values.Add(valueSource);
            }

            return values;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source is List<object> items)
            {
                var innerPropertyType = this.GetInnerPublishedPropertyType(propertyType);

                var objects = new List<object>();

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        objects.Add(innerPropertyType.ConvertSourceToObject(item, preview));
                    }
                }

                // Force result to right type
                var targetType = innerPropertyType.ClrType;
                var result = Array.CreateInstance(targetType, objects.Count);
                for (var i = 0; i < objects.Count; i++)
                {
                    var attempt = objects[i].TryConvertTo(targetType);
                    if (attempt.Success)
                        result.SetValue(attempt.Result, i);
                }

                return result;
            }

            return base.ConvertSourceToObject(propertyType, source, preview);
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source is List<object> items)
            {
                var innerPropertyType = this.GetInnerPublishedPropertyType(propertyType);

                var elements = new List<XElement>();

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        var xpathValue = innerPropertyType.ConvertSourceToXPath(item, preview);
                        var element = new XElement("value", xpathValue);
                        elements.Add(element);
                    }
                }

                return new XElement("values", elements).CreateNavigator();
            }

            return base.ConvertSourceToXPath(propertyType, source, preview);
        }

        private PublishedPropertyType GetInnerPublishedPropertyType(PublishedPropertyType propertyType)
        {
            var cacheKey = string.Format(
                "Our.Umbraco.PropertyList.PropertyListValueConverter.GetInnerPublishedPropertyType_{0}_{1}",
                propertyType.DataTypeId,
                propertyType.ContentType.Id);

            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<PublishedPropertyType>(
                cacheKey,
                () =>
                {
                    var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                    var prevalues = dataTypeService.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
                    var dict = prevalues.PreValuesAsDictionary;
                    if (dict.ContainsKey("dataType"))
                    {
                        var dtdPreValue = dict["dataType"];
                        if (Guid.TryParse(dtdPreValue.Value, out Guid dtdGuid))
                        {
                            var dtd = dataTypeService.GetDataTypeDefinitionById(dtdGuid);

                            return new PublishedPropertyType(propertyType.ContentType, new PropertyType(dtd, propertyType.PropertyTypeAlias));
                        }
                    }

                    return null;
                });
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            var innerPropertyType = GetInnerPublishedPropertyType(propertyType);

            return innerPropertyType != null
                ? typeof(IEnumerable<>).MakeGenericType(innerPropertyType.ClrType)
                : typeof(IEnumerable<object>);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }

        internal static void ClearDataTypeCache(int dataTypeId)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                string.Concat("Our.Umbraco.PropertyList.PropertyListValueConverter.GetInnerPublishedPropertyType_", dataTypeId));
        }
    }
}