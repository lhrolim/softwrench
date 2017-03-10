using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NHibernate.Util;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Util;

namespace softWrench.sW4.Dynamic.Services {
    public class ScriptsService : BaseScriptService {

        protected readonly SimpleInjectorGenericFactory SimpleInjectorGenericFactory;
        private readonly I18NResolver _i18NResolver;

        protected readonly Dictionary<DynComponentStatus, string> DefaultI18N = new Dictionary<DynComponentStatus, string>{
            { DynComponentStatus.Outdated, "Outdated"},
            { DynComponentStatus.Undeployed, "Undeployed"},
            { DynComponentStatus.UpToDate, "Up to Date"},
            { DynComponentStatus.WillBeDeployed, "Will be Deployed"},
            { DynComponentStatus.WillBeUndeployed, "Will be Undeployed"},
        };

        public ScriptsService(ISWDBHibernateDAO dao, SimpleInjectorGenericFactory simpleInjectorGenericFactory, I18NResolver i18NResolver) : base(dao)
        {
            SimpleInjectorGenericFactory = simpleInjectorGenericFactory;
            _i18NResolver = i18NResolver;
        }

        public IContainerReloader Reloader {
            get; set;
        }

        public virtual object EvaluateScript(string script) {
            if (string.IsNullOrEmpty(script)) {
                return null;
            }
            string newScript;
            var imports = ProcessImports(script, out newScript);
            var result = InnerParse(newScript, imports);
            return result;
        }

        public virtual IList<ScriptEntry> GetAllScripts() {
            return DAO.FindAll<ScriptEntry>(typeof(ScriptEntry));
        }

        [Transactional(DBType.Swdb)]
        public virtual void SaveScript(ScriptDTO dto) {
            var entry = new ScriptEntry {
                Id = dto.Id,
                Name = dto.Name,
                Target = dto.Target,
                Description = dto.Description,
                Script = dto.Script
            };
            DAO.Save(entry);
        }

        public virtual void ValidateScriptEntry(ScriptEntry entry, bool shouldBeOnContainer) {
            if (!SimpleInjectorGenericFactory.ContainsService(entry.Target)) {
                var msg = $"Target invalid. There is no component with name \"{entry.Target}\".";
                throw ExceptionUtil.InvalidOperation(msg);
            }

            ValidateSameTarget(entry, shouldBeOnContainer);

            var result = EvaluateScript(entry.Script);
            var type = result as Type;
            if (type == null) {
                throw ExceptionUtil.InvalidOperation("The script return is not a type.");
            }

            if (!typeof(IComponent).IsAssignableFrom(type)) {
                throw ExceptionUtil.InvalidOperation("The type of the script return is not a component sub-type.");
            }

            var originalType = SimpleInjectorGenericFactory.GetDynOriginalType(entry.Target);
            if (originalType == null) {
                var originalComponent = SimpleInjectorGenericFactory.GetObject<object>(entry.Target);
                originalType = originalComponent.GetType();
            }

            if (!originalType.IsAssignableFrom(type)) {
                throw ExceptionUtil.InvalidOperation("The type of the script return is not a sub-type of the target.");
            }
        }

        public override void ReloadContainer(AScriptEntry singleDynComponent) {
            Reloader.ReloadContainer((ScriptEntry) singleDynComponent);
        }

        [Transactional(DBType.Swdb)]
        public virtual void ContainerReloaded(ScriptEntry singleDynComponent) {
            var v = GetSystemVersion();
            if (singleDynComponent == null) {
                DAO.ExecuteSql("UPDATE DYN_SCRIPT_ENTRY SET Isoncontainer = 1 where Deploy = ? AND Appliestoversion = ?", true, v);
                DAO.ExecuteSql("UPDATE DYN_SCRIPT_ENTRY SET Isoncontainer = 0 where Deploy != ? OR Appliestoversion != ?", true, v);
                DAO.ExecuteSql("UPDATE DYN_SCRIPT_ENTRY SET Isuptodate = 1");
                return;
            }

            singleDynComponent.Isoncontainer = ShouldBeOnContainer(singleDynComponent);
            singleDynComponent.Isuptodate = true;
            DAO.Save(singleDynComponent);
        }


      

        public virtual bool ShouldBeOnContainer(ScriptEntry entry) {
            return ShouldBeOnContainer(entry.Deploy, entry.Appliestoversion);
        }

        public virtual bool ShouldBeOnContainer(bool deploy, string appliestoversion) {
            return deploy && GetSystemVersion().Equals(appliestoversion);
        }

        public virtual string GetStatus(bool shouldBeOnContainer, bool isOnContainer, bool isUpToDate) {
            var status = CalcStatus(shouldBeOnContainer, isOnContainer, isUpToDate);
            return _i18NResolver.I18NValue("dynamc.status." + status, DefaultI18N[status]);
        }

        protected virtual DynComponentStatus CalcStatus(bool shouldBeOnContainer, bool isOnContainer, bool isUpToDate) {
            if (!shouldBeOnContainer) {
                return isOnContainer ? DynComponentStatus.WillBeUndeployed : DynComponentStatus.Undeployed;
            }

            if (!isOnContainer) {
                return DynComponentStatus.WillBeDeployed;
            }
            return isUpToDate ? DynComponentStatus.UpToDate : DynComponentStatus.Outdated;
        }

        protected virtual void ValidateSameTarget(ScriptEntry entry, bool shouldBeOnContainer) {
            if (!shouldBeOnContainer) {
                return;
            }

            var entries = DAO.FindByQuery<ScriptEntry>(ScriptEntry.ScriptByTargetDeployVersion, entry.Target, true, entry.Appliestoversion);
            if (entries == null || entries.Count == 0) {
                return;
            }

            var msg =
                $"There is already a dynamic component with the target \"{entry.Target}\" marked to be on container.";
            if (entry.Id == null) {
                throw ExceptionUtil.InvalidOperation(msg);
            }
            entries.ForEach(e => {
                if (e.Id != entry.Id) {
                    throw ExceptionUtil.InvalidOperation(msg);
                }
            });
        }

        private static object InnerParse(string script, IEnumerable<string> imports) {
            var task = CSharpScript.EvaluateAsync(script, ScriptOptions.Default.WithReferences(AssemblyLocator.GetAssemblies()).WithImports(imports));
            task.Wait();
            return task.Result;
        }



        private IEnumerable<string> ProcessImports(string script, out string newScript) {
            var imports = new List<string>();
            foreach (Match match in IsImport.Matches(script)) {
                var import = match.Value;
                import = import.ReplaceFirstOccurrence("using", "");
                import = import.Trim();
                if (import.EndsWith(";")) {
                    import = import.Substring(0, import.Length - 1);
                }
                import = import.Trim();
                imports.Add(import);
            }
            newScript = IsImport.Replace(script, "");
            return imports;
        }

        public class ScriptDTO {
            public int? Id {
                get; set;
            }
            public string Name {
                get; set;
            }
            public string Target {
                get; set;
            }
            public string Description {
                get; set;
            }
            public string Script {
                get; set;
            }
        }

        public class ReloadContainerDTO {
            public string Comment {
                get; set;
            }
            public string Username {
                get; set;
            }
        }
    }
}
