using OmniSharp;
using System.Threading.Tasks;
using System;
using System.Reflection;
using OmniSharp.Mef;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OmniSharp.Models;
using Cake.OmniSharp.Extensions;

namespace Cake.OmniSharp.RequestHandlers
{
    public abstract class CakeRequestHandler<TRequest, TResponse> : RequestHandler<TRequest, TResponse>
    {
        private string _endpointName;
        public string EndpointName
        {
            get
            {
                if(string.IsNullOrEmpty(_endpointName))
                {
                    _endpointName = GetType().GetTypeInfo().GetCustomAttribute<OmniSharpHandlerAttribute>()?.EndpointName;
                }

                return _endpointName;
            }
        }

        [ImportMany]
        public IEnumerable<Lazy<IRequestHandler, OmniSharpLanguage>> Handlers { get; set; }
        public OmniSharpWorkspace Workspace { get; }
        public Lazy<RequestHandler<TRequest, TResponse>> Service { get; }

        protected CakeRequestHandler(OmniSharpWorkspace workspace)
        {
            Workspace = workspace;
            Service = new Lazy<RequestHandler<TRequest, TResponse>>(() =>
            {
                return Handlers.FirstOrDefault(s =>
                    s.Metadata.EndpointName.Equals(EndpointName, StringComparison.Ordinal) &&
                    s.Metadata.Language.Equals(LanguageNames.CSharp, StringComparison.Ordinal))?.Value 
                    as RequestHandler<TRequest, TResponse>;
            });
        }

        public virtual async Task<TResponse> Handle(TRequest request)
        {
            var service = Service.Value;
            if(service == null)
            {
                throw new NotSupportedException();
            }

            request = await TranslateRequestAsync(request);

            var response = await service.Handle(request);

            return await TranslateResponse(response, request);
        }

        protected virtual async Task<TRequest> TranslateRequestAsync(TRequest req)
        {
            var request = req as Request;

            if (request == null)
            {
                return req;
            }

            request = await request.TranslateAsync(Workspace);

            return req;
        }

        protected virtual Task<TResponse> TranslateResponse(TResponse response, TRequest request)
        {
            return Task.FromResult(response);
        }
    }
}
