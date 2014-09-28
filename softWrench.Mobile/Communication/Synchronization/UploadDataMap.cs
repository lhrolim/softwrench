using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Data;
using softWrench.Mobile.Exceptions;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class UploadDataMap {
        private static Result TryParseFaultResponse(ExtendedHttpRequestException e) {
            if (null == e.Response) {
                return new Result(false);
            }

            try {
                var response = JsonParser.FromJson<FaultResponse>(e.Response);
                return new Result(false, response.ErrorMessage, response.ErrorStack);
            } catch (Exception) {
                // We're trying our best here,
                // but no guarantees.
                return new Result(false);
            }
        }

        private static StringContent CreateSaveContent(CompositeData data) {
            var content = new Dictionary<string, object>(
                ApplicationBehaviorDispatcher.OnBeforeUpload(data.Composite, data.Application));

            foreach (var components in data.Components) {
                var closuredComponents = components;

                // All component data maps served by the
                // same application are nested inside an
                // array named by the application.
                content[components.Key.ApplicationName] = components
                    .Select(c => ApplicationBehaviorDispatcher.OnBeforeUpload(c, closuredComponents.Key))
                    .ToList();
            }

            return new StringContent(
                content.ToJson(),
                Encoding.UTF8,
                HttpCall.JsonMediaType.MediaType);
        }

        public async Task<Result> ExecuteAsync(CompositeData data, string id, CancellationToken cancellationToken = default(CancellationToken)) {
            var isCreatedLocally = data.Composite.LocalState.IsLocal;

            var uri = isCreatedLocally
                ? User.Settings.Routes.InsertData(data.Composite.Application)
                : User.Settings.Routes.UpdateData(data.Composite.Application, id);

            var content = CreateSaveContent(data);

            try {
                if (isCreatedLocally) {
                    using (await HttpCall.PostStreamAsync(uri, content, cancellationToken)) {
                        return new Result(true);
                    }
                }
                using (await HttpCall.PutStreamAsync(uri, content, cancellationToken)) {
                    return new Result(true);
                }
            } catch (ExtendedHttpRequestException e) {
                return TryParseFaultResponse(e);
            } catch (HttpRequestException) {
                return new Result(false);
            }
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable once ClassNeverInstantiated.Local
        private class FaultResponse {
            public string ErrorMessage { get; set; }
            public string ErrorStack { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public class CompositeData {
            private readonly ApplicationSchemaDefinition _application;
            private readonly DataMap _composite;
            private readonly ILookup<ApplicationSchemaDefinition, DataMap> _components;

            public CompositeData(ApplicationSchemaDefinition compositeApplication, DataMap compositeDataMap, ILookup<ApplicationSchemaDefinition, DataMap> components) {
                _application = compositeApplication;
                _composite = compositeDataMap;
                _components = components;
            }

            public ApplicationSchemaDefinition Application {
                get { return _application; }
            }

            public DataMap Composite {
                get { return _composite; }
            }

            public ILookup<ApplicationSchemaDefinition, DataMap> Components {
                get { return _components; }
            }
        }

        public class Result {
            private readonly bool _isSuccess;
            private readonly string _errorMessage;
            private readonly string _errorStack;

            public Result(bool isSuccess, string errorMessage, string errorStack) {
                _isSuccess = isSuccess;
                _errorMessage = errorMessage;
                _errorStack = errorStack;
            }

            public Result(bool isSuccess)
                : this(isSuccess, null, null) {
            }

            public bool IsSuccess {
                get { return _isSuccess; }
            }

            public string ErrorMessage {
                get { return _errorMessage; }
            }

            public string ErrorStack {
                get { return _errorStack; }
            }
        }
    }
}