using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Entities;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SimpleInjector;

namespace softwrench.sw4.Hapag.Security {
    public interface IHlagLocationManager : ISingletonComponent {
        IEnumerable<HlagGroupedLocation> FindLocationsOfParentLocation(PersonGroup group);
        /// <summary>
        /// Returns a list of all the available locations of the current logged user, taking in consideration whether we are inside XTIC FR 
        /// (in this case returning just area or groups).
        /// 
        /// This is a cached check (built upon login) not hitting database everytime
        /// </summary>
        /// <returns></returns>
        IEnumerable<IHlagLocation> FindAllLocationsOfCurrentUser(ApplicationMetadata application);

        IEnumerable<HlagGroupedLocation> FindAllLocations();
        IEnumerable<IAssociationOption> FindDefaultITCsOfLocation(string subcustomer);

        IEnumerable<IAssociationOption> FindCostCentersOfITC(string subCustomer, string personId = null);

        [NotNull]
        HlagGroupedLocation[] GetLocationsOfLoggedUser(Boolean forceXITCContext=false);

        [NotNull]
        HlagGroupedLocation[] GetLocationsOfUser(InMemoryUser user,Boolean forceXITCContext = false);


    }
}