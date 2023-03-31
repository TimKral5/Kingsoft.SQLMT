using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kingsoft.SQLMT
{
    public class AppConfig
    {
        [JsonProperty("con-string")]
        public string ConnectionString { get; set; }
        [JsonProperty("server")]
        public string Server { get; set; }
        [JsonProperty("database")]
        public string Database { get; set; }
        [JsonProperty("user")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }

        public AppConfig()
        {

        }
    }
}
