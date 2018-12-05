using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Handles JSON serialization & deserialization based on Newtonsoft.Json.
/// 
/// The goal is that this class handles deserialization exactly like dotnet core MVC
/// So that it can be used for the websocket & client modules.
/// 
/// More info: https://medium.com/@agriciuc/working-with-json-net-in-net-core-rc2-part-1-5f3a65f4e11#.cv3wxfkp9
/// </summary>
public class JsonSerializer : ISerializer {

    private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings() {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public M Deserialize<M>(string json) where M : class {
        try {
            return JsonConvert.DeserializeObject<M>(json, SETTINGS);
        } catch (Exception) {
            return default(M);
        }
    }

    public string Serialize(object model) {
        return JsonConvert.SerializeObject(model, SETTINGS);
    }
}
