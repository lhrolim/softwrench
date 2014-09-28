using System;
using System.Threading;
using System.Threading.Tasks;
using MonoTouch.UIKit;

namespace softWrench.iOS.Utilities
{
    public class AsyncSearch<T> : IDisposable
    {
        private static string NormalizeSearchText(string searchText)
        {
            return (searchText ?? "").Trim();
        }

        private static ActivityIndicator StartActivityIndicator(UIView view)
        {
            return ActivityIndicator.Start(view);
        }

        private static void StopActivityIndicator(ActivityIndicator activityIndicator)
        {
            activityIndicator.Dispose();
        }

        private readonly UIView _view;
        private readonly AsyncSearchOptions<T> _options;
        private readonly EventWaitHandle _waitHandler;

        private string _searchText;
        private int _token;

        public AsyncSearch(UIView frameForSpinner, AsyncSearchOptions<T> options)
        {
            _view = frameForSpinner;
            _waitHandler = new AutoResetEvent(false);
            _options = options;
        }

        private void ReloadDataOnMainThread(T result, ActivityIndicator activityIndicator)
        {
            UIApplication
                .SharedApplication
                .InvokeOnMainThread(() =>
                                        {
                        StopActivityIndicator(activityIndicator);
                        _options.Reload(result);
                    });
        }

        private async void Search()
        {
            ActivityIndicator activityIndicator = null;

            UIApplication
                .SharedApplication
                .InvokeOnMainThread(() => activityIndicator = StartActivityIndicator(_view));

            var result = await _options.Search(_searchText);
            ReloadDataOnMainThread(result, activityIndicator);
        }

        private void ScheduleSearch(string searchText, bool immediate = false)
        {
            // Let's generate a unique ticket for this call.
            var myToken = Interlocked.Increment(ref _token);

            _searchText = searchText;

            Task.Run(() =>
            {
                // If we were requested to perform a search
                // right now (immediate = true), let's skip
                // the waiting and cut directly to the search.
                // Otherwise we'll sleep a little and see what
                // happens.

                // When we wake up, if we woke up BECAUSE the
                // timeout expired, great, this means no other
                // search was scheduled during our quick rest,
                // so we may proceed.
                //
                // But if we woke up BEFORE the timeout, it means
                // another search was triggered and so we expired.
                // I'm sorry for your loss :(
                while (false == immediate && _waitHandler.WaitOne(_options.Delay) && myToken == _token)
                {
                }

                if (myToken == _token)
                {                    
                    Search();
                }
            });

            // Let's wake up all previous searches.
            _waitHandler.Set();
        }

        public void NotifySearchTextChanged(string searchText)
        {
            var text = NormalizeSearchText(searchText);

            // We'll not waste our time performing searches
            // with strings too small, unless we have no
            // string at all, which means all data must be
            // fetched.
            if (false == string.IsNullOrEmpty(text) && text.Length < _options.MininumSearchTextLength)
            {
                return;
            }

            ScheduleSearch(text);
        }

        public void NotifySearchButtonClicked(string searchText)
        {
            var text = NormalizeSearchText(searchText);
            ScheduleSearch(text, true);
        }

        public void Dispose()
        {
            _waitHandler.Dispose();
        }
    }
}