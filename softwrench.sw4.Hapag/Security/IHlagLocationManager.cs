using System.Collections.Generic;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Data.Association;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.Hapag.Security
{
    public interface IHlagLocationManager :ISingletonComponent
    {
        IEnumerable<HlagGroupedLocation> FindLocationsOfParentLocation(PersonGroup group);
        /// <summary>
        /// Returns a list of all the available locations of the current logged user, taking in consideration whether we are inside XTIC FR 
        /// (in this case returning just area or groups).
        /// 
        /// This is a cached check (built upon login) not hitting database everytime
        /// </summary>
        /// <returns></returns>
        IEnumerable<HlagGroupedLocation> FindAllLocationsOfCurrentUser();

        IEnumerable<HlagGroupedLocation> FindAllLocations();
        Task<IEnumerable<IAssociationOption>> FindDefaultITCsOfLocation(string subcustomer);

        Task<IEnumerable<IAssociationOption>> FindCostCentersOfITC(string subCustomer, string personId);
    }
}