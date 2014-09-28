using System.Linq;
using softWrench.sW4.Data.API;
using softwrench.sW4.Shared2.Metadata.Applications.Command;

namespace softWrench.sW4.Metadata.Applications.Command {
    public class ApplicationCommandUtils {

        public static ApplicationCommand GetApplicationCommand(ApplicationMetadata applicationMetadata, string commandId) {
            return applicationMetadata.Schema.CommandSchema.Commands.FirstOrDefault(x => x.Id == commandId);
        }
    }
}
