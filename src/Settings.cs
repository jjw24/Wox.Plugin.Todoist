using System;
using System.Collections.Generic;
using System.Net;

namespace Wox.Plugin.Todoist
{
    public class Settings
    {
    
        public HttpStatusCode LastFailedStatusCode { get; set; }

        public List<string> FailedRequests { get; set; }
        
        public Settings()
        {
            api_key = "";
            FailedRequests = new List<string>();
            LastFailedStatusCode = HttpStatusCode.OK;
        }

        public string api_key { get; set; }
    }
}
