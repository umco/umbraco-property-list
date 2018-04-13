using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        PropertyEditorAlias,
        "Property List",
        "JSON",
        "~/App_Plugins/PropertyList/Views/propertylist.html",
        Group = "Lists",
        Icon = "icon-bulleted-list",
        IsParameterEditor = false)]
    [PropertyEditorAsset(ClientDependencyType.Css, "~/App_Plugins/PropertyList/css/propertylist.css")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.controllers.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "~/App_Plugins/PropertyList/js/propertylist.resources.js")]
    public class PropertyListPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.PropertyList";

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
                { "dataType", DefaultTextstringPropertyEditorGuid },
                { "minItems", 0 },
                { "maxItems", 0 }
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