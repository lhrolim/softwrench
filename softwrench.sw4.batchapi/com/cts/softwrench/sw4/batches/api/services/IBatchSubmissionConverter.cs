﻿using System;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.application;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services {
    public interface IBatchSubmissionConverter : IApplicationFiltereable,IComponent {

        JArray BreakIntoRows(JObject mainDatamap);
        
        Boolean ShouldSubmit(JObject row);

        /// <summary>
        /// Temporaraly returning Object, but it should be OperationWrapper instead, but that would cause an unresolved circular reference.
        /// 
        /// Need more modularization, and implementations need to make sure to return a CrudOperationData as for now
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        Object Convert(JObject row);
      
    }
}