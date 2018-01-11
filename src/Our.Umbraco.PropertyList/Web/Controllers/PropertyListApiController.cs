using System;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.PropertyList.Web.Controllers
{
    [PluginController("PropertyList")]
    public class PropertyListApiController : UmbracoAuthorizedJsonController
    {
        // TODO: Once Umbraco's DataTypeController supports retrieving a datatype by Guid, this method can be removed.
        // https://github.com/umbraco/Umbraco-CMS/blob/dev-v7/src/Umbraco.Web/Editors/DataTypeController.cs
        public DataTypeDisplay GetDataTypeByKey(Guid key)
        {
            var dataType = Services.DataTypeService.GetDataTypeDefinitionById(key);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dataType);
        }

        // TODO: Once Umbraco's ContentTypeController supports retrieving the scaffold by Guid, this method can be removed.
        // https://github.com/umbraco/Umbraco-CMS/blob/dev-v7/src/Umbraco.Web/Editors/ContentTypeController.cs
        public ContentPropertyDisplay GetPropertyTypeScaffoldByKey(Guid key)
        {
            var dataTypeDiff = Services.DataTypeService.GetDataTypeDefinitionById(key);

            if (dataTypeDiff == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var preVals = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeDiff.Id);
            var editor = PropertyEditorResolver.Current.GetByAlias(dataTypeDiff.PropertyEditorAlias);

            return new ContentPropertyDisplay()
            {
                Editor = dataTypeDiff.PropertyEditorAlias,
                Validation = new PropertyTypeValidation() { },
                View = editor.ValueEditor.View,
                Config = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals)
            };
        }
    }
}