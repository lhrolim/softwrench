using System.Collections.Generic;
using softwrench.sw4.api.classes.integration;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller {
    public class BatchResultData {

        public List<BatchItemProblem> ItemsWithProblems { get; set; } = new List<BatchItemProblem>();
        public List<MaximoIdWrapper> SuccessfulCreatedItems { get; set; } = new List<MaximoIdWrapper>();
        public List<MaximoIdWrapper> SuccessfulUpdatedItems { get; set; } = new List<MaximoIdWrapper>();


    }
}
