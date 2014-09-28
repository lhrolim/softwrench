using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    /// <summary>
    ///     An extension point where each application can customize
    ///     its behavior on key points of the application lifecycle.
    /// </summary>
    internal class ApplicationBehavior
    {
        /// <summary>
        ///     All custom commands offered by this application behavior.
        /// </summary>
        private readonly IEnumerable<IApplicationCommand> _commands;

        /// <summary>
        ///     Sets the value of the specified attribute if it is
        ///     declared on the application metadata. Otherwise, no
        ///     operation is performed.
        /// </summary>
        /// <param name="application">The application metadata where the attribute must be declared.</param>
        /// <param name="dataMap">The data map target of the operation.</param>
        /// <param name="attribute">The attribute to set.</param>
        /// <param name="value">The value to set, if the field exists.</param>
        private static void SetFieldIfExists(ApplicationSchemaDefinition application, DataMap dataMap, string attribute, string value)
        {
            var field = application
                .Fields
                .FirstOrDefault(f => f.Attribute == attribute);

            if (null != field)
            {
                dataMap.Value(field, value);
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ApplicationBehavior"/>
        ///     class that supports the specified list of application commands.
        /// </summary>
        /// <param name="commands">The list of commands supported by the application.</param>
        public ApplicationBehavior(IEnumerable<IApplicationCommand> commands)
        {
            if (commands == null) throw new ArgumentNullException("commands");

            _commands = commands;
        }

        /// <summary>
        ///     Invoked when a brand-new data map is created by the user.
        ///     This method can be used to initialize default values or
        ///     perform any related tasks.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="dataMap">The empty data map to hold entity data.</param>
        public virtual void OnNew(OnNewContext context, DataMap dataMap)
        {
            // Inspects the metadata looking for SITEID and
            // ORGID. If they exist, initialize them with
            // the ORGID and SITEID of the current user.
            SetFieldIfExists(context.Application, dataMap, "orgid", context.User.OrgId);
            SetFieldIfExists(context.Application, dataMap, "siteid", context.User.SiteId);
        }

        /// <summary>
        ///     Invoked when a data map is loaded from the repository.
        ///     This method can be used to tweak data to be displayed
        ///     by the UI or perform any related tasks.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="dataMap">The data map that was just loaded from the repository.</param>
        public virtual void OnLoad(OnLoadContext context, DataMap dataMap)
        {
        }

        /// <summary>
        ///     Invoked just before a data map is stored in the repository.
        ///     This method can be used to process data entered by the user
        ///     or perform any related tasks.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="dataMap">The data map that is about to be stored in the repository.</param>
        public virtual void OnBeforeSave(OnBeforeSaveContext context, DataMap dataMap)
        {
        }

        /// <summary>
        ///     Invoked just before a data map is binded and displayed by
        ///     an UI. This method can be used to evaluate which commands
        ///     are available or perform any related tasks.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="dataMap">The data map that is about to be displayed by the UI.</param>
        public virtual void OnBeforeShow(OnBeforeShowContext context, DataMap dataMap)
        {
            // Let's iterate through the list of all commands
            // giving them opportunity to indicate they are
            // available to be interacted with by the user.
            foreach (var command in _commands)
            {
                command.Register(context, dataMap);
            }
        }

        /// <summary>
        ///     Invoked just before a data map is uploaded to the upstream
        ///     server. This method can be used to change data "in-flight"
        ///     before it's sent to the server or perform any related tasks.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="dataMap">The data map that is about to be uploaded to the server.</param>
        public virtual void OnBeforeUpload(OnBeforeUploadContext context, DataMap dataMap)
        {
            // The server expect a flattened list of fields, i.e.
            // they should not be wrapped inside a data map object.
            var flattened = new Dictionary<string, object>(dataMap
                .Fields
                .ToDictionary(k => k.Key, v => (object) v.Value));

            context.Content = flattened;
        }
    }
}