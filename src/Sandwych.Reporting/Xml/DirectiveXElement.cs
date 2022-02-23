using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Sandwych.Reporting.Xml
{
    /// <summary>
    /// Block Directive XElement
    /// </summary>
    public class DirectiveXElement : XElement
    {
        public const string ElementName = "dtl-directive";

        public DirectiveXElement(string directive) : base(ElementName)
        {
            this.Directive = directive?.Trim() ?? throw new ArgumentNullException(nameof(directive));
            if(!this.Directive.StartsWith("{%") || !this.Directive.EndsWith("%}") ){
                throw new SyntaxErrorException(directive);
            }

            this.Add(new RawXText(directive));
        }

        public string Directive { get; }

        public  static void SanitizeDirectiveElements(XContainer root)
        {
            var directiveElements = root.Descendants(ElementName).ToArray();
            foreach (var directiveElement in directiveElements)
            {
                ReduceDirectiveElement(directiveElement);
            }
        }

        public static void ReduceDirectiveElement(XElement directiveElement)
        {
            var reducedElement = directiveElement;
            var finished = false;
            while (!finished)
            {
                if (reducedElement.Parent.Value == reducedElement.Value)
                {
                    reducedElement = reducedElement.Parent;
                }
                else
                {
                    finished = true;
                }
            }
            reducedElement.ReplaceWith(new RawXText(directiveElement.Value));
        }
    }
}