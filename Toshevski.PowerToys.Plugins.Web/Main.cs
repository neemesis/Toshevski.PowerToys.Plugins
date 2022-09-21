using ManagedCommon;
using Wox.Plugin;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;
using Wox.Infrastructure;
using System.Text.Json;
using System.IO;

namespace Toshevski.PowerToys.Plugins.Web
{
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Favorite Web Pages";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Quickly access favorite web pages";

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; } = "dark.jpg";

        private bool Disposed { get; set; }

        private Model Model { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            var path = Path.Combine(Path.GetDirectoryName(Context!.CurrentPluginMetadata.ExecuteFilePath)!,
                "webPages.json");

            Model = JsonSerializer.Deserialize<Model>(File.ReadAllText(path))!;

            var cleanedQuery = query.RawQuery
                .ToLower()
                .Replace("web", "")
                .Trim();

            var pages = Model.Favorites.Where(x => x.Shortcut.ToLower().Contains(cleanedQuery));

            foreach (var page in pages)
                results.Add(new Result()
                {
                    QueryTextDisplay = page.Shortcut,
                    IcoPath = BrowserInfo.IconPath,
                    Title = page.Url,
                    SubTitle = $"Press 'Enter' to open {page.Url}",
                    Action = action =>
                    {
                        if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, page.Url))
                        {
                            return false;
                        }

                        return true;
                    }
                });

            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var menuResults = new List<ContextMenuResult>();

            menuResults.Add(new ContextMenuResult
            {
                PluginName = Name,
                Title = "Copy url",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE8C8", // E8C8 => Symbol: Copy,
                AcceleratorKey = Key.Enter,
                Action = _ => CopyToClipboard(selectedResult.Title)
            });

            return menuResults;
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

        private static bool CopyToClipboard(string value)
        {
            Clipboard.SetText(value);
            return true;
        }
    }

}