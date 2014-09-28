using System;
using System.Net;
using MonoTouch.CoreFoundation;
using MonoTouch.SystemConfiguration;
using softWrench.Mobile.Metadata;

namespace softWrench.iOS.Utilities
{    
    internal static partial class NetworkStatus
    {
        public enum NetworkReachabilityResult
        {
            NotReachable,
            ReachableViaCarrierDataNetwork,
            ReachableViaWiFiNetwork
        }

        private static class NetworkReachability
        {
            public static string HostName
            {
                get
                {
                    return User
                        .Settings
                        .ServerAsUri()
                        .Host;
                }
            }

            public static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
            {
                // Is it reachable with the current network configuration?
                bool isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

                // Do we need a connection to reach it?
                bool noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

                // Since the network stack will automatically try to get the WAN up,
                // probe that
                if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    noConnectionRequired = true;

                return isReachable && noConnectionRequired;
            }

            // Is the host reachable with the current network configuration
            public static bool IsHostReachable(string host)
            {
                if (host == null || host.Length == 0)
                    return false;

                using (var r = new MonoTouch.SystemConfiguration.NetworkReachability(host))
                {
                    NetworkReachabilityFlags flags;

                    if (r.TryGetFlags(out flags))
                    {
                        return IsReachableWithoutRequiringConnection(flags);
                    }
                }
                return false;
            }
                        
            // Is the host reachable with the current network configuration
            public static bool IsHostReachable()
            {
                return IsHostReachable(HostName);
            }

            // 
            // Raised every time there is an interesting reachable event, 
            // we do not even pass the info as to what changed, and 
            // we lump all three status we probe into one
            //
            public static event EventHandler ReachabilityChanged;

            private static void OnChange(NetworkReachabilityFlags flags)
            {
                var h = ReachabilityChanged;
                if (h != null)
                    h(null, EventArgs.Empty);
            }

            //
            // Returns true if it is possible to reach the AdHoc WiFi network
            // and optionally provides extra network reachability flags as the
            // out parameter
            //
            private static MonoTouch.SystemConfiguration.NetworkReachability adHocWiFiNetworkReachability;

            public static bool IsAdHocWiFiNetworkAvailable(out NetworkReachabilityFlags flags)
            {
                if (adHocWiFiNetworkReachability == null)
                {
                    adHocWiFiNetworkReachability =
                        new MonoTouch.SystemConfiguration.NetworkReachability(new IPAddress(new byte[] {169, 254, 0, 0}));
                    adHocWiFiNetworkReachability.SetCallback(OnChange);
                    adHocWiFiNetworkReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
                }

                if (!adHocWiFiNetworkReachability.TryGetFlags(out flags))
                    return false;

                return IsReachableWithoutRequiringConnection(flags);
            }

            private static MonoTouch.SystemConfiguration.NetworkReachability defaultRouteReachability;

            private static bool IsNetworkAvailable(out NetworkReachabilityFlags flags)
            {
                if (defaultRouteReachability == null)
                {
                    defaultRouteReachability = new MonoTouch.SystemConfiguration.NetworkReachability(new IPAddress(0));
                    defaultRouteReachability.SetCallback(OnChange);
                    defaultRouteReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
                }
                if (!defaultRouteReachability.TryGetFlags(out flags))
                    return false;
                return IsReachableWithoutRequiringConnection(flags);
            }

            private static MonoTouch.SystemConfiguration.NetworkReachability remoteHostReachability;

            public static NetworkReachabilityResult RemoteHostStatus()
            {
                NetworkReachabilityFlags flags;
                bool reachable;

                if (remoteHostReachability == null)
                {
                    remoteHostReachability = new MonoTouch.SystemConfiguration.NetworkReachability(HostName);

                    // Need to probe before we queue, or we wont get any meaningful values
                    // this only happens when you create NetworkReachability from a hostname
                    reachable = remoteHostReachability.TryGetFlags(out flags);

                    remoteHostReachability.SetCallback(OnChange);
                    remoteHostReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
                }
                else
                    reachable = remoteHostReachability.TryGetFlags(out flags);

                if (!reachable)
                    return NetworkReachabilityResult.NotReachable;

                if (!IsReachableWithoutRequiringConnection(flags))
                    return NetworkReachabilityResult.NotReachable;

                if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    return NetworkReachabilityResult.ReachableViaCarrierDataNetwork;

                return NetworkReachabilityResult.ReachableViaWiFiNetwork;
            }

            public static NetworkReachabilityResult InternetConnectionStatus()
            {
                NetworkReachabilityFlags flags;
                bool defaultNetworkAvailable = IsNetworkAvailable(out flags);
                if (defaultNetworkAvailable)
                {
                    if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                        return NetworkReachabilityResult.NotReachable;
                }
                else if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    return NetworkReachabilityResult.ReachableViaCarrierDataNetwork;
                else if (flags == 0)
                    return NetworkReachabilityResult.NotReachable;
                return NetworkReachabilityResult.ReachableViaWiFiNetwork;
            }

            public static NetworkReachabilityResult LocalWifiConnectionStatus()
            {
                NetworkReachabilityFlags flags;
                if (IsAdHocWiFiNetworkAvailable(out flags))
                {
                    if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                        return NetworkReachabilityResult.ReachableViaWiFiNetwork;
                }
                return NetworkReachabilityResult.NotReachable;
            }
        }
    }
}

