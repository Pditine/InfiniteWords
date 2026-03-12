using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace InfiniteWords_Win;

public partial class WordWindow : Window
{
    private WordContainer _wordContainer;
    private int _currentIndex = -1;
    
    public WordWindow(string categoryName)
    {
        InitializeComponent();
        _wordContainer = DataManager.GetWordContainer(categoryName);
        if (_wordContainer.Words.Count > 0)
        {
            Next();
        }
    }

    private void Next()
    {
        if (_wordContainer.Words.Count == 0)
        {
            return;
        }

        _currentIndex++;
        if (_currentIndex >= _wordContainer.Words.Count)
        {
            _currentIndex = 0;
        }
        var currentWord = _wordContainer.Words[_currentIndex];
        NumTextBlock.Text = $"{_currentIndex + 1}/{_wordContainer.Words.Count}";
        WordTextBlock.Text = currentWord.Text;
        MeaningTextBlock.Text = currentWord.Type + "  " + currentWord.Meaning;
    }

    private void Previous()
    {
        if (_wordContainer.Words.Count == 0)
        {
            return;
        }

        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = _wordContainer.Words.Count - 1;
        }
        var currentWord = _wordContainer.Words[_currentIndex];
        NumTextBlock.Text = $"{_currentIndex + 1}/{_wordContainer.Words.Count}";
        WordTextBlock.Text = currentWord.Text;
        MeaningTextBlock.Text = currentWord.Type + "  " + currentWord.Meaning;
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.J || e.Key == Key.Left)
        {
            Previous();
            e.Handled = true;
        }
        else if (e.Key == Key.K || e.Key == Key.Right)
        {
            Next();
            e.Handled = true;
        }else if (e.Key == Key.Escape)
        {
            Back();
            e.Handled = true;
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
            if (control is Button)
            {
                return true;
            }
    
            control = control.Parent as Control;
        }
    
        return false;
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        Back();
    }

    private void Back()
    {
        var selectWindow = new InfiniteWords_Win.SelectWindow
        {
            Position = Position
        };
        selectWindow.Show();
        Close();
    }
}