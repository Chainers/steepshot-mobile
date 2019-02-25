using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SteemDataScraper
{
    public class SnakeCaseValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            context.ValueProviders.Insert(0, new SnakeCaseQueryStringValueProvider(BindingSource.Query, context.ActionContext.HttpContext.Request.Query, CultureInfo.InvariantCulture));
            return Task.CompletedTask;
        }
    }

    public class SnakeCaseQueryStringValueProvider : QueryStringValueProvider
    {
        public SnakeCaseQueryStringValueProvider(BindingSource bindingSource, IQueryCollection values, CultureInfo culture)
            : base(bindingSource, values, culture)
        {
        }

        public override ValueProviderResult GetValue(string key)
        {
            var result = base.GetValue(key);

            if (result != ValueProviderResult.None)
                return result;

            var snakeCase = string.Concat(key.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();

            result = base.GetValue(snakeCase);
            return result;
        }
    }
}