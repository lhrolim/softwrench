using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Extensions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.UI.Binding
{
    internal static class UiBinder
    {
        private static IList<FieldBinding> BindFields(DataMap dataMap, ApplicationSchemaDefinition application, Func<ApplicationFieldDefinition, IValueProvider> valueProviderFactory)
        {
            var bindings = application
                .Fields
                .Select(f => new FieldBinding(f, valueProviderFactory(f)))
                .ToList();

            foreach (var binding in bindings)
            {
                try
                {
                    var value = dataMap.Value(binding.Metadata);
                    // Formats the raw value to its
                    // appropriate representation;
                    var formattedValue = binding
                        .Metadata
                        .Widget()
                        .Format(value);

                    binding.ValueProvider.Value = formattedValue;
                }
                catch
                {
                    Console.Write(binding.Metadata.Attribute);
                }

           
            }

            return bindings;
        }

        private static IList<CommandBinding> BindCommands(IEnumerable<IApplicationCommand> commands)
        {
            return commands
                .Select(c => new CommandBinding(c))
                .ToList();
        }

        public static FormBinding Bind(DataMap dataMap, ApplicationSchemaDefinition application, bool isNew, Func<ApplicationFieldDefinition, IValueProvider> valueProviderFactory)
        {
            var registeredCommands = ApplicationBehaviorDispatcher
                .OnBeforeShow(dataMap, application);
            
            var fields = BindFields(dataMap, application, valueProviderFactory);
            var commands = BindCommands(registeredCommands);

            return new FormBinding(dataMap, fields, commands, isNew);
        }
    }
}
