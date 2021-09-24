using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class FilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
                throw new ArgumentNullException(nameof(bindingContext));
            
            var filter=  bindingContext.ValueProvider.GetValue("filter").FirstOrDefault() ?? string.Empty;
            if (filter != String.Empty)
            {
                bindingContext.Result = ModelBindingResult.Success(Filter.Parse(filter));
            }

            return Task.CompletedTask;
        }
    }
}