using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Exceptions;

namespace softWrench.Mobile.Communication.Http
{
    internal static class HttpCall
    {
        private static readonly Lazy<HttpClient> HttpClientLazy = new Lazy<HttpClient>(InitializeHttpClient);

        private static HttpClient InitializeHttpClient()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var handler = new HttpClientHandler();

            // Thank you, but we're not interested            
            // on following those pesky redirects.
            handler.AllowAutoRedirect = false;

            // Use a cookie container for
            // maintaining auth state.
            handler.CookieContainer = CookieContainerProvider.Container;

            // Turns HTTP compression on,
            // if available on the client.
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            var httpClient = new HttpClient(handler);

            // We'll not send 100-continue for POSTs.
            httpClient
                .DefaultRequestHeaders
                .ExpectContinue = false;

            //TODO: for Fiddler routing
            httpClient
                .DefaultRequestHeaders
                .Add("X-sW4", "sW4");

            return httpClient;
        }

        private static HttpRequestMessage CreateHttpRequest(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);

            request.Headers.Accept.Add(JsonMediaTypeWithQuality);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return request;
        }

        private static async Task<StreamReader> ReadAsStreamAsync(HttpResponseMessage response)
        {
            var stream = await response
                .Content
                .ReadAsStreamAsync();

            return new StreamReader(stream);
        }

        private static async Task<HttpResponseMessage> EnsureSuccessStatusCodeExtended(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var message = string.Format("{0} ({1})", response.StatusCode, response.ReasonPhrase);

            // We'll throw a specific
            // exception for 401's.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpUnauthorizedRequestException(message);
            }

            string content;
            try
            {
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                // If we weren't able to fetch the response
                // content, no problem. It's a faulty http
                // status anyway, so let's not overreact.
                content = null;
            }
            Debug.Write(content);

            throw new ExtendedHttpRequestException(content, message);
        }

        public static async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = CreateHttpRequest(HttpMethod.Get, uri);
            var response = await HttpClient.SendAsync(request, cancellationToken);
            
            // Let's throw if we didn't
            // receive an HTTP 2XX back.
            return await response.EnsureSuccessStatusCodeExtended();
        }

        public static async Task<StreamReader> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await GetAsync(uri, cancellationToken);
            return await ReadAsStreamAsync(response);
        }

        public static async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = CreateHttpRequest(HttpMethod.Post, uri);
            request.Content = content;

            var response = await HttpClient.SendAsync(request, cancellationToken);

            // Let's throw if we didn't
            // receive an HTTP 2XX back.
            return await response.EnsureSuccessStatusCodeExtended();
        }

        public static async Task<StreamReader> PostStreamAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await PostAsync(uri, content, cancellationToken);
            return await ReadAsStreamAsync(response);
        }

        public static async Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = CreateHttpRequest(HttpMethod.Put, uri);
            request.Content = content;

            var response = await HttpClient.SendAsync(request, cancellationToken);

            // Let's throw if we didn't
            // receive an HTTP 2XX back.
            return await response.EnsureSuccessStatusCodeExtended();
        }

        public static async Task<StreamReader> PutStreamAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await PutAsync(uri, content, cancellationToken);
            return await ReadAsStreamAsync(response);
        }

        private static HttpClient HttpClient
        {
            get { return HttpClientLazy.Value; }
        }

        public static MediaTypeHeaderValue JsonMediaType
        {
            get
            {
                return MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
        }

        public static MediaTypeWithQualityHeaderValue JsonMediaTypeWithQuality
        {
            get
            {
                return MediaTypeWithQualityHeaderValue.Parse("application/json");
            }
        }
    }
}