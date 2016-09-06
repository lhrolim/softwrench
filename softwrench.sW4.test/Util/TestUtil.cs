using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;

namespace softwrench.sW4.test.Util {
    class TestUtil {
        internal static IEnumerable<string> ClientNames() {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory + "\\Client\\";
            var clients = Directory.GetDirectories(baseDir);
            IList<string> clientNames = new List<string>();
            foreach (var client in clients) {
                clientNames.Add(client.Replace(baseDir, ""));
            }
            return new List<string>(clientNames).Where(c => !c.Contains("@internal"));
        }

        internal static void ResetMocks(params Mock[] mocks) {
            if (mocks == null) {
                return;
            }
            foreach (var mock in mocks) {
                mock.Reset();
            }
        }

        public static Mock<T> CreateMock<T>() where T : class {
            var mockType = typeof(T);
            if (mockType.IsInterface || mockType.IsAbstract) {
                //parameterless
                return new Mock<T>();
            }

            var constructors = mockType.GetConstructors();
            var constructor = mockType.GetConstructor(Type.EmptyTypes);
            if (constructor != null) {
                //parameterless
                return new Mock<T>();
            }
            var firstConstructor = constructors[0];
            var parameterInfos = firstConstructor.GetParameters();
            var numberOfParameters = parameterInfos.Length;
            var parameters = new object[numberOfParameters];
            for (int i = 0; i < parameterInfos.Length; i++) {
                parameters[i++] = null;
            }
            return new Mock<T>(parameters.Cast<object>().ToArray());
        }


        internal static void VerifyMocks(params Mock[] mocks) {
            if (mocks == null) {
                return;
            }
            foreach (var mock in mocks) {
                mock.VerifyAll();
            }
        }


    }
}
