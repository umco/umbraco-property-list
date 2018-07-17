using System;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.PropertyList
{
    public class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType != MessageType.RefreshByJson)
                    return;

                // NOTE: The properties for the JSON payload are available here: (Currently there isn't a public API to deserialize the payload)
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.6.0/src/Umbraco.Web/Cache/DataTypeCacheRefresher.cs#L66-L70
                // TODO: Once Umbraco's `DataTypeCacheRefresher.DeserializeFromJsonPayload` is public, we can deserialize correctly.
                // https://github.com/umbraco/Umbraco-CMS/blob/release-7.6.0/src/Umbraco.Web/Cache/DataTypeCacheRefresher.cs#L27
                var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int), UniqueId = Guid.Empty } });
                if (payload == null)
                    return;

                foreach (var item in payload)
                {
                    PropertyEditors.PropertyListValidator.ClearDataTypeCache(item.UniqueId);
                    ValueConverters.PropertyListValueConverter.ClearDataTypeCache(item.Id);
                }
            };
        }
    }
}