using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace InfiniteWords_Win;

public partial class SelectWindow : Window
{
    public SelectWindow()
    {
        InitializeComponent();
        LoadCategories();
    }

    public void SetInitPosition()
    {
        var screen = Screens?.Primary;
        if (screen is null) return;

        const int margin = 50;
        var workArea = screen.WorkingArea; // PixelRect
        var scale = screen.Scaling;        // DPI scale

        var windowWidthPx = (int)Math.Round(Width * scale);
        var windowHeightPx = (int)Math.Round(Height * scale);

        var x = workArea.X + workArea.Width - windowWidthPx - margin;
        var y = workArea.Y + workArea.Height - windowHeightPx - margin;

        Position = new PixelPoint(x, y);
    }

    private void LoadCategories()
    {
        var categories = DataManager.GetCategories();
        CategoryListBox.ItemsSource = categories;
        if (categories.Count > 0)
        {
            CategoryListBox.SelectedIndex = 0;
        }
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !IsInteractiveControl(e.Source))
        {
            BeginMoveDrag(e);
        }
    }

    private static bool IsInteractiveControl(object? source)
    {
        if (source is not Control startControl)
        {
            return false;
        }

        Control? control = startControl;
        while (control is not null)
        {
            if (control is Button || control is ListBox || control is ListBoxItem)
            {
                return true;
            }

            control = control.Parent as Control;
        }

        return false;
    }

    private void OpenWordWindowButton_Click(object? sender, RoutedEventArgs e)
    {
        var category = CategoryListBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(category))
        {
            return;
        }

        var wordWindow = new WordWindow(category)
        {
            Position = Position
        };
        wordWindow.Show();
        Close();
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnVersionClick(object? sender, RoutedEventArgs e)
    {
        const string url = Const.Website;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}
