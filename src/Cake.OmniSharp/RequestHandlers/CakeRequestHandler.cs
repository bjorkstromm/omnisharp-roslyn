using OmniSharp;
using System.Threading.Tasks;
using OmniSharp.Roslyn.CSharp.Services.Structure;
using System;
using System.Reflection;
using OmniSharp.Mef;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Composition.Hosting;

namespace Cake.OmniSharp.RequestHandlers
{
    public abstract class CakeRequestHandler<TService, TRequest, TResponse> : RequestHandler<TRequest, TResponse>
        where TService : RequestHandler<TRequest, TResponse>
    {
        public OmniSharpWorkspace Workspace { get; private set; }
        public TService Service { get; private set; }

        protected CakeRequestHandler(OmniSharpWorkspace workspace, TService service)
        {
            Workspace = workspace;
            Service = service;
        }

        public virtual Task<TResponse> Handle(TRequest request)
        {
            // TODO: Position translation
            return Service.Handle(request);
        }
    }

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
        public OmniSharpWorkspace Workspace { get; private set; }
        public Lazy<RequestHandler<TRequest, TResponse>> Service { get; private set; }

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

        public virtual Task<TResponse> Handle(TRequest request)
        {
            var service = Service.Value;
            if(service == null)
            {
                throw new NotSupportedException();
            }

            return service.Handle(request);
        }
    }
}
