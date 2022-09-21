using ManagedCommon;
using Wox.Plugin;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;
using Wox.Infrastructure;
using System.IO;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Toshevski.PowerToys.Plugins.Http
{
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Http Collections";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Send predefined http requests";

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; } = "dark.jpg";

        private bool Disposed { get; set; }

        private HttpRequests Requests { get; set; }
        static HttpClient client = new HttpClient();

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            var cleanedQuery = query.Search
                .ToLower()
                .Trim();

            var requests = Requests.Requests.Where(x => x.Shortcut.ToLower().Contains(cleanedQuery));

            foreach (var request in requests)
                results.Add(new Result()
                {
                    QueryTextDisplay = request.Shortcut,
                    IcoPath = IconPath,
                    Title = request.Url,
                    SubTitle = $"Press 'Enter' to send request named '{request.Url}'",
                    Action = action =>
                    {
                        var hrm = new HttpRequestMessage(GetMethod(request.Method), new Uri(request.Url));
                        if (request.Content != null)
                            hrm.Content = new StringContent(request.Content);

                        if (request.MediaType != null)
                        {
                            hrm.Headers.Add("Content-Type", request.MediaType);
                        }

                        if (request.Headers != null)
                        {
                            foreach (var h in request.Headers)
                            {
                                hrm.Headers.Add(h.Key, h.Value);
                            }
                        }

                        var response = client.Send(hrm);

                        var responseContent = response.Content.ReadAsStringAsync().Result;

                        if (response.IsSuccessStatusCode)
                            Context!.API.ShowMsg("Your request was sucessfully sent!", responseContent);
                        else
                            Context!.API.ShowMsg("Your request failed!", responseContent);

                        return true;
                    }
                });

            return results;
        }

        private HttpMethod GetMethod(string? method)
        {
            switch (method)
            {
                case "GET": return HttpMethod.Get;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "HEAD": return HttpMethod.Head;
                case "OPTIONS": return HttpMethod.Options;
            }
            return HttpMethod.Get;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));

            var path = Path.Combine(Path.GetDirectoryName(Context.CurrentPluginMetadata.ExecuteFilePath)!,
                "predefinedRequests.json");

            Requests = JsonSerializer.Deserialize<HttpRequests>(File.ReadAllText(path))!;
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            Disposed = true;
        }
    }
}