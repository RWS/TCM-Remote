// Copyright (c) 2024-2026 RWS Holdings plc and its subsidiaries.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tridion.TCMRemote.Models
{
    public class Links
    {
        [JsonProperty("$type")]
        public string type { get; set; }

        [JsonProperty("OpenAPI Docs")]
        public string OpenAPIDocs { get; set; }

        [JsonProperty("Swagger UI")]
        public string SwaggerUI { get; set; }

        [JsonProperty("Access Management Url")]
        public string AccessManagementUrl { get; set; }
    }

    public class Capabilities
    {
        [JsonProperty("$type")]
        public string type { get; set; }
        public List<string> EnabledFeatures { get; set; }
        public Links Links { get; set; }
    }
}
