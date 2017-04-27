using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using Cake.Core.IO;
using Cake.Core.Scripting;
using System.Xml;
using System.Xml.Linq;
using Cake.Core;

namespace Cake.OmniSharp.Scripting
{
    [Export, Shared]
    public class CakeDocumentationProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDictionary<string, XDocument> _documentation;

        [ImportingConstructor]
        public CakeDocumentationProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _documentation = new Dictionary<string, XDocument>();
        }

        public void AddDocumentation(FilePath assemblyLocation)
        {
            if (!_documentation.ContainsKey(assemblyLocation.FullPath) && _fileSystem.Exist(assemblyLocation.ChangeExtension("xml")))
            {
                _documentation[assemblyLocation.FullPath] = GetXDocument(assemblyLocation.ChangeExtension("xml"));
            }
        }

        private XDocument GetXDocument(FilePath filePath)
        {
            using (var stream = _fileSystem.GetFile(filePath).OpenRead())
            using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
            {
                return XDocument.Load(xmlReader);
            }
        }

        public string Generate(ScriptAlias alias)
        {
            var xmlFile = new FilePath(alias.Method.Module.Assembly.Location).FullPath;

            if (!_documentation.ContainsKey(xmlFile))
            {
                return string.Empty;
            }

            var name = GetName(alias);

            var element = (from doc in _documentation[xmlFile].Elements("doc")
                from members in doc.Elements("members")
                from member in members.Elements("member")
                where member.Attribute("name").Value.Equals(name, StringComparison.Ordinal)
                select member).FirstOrDefault();

            if (element == null)
            {
                return string.Empty;
            }
                    
            var builder = new StringBuilder();

            foreach (var xElement in element.Elements())
            {
                builder.AppendLine($"/// {xElement.ToString().Replace("\r\n", "\r\n///")}");
            }

            return builder.ToString();
        }

        private string GetName(ScriptAlias @alias)
        {
            var builder = new StringBuilder();
            builder.Append(alias.Type == ScriptAliasType.Method ? "M:" : "P:");
            builder.Append(alias.Method.GetFullName());
            builder.Append("(");
            builder.Append(string.Join(",", alias.Method.GetParameters().Select(p => p.ParameterType.FullName)));           
            builder.Append(")");

            return builder.ToString();
        }
    }
}
