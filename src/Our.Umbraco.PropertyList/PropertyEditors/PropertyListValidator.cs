using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Our.Umbraco.PropertyList.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using static Our.Umbraco.PropertyList.PropertyListConstants;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    internal class PropertyListValidator : IPropertyValidator
    {
        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            var results = new List<ValidationResult>();

            var model = JsonConvert.DeserializeObject<PropertyListValue>(value?.ToString());
            if (model == null)
                return results;

            bool getIntValue(string alias, out int number)
            {
                number = 0;
                return preValues.PreValuesAsDictionary.ContainsKey(alias) &&
                    int.TryParse(preValues.PreValuesAsDictionary[alias].Value, out number) &&
                    number > 0;
            }

            if (preValues.IsDictionaryBased)
            {
                var dict = preValues.PreValuesAsDictionary;
                if (getIntValue(PreValueKeys.MinItems, out int minItems) && model.Values.Count < minItems)
                {
                    results.Add(new ValidationResult($"There are {model.Values.Count} items in the list, when the minimum is set to {minItems}."));
                }

                if (getIntValue(PreValueKeys.MaxItems, out int maxItems) && model.Values.Count > maxItems)
                {
                    results.Add(new ValidationResult($"There are {model.Values.Count} items in the list, when the maximum is set to {maxItems}."));
                }
            }

            var meta = GetInnerPropertyMetaData(model.DataTypeGuid);
            if (meta == null)
                return results;

            var validators = meta.Item3;
            if (validators == null || validators.Any() == false)
                return results;

            foreach (var itemValue in model.Values)
            {
                // TODO: Consider what Mandatory and RegExp mean in the context of Property List, and how they should be handled.

                foreach (var validator in validators)
                {
                    results.AddRange(validator.Validate(itemValue, meta.Item1, meta.Item2));
                }
            }

            return results;
        }

        private Tuple<PreValueCollection, PropertyEditor, List<IPropertyValidator>> GetInnerPropertyMetaData(Guid dataTypeGuid)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<Tuple<PreValueCollection, PropertyEditor, List<IPropertyValidator>>>(
                string.Format(PropertyValidatorKeys.CacheKeyFormat, dataTypeGuid),
                 () =>
                 {
                     var service = ApplicationContext.Current.Services.DataTypeService;

                     var dtd = service.GetDataTypeDefinitionById(dataTypeGuid);
                     if (dtd == null)
                         return null;

                     var prevalues = service.GetPreValuesCollectionByDataTypeId(dtd.Id);
                     if (prevalues == null)
                         return null;

                     var propertyEditor = PropertyEditorResolver.Current.GetByAlias(dtd.PropertyEditorAlias);
                     if (propertyEditor == null || propertyEditor.ValueEditor == null)
                         return null;

                     return Tuple.Create(prevalues, propertyEditor, propertyEditor.ValueEditor.Validators);
                 });
        }

        internal static void ClearDataTypeCache(Guid dataTypeGuid)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                string.Format(PropertyValidatorKeys.CacheKeyFormat, dataTypeGuid));
        }
    }
}