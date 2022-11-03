using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wox.Plugin.Todoist
{
    class TodoistAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public TodoistAPI()
        {

        }

        public Task<HttpStatusCode> CreateQuickTask(string api_key, string task)
        {
            var values =
                new Dictionary<string, string>
                {
                   { "token", api_key},
                   { "text", task }
                };
            var content = new FormUrlEncodedContent(values);

            var request = client.PostAsync("https://api.todoist.com/sync/v9/quick/add", content);
            return request.ContinueWith((taskRequest) =>
            {
                try
                {
                    HttpResponseMessage response = taskRequest.Result;
                    return response.StatusCode;
                }
                catch (Exception)
                {
                    return HttpStatusCode.ServiceUnavailable;
                }

            });
        }
    }
}
