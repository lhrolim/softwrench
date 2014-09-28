using SQLite;

namespace softWrench.Mobile.Metadata
{
    internal sealed class User
    {
        public static User Current { get; set; }

        public static Settings Settings { get; set; }

        [PrimaryKey]
        public string UserName { get; set; }

        public string OrgId { get; set; }

        public string SiteId { get; set; }
    }
}