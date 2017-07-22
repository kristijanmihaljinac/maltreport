﻿using System;
using System.Collections.Generic;
using Sandwych.Reporting.OpenDocument;
using Sandwych.Reporting.Tests.Common;
using Xunit;

namespace Sandwych.Reporting.Tests.OpenDocument
{

    public class OdsTemplateTest
    {
        private const string Template2OdsName = "Sandwych.Reporting.Tests.OpenDocument.Templates.Template2.ods";

        [Fact]
        public void CanCompileOdsDocumentTemplate()
        {
            using (var stream = DocumentTestHelper.GetResource(Template2OdsName))
            {
                var odt = new OdfDocument();
                odt.Load(stream);
                var template = new OdfTemplate(odt);
            }
        }

        [Fact]
        public void CanRenderOdsTemplate()
        {
            OdfTemplate template;
            using (var stream = DocumentTestHelper.GetResource(Template2OdsName))
            {
                template = new OdfTemplate(stream);
            }

            var dataSet = new TestingDataSet();
            var context = new Dictionary<string, object>()
            {
                { "table1", dataSet.Table1 },
                { "so", dataSet.SimpleObject },
            };

            var result = template.Render(context);

            result.Save(@"c:\tmp\out.ods");
        }

    }
}