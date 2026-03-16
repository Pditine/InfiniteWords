using CommunityToolkit.Mvvm.ComponentModel;

namespace InfiniteWords.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
}