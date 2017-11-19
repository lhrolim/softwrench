using System;

namespace softwrench.sw4.problem.classes.api {
    /// <summary>
    /// marker interface for storing data at the problem table.
    /// Implementors must know how to serialize the data correctly
    /// </summary>
    public interface IProblemData {
        string Serialize();
    }
}