using System;
using System.Collections.Generic;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.WS.Ism.Base {
    abstract class BaseISMTicketDecorator : BaseISMDecorator {

        private readonly ILog Log = LogManager.GetLogger(typeof(BaseISMTicketDecorator));

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            if (Log.IsDebugEnabled) {
                Log.Debug(jsonObject);
            }
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            PopulateServiceIncident(webServiceObject, jsonObject);
            PopulateServiceProviders(jsonObject, webServiceObject);
            PopulateMetrics(webServiceObject, jsonObject);
            PopulateProblem(jsonObject, webServiceObject, maximoTemplateData.OperationData.EntityMetadata.Name, false);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            PopulateServiceProviders(jsonObject, webServiceObject);
            PopulateProblem(jsonObject, webServiceObject, maximoTemplateData.OperationData.EntityMetadata.Name, true);
            webServiceObject.RequesterID = (string)jsonObject.GetAttribute("ticketid");
            HandleWorkLog(jsonObject, webServiceObject);
        }

        protected static void PopulateServiceIncident(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            webServiceObject.RequesterID = "";
            webServiceObject.WorkflowStatus = "QUEUED";
            webServiceObject.ProviderID = "";
            webServiceObject.ProviderPrioritySpecified = true;
            webServiceObject.CurrentState = "";
            webServiceObject.ProviderPriority = 4; //Need to find out what field maps to provider priority. Hard coded a priority for now.
        }

        protected virtual Metrics PopulateMetrics(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            if (webServiceObject.Metrics == null) {
                webServiceObject.Metrics = new Metrics();
            }
            var metrics = webServiceObject.Metrics;
            metrics.TicketOpenedDateTimeSpecified = true;
            metrics.ProblemOccurredDateTimeSpecified = true;
            metrics.TicketOpenedDateTime = DateTime.Now.FromServerToRightKind();
            metrics.ProblemOccurredDateTime = DateTime.Now.FromServerToRightKind();
            return metrics;
        }

        protected virtual string GetTemplateId(CrudOperationData jsonObject) {
            return jsonObject.GetAttribute("templateid") as string;
        }

        protected virtual Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject, string entityName, Boolean update) {
            if (webServiceObject.Problem == null) {
                webServiceObject.Problem = new Problem();
            }

            var problem = webServiceObject.Problem;
            problem.ProblemType = GetProblemType();
            problem.CustomerID = ISMConstants.DefaultCustomerName;

            HandleTitle(webServiceObject, jsonObject);
            HandleLongDescription(webServiceObject, jsonObject, jsonObject.ApplicationMetadata, update);

            problem.System = ISMConstants.System;

            problem.FlexFields = new[]
            {
                new FlexFieldsFlexField { mappedTo = "CHANGEBY", id = "0",Value = SecurityFacade.CurrentUser().MaximoPersonId},
                new FlexFieldsFlexField { mappedTo = "CHANGEDATE", id = "0",Value = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")},
                new FlexFieldsFlexField { mappedTo = "CINUM", id = "0",Value = jsonObject.GetAttribute("cinum") as string},
                new FlexFieldsFlexField { mappedTo = "TemplateID", id = "0",Value =  GetTemplateId(jsonObject)}
            };

            //PROVIDER ASSIGNED GROUP ELEMENT
            if (problem.ProviderAssignedGroup == null) {
                problem.ProviderAssignedGroup = new RequesterAssignedGroup {
                    Group = new Group1()
                };
            }
            var group = problem.ProviderAssignedGroup.Group;
            group.Address = new Address();
            var address = group.Address;
            address.OrganizationID = ISMConstants.OrganizationId;
            address.LocationID = ISMConstants.LocationId;

            var submittingAction = "true".Equals(jsonObject.GetUnMappedAttribute("#submittingaction"));
            if (update && !submittingAction) {
                if (problem.Abstract != null && !problem.Abstract.StartsWith("@@")) {
                    problem.Abstract = "@@" + problem.Abstract;
                }
            }

            return problem;
        }

        protected virtual void HandleTitle(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            webServiceObject.Problem.Abstract = ((String)jsonObject.GetAttribute("description"));
        }

        public override string ClientFilter() {
            return "hapag";
        }

        protected virtual string GetProblemType() {
            return "SR";
        }

        protected void HandleWorkLog(CrudOperationData entity, ServiceIncident maximoTicket) {
            var maximoWorklogs = entity.GetRelationship("worklog_");
            var worklogList = new List<Activity>();
            var user = SecurityFacade.CurrentUser();
            foreach (var jsonWorklog in (IEnumerable<CrudOperationData>)maximoWorklogs) {
                var worklogid = jsonWorklog.GetAttribute("worklogid");
                if (worklogid == null) {
                    var activity = LongDescriptionHandler.HandleLongDescription(new Activity(), jsonWorklog, "ActionLog");
                    activity.ActionLogSummary = (string)jsonWorklog.GetAttribute("description");
                    activity.type = "WorkLog";
                    activity.UserID = ISMConstants.AddEmailIfNeeded(user.MaximoPersonId);
                    //activity.LogDateTimeSpecified = true;
                    //activity.LogDateTime = DateTime.Now;
                    activity.ActivityType = "CLIENTNOTE";
                    worklogList.Add(activity);
                }
            }
            maximoTicket.Activity = ArrayUtil.PushRange(maximoTicket.Activity, worklogList);
        }

        protected virtual void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            var problem = webServiceObject.Problem;
            var ld = (CrudOperationData)entity.GetRelationship("longdescription");
            if (ld != null) {
                problem.Description = (string)ld.GetAttribute("ldtext");
            }
        }


        protected void PopulateServiceProviders(CrudOperationData entity, ServiceIncident webServiceObject) {
            var serviceProvider = new ServiceProvider[2];

            var reportedBy = new Person {
                Role = PersonRole.ReportedBy,
                RoleSpecified = true,
                ContactID = ISMConstants.AddEmailIfNeeded(SecurityFacade.CurrentUser().MaximoPersonId),
            };

            serviceProvider[0] = new ServiceProvider {
                Person = reportedBy
            };

            var affectedUserContact = ISMConstants.AddEmailIfNeeded(GetAffectedPerson(entity));
            if (affectedUserContact != null) {
                var affectedUser = new Person {
                    ContactID = affectedUserContact,
                    Role = PersonRole.AffectedUser,
                    RoleSpecified = true
                };

                serviceProvider[1] = new ServiceProvider {
                    Person = affectedUser
                };
            }

            webServiceObject.ServiceProvider = serviceProvider;

        }

        protected abstract string GetAffectedPerson(CrudOperationData entity);


    }
}
