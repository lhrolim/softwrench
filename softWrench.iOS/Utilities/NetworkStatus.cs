using System;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS.Utilities
{
    internal static partial class NetworkStatus
    {
        public static bool IsReachable()
        {
            try
            {
                if (NetworkReachabilityResult.NotReachable == NetworkReachability.InternetConnectionStatus())
                {
                    return false;
                }

                if (false == NetworkReachability.IsHostReachable())
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool StartIfReachable(this Task task, Action runIfNotReachable)
        {
            if (false == IsReachable())
            {
                runIfNotReachable();
                return false;
            }

            task.Start();
            return true;
        }

        public static void StartIfReachable(this Task task)
        {
            StartIfReachable(task, () => { });
        }

        public static Task StartIfReachable(this Func<Task> taskStarter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return IsReachable()
                ? taskStarter()
                : CompletedTask.Instance;
        }

        public static Task<T> StartIfReachable<T>(this Func<Task<T>> taskStarter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return IsReachable()
                ? taskStarter()
                : CompletedTask.Of<T>();
        }
    }
}

