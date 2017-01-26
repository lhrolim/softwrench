// ReSharper disable InconsistentNaming
namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest
{
    public enum MaxSrStatus
    {
        CANCELLED, APPFM, APPLM, APPR, COMP, CLOSED, DRAFT, HISTEDIT, INPROG, NEW, PENDING, QUEUED, REJECTED, RESOLVCONF,
        RESOLVED, SLAHOLD,
        DUPLICATE,SPAM,
        //used for simplifying the LINQ logic for a non-existent status
        FAKE
        
    }
}