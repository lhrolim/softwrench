using System;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Dynamic {
    public class ScriptExempleStartup : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        private const string Script = @"// The imports are provided on the beginning of the script.
using softWrench.sW4.Dynamic;

// No namespace definition is needed only
// the dyn component class, in this example
// it extends the original component.
// Note also the target for this example is 'scriptExample'
// is the name of original component 
// (or the name of the type with the first letter lowercased)

// To run this example on the eval script page:
// - eval the expression: softWrench.sW4.Dynamic.ScriptExample.GetValueForEvalPage();
// - this should give the result: 2;
// - then deploy this dynamic component;
// - eval the same expression again;
// - the result now should be: 4

// ps.: Remember that to be deployed a dynamic component
// needs to be marked to be deployed and it's version
// needs to be the same of the application (ignoring the snapshot sufix).

public class DynScriptExample : ScriptExample {

    // if the original component does not have a parameterless constructor
    // a constructor with at least the same parameters has to be created
    public DynScriptExample(ScriptExampleCalculator calculator) : base(calculator) {
    }

    // the original method is overriden, but to do so it has to be virtual
    public override int Calculate() {
        return Calculator.Sum(2, 2);
    }
}

// the type of the class created is provided as the script return
return typeof(DynScriptExample);";

        private readonly ISWDBHibernateDAO _dao;
        private readonly ScriptsService _service;

        private const string Name = "DynScriptExample";
        private const string Target = "scriptExample";

        public ScriptExempleStartup(ISWDBHibernateDAO dao, ScriptsService service) {
            _dao = dao;
            _service = service;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var entry = _dao.FindSingleByQuery<ScriptEntry>(ScriptEntry.ScriptByName, Name);
            entry = entry ?? new ScriptEntry();
            ResetExampleEntry(entry);
            _dao.Save(entry);
        }

        private void ResetExampleEntry(ScriptEntry entry) {
            entry.Name = Name;
            entry.Target = Target;
            entry.Script = Script;
            entry.Deploy = false;
            entry.Lastupdate = DateTime.Now;
            entry.Isoncontainer = false;
            entry.Isuptodate = true;
            entry.Appliestoversion = _service.GetSystemVersion();
        }
    }
}
