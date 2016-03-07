﻿using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch.customizations;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch {
    public class FirstSolarBaseLocationFinder : ISingletonComponent {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarBaseLocationFinder));
        private const string BaseLocationQuery = "SELECT location, description FROM locations WHERE {0} ORDER BY description ASC";
        private const string BaseLocationClause = "(location LIKE ? AND LEN(location) - LEN(REPLACE(location, '-', '')) = ?)";

        private readonly IMaximoHibernateDAO _maximoHibernateDao;
        private readonly CustomizationContainer _customizationContainer;

        public FirstSolarBaseLocationFinder(IMaximoHibernateDAO maximoHibernateDao) {
            _maximoHibernateDao = maximoHibernateDao;
            _customizationContainer = new CustomizationContainer();
        }

        private void AddLocationClause(ICollection<string> clauses, ICollection<object> parameters, List<string> facilities, string baseLike) {
            facilities.ForEach(f => AddLocationClause(clauses, parameters, f, baseLike));
        }

        private void AddLocationClause(ICollection<string> clauses, ICollection<object> parameters, string facility, string baseLike) {
            var customization = _customizationContainer.GetCustomization(facility);
            var parameter = customization != null ?
                customization.BuildLikeParameter(facility, baseLike) :
                facility + baseLike;
            parameters.Add(parameter);
            parameters.Add(parameter.Count(c => c.Equals('-')));
            clauses.Add(BaseLocationClause);
        }

        /// <summary>
        /// Find locations combining facilities and a list of like clauses without the beginning facility.
        /// </summary>
        /// <param name="facilities"></param>
        /// <param name="baseLikes"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> FindBaseLocations(List<string> facilities, List<string> baseLikes) {
            Log.Debug("Searching for locations...");
            if (facilities == null || !facilities.Any() || baseLikes == null || !baseLikes.Any()) {
                Log.Debug("No facilities or like clauses.");
                return new List<Dictionary<string, string>>();
            }

            var clauses = new List<string>();
            var parameters = new List<object>();
            baseLikes.ForEach(bl => AddLocationClause(clauses, parameters, facilities, bl));
            var baseLocationQuery = string.Format(BaseLocationQuery, string.Join(" OR ", clauses));
            Log.Debug(string.Format("Clauses to search: {0}", string.Join(", ", parameters)));
            return _maximoHibernateDao.FindByNativeQuery(baseLocationQuery, parameters.ToArray());
        }
    }
}