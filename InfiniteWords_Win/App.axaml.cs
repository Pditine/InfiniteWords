using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace InfiniteWords_Win;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataManager.LoadData();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var selectWindow = new InfiniteWords_Win.SelectWindow();
            desktop.MainWindow = selectWindow;
            selectWindow.SetInitPosition();
        }

        base.OnFrameworkInitializationCompleted();
    }
}