using System;
using System.Linq;
using System.Text;
using Cake.Core.IO;
using Cake.Core.Scripting;
using System.Xml;
using System.Xml.Linq;
using Cake.Core;

namespace Cake.OmniSharp.Scripting
{
    internal class DocumentationGenerator
    {
        public string Generate(ScriptAlias alias)
        {
            var xmlFile = new FilePath(alias.Method.Module.Assembly.Location).ChangeExtension("xml").FullPath;
            var name = GetName(alias);

            if (!System.IO.File.Exists(xmlFile))
            {
                return string.Empty;
            }

            using (var xmlStream = System.IO.File.OpenRead(xmlFile))
            {
                using (var xmlReader = XmlReader.Create(xmlStream))
                {
                    var element = (from doc in XDocument.Load(xmlReader).Elements("doc")
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
            }
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
