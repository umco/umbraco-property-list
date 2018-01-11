using Newtonsoft.Json;
using Our.Umbraco.PropertyList.Converters;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.PropertyList
{
    public class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CacheRefresherBase<DataTypeCacheRefresher>.CacheUpdated += DataTypeCacheRefresher_Updated;
        }

        private void DataTypeCacheRefresher_Updated(DataTypeCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (e.MessageType == MessageType.RefreshByJson)
            {
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int) } });
                if (payload != null)
                {
                    foreach (var item in payload)
                    {
                        PropertyListValueConverter.ClearDataTypeCache(item.Id);
                    }
                }
            }
        }
    }
}