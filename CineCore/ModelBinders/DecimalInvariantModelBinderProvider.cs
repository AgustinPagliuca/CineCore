using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace CineCore.ModelBinders
{
    public class DecimalInvariantModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            IModelBinder? binder;

            var esDecimal = context.Metadata.ModelType == typeof(decimal)
                         || context.Metadata.ModelType == typeof(decimal?);

            if (esDecimal)
            {
                binder = new DecimalInvariantModelBinder();
            }
            else
            {
                binder = null;
            }

            return binder;
        }
    }
}