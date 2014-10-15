namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ResourceCommand : ICommandDisplayable {

        public string Id { get; set; }
        public string Path { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }

        public string Type { get { return typeof(ResourceCommand).Name; } }

        public ResourceCommand(string id, string path, string role) {
            Id = id;
            Path = path;
            Role = role;
        }

    }
}
