﻿using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Scheduler;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers.Routing {
    public class SuccessMessageHandler : IComponent {
        private static readonly I18NResolver Resolver = new I18NResolver();
        public String FillSucessMessage(ApplicationMetadata applicationMetadata, string id, string operation) {
            string successMessage = null;
            var applicationCommand = ApplicationCommandUtils.GetApplicationCommand(applicationMetadata, operation);
            if (applicationCommand != null) {
                successMessage = applicationCommand.SuccessMessage;
            } else {
                //TODO: refactor this
                var baseMessage = applicationMetadata.Title + (id == null ? " " : " " + id + " ");
                switch (operation) {
                    case OperationConstants.CRUD_CREATE:
                        return baseMessage + Resolver.I18NValue("messagesection.success.created", "successfully created");
                    case OperationConstants.CRUD_UPDATE:
                        return baseMessage + Resolver.I18NValue("messagesection.success.updated", "successfully updated");
                    case OperationConstants.CRUD_DELETE:
                        return baseMessage + Resolver.I18NValue("messagesection.success.deleted", "successfully deleted");
                    default:
                        return Resolver.I18NValue("messagesection.success." + operation.ToLower(), "Operation Executed successfully",null, new object[] { id });
                }
            }
            return successMessage;
        }

        public static String FillSuccessMessage(JobCommandEnum jobCommand) {
            var successMessage = "Job ";
            switch (jobCommand) {
                case JobCommandEnum.Execute:
                    successMessage += Resolver.I18NValue("messagesection.success.job.executed", "successfully executed");
                    break;
                case JobCommandEnum.Schedule:
                    successMessage += Resolver.I18NValue("messagesection.success.job.scheduled", "successfully scheduled");
                    break;
                case JobCommandEnum.Pause:
                    successMessage += Resolver.I18NValue("messagesection.success.job.paused", "successfully paused");
                    break;
                case JobCommandEnum.ChangeCron:
                    successMessage += Resolver.I18NValue("messagesection.success.job.changecron", "cron successfully changed");
                    break;
            }
            return successMessage;
        }
    }
}
