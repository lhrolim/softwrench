using cts.commons.simpleinjector;
using softWrench.sW4.Email;

namespace softWrench.sW4.Dynamic {
    public interface IDynComponentEmailer : ISingletonComponent {
        void SendDynComponentCreatedEmail(DynComponentCreatedEmail email);
        void SendDynComponentUpdatedEmail(DynComponentUpdatedEmail email);
        void SendDynComponentDeletedEmail(DynComponentDeleteEmail email);
        bool FillBaseEmailDTO(BaseEmailDto dto, string ipAddress, string comment, string userName, string subject);
    }

    public class BaseDynComponentEmailDto : BaseEmailDto {
        public string ReloadSufix { get; set; }
    }

    public class DynComponentCreatedEmail : BaseDynComponentEmailDto {
        public ScriptEntry Entry { get; set; }
    }

    public class DynComponentUpdatedEmail : BaseDynComponentEmailDto {
        public ScriptEntry OldEntry { get; set; }
        public ScriptEntry NewEntry { get; set; }
    }

    public class DynComponentDeleteEmail : BaseDynComponentEmailDto {
        public ScriptEntry Entry { get; set; }
    }
}
