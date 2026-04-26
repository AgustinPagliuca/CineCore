using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CineCore.ModelBinders
{
    public class DecimalInvariantModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(0m);
            }
            else
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
                var entrada = valueProviderResult.FirstValue;

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    bindingContext.Result = ModelBindingResult.Success(0m);
                }
                else
                {
                    var entradaNormalizada = entrada.Replace(',', '.');

                    if (decimal.TryParse(entradaNormalizada, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultado))
                    {
                        bindingContext.Result = ModelBindingResult.Success(resultado);
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Ingresá un número válido.");
                        bindingContext.Result = ModelBindingResult.Failed();
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}