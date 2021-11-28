using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Aprox.Api.Model
{
    public class PveResource
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("node")]
        public string Node { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        public PveResource(string id, string node, string type)
        {
            Id = id;
            Node = node;
            Type = type;
        }
    }
}
