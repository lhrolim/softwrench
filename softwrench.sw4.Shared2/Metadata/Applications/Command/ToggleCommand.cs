namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ToggleCommand : ICommandDisplayable {
        public ToggleCommand(string id, string position, string initialStateExpression, ICommandDisplayable onCommand, ICommandDisplayable offCommand) {
            Id = id;
            OnCommand = onCommand;
            OffCommand = offCommand;
            State = false;
            InitialStateExpression = initialStateExpression;
            Position = position;
        }

        public bool State { get; set; }

        public ICommandDisplayable OnCommand {
            get; set;
        }

        public ICommandDisplayable OffCommand {
            get; set;
        }

        public string InitialStateExpression {
            get; set;
        }

        public string Id { get; set; }

        public string Role { get; set; }

        public string Position { get; set; }

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
