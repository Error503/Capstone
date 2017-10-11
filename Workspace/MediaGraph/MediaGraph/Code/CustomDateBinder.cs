using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace MediaGraph.Code
{
    public class CustomDateBinder : IModelBinder
    {

        public object BindModel(ControllerContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            DateTime? dateTimeValue = null;
            DateTime parsed;
            // If the value was retreived
            if (!DateTime.TryParse(bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue, out parsed))
            {
                // Attempt a manual parsing
                string rawInput = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
                string[] split = rawInput.Split('-');
                int year = 0, month = 0, day = 0;

                if (int.TryParse(split[0], out year) && int.TryParse(split[1], out month) && int.TryParse(split[2], out day))
                {
                    dateTimeValue = new DateTime(year, month, day);
                }
            } 
            else
            {
                dateTimeValue = parsed;
            }

            return dateTimeValue;
        }

        /// <summary>
        /// Attempts to get a value of the specified type from the given key in the model.
        /// </summary>
        /// <typeparam name="T">The type of the value to get</typeparam>
        /// <param name="context">The model binding context object</param>
        /// <param name="key">The string key of the value to retreive</param>
        /// <returns>The value retreieved from the model</returns>
        private Nullable<T> GetValue<T>(ModelBindingContext context, string key) where T : struct
        {
            // Attempt to get the value
            ValueProviderResult result = context.ValueProvider.GetValue(context.ModelName + "." + key);

            // If the value was not received, try without a prefix
            if(result == null && context.FallbackToEmptyPrefix)
            {
                result = context.ValueProvider.GetValue(context.ModelName + "." + key);
            }

            // Return a null value or the converted value of the value received
            return result == null ? null : (Nullable<T>)result.ConvertTo(typeof(T));
        }
    }

    public class CustomDateAttribute : CustomModelBinderAttribute
    {
        private IModelBinder binder;

        /// <summary>
        /// Specifies that this public field or property is to use the CustomDateBinder.
        /// </summary>
        /// <param name="label">The name of the public field or property</param>
        public CustomDateAttribute(string label)
        {
            binder = new CustomDateBinder();
        }

        public override IModelBinder GetBinder()
        {
            return binder;
        }
    }

}