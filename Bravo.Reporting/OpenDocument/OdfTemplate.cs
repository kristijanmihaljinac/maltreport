﻿//作者：李维
//创建时间：2010-08-20

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Security;
using System.Xml;
using System.Diagnostics;
using System.Globalization;

using NVelocity.Runtime;
using NVelocity.App;
using NVelocity.App.Events;
using NVelocity;
using NVelocity.Context;

namespace Bravo.Reporting.OpenDocument
{
    public class OdfTemplate : OdfDocument, ITemplate
    {
        private IDictionary<Image, string> userImages
            = new Dictionary<Image, string>();

        private IDocument resultDocument;

        private IDictionary<Type, IRenderFilter> filters;


        #region ITemplate 接口实现

        public IDocument Render(IDictionary<string, object> data)
        {
            this.resultDocument = new OdfDocument();
            this.CopyTo(this.resultDocument);

            this.filters = new Dictionary<Type, IRenderFilter>()
            {
                 { typeof(string), new XmlStringRenderFilter() },
                 { typeof(Image), new OdfImageRenderFilter(this.userImages, this.resultDocument) },
            };

            var ctx = CreateVelocityContext(data);

            var ve = new VelocityEngine();
            ve.Init();

            //执行主渲染过程
            this.MainRender(ctx, ve);

            return this.resultDocument;
        }

        #endregion

        private void MainRender(VelocityContext ctx, VelocityEngine ve)
        {
            using (var inStream = this.resultDocument.GetEntryInputStream(this.resultDocument.MainContentEntryPath))
            using (var reader = new StreamReader(inStream, Encoding.UTF8))
            using (var ws = this.resultDocument.GetEntryOutputStream(this.resultDocument.MainContentEntryPath))
            using (var writer = new StreamWriter(ws))
            {
                //执行渲染
                var successed = ve.Evaluate(ctx, writer, "TemplateRender", reader);
                if (!successed)
                {
                    throw new TemplateException("Render template failed");
                }
                writer.Flush();
            }
        }

        private VelocityContext CreateVelocityContext(IDictionary<string, object> data)
        {
            var ctx = new VelocityContext();
            foreach (var pair in data)
            {
                ctx.Put(pair.Key, pair.Value);
            }

            EventCartridge eventCart = new EventCartridge();
            eventCart.ReferenceInsertion += this.OnReferenceInsertion;
            ctx.AttachEventCartridge(eventCart);
            return ctx;
        }

        /// <summary>
        /// 此事件转义 XML 字符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnReferenceInsertion(object sender, ReferenceInsertionEventArgs e)
        {
            var t = e.OriginalValue.GetType();
            IRenderFilter filter = null;
            var hasFilter = this.filters.TryGetValue(t, out filter);
            if (hasFilter)
            {
                Debug.Assert(filter != null);
                e.NewValue = filter.Filter(e.OriginalValue);
            }
        }

    }
}
