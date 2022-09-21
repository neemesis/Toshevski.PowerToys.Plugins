namespace Toshevski.PowerToys.Plugins.Web
{
    public class Model
    {
        public List<Page> Favorites { get; set; }
    }

    public class Page
    {
        public string Shortcut { get; set; }
        public string Url { get; set; }
    }
}
