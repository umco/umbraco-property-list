using System.Collections.Generic;
using ClientDependency.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;
using static Our.Umbraco.PropertyList.PropertyListConstants;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    [PropertyEditor(PropertyEditorKeys.Alias, PropertyEditorKeys.Name, PropertyEditorValueTypes.Json, PropertyEditorKeys.ViewPath, Group = PropertyEditorKeys.Group, Icon = PropertyEditorKeys.Icon, IsParameterEditor = false)]
    [PropertyEditorAsset(ClientDependencyType.Javascript, PropertyEditorKeys.JavaScriptPath)]
    public class PropertyListPropertyEditor : PropertyEditor
    {
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
                { PreValueKeys.DataType, DataTypeGuids.DefaultTextstring },
                { PreValueKeys.MinItems, 0 },
                { PreValueKeys.MaxItems, 0 },
                { PreValueKeys.HideLabel, 0 }
            };
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new PropertyListPreValueEditor();
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PropertyListPropertyValueEditor(base.CreateValueEditor());
        }
    }
}