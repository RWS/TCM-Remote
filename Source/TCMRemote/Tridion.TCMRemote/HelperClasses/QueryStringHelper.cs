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

using System;
using System.Collections.Specialized;

namespace Tridion.TCMRemote.HelperClasses
{
    public static class QueryStringHelper
    {
        public static NameValueCollection ParseQueryString(string queryString)
        {
            var collection = new NameValueCollection();

            if (string.IsNullOrEmpty(queryString))
                return collection;

            // Remove the leading '?' if present
            queryString = queryString.TrimStart('?');

            var pairs = queryString.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                collection.Add(key, value);
            }

            return collection;
        }

        public static NameValueCollection ParseQueryString(Uri uri)
        {
            return ParseQueryString(uri.Query);
        }
    }
}
