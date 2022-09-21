using ManagedCommon;
using Wox.Plugin;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;
using Wox.Infrastructure;

namespace Toshevski.PowerToys.Plugins.Crypto
{
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Crypto Plugin";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Check current price of pairs from Binance";

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; } = "Images/logo.png";

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var cleanedQuery = query.RawQuery
                .ToLower()
                .Replace("crypto", "")
                .Trim()
                .ToUpper();

            var binanceRes = GetFromBinance(cleanedQuery);

            var results = new List<Result>();

            if (binanceRes != null)
                results.Add(new Result()
                {
                    QueryTextDisplay = $"You have entered {cleanedQuery}",
                    IcoPath = IconPath,
                    Title = $"{cleanedQuery}: {binanceRes.Price}",
                    SubTitle = "Press 'Enter' to open Binance",
                    Action = action =>
                    {
                        if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, $"https://www.binance.com/en/trade/{cleanedQuery}"))
                        {
                            return false;
                        }

                        return true;
                    }
                });
            
            return results;
        }

        private Model? GetFromBinance(string coinPair)
        {
            try
            {
                return new HttpClient().GetFromJsonAsync<Model>($"https://www.binance.com/api/v3/ticker/price?symbol={coinPair}").Result;
            }
            catch { }
            return null;
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
                Title = "Copy price",
                FontFamily = "Segoe MDL2 Assets",
                Glyph = "\xE8C8", // E8C8 => Symbol: Copy,
                AcceleratorKey = Key.Enter,
                Action = _ => CopyToClipboard(selectedResult.Title.Split(":")[1].Trim())
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