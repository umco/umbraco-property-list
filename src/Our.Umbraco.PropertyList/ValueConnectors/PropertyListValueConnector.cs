using System.Collections.Generic;
using Newtonsoft.Json;
using Our.Umbraco.PropertyList.Models;
using Our.Umbraco.PropertyList.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;

namespace Our.Umbraco.PropertyList.ValueConnectors
{
    // https://our.umbraco.org/apidocs/csharp/api/Umbraco.Core.Deploy.IValueConnector.html
    public class PropertyListValueConnector : IValueConnector
    {
        public IEnumerable<string> PropertyEditorAliases => new[] { PropertyListPropertyEditor.PropertyEditorAlias };

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            // get the property value
            var value = property.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // deserialize it
            var model = JsonConvert.DeserializeObject<PropertyListValue>(value);
            if (model == null)
                return null;

            // add the selected data-type as a dependency
            var udi = Udi.Create(Constants.UdiEntityType.DataType, model.DataTypeGuid);
            dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Match));

            // loop through each value
            foreach (var item in model.Values)
            {
                // pass it to its own value-connector

                // TODO: How to access the `ValueConnectorCollection`?
                // I suspect that this may need to be added to the Deploy Contrib project.
            }

            return JsonConvert.SerializeObject(model);
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            // take the value
            if (string.IsNullOrWhiteSpace(value))
                return;

            // deserialize it
            var model = JsonConvert.DeserializeObject<PropertyListValue>(value);
            if (model == null)
                return;

            // loop through each value
            foreach (var item in model.Values)
            {
                // pass it to its own value-connector

                // TODO: How to access the `ValueConnectorCollection`?
                // I suspect that this may need to be added to the Deploy Contrib project.
            }
        }
    }
}