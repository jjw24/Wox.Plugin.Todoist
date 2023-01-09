using Flow.Launcher.Plugin;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Wox.Plugin.Todoist
{
    public class TodoistPlugin : IPlugin, ISettingProvider
    {

        private static readonly TodoistAPI client = new TodoistAPI();
        private static WoxSettingsStorage configuration;

        private SettingsControl settingsControl;
        public Dispatcher controlDispatcher { get; set; }

        private static readonly int FAILED_TASK_RESULT_SCORE = 1;
        private static readonly int VALID_RESULT_SCORE = 1000000;

        private PluginInitContext context;

        private Settings settings;

        public TodoistPlugin()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        }

        public Control CreateSettingPanel()
        {
            settingsControl = new SettingsControl(configuration, this);
            return settingsControl;
        }

      
        public void Init(PluginInitContext context)
        {
            this.context = context;

            settings = context.API.LoadSettingJsonStorage<Settings>();

            configuration = new WoxSettingsStorage(settings);
        }

        public List<Result> Query(Query query)
        {
            string task = query.Search;
            string api_key = configuration.Api_key;
            var results = new List<Result>();

            if (api_key.Trim().Length != 0)
            {
                results.Add(GetResultValid(api_key,task, query.ActionKeyword));

                if (configuration.FailedRequests.Count != 0)
                    results.Add(getResultTasksFailed(query));

                return results;
            }
            else
            {
                return new List<Result> { getResultMissingAPI() };
            }
        }

        public Result GetResultValid(string api_key, string task, string actionKeyword)
        {
            return new Result
            {

                Title = $"Add task: '{task}'.",
                IcoPath = "images\\icon.png",
                Score = VALID_RESULT_SCORE,
                Action = _ =>
                {
                    if (string.IsNullOrEmpty(task))
                        return false;

                    var request = client.CreateQuickTask(api_key, task);

                    request.ContinueWith((taskRequest) =>
                    {

                        HttpStatusCode responseStatus = taskRequest.Result;

                        switch (responseStatus)
                        {
                            case HttpStatusCode.OK:
                                context.API.ChangeQuery(string.IsNullOrEmpty(actionKeyword) ? string.Empty : $"{actionKeyword} ");
                                context.API.ShowMsg("Success", $"Task {task} added", context.CurrentPluginMetadata.IcoPath);
                                break;
                            
                            default:
                                configuration.SetLastFailedHttpCode(responseStatus);
                                configuration.SetLastFailedHttpCode(responseStatus);
                                configuration.AddFailedRequest(task);
                                UpdateSettingsControl();
                                context.API.ChangeQuery(string.IsNullOrEmpty(actionKeyword) ? string.Empty : $"{actionKeyword} ", true);
                                break;
                        }
                    });
                   
                    return false;
                },
               
            };
        }


        public Result getResultMissingAPI()
        {
            return new Result
            {

                Title = "You must set your todoist API token in the plugin settings before adding tasks",
                IcoPath = "images\\icon.png",
                Action = _ =>
                { return true; },
                
            };
        }

        public Result getResultTasksFailed(Query query)
        {
            return new Result
            {

                Title = $"Failed to create {configuration.FailedRequests.Count} tasks. Click here to try again. You can check the settings for more info.",
                IcoPath = "images\\icon.png",
                Score = FAILED_TASK_RESULT_SCORE,
                Action = _ =>
                { 
                  ResendFailedTasks(query.RawQuery);
                  return false;
                }
            };
        }

        public void ResendFailedTasks(string fullQuery = "")
        {
            List<string> tasks = configuration.FailedRequests;
            string api_key = configuration.Api_key;
            configuration.EmptyLastFailedHttpCode();

            foreach (string task in tasks)
            {
                var request = client.CreateQuickTask(api_key, task);

                request.ContinueWith((taskRequest) =>
                {
                    HttpStatusCode responseStatus = taskRequest.Result;
                    if (responseStatus == HttpStatusCode.OK)
                    {
                        configuration.RemoveFailedRequest(task);
                        if (!string.IsNullOrEmpty(fullQuery)) // called from SettingsControl.xaml.cs
                            context.API.ChangeQuery(fullQuery, true);
                    }
                    else
                    {
                        configuration.SetLastFailedHttpCode(responseStatus);
                    }

                    UpdateSettingsControl();

                });
            }
        }


        public void UpdateSettingsControl()
        {
             if (settingsControl!=null && controlDispatcher!= null)
             {
                controlDispatcher.BeginInvoke(
                new ThreadStart(() => { settingsControl.ConfigureFailedTasks(); }));
            }
        }
    }
}

