namespace softwrench.sw4.api.classes.application {

    public abstract class ActionBasedApplicationFiltereable : IGenericApplicationFiltereable {

        public abstract string ApplicationName();
        public abstract string ClientFilter();
        public string ExtraKey() {
            return Action();
        }

        public abstract string Action();
        
    }
}
