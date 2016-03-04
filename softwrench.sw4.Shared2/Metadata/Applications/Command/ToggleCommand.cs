namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ToggleCommand : ICommandDisplayable {
        public ToggleCommand(string id, string position, string initialStateExpression, ToggleChildCommand onCommand, ToggleChildCommand offCommand) {
            Id = id;
            OnCommand = onCommand;
            OffCommand = offCommand;
            State = false;
            InitialStateExpression = initialStateExpression;
            Position = position;
        }

        public bool State { get; set; }

        public ToggleChildCommand OnCommand {
            get; set;
        }

        public ToggleChildCommand OffCommand {
            get; set;
        }

        public string InitialStateExpression {
            get; set;
        }

        public string Id { get; set; }

        public string Role { get; set; }

        public string Position { get; set; }

        public string ShowExpression {
            get; set;
        }

        public string PermissionExpression {
            get; set;
        }

        public string Label { get; set; }

        public string Type {
            get {
                return typeof(ToggleCommand).Name;
            }
        }

        public ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand) {
            return this;
        }
    }
}
