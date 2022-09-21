using ManagedCommon;
using Wox.Plugin;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;
using Wox.Infrastructure;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using Toshevski.PowerToys.Plugins.TestNewtonSoft;

namespace Toshevski.PowerToys.Plugins.Web
{
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "TEST";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "TEST";

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; } = "Images/logo.png";

        private bool Disposed { get; set; }

        private Model Model { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        /// 
        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            var res = JsonConvert.SerializeObject(new
            {
                Abc = "fsdf"
            });

            results.Add(new Result()
            {
                QueryTextDisplay = $"You have entered {res}",
                IcoPath = BrowserInfo.IconPath,
                Title = res,
                SubTitle = $"Press 'Enter' to open {res}",
                Action = action =>
                {
                    if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, res))
                    {
                        return false;
                    }

                    return true;
                }
            });



            results.Add(new Result()
            {
                QueryTextDisplay = $"ExecuteFileName {res}",
                IcoPath = BrowserInfo.IconPath,
                Title = Context.CurrentPluginMetadata.ExecuteFileName,
                SubTitle = $"Press 'Enter' to open {res}",
                Action = action =>
                {
                    if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, res))
                    {
                        return false;
                    }

                    return true;
                }
            });



            results.Add(new Result()
            {
                QueryTextDisplay = Path.GetDirectoryName(Context.CurrentPluginMetadata.ExecuteFilePath)!,
                IcoPath = BrowserInfo.IconPath,
                Title = Path.GetDirectoryName(Context.CurrentPluginMetadata.ExecuteFilePath)!,
                SubTitle = $"Press 'Enter' to open {res}",
                Action = action =>
                {
                    if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, Context.CurrentPluginMetadata.ExecuteFilePath))
                    {
                        return false;
                    }

                    return true;
                }
            });

            // show msg dialog
            //Context.API.ShowMsg("Some MSG here");

            // show notification, windows style
            //Context.API.ShowNotification("Notif text");

            // INFO: open newe UWP window
            //StaThreadWrapper(() =>
            //{
            //    var win = new Test();
            //    win.Show();
            //});


            return results;
        }

        private static void StaThreadWrapper(Action action)
        {
            var t = new Thread(o =>
            {
                action();
                System.Windows.Threading.Dispatcher.Run();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));

            var path = Path.Combine(Path.GetDirectoryName(Context.CurrentPluginMetadata.ExecuteFilePath)!,
                "Newtonsoft.Json.dll");

            Assembly.LoadFrom(path);

#pragma warning disable CA1416 // Validate platform compatibility
            Context.API.ThemeChanged += OnThemeChanged;
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            UpdateIconPath(Context.API.GetCurrentTheme());
#pragma warning restore CA1416 // Validate platform compatibility
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var menuResults = new List<ContextMenuResult>();

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

            if (Context != null && Context.API != null)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                Context.API.ThemeChanged -= OnThemeChanged;
#pragma warning restore CA1416 // Validate platform compatibility
            }

            Disposed = true;
        }

        private static bool CopyToClipboard(string value)
        {
            Clipboard.SetText(value);
            return true;
        }

        //private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite 
        //    ? "Images/light.png" : "Images/dark.png";

        private void UpdateIconPath(Theme theme) => IconPath = "Images/logo.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }

}