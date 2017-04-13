using OmniSharp;
using System.Threading.Tasks;
using OmniSharp.Roslyn.CSharp.Services.Structure;
using System;

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
}
