using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using NAnt.Core;
using NAnt.Core.Attributes;
using Newtonsoft.Json.Linq;
using Task = NAnt.Core.Task;

namespace softWrench.sw4.nant.classes {

    public class ChangeLogTask : Task {
        [TaskAttribute("dochangelog", Required = false)]
        public string DoChangelog { get; set; }

        [TaskAttribute("pattern", Required = false)]
        public string Pattern { get; set; }

        [TaskAttribute("initversion", Required = false)]
        public string InitVersion { get; set; }

        [TaskAttribute("endversion", Required = false)]
        public string EndVersion { get; set; }

        [TaskAttribute("includeinit", Required = false)]
        public string IncludeInit { get; set; }

        [TaskAttribute("includeend", Required = false)]
        public string IncludeEnd { get; set; }

        private const string Username = "emfmesquita";
        private const string Password = "FE7IFKu0Biqc2ptn1TW2";
        private const string ReleasesPath = "/repos/controltechnologysolutions/softwrench/releases";
        private const string ChangelogPath = "changelog.html";
        private const string InstalationTasksPath = "instalationTasks.html";
        public static Regex VersionNumberRegex = new Regex("([0-9]+\\.)*[0-9]+");
        public static Regex InstalationTaskRegex = new Regex("<h2>[\\W]*(installation task)((?!<h2>).)*", RegexOptions.IgnoreCase);

        private readonly SortedDictionary<string, string> _releases = new SortedDictionary<string, string>();
        private readonly SortedDictionary<string, string> _instalationTasks = new SortedDictionary<string, string>();
        private Regex _tagRegex;
        private bool _includeInit = true;
        private bool _includeEnd = true;

        protected override void ExecuteTask() {
            if (string.IsNullOrEmpty(DoChangelog) || !"true".Equals(DoChangelog)) {
                Log(Level.Info, "Changelog skipped due to \"dochangelog\" property.");
                return;
            }

            if (!string.IsNullOrEmpty(Pattern)) {
                _tagRegex = new Regex(Pattern, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(IncludeInit)) {
                _includeInit = "true".Equals(IncludeInit);
            }
            if (!string.IsNullOrEmpty(IncludeEnd)) {
                _includeEnd = "true".Equals(IncludeEnd);
            }
            InitVersion = ExtractVersion(InitVersion);
            EndVersion = ExtractVersion(EndVersion);

            LoadReleases();
            CreateChangelogFile(BuildContent(_releases), ChangelogPath);
            CreateChangelogFile(BuildContent(_instalationTasks), InstalationTasksPath);
        }

        public void Test() {
            ExecuteTask();
        }

        private void LoadReleases() {
            Load(ReleasesPath, AddRelease);
        }

        // extract version from tag
        private string ExtractVersion(string tag) {
            if (string.IsNullOrEmpty(tag)) {
                return null;
            }
            var m = VersionNumberRegex.Match(tag);
            if (m.Length > 0) {
                return m.Groups[0].Value;
            }
            Log(Level.Info, string.Format("The version {0} is not valid", tag));
            return null;
        }

        // create changelog file
        private static void CreateChangelogFile(string content, string path) {
            // Delete the file if it exists.
            if (File.Exists(path)) {
                File.Delete(path);
            }

            // Create the file.
            using (var fs = File.Create(path)) {
                var info = new UTF8Encoding(true).GetBytes(content);
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }

        // build changelog html content
        private string BuildContent(SortedDictionary<string, string> source) {
            var sb = new StringBuilder();
            sb.Append("<html>\n\n");
            var entries = source.ToList();
            entries.Reverse();
            entries.ForEach(e => BuildContent(e, sb));
            sb.Append("\n\n</html>");
            return sb.ToString();
        }

        // build html content from one entry
        private void BuildContent(KeyValuePair<string, string> entry, StringBuilder sb) {
            var tag = entry.Key;
            if (_tagRegex != null && !_tagRegex.IsMatch(tag.Trim())) {
                return;
            }
            if (!IsVersionInRange(tag)) {
                return;
            }

            sb.Append(string.Format("\n<br/><h1>{0}</h1>\n\n", tag));
            sb.Append(entry.Value);
            sb.Append("\n\n<hr>\n\n");
        }

        private bool IsVersionInRange(string tag) {
            if (string.IsNullOrEmpty(InitVersion) && string.IsNullOrEmpty(EndVersion)) {
                return true;
            }

            var match = VersionNumberRegex.Match(tag);
            if (match.Length <= 0) {
                return false;
            }

            var version = match.Groups[0].Value;
            if (!string.IsNullOrEmpty(InitVersion)) {
                var comparison = string.Compare(InitVersion, version, StringComparison.Ordinal);
                if ((_includeInit && comparison > 0) || (!_includeInit && comparison >= 0)) {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(EndVersion)) {
                var comparison = string.Compare(EndVersion, version, StringComparison.Ordinal);
                if ((_includeEnd && comparison < 0) || (!_includeEnd && comparison <= 0)) {
                    return false;
                }
            }

            return true;
        }

        // load all pages from a github resource
        private static void Load(string path, Action<JToken> loadAction) {
            var lastPage = 1;
            var currentPage = 1;
            while (currentPage <= lastPage) {
                var loadPage = LoadPage(currentPage, path, loadAction);
                loadPage.Wait();
                lastPage = loadPage.Result;
                currentPage++;
            }
        }

        // get last page number from response header
        private static int GetLastPage(HttpResponseMessage response) {
            var linkHeader = response.Headers.FirstOrDefault(h => "Link".Equals(h.Key));
            if (linkHeader.Value == null) {
                return -1;
            }
            var link = linkHeader.Value.First();
            var start = link.LastIndexOf("page=", StringComparison.Ordinal) + 5;
            var end = link.LastIndexOf(">", StringComparison.Ordinal);
            var length = end - start;
            var lastPageString = link.Substring(start, length);
            return int.Parse(lastPageString);
        }

        // add auth headers on request
        private static void AddHeaders(HttpClient client) {
            // base64 of username:password added as header of request
            var plainTextBytes = Encoding.UTF8.GetBytes(Username + ":" + Password);
            var header = "Basic " + Convert.ToBase64String(plainTextBytes);
            client.DefaultRequestHeaders.Add("Authorization", header);
            client.DefaultRequestHeaders.Add("User-Agent", "Changelog");
        }

        // load a page from github
        private static async System.Threading.Tasks.Task<int> LoadPage(int page, string path, Action<JToken> loadAction) {
            var client = new HttpClient();
            AddHeaders(client);

            // build of the url
            var ghApiUrl = new StringBuilder("https://api.github.com");
            ghApiUrl.Append(path).Append("?page=").Append(page);

            var response = await client.GetAsync(ghApiUrl.ToString());
            var lastPage = GetLastPage(response);
            var content = await response.Content.ReadAsStringAsync();
            var jArray = JArray.Parse(content);
            jArray.ToList().ForEach(loadAction);
            return (lastPage);
        }

        // load a entry
        private void AddRelease(JToken jToken) {
            try {
                var releaseNotes = jToken.Value<string>("body");
                var tag = jToken.Value<string>("tag_name");
                releaseNotes = releaseNotes.Replace("\n", "");
                releaseNotes = releaseNotes.Replace("\r", "");
                if (string.IsNullOrEmpty(releaseNotes.Trim())) {
                    return;
                }
                _releases.Add(tag, releaseNotes);

                var match = InstalationTaskRegex.Match(releaseNotes);
                if (match.Length <= 0) {
                    return;
                }
                _instalationTasks.Add(tag, match.Groups[0].Value);
            } catch (Exception ex) {
                // just ignores
            }
        }
    }
}
