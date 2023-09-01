using Microsoft.Extensions.Logging;
using Prism.Navigation;
using ReactiveUI;
using Shiny.Extensions.Dialogs;
using Shiny.Extensions.Localization;
using Shiny.Stores;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Shiny
{
    public abstract class BaseViewModel : ReactiveObject, IDestructible, IValidationViewModel
    {
        protected BaseViewModel(bool useValidation = false)
        {
            //this.isInternetAvaibale = ShinyHost
            //    .Resolve<IConnectivity>()
            //    .WhenInternetStatusChanged()
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .ToProperty(this, x => x.IsInternetAvailable)
            //    .DisposeWith(this.DestroyWith);

            Localize = ShinyHost.Resolve<ILocalizationSource>(); // try to set the default section if there is one

            if (useValidation)
            {
                var validationService = ShinyHost.Resolve<IValidationService>();
                if (validationService != null)
                {
                    Validation = validationService.Bind(this);
                    DestroyWith.Add(Validation);
                }
            }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.RaiseAndSetIfChanged(ref isBusy, value);
        }

        private string? busyText;
        public string? BusyText
        {
            get => busyText;
            set => this.RaiseAndSetIfChanged(ref busyText, value);
        }

        private string? title;
        public string? Title
        {
            get => title;
            protected set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private readonly ObservableAsPropertyHelper<bool> isInternetAvaibale;
        public bool IsInternetAvailable => isInternetAvaibale.Value;
        public IValidationBinding? Validation { get; private set; }

        private CompositeDisposable? deactivateWith;
        public CompositeDisposable DeactivateWith => deactivateWith ??= new CompositeDisposable();

        private CompositeDisposable? destroyWith;
        protected internal CompositeDisposable DestroyWith => destroyWith ??= new CompositeDisposable();

        private CancellationTokenSource? deactiveToken;
        /// <summary>
        /// The destroy cancellation token - called when your model is deactivated
        /// </summary>
        protected CancellationToken DeactivateToken
        {
            get
            {
                deactiveToken ??= new CancellationTokenSource();
                return deactiveToken.Token;
            }
        }

        private CancellationTokenSource? destroyToken;
        /// <summary>
        /// The destroy cancellation token - called when your model is destroyed
        /// </summary>
        protected CancellationToken DestroyToken
        {
            get
            {
                destroyToken ??= new CancellationTokenSource();
                return destroyToken.Token;
            }
        }

        private ILogger? logger;
        /// <summary>
        /// A lazy loader logger instance for this viewmodel instance
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                logger ??= ShinyHost.LoggerFactory.CreateLogger(GetType().AssemblyQualifiedName);
                return logger;
            }
            set => logger = value;
        }

        private IDialogs? dialogs;
        /// <summary>
        /// Dialog service from the service provider
        /// </summary>
        public IDialogs Dialogs
        {
            get
            {
                dialogs ??= ShinyHost.Resolve<IDialogs>();
                return dialogs;
            }
            protected set => dialogs = value;
        }

        private ILocalizationManager? localize;
        /// <summary>
        /// Localization manager from the service provider
        /// </summary>
        public ILocalizationManager LocalizationManager
        {
            get
            {
                localize ??= ShinyHost.Resolve<ILocalizationManager>();
                return localize;
            }
            protected set => localize = value;
        }



        /// <summary>
        /// The localization source for this instance - will attempt to use the default section (if registered)
        /// </summary>
        public ILocalizationSource? Localize { get; protected set; }


        /// <summary>
        /// This can be called manually, generally used when your viewmodel is going to the background in the nav stack
        /// </summary>
        protected virtual void Deactivate()
        {
            deactiveToken?.Cancel();
            deactiveToken?.Dispose();
            deactiveToken = null;

            deactivateWith?.Dispose();
            deactivateWith = null;
        }


        /// <summary>
        /// Called when the viewmodel is being destroyed (not in nav stack any longer)
        /// </summary>
        public virtual void Destroy()
        {
            destroyToken?.Cancel();
            destroyToken?.Dispose();

            Deactivate();
            destroyWith?.Dispose();
        }


        /// <summary>
        /// Binds to IsBusy while your command works
        /// </summary>
        /// <param name="command"></param>
        protected void BindBusyCommand(ICommand command)
            => BindBusyCommand((IReactiveCommand)command);


        /// <summary>
        /// Binds to IsBusy while your command works
        /// </summary>
        /// <param name="command"></param>
        protected void BindBusyCommand(IReactiveCommand command) =>
            command.IsExecuting.Subscribe(
                x => IsBusy = x,
                _ => IsBusy = false,
                () => IsBusy = false
            )
            .DisposeWith(DeactivateWith);


        /// <summary>
        /// Calls the loading task from dialog service while your command works
        /// </summary>
        /// <param name="action"></param>
        /// <param name="loadingText"></param>
        /// <param name="useSnackbar"></param>
        /// <param name="canExecute"></param>
        /// <returns></returns>
        protected ICommand LoadingCommand(
            Func<Task> action,
            string loadingText = "Loading...",
            bool useSnackbar = false,
            IObservable<bool>? canExecute = null
        ) => ReactiveCommand.CreateFromTask(() => Dialogs.LoadingTask(action, loadingText, useSnackbar), canExecute);


        /// <summary>
        /// Records the state of this model type for all get/set properties
        /// </summary>
        protected virtual void RememberUserState()
        {
            var binder = ShinyHost.Resolve<IObjectStoreBinder>();
            binder.Bind(this);

            DestroyWith.Add(Disposable.Create(() =>
                binder.UnBind(this)
            ));
        }


        /// <summary>
        /// Reads localization key from localization service
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual string? this[string key]
        {
            get
            {
                if (LocalizationManager == null)
                    throw new InvalidOperationException("Localization has not been initialized in your DI container");

                if (key.Contains(":") || Localize == null)
                    return LocalizationManager[key];

                return Localize[key];
            }
        }
    }
}
