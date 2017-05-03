using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Our.Umbraco.PropertyList
{
    public class Bootstrap : ApplicationEventHandler
    {
        private CacheHelper _applicationCache;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _applicationCache = applicationContext.ApplicationCache;

            DataTypeService.Saved += DataTypeService_Saved;
        }

        private void DataTypeService_Saved(IDataTypeService sender, SaveEventArgs<IDataTypeDefinition> e)
        {
            var cacheKeyPrefix = "Our.Umbraco.PropertyList.PropertyListValueConverter.GetInnerPublishedPropertyType_";

            foreach (var dataType in e.SavedEntities)
            {
                _applicationCache.RuntimeCache.ClearCacheByKeySearch(string.Concat(cacheKeyPrefix, dataType.Id));
            }
        }
    }
}