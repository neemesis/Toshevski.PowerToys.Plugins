using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toshevski.PowerToys.Plugins.Http
{
    public class HttpRequests
    {
        public List<HttpRequest> Requests { get; set; }
    }

    public class HttpRequest
    {
        public string Shortcut { get; set; }
        public string Url { get; set; }
        public string? Method { get; set; }
        public string? MediaType { get; set; }
        public string? Content { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}
