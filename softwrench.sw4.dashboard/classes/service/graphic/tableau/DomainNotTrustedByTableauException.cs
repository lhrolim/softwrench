using softwrench.sw4.dashboard.classes.service.graphic.exception;

namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    public class DomainNotTrustedByTableauException : GraphicStorageSystemException {
        public DomainNotTrustedByTableauException(string message) : base(message) {}

        public static DomainNotTrustedByTableauException InvalidTicket(string ticket) {
            return new DomainNotTrustedByTableauException(string.Format("Tableau server returned an invalid trusted ticket '{0}'. " +
                                                                        "Make sure this domain is configured as trusted in the tableau server.", ticket));
        }
    }
}
