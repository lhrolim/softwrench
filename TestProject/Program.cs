using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using softwrench.sW4.audit.classes.Model;

namespace TestProject {
    class Program {
        static void Main(string[] args) {
            var trail = new AuditTrail();
            var queries = new List<AuditQuery>();
            for (int i = 0;i < 100;i++) {
                queries.Add(new AuditQuery());
            }

            for (int i = 0;i < 100;i++) {
                var i1 = i;
                Task.Run(() => {
                    trail.Queries.Add(new AuditQuery());
                    Console.WriteLine("test " + i1);
                });
            }
            Console.ReadKey();



        }
    }
}
