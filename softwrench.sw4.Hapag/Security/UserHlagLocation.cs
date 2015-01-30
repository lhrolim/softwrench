using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace softwrench.sw4.Hapag.Security {
    public class UserHlagLocation {

        /// <summary>
        /// This represents all the non-grouped locations no matter if they are direct linked. Used in My profile screen
        /// </summary>
        [NotNull]
        public ISet<HlagLocation> Locations { get; set; }


        /// <summary>
        /// This represents only the grouped (i.e costcenters grouped by customer) locations direct linked to the user (XXX-LC groups)
        /// </summary>
        [NotNull]
        public ISet<HlagGroupedLocation> DirectGroupedLocations{ get; set; }

        /// <summary>
        /// This represents only the grouped (i.e costcenters grouped by customer) locations direct linked to the user (XXX-LC groups)
        /// </summary>
        [NotNull]
        public ISet<HlagGroupedLocationsNoPrefixDecorator> DirectGroupedLocationsNoPrefix { get; set; }

        /// <summary>
        /// This represents only the grouped (i.e costcenters grouped by customer) locations provenient by Region/Area groups
        /// </summary>
        [NotNull]
        public ISet<HlagGroupedLocation> GroupedLocationsFromParent { get; set; }

        /// <summary>
        /// This represents all the grouped (i.e costcenters grouped by customer) locations of the user,no matter if they are direct linked, or bounded via a Region/Area location.
        /// </summary>
        public IEnumerable<HlagGroupedLocation> GroupedLocations { get; set; }

        public override string ToString() {
            return string.Format("Locations: {0}, GroupedLocations: {1}", Locations, DirectGroupedLocations);
        }
    }
}
