using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;

namespace softwrench.sW4.test.Util {
    public class TestUtil {
        public static IEnumerable<string> ClientNames() {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory + "\\Client\\";
            var clients = Directory.GetDirectories(baseDir);
            IList<string> clientNames = new List<string>();
            foreach (var client in clients) {
                clientNames.Add(client.Replace(baseDir, ""));
            }
            return new List<string>(clientNames).Where(c => !c.Contains("@internal"));
        }

        public static void ResetMocks(params Mock[] mocks) {
            if (mocks == null) {
                return;
            }
            foreach (var mock in mocks) {
                if (mock != null) {
                    mock.Reset();
                }
            }
        }

        public static Mock<T> CreateMock<T>(bool useStrict=false) where T : class {
            var mockType = typeof(T);
            var behavior = useStrict ? MockBehavior.Strict : MockBehavior.Default;

            if (mockType.IsInterface || mockType.IsAbstract) {
                //parameterless
                return new Mock<T>(behavior);
            }

            var constructors = mockType.GetConstructors();
            var constructor = mockType.GetConstructor(Type.EmptyTypes);
            if (constructor != null) {
                //parameterless
                return new Mock<T>(behavior);
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


        public static void VerifyMocks(params Mock[] mocks) {
            if (mocks == null) {
                return;
            }
            foreach (var mock in mocks) {
                mock.VerifyAll();
            }
        }


    }
}
