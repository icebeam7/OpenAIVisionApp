using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenAIVisionApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        string title;

        [ObservableProperty]
        bool isBusy;
    }
}
