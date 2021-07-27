using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Indentation.CSharp;
using System.Xml;
using AvaloniaEdit.Highlighting.Xshd;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace AvaloniaEditDemo.Views
{
    public class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private Button _runButton;
        private Button _openFileButton;
        private Button _createFileButton;
        private Button _saveFileButton;
        private TextBox _console;
        private StackPanel _stackPanel;
        private ScrollViewer _scrollViewer;
        private readonly TextBlock _executionStatus;
        private bool isSuccessfulRun = true;

        public MainWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextChanged += _textEditor_TextChanged;
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            _textEditor.SyntaxHighlighting.MainRuleSet.Name = "print";
            _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy();

            _executionStatus = this.FindControl<TextBlock>("executionStatus");

            _stackPanel = this.FindControl<StackPanel>("stackPanel");

            _scrollViewer = this.FindControl<ScrollViewer>("scrollViewer");

            _runButton = this.FindControl<Button>("Run");
            _runButton.Click += _runControlBtn_Click;

            _saveFileButton = this.FindControl<Button>("SaveFile");
            _saveFileButton.Click += _saveFileButton_Click;

            _createFileButton = this.FindControl<Button>("CreateFile");
            _createFileButton.Click += _createFileButton_Click;

            _openFileButton = this.FindControl<Button>("OpenFile");
            _openFileButton.Click += _openControlBtn_Click;

            _console = this.FindControl<TextBox>("console");
            // тут добавляю 2, потому что в текст эдиторе почему-то первая строка имеет пробел в 2 пикселя сверху 
            // независимо от шрифта
            var but = new Button() { Height = _textEditor.TextArea.TextView.DefaultLineHeight + 2, Margin = Thickness.Parse("0,0"), Width = _stackPanel.Width, Background = Brush.Parse("Yellow") };
            but.Click += but_Click;
            void but_Click(object sender, RoutedEventArgs e)
            {
                counterOfBreakpoint = 0;
                if (but.Background == Brush.Parse("Yellow"))
                {
                    but.Background = Brush.Parse("Purple");
                }
                else
                {
                    but.Background = Brush.Parse("Yellow");
                }
            }
            _stackPanel.Children.Add(but);
            // syntax highlighting 
            using (StreamReader s =
            new StreamReader(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "/highlighter.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    _textEditor.SyntaxHighlighting =
                    HighlightingLoader.Load(
                    reader,
                    HighlightingManager.Instance);
                }
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        int counterOfBreakpoint = 0;
        string openedFile = null;
        private void checkColorButton()
        {
            Button lastBut = new Button();
            foreach (Button buts in _stackPanel.Children)
            {
                if (buts.Background == Brush.Parse("Purple") || buts.Background == Brush.Parse("Red"))
                {
                    lastBut = buts;
                }
            }
            if (lastBut.Background == Brush.Parse("Red"))
            {
                lastBut.Background = Brush.Parse("Purple");
            }
        }
        private Button drawRedBreakpoint(int line)
        {
            var numberOfExecutedButtons = 0;
            Button redButty = new Button();
            foreach (Button buttons in _stackPanel.Children)
            {
                if (buttons.Background == Brush.Parse("Red"))
                {
                    buttons.Background = Brush.Parse("Purple");
                }
                if (numberOfExecutedButtons == line)
                {
                    buttons.Background = Brush.Parse("Red");
                    redButty = buttons;
                    break;
                }
                else
                {
                    numberOfExecutedButtons++;
                }
            }
            return redButty;
        }
        private void sendMessageToConsole(string exception)
        {
            _console.Text = exception;
            _executionStatus.Background = Brushes.Red;
            _runButton.IsEnabled = true;
            isSuccessfulRun = false;
        }
        private (int, bool) findLineNewBreakpoint(bool hasBreakpoint)
        {
            var line = 0;
            var numOfButton = 0;
            foreach (Button button in _stackPanel.Children)
            {
                if (button.Background == Brush.Parse("Purple") || button.Background == Brush.Parse("Red"))
                {
                    if (numOfButton == counterOfBreakpoint)
                    {
                        hasBreakpoint = true;
                        counterOfBreakpoint++;
                        break;
                    }
                    else
                    {
                        numOfButton++;
                    }
                }
                line++;
            }
            return (line, hasBreakpoint);
        }
        private void executeCodeWithPrintValues(string text)
        {
            var parsed = Arithm.Main.parse(text);
            var task = new Task<Dictionary<string, string>>(() =>
            {
                try
                {
                    var dict = Arithm.Interpreter.run(parsed);
                    return dict.Item3;
                }
                catch (Exception)
                {
                    return null;
                }
            }
             );
            task.ContinueWith(t =>
                Dispatcher.UIThread.Post(() =>
                {
                    if (t.Result == null)
                    {
                        sendMessageToConsole("Failed while parsing");
                    }
                    else
                    {
                        _console.Text = "";
                        foreach (var values in t.Result.Values)
                        {
                            _console.Text += $"{values}{Environment.NewLine}";
                        }
                        if (isSuccessfulRun)
                        {
                            _executionStatus.Background = Brushes.Green;
                        }
                        _console.Text += "Execution finished.";
                    }
                }));
            task.Start();
        }
        private void executeCodeWithBreakpoint(int line)
        {
            string textToExecute = "";
            string[] lines = _textEditor.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var redButton = drawRedBreakpoint(line);
            for (var counter = 0; counter < line; counter++)
            {
                textToExecute += lines[counter] + " ";
            }
            var parsed = Arithm.Main.parse(textToExecute);
            var task = new Task<Dictionary<string, string>>(() =>
            {
                try
                {
                    return Arithm.Interpreter.run(parsed).Item2;
                }
                catch (Exception)
                {
                    return null;
                }
            });
            task.ContinueWith(t =>
                Dispatcher.UIThread.Post(() =>
                {
                    if (t.Result == null)
                    {
                        sendMessageToConsole("Failed while parsing");
                    }
                    else
                    {
                        _console.Text = $"Local variables{ Environment.NewLine }";
                        _runButton.IsEnabled = true;
                        foreach ((string keys, string values) in t.Result)
                        {
                            _console.Text += $"{keys[^2..].Replace("\"", "")} = {values}{ Environment.NewLine }";
                        }
                        if (isSuccessfulRun)
                        {
                            _executionStatus.Background = Brushes.Green;
                        }
                    }
                }
            ));
            task.Start();
        }
        public void _runControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_textEditor.Text.Replace(" ", "").Replace($"{Environment.NewLine}", "") == "")
            {
                _executionStatus.Background = Brushes.Green;
                _console.Text = "Execution finished.";
            }
            else
            {
                _console.Text = "";
                isSuccessfulRun = true;
                _executionStatus.Background = Brushes.Yellow;
                _console.Text = "Execution started.";
                var hasBreakpoints = false;
                var line = 0;
                (line, hasBreakpoints) = findLineNewBreakpoint(hasBreakpoints);
                if (!hasBreakpoints)
                {
                    try
                    {
                        checkColorButton();
                        string text = _textEditor.Text;
                        executeCodeWithPrintValues(text);
                    }
                    catch (Exception exception)
                    {
                        sendMessageToConsole(exception.Message);
                    }
                }
                else
                {
                    try
                    {
                        executeCodeWithBreakpoint(line);
                    }
                    catch (Exception exception)
                    {
                        sendMessageToConsole(exception.Message);
                    }
                }
            }
        }
        async private void _openControlBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var path = await ofd.ShowAsync(this);
            try
            {
                using (StreamReader sr = new StreamReader(path[0]))
                {
                    string text = sr.ReadToEnd();
                    openedFile = path[0];
                    _textEditor.Text = text;
                }
            }
            catch (Exception)
            { }
        }
        async private void _createFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            var path = await sfd.ShowAsync(this);
            if (!File.Exists(path))
            {
                try
                {
                    File.CreateText(path);
                }
                catch (Exception)
                { }
            }
        }
        async private void _saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sw = new SaveFileDialog();
            string path = openedFile ?? await sw.ShowAsync(this);
            try
            {
                System.IO.File.WriteAllText(path, _textEditor.Text);
            }
            catch (Exception)
            { }
        }
        public void _textEditor_TextChanged(object sender, EventArgs e)
        {
            var lines = _textEditor.LineCount;
            int childrens = _stackPanel.Children.Count;
            if (childrens > lines)
            {
                _stackPanel.Children.RemoveRange(lines, childrens - lines);
            }
            else if (childrens < lines)
            {

                for (var i = childrens; i < lines; i++)
                {
                    var button = new Button() { Height = _textEditor.TextArea.TextView.DefaultLineHeight, Width = _stackPanel.Width, Background = Brush.Parse("Yellow"), Margin = Thickness.Parse("0,0") };
                    button.Click += breakPoint_Click;
                    void breakPoint_Click(object sender, RoutedEventArgs e)
                    {
                        counterOfBreakpoint = 0;
                        if (button.Background == Brush.Parse("Yellow"))
                        {
                            button.Background = Brush.Parse("Purple");
                        }
                        else
                        {
                            button.Background = Brush.Parse("Yellow");
                        }
                    }
                    var caretLine = _textEditor.TextArea.Caret.Line;
                    _stackPanel.Children.Add(button);
                }
            }
        }
        private class MyOverloadProvider : IOverloadProvider
        {
            private readonly IList<(string header, string content)> _items;
            private int _selectedIndex;

            public MyOverloadProvider(IList<(string header, string content)> items)
            {
                _items = items;
                SelectedIndex = 0;
            }

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentHeader));
                    OnPropertyChanged(nameof(CurrentContent));

                }
            }

            public int Count => _items.Count;
            public string CurrentIndexText => null;
            public object CurrentHeader => _items[SelectedIndex].header;
            public object CurrentContent => _items[SelectedIndex].content;

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class MyCompletionData : ICompletionData
        {
            public MyCompletionData(string text)
            {
                Text = text;
            }

            public IBitmap Image => null;

            public string Text { get; }

            public object Content => Text;

            public object Description => "Description for " + Text;

            public double Priority { get; } = 0;

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }
    }
}
