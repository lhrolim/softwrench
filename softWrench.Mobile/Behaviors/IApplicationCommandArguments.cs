using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal interface IApplicationCommandArguments
    {
        ApplicationSchemaDefinition ApplicationSchemaDefinition { get; }
        DataMap DataMap { get; }
        CompositeDataMap Composite { get; }
        DataRepository DataRepository { get; }
        MetadataRepository MetadataRepository { get; }
        User User { get; }
    }
}