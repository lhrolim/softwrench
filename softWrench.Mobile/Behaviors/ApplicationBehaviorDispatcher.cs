using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using softWrench.Mobile.Applications;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors {
    internal static class ApplicationBehaviorDispatcher {
        //TODO: keep cache size manageable?
        private static readonly ConcurrentDictionary<ApplicationSchemaDefinition, ApplicationBehavior> Behaviors = new ConcurrentDictionary<ApplicationSchemaDefinition, ApplicationBehavior>();
        private static readonly MetadataRepository MetadataRepository = MetadataRepository.GetInstance();

        private static ApplicationBehavior GetBehavior(ApplicationSchemaDefinition application) {
            return Behaviors.GetOrAdd(application, ResolveBehavior);
        }

        private static ApplicationBehavior ResolveBehavior(ApplicationSchemaDefinition application) {
            return ApplicationBehaviorResolver.Resolve(application);
        }

        public static void OnNew(DataMap dataMap, ApplicationSchemaDefinition application, CompositeDataMap composite) {
            var context = new OnNewContext(application, MetadataRepository, User.Current, composite);
            var behavior = GetBehavior(context.Application);

            behavior.OnNew(context, dataMap);
        }

        public static void OnLoad(DataMap dataMap, ApplicationSchemaDefinition application) {
            var context = new OnLoadContext(application, MetadataRepository, User.Current);
            var behavior = GetBehavior(context.Application);

            behavior.OnLoad(context, dataMap);
        }

        public static void OnBeforeSave(DataMap dataMap, ApplicationSchemaDefinition application) {
            var context = new OnBeforeSaveContext(application, MetadataRepository, User.Current);
            var behavior = GetBehavior(context.Application);

            behavior.OnBeforeSave(context, dataMap);
        }

        public static IEnumerable<IApplicationCommand> OnBeforeShow(DataMap dataMap, ApplicationSchemaDefinition application) {
            var context = new OnBeforeShowContext(application, MetadataRepository, User.Current);
            var behavior = GetBehavior(context.Application);

            behavior.OnBeforeShow(context, dataMap);
            return context.Commands;
        }

        public static IDictionary<string, object> OnBeforeUpload(DataMap dataMap, ApplicationSchemaDefinition application) {
            var context = new OnBeforeUploadContext(application, MetadataRepository, User.Current);
            var behavior = GetBehavior(context.Application);

            behavior.OnBeforeUpload(context, dataMap);
            return context.Content;
        }

        public static string PlatformSpecificProbingNamespace { get; set; }

        private static class ApplicationBehaviorResolver {
            private static ApplicationBehavior Instantiate(string @namespace, string applicationName, IEnumerable<IApplicationCommand> commands) {
                // We'll look for a type called `{ApplicationName}Behavior`
                // (e.g. `WorkorderBehavior`) in the target namespace.
                var fullName = string.Format("{0}.{1}Behavior", @namespace, applicationName);

                var type = Assembly
                    .GetExecutingAssembly()
                    .GetType(fullName, false, true);

                if (null == type) {
                    return null;
                }

                var constructor = type.GetConstructor(new[] { typeof(IEnumerable<IApplicationCommand>) });

                if (null == constructor) {
                    return null;
                }

                return (ApplicationBehavior)constructor
                    .Invoke(new object[] { commands });
            }

            public static ApplicationBehavior Resolve(ApplicationSchemaDefinition application) {
                var platformSpecificNamespace = string.Format("{0}.{1}", PlatformSpecificProbingNamespace, application.ApplicationName);
                var applicationSpecificNamespace = string.Format("{0}.{1}", typeof(NamespaceAnchor).Namespace, application.ApplicationName);

                // Let's probe for all custom commands
                // eligible for this application behavior.
                var commands = CommandResolver.Resolve(new[] { platformSpecificNamespace, applicationSpecificNamespace });

                // Do we have a behavior specific for this
                // application and platform? e.g. workorder
                // on iOS.
                // ReSharper disable once PossibleMultipleEnumeration
                var platformSpecificBehavior = Instantiate(platformSpecificNamespace, application.ApplicationName, commands);

                if (null != platformSpecificBehavior) {
                    return platformSpecificBehavior;
                }

                // Do we have a behavior specific for this
                // application, but platform-independent?
                // ReSharper disable once PossibleMultipleEnumeration
                var applicationSpecificBehavior = Instantiate(applicationSpecificNamespace, application.ApplicationName, commands);

                if (null != applicationSpecificBehavior) {
                    return applicationSpecificBehavior;
                }

                // Damn it, no luck. Let's return the
                // base application behavior then.
                // ReSharper disable once PossibleMultipleEnumeration
                return new ApplicationBehavior(commands);
            }
        }

        private static class CommandResolver {
            private static bool IsCommand(string targetNamespace, Type type) {
                var isInsideTargetNamespace = string.Equals(type.Namespace, targetNamespace, StringComparison.InvariantCultureIgnoreCase);

                if (false == isInsideTargetNamespace) {
                    return false;
                }

                var @interface = type.GetInterface(typeof(IApplicationCommand).Name);
                return null != @interface;
            }

            public static IEnumerable<IApplicationCommand> Resolve(IEnumerable<string> targetNamespaces) {
                var commands = new List<IApplicationCommand>();

                foreach (var targetNamespace in targetNamespaces) {
                    commands.AddRange(
                        Assembly
                            .GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => IsCommand(targetNamespace, t))
                            .Select(t => t.GetConstructor(new Type[0]))
                            .Select(c => (IApplicationCommand)c.Invoke(new object[0]))
                            .ToList());
                }

                return commands;
            }
        }
    }
}