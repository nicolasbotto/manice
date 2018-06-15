using Newtonsoft.Json;
using System.Collections.Generic;

namespace CommLendingWeb.Models
{
	public class Entity
    {
		public string Id { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public IDictionary<string, object> AdditionalData { get; set; }
	}
}
