﻿using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace softWrench.sW4.Util.Scripting {
    class ScriptingUtil {

        static Assembly CompileCode(string code) {
            // Create a code provider
            // This class implements the 'CodeDomProvider' class as its base. All of the current .Net languages (at least Microsoft ones)
            // come with thier own implemtation, thus you can allow the user to use the language of thier choice (though i recommend that
            // you don't allow the use of c++, which is too volatile for scripting use - memory leaks anyone?)
            Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider();

            // Setup our options
            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false; // we want a Dll (or "Class Library" as its called in .Net)
            options.GenerateInMemory = true; // Saves us from deleting the Dll when we are done with it, though you could set this to false and save start-up time by next time by not having to re-compile
            // And set any others you want, there a quite a few, take some time to look through them all and decide which fit your application best!

            // Add any references you want the users to be able to access, be warned that giving them access to some classes can allow
            // harmful code to be written and executed. I recommend that you write your own Class library that is the only reference it allows
            // thus they can only do the things you want them to.
            // (though things like "System.Xml.dll" can be useful, just need to provide a way users can read a file to pass in to it)
            // Just to avoid bloatin this example to much, we will just add THIS program to its references, that way we don't need another
            // project to store the interfaces that both this class and the other uses. Just remember, this will expose ALL public classes to
            // the "script"
            options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            // Compile our code
            CompilerResults result;
            result = csProvider.CompileAssemblyFromSource(options, code);

            if (result.Errors.HasErrors) {
                // TODO: report back to the user that the script has errored
                return null;
            }

            if (result.Errors.HasWarnings) {
                // TODO: tell the user about the warnings, might want to prompt them if they want to continue
                // runnning the "script"
            }

            return result.CompiledAssembly;
        }

        public static object RunScript(String code, string methodName, object[] parameters) {
            var script = CompileCode(code);
            // Now that we have a compiled script, lets run them
            foreach (Type type in script.GetExportedTypes()) {
                ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                if (constructor == null) {
                    throw new InvalidOperationException("script must have a default constructor");
                }
                var obj = constructor.Invoke(null);
                return ReflectionUtil.Invoke(obj, methodName, parameters);
            }
            throw new InvalidOperationException();
        }

    }
}
