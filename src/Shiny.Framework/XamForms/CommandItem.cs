using ReactiveUI;
using System.Windows.Input;


namespace Shiny.XamForms
{
    public class CommandItem : ReactiveObject
    {
        private string? imageUri;
        public string? ImageUri
        {
            get => imageUri;
            set => this.RaiseAndSetIfChanged(ref imageUri, value);
        }

        private string? text;
        public string? Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        private string? detail;
        public string? Detail
        {
            get => detail;
            set => this.RaiseAndSetIfChanged(ref detail, value);
        }


        public ICommand? PrimaryCommand { get; set; }
        public ICommand? SecondaryCommand { get; set; }
        public object? Data { get; set; }
    }
}
