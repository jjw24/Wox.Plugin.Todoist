using System.Collections.Generic;
using System.Net;

namespace Wox.Plugin.Todoist
{
    public class WoxSettingsStorage
    {

        private Settings settings;

        public WoxSettingsStorage(Settings settings)
        {
            this.settings = settings;
        }

        public string Api_key
        {
            get
            {
                return settings.api_key;
            }

            set
            {
                settings.api_key = value;
            }
        }

        public List<string> FailedRequests
        {
            get
            {
                return settings.FailedRequests;
            }
        }

        public void AddFailedRequest(string value)
        {
            FailedRequests.Add(value);
        }

        public void RemoveFailedRequest(string value)
        {
            FailedRequests.Remove(value);
        }
       
        public void SetLastFailedHttpCode(HttpStatusCode code)
        {
            settings.LastFailedStatusCode = code;
        }

        public void EmptyLastFailedHttpCode()
        {
            settings.LastFailedStatusCode = HttpStatusCode.OK;
        }

      
        public HttpStatusCode GetLastFailedHttpCode()
        {
            return settings.LastFailedStatusCode;
        }
      
    }
}
