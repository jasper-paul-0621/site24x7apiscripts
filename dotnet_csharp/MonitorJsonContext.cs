using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Site24x7Integration
{
    [JsonSerializable(typeof(List<Monitor>))]
    public partial class MonitorJsonContext : JsonSerializerContext { }
}
