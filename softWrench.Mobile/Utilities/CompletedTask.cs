using System.Threading.Tasks;

namespace softWrench.Mobile.Utilities
{
    internal static class CompletedTask
    {
        private static Task<T> CreateCompletedTask<T>(T result)
        {
            var source = new TaskCompletionSource<T>();
                                                
            source.SetResult(result);
            return source.Task;
        }

        public static Task<T> Of<T>()
        {
            return CreateCompletedTask(default(T));
        }

        public static Task<T> Of<T>(T result)
        {
            return CreateCompletedTask(result);
        }

        public static Task Instance
        {
            get
            {
                return CreateCompletedTask(default(bool));
            }
        }
    }
}
