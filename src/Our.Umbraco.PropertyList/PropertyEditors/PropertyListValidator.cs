using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    internal class PropertyListValidator : IPropertyValidator
    {
        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}