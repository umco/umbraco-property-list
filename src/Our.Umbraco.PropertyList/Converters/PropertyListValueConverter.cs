using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Our.Umbraco.PropertyList.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.PropertyList.Converters
{
    public class PropertyListValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(PropertyListPropertyEditor.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var innerPropertyType = this.GetInnerPublishedPropertyType(propertyType);

            var elements = XElement.Parse(source.ToString());

            var values = new List<object>();

            foreach (var element in elements.XPathSelectElements("value"))
            {
                var valueData = element.Value;
                var valueSource = innerPropertyType.ConvertDataToSource(valueData, preview);

                values.Add(valueSource);
            }

            return values;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            var items = source as List<object>;
            if (items != null)
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
                var result = Array.CreateInstance(innerPropertyType.ClrType, objects.Count);
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
            // TODO: Review if we need to call `ConvertSourceToXPath` for each of the values?
            return new XPathDocument(new StringReader(source.ToString())).CreateNavigator();
        }

        private PublishedPropertyType GetInnerPublishedPropertyType(PublishedPropertyType propertyType)
        {
            var cacheKey = string.Format(
                "Our.Umbraco.PropertyList.PropertyListValueConverter.GetInnerPublishedPropertyType_{0}_{1}",
                propertyType.DataTypeId,
                propertyType.ContentType.Id);

            return (PublishedPropertyType)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                cacheKey,
                () =>
                {
                    var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                    var prevalues = dataTypeService.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
                    var dict = prevalues.PreValuesAsDictionary;
                    if (dict.ContainsKey("dataType"))
                    {
                        var dtdPreValue = dict["dataType"];
                        Guid dtdGuid;
                        if (Guid.TryParse(dtdPreValue.Value, out dtdGuid))
                        {
                            var dtd = dataTypeService.GetDataTypeDefinitionById(dtdGuid);

                            return new PublishedPropertyType(propertyType.ContentType, new PropertyType(dtd));
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
    }
}