using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Sandwych.Reporting.Textilize;

namespace Sandwych.Reporting
{
    public abstract class AbstractTemplate<TDocument>
        where TDocument : IDocument
    {
        private readonly TDocument _document;
        private readonly static IAsyncFilter[] s_emptyAsyncFilters = new IAsyncFilter[] { };

        public TDocument TemplateDocument => _document;

        public AbstractTemplate(TDocument document)
        {
            if (document.IsNew)
            {
                throw new ArgumentOutOfRangeException(nameof(document), "The template document must not be new(empty)");
            }

            _document = document;
            this.PrepareTemplate();
        }

        public TDocument Render(TemplateContext context) =>
            Task.Run(() => this.RenderAsync(context)).Result;

        public abstract Task<TDocument> RenderAsync(TemplateContext context);

        protected abstract void PrepareTemplate();

        protected static IAsyncFilter[] EmptyAsyncFilters => s_emptyAsyncFilters;

        protected virtual IEnumerable<IAsyncFilter> GetInternalAsyncFilters(TDocument document) => s_emptyAsyncFilters;

        protected virtual FluidTemplateContext CreateFluidTemplateContext(TDocument document, TemplateContext context)
        {
            var ftc = new FluidTemplateContext(context.Values);
            ftc.CultureInfo = context.Culture;
            this.RegisterInternalFilters(document, ftc);
            return ftc;
        }

        private void RegisterInternalFilters(TDocument document, FluidTemplateContext templateContext)
        {
            foreach (var asyncFilter in this.GetInternalAsyncFilters(document))
            {

    //public delegate ValueTask<FluidValue> AsyncFilterDelegate(FluidValue input, FilterArguments arguments, TemplateContext context);
                templateContext.Options.Filters.AddFilter(asyncFilter.Name, asyncFilter.ExecuteAsync);
            }
        }

    }
}
