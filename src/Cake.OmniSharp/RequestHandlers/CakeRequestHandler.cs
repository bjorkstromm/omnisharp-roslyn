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
using OmniSharp.Models;
using Cake.Core.IO;

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
        public OmniSharpWorkspace Workspace { get; private set; }
        public Lazy<RequestHandler<TRequest, TResponse>> Service { get; private set; }

        private bool _translateResponse;

        protected CakeRequestHandler(OmniSharpWorkspace workspace, bool translateResponse = true)
        {
            Workspace = workspace;
            Service = new Lazy<RequestHandler<TRequest, TResponse>>(() =>
            {
                return Handlers.FirstOrDefault(s =>
                    s.Metadata.EndpointName.Equals(EndpointName, StringComparison.Ordinal) &&
                    s.Metadata.Language.Equals(LanguageNames.CSharp, StringComparison.Ordinal))?.Value 
                    as RequestHandler<TRequest, TResponse>;
            });

            _translateResponse = translateResponse;
        }

        public virtual async Task<TResponse> Handle(TRequest req)
        {
            var service = Service.Value;
            if(service == null)
            {
                throw new NotSupportedException();
            }

            // Translate if possible
            int offset = 0;
            var request = req as Request;
            if (request != null)
            {
                if (request.FileName == null)
                {
                    return default(TResponse);
                }

                var document = Workspace.GetDocument(request.FileName);
                if (document == null)
                {
                    return default(TResponse);
                }

                var filePath = new FilePath(request.FileName);
                var sourceText = await document.GetTextAsync();

                offset = sourceText.Lines.FirstOrDefault(line => line.ToString().Equals($"#line 1 \"{filePath.FullPath}\"")).LineNumber + 1;

                request.Line += offset;
            }

            var res = await service.Handle(req);

            if(_translateResponse && res is QuickFixResponse)
            {
                var response = res as QuickFixResponse;

                foreach(var fix in response.QuickFixes)
                {
                    fix.Line -= offset;
                }
            }

            return res;
        }
    }
}
