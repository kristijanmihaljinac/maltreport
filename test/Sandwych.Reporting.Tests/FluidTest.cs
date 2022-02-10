using System;
using System.Collections.Generic;
using System.Text;
using Fluid;
using NUnit.Framework;

namespace Sandwych.Reporting.Tests
{
    public class Item
    {
        public int Number { get; set; }
    }

    [TestFixture]
    public class FluidTest
    {

        [Test]
        public void FluidShouldWorksFine()
        {
            var model = new
            {
                Str1 = "Fluid",
                Str2 = "Template",
                Numbers = new Item[]
                {
                    new Item { Number = 1 },
                    new Item { Number = 2 }
                }
            };
            var source = "Hello {{p.Str1}} {{ p.Str2 }} [{% for i in p.Numbers %}{{i.Number}}{% endfor %}]";

            var parser = new FluidParser(); 
            Assert.True(parser.TryParse(source, out var template));

            var context = new Fluid.TemplateContext();
            context.Options.MemberAccessStrategy.Register(model.GetType()); // Allows any public property of the model to be used
            context.Options.MemberAccessStrategy.Register(typeof(Item));
            context.SetValue("p", model);
            var result = template.Render(context);

            Assert.AreEqual($"Hello {model.Str1} {model.Str2} [{model.Numbers[0].Number}{model.Numbers[1].Number}]", result);
        }

        [Test]
        public void FluidShouldWorksWithDynamicObject()
        {
            dynamic model = new
            {
                Str1 = "Fluid",
                Str2 = "Template",
                Numbers = new Item[]
                {
                    new Item { Number = 1 },
                    new Item { Number = 2 }
                }
            };

            var parser = new FluidParser();
            var source = "Hello {{p.Str1}} {{ p.Str2 }} [{% for i in p.Numbers %}{{i.Number}}{% endfor %}]";
            Assert.True(parser.TryParse(source, out var template));
            var context = new Fluid.TemplateContext();
            context.SetValue("p", Fluid.Values.FluidValue.Create(model, new TemplateOptions()));
            context.Options.MemberAccessStrategy.Register(model.GetType() as Type);
            context.Options.MemberAccessStrategy.Register(typeof(Item));
            var result = template.Render(context);

            Assert.AreEqual($"Hello {model.Str1} {model.Str2} [{model.Numbers[0].Number}{model.Numbers[1].Number}]", result);

        }


    }
}
