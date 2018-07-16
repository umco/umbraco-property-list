using System.Collections.Generic;
using ClientDependency.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;
using static Our.Umbraco.PropertyList.PropertyListConstants;

namespace Our.Umbraco.PropertyList.PropertyEditors
{
    [PropertyEditor(PropertyEditorAlias, PropertyEditorName, PropertyEditorValueTypes.Json, PropertyEditorViewPath, Group = "Lists", Icon = "icon-bulleted-list", IsParameterEditor = false)]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.js")]
    public class PropertyListPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.PropertyList";
        public const string PropertyEditorName = "Property List";
        public const string PropertyEditorViewPath = "~/App_Plugins/PropertyList/Views/propertylist.html";

        internal const string DefaultTextstringPropertyEditorGuid = "0cc0eba1-9960-42c9-bf9b-60e150b429ae"; // Guid for default Textstring

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
                { PreValueKeys.DataType, DefaultTextstringPropertyEditorGuid },
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