using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;


namespace Shiny.Impl
{
    public class ValidationBinding : ReactiveObject, IValidationBinding
    {
        private readonly IDisposable dispose;
        private readonly Dictionary<string, bool> touched = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> errors = new Dictionary<string, string>();


        public ValidationBinding(IValidationService service, IReactiveObject reactiveObj)
        {
            dispose = reactiveObj
                .WhenAnyProperty()
                .SubOnMainThread(x =>
                {
                    var error = service.ValidateProperty(reactiveObj, x.PropertyName)?.FirstOrDefault();
                    Set(x.PropertyName, error);
                });
        }


        public IReadOnlyDictionary<string, string> Errors => errors;
        public IReadOnlyDictionary<string, bool> Touched => touched;


        internal void Set(string propertyName, string? errorMessage)
        {
            if (!touched.ContainsKey(propertyName))
            {
                touched[propertyName] = true;
                this.RaisePropertyChanged(nameof(Touched));
            }
            if (errors.ContainsKey(propertyName))
            {
                // change
                errors.Remove(propertyName);
                if (errorMessage != null)
                    errors[propertyName] = errorMessage;

                this.RaisePropertyChanged(nameof(Errors));
            }
            else if (errorMessage != null)
            {
                // change
                errors[propertyName] = errorMessage;
                this.RaisePropertyChanged(nameof(Errors));
            }
        }


        public void Dispose() => dispose.Dispose();
    }
}
