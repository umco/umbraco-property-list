using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Our.Umbraco.PropertyList.Models
{
    public class PropertyListValue
    {
        [JsonProperty("dtd")]
        public Guid DataTypeGuid { get; set; }

        [JsonProperty("values")]
        public List<object> Values { get; set; }
    }
}