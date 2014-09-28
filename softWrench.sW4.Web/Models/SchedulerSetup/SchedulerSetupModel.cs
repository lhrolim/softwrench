
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace softWrench.sW4.Web.Models.SchedulerSetup {

    public class SchedulerSetupModel {
        private readonly string _schedulerSetupModelResponseJson;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Cron { get; set; }
        public bool Active { get; set; }
        public bool IsScheduled { get; set; }

        public SchedulerSetupModel(int id, string name, string description, string cron, bool active, bool isScheduled) {
            Id = id;
            Name = name;
            Description = description;
            Cron = cron;
            Active = active;
            IsScheduled = isScheduled;
        }

        public SchedulerSetupModel() {

        }

        public SchedulerSetupModel(List<SchedulerSetupModel> list) {
            _schedulerSetupModelResponseJson = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public override string ToString() {
            return string.Format("Id: {0}, Name: {1}, Description: {2}, Cron: {3}, Active: {4}, IsScheduled: {5}");
        }

        public string SchedulerSetupModelResponseJson {
            get { return _schedulerSetupModelResponseJson; }
        }
    }
}