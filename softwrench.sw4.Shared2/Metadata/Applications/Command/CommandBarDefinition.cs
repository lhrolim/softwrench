﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata.Exception;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Command;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class CommandBarDefinition : BaseDefinition {
        public string Id {
            get; set;
        }

        public string Position {
            get; set;
        }

        public bool? Dynamic {
            get; set;
        }

        public bool ExcludeUndeclared {
            get; set;
        }
        [NotNull]
        public List<ICommandDisplayable> Commands {
            get; set;
        }

        /// <summary>
        /// Holds the list of commands originally declared before any customization has happened
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public ISet<string> OriginalCommandIds {
            get; set;
        }

        [JsonIgnore]
        [NotNull]
        public IEnumerable<ICommandDisplayable> CustomizedCommands {
            get {
                return Commands.Where(c => !OriginalCommandIds.Contains(c.Id));
            }
        }

        public ClientPlatform? Platform {
            get; set;
        }

        public ApplicationCommand PrimaryCommand {
            get { return Commands.OfType<ApplicationCommand>().FirstOrDefault(f => f.Primary.HasValue && f.Primary.Value); }
        }

        public CommandBarDefinition(string id, string position, Boolean excludeUndeclared, IEnumerable<ICommandDisplayable> commands) {
            Id = id;
            Position = position;
            Commands = new List<ICommandDisplayable>(commands);
            ExcludeUndeclared = excludeUndeclared;
            if (Id == null && Position == null) {
                throw new InvalidOperationException("Command must declare either Id or a Stereotype");
            }
            if (Id == null) {
                //the id of the command will be a hash followed by the position value, so that we can refer to it later by id
                Id = "#" + Position;
            }
            var count = commands.OfType<ApplicationCommand>().Count(a => a.Primary.HasValue && a.Primary.Value);
            if (count > 1) {
                //TODO: throw exception, but need to check eventual showexpressions
                LogManager.GetLogger(typeof(CommandBarDefinition)).Warn("only one primary command should be declared");
            }

        }

        public bool IsDynamic() {
            if (Dynamic != null) {
                //cache
                return Dynamic.Value;
            }
            var result = false;
            foreach (var command in Commands) {
                if (command is ContainerCommand) {
                    result = result || ((ContainerCommand)command).IsDynamic();
                }
                if (!string.IsNullOrEmpty(command.Role) || !string.IsNullOrEmpty(command.PermissionExpression)) {
                    Dynamic = true;
                    return true;
                }

            }
            Dynamic = result;
            return result;
        }

        public ICommandDisplayable FindById(string id) {
            foreach (var command in Commands) {
                if (command is ContainerCommand) {
                    var result = ((ContainerCommand)command).FindById(id);
                    if (result != null) {
                        return result;
                    }
                } else if (command.Id == id) {
                    return command;
                }
            }
            return null;
        }

        public override string ToString() {
            return string.Format("Id: {0} Number of Commands: {1}", Id, Commands.Count);
        }
    }
}
