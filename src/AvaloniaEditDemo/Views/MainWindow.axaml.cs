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
using System.Linq;
using Arithm;
using static Arithm.Interpreter;

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
            // here I add 2, because in the editor's text for some reason the first line has a space of 2 pixels on top
            // regardless of font
            var button = createButton();
            _stackPanel.Children.Add(button);
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

        private int counterOfBreakpoint = 0;
        private bool isFirstDebug = true;
        private bool firstButton = true;
        private string openedFile = null;
        private int previousLine = 0;
        private static Dicts dicts = new Dicts
        (
            variablesDictionary: new Dictionary<string, string>(),
            interpretedDictionary: new Dictionary<AST.VName, AST.Expression>()
        );
        private int currentLine = 0;

        private void deleteAllBreakpoints()
        {
            foreach (Button buts in _stackPanel.Children)
            {
                if (buts.Background == Brush.Parse("Green") || buts.Background == Brush.Parse("Red"))
                {
                    buts.Background = Brush.Parse("Yellow");
                }
            }
        }
        private Button createButton()
        {
            var button = new Button() { Height = _textEditor.TextArea.TextView.DefaultLineHeight, Margin = Thickness.Parse("0,0"), Width = _stackPanel.Width, Background = Brush.Parse("Yellow") };
            if (firstButton)
            {
                button.Height += 2;
                firstButton = false;
            }
            button.Click += but_Click;
            void but_Click(object sender, RoutedEventArgs e)
            {
                if (button.Background == Brush.Parse("Yellow"))
                {
                    button.Background = Brush.Parse("Green");
                }
                else
                {
                    button.Background = Brush.Parse("Yellow");
                }            
            }
            return button;
        }
        private Button getExecutableButton(int line)
        {
            var numberOfExecutedButtons = 0;
            Button executableButton = new Button();
            foreach (Button buttons in _stackPanel.Children)
            {
                if (buttons.Background == Brush.Parse("Red"))
                {
                    buttons.Background = Brush.Parse("Green");
                }
                if (numberOfExecutedButtons == line)
                {
                    buttons.Background = Brush.Parse("Red");
                    executableButton = buttons;
                    break;
                }
                else
                {
                    numberOfExecutedButtons++;
                }
            }
            return executableButton;
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
                if (!isFirstDebug)
                {
                    if (button.Background == Brush.Parse("Red"))
                    {
                        numOfButton = counterOfBreakpoint - 1; 
                    }
                }
                if (button.Background == Brush.Parse("Green") || button.Background == Brush.Parse("Red"))
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
            if (isFirstDebug)
            {
                isFirstDebug = false;
            }
            return (line, hasBreakpoint);
        }
        private void executeAllCode(string text)
        {
            counterOfBreakpoint = 0;
            isFirstDebug = true;
            var parsedText = Arithm.Interpreter.parse(text);
            var task = new Task<string>(() =>
            {
                return Arithm.Interpreter.runPrint(parsedText);                               
            }
             );
            task.ContinueWith(t =>
                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {                    
                        _console.Text = t.Result;
                        if (isSuccessfulRun)
                        {
                            _executionStatus.Background = Brushes.Green;
                        }
                        _console.Text += "Execution finished.";                     
                    }
                    catch (Exception exception)
                    {
                        sendMessageToConsole(exception.Message);
                    }
                }));
            task.Start();
        }
        private void executeCodeWithBreakpoint()
        {
            if (previousLine >= currentLine)
            {
                previousLine = 0;
                dicts = new Dicts
                (
                    variablesDictionary: new Dictionary<string, string>(),
                    interpretedDictionary: new Dictionary<AST.VName, AST.Expression>()
                );
            }
            var prev = previousLine;
            var cur = currentLine;
            string[] lines = _textEditor.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var executableBreakpoint = getExecutableButton(currentLine);
            var textToExecute = string.Join(" ", lines[previousLine .. currentLine]);
            if (textToExecute.Trim() == "") 
            {
                _executionStatus.Background = Brushes.Green;
                _console.Text = $"Local variables is empty";
            }
            else
            {
                var parsedText = Arithm.Interpreter.parse(textToExecute);
                var task = new Task<Dicts>(() =>
                {
                    dicts = Arithm.Interpreter.runVariables(dicts, parsedText);
                    return dicts;
                });
                task.ContinueWith(t =>
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            if (t.Result.VariablesDictionary.Count == 0)
                            {
                                _console.Text = $"Local variables is empty";
                            }
                            else
                            {
                                _console.Text = $"Local variables{ Environment.NewLine }";
                                _runButton.IsEnabled = true;
                                foreach ((string keys, string values) in t.Result.VariablesDictionary)
                                {
                                    _console.Text += $"{keys} = {values}{ Environment.NewLine }";                               
                                }
                                if (isSuccessfulRun)
                                {
                                    _executionStatus.Background = Brushes.Green;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            sendMessageToConsole(exception.Message);
                        }                
                    }

                ));                
                task.Start();
            }
        }
        public void _runControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_textEditor.Text.Trim() == "") 
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
                (currentLine, hasBreakpoints) = findLineNewBreakpoint(hasBreakpoints);
                if (!hasBreakpoints)
                {
                    try
                    {
                        deleteAllBreakpoints();
                        string text = _textEditor.Text;
                        executeAllCode(text);
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
                        executeCodeWithBreakpoint();
                    }
                    catch (Exception exception)
                    {
                        sendMessageToConsole(exception.Message);
                    } 
                }
                previousLine = currentLine;
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
            catch (Exception except)
            {
                sendMessageToConsole(except.Message);
            }
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
                catch (Exception except)
                {
                    sendMessageToConsole(except.Message); 
                }
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
            catch (Exception except)
            { 
                sendMessageToConsole(except.Message); 
            }
        }
        public void _textEditor_TextChanged(object sender, EventArgs e)
        {           
            var lines = _textEditor.LineCount;
            int numberOfButtons = _stackPanel.Children.Count;
            if (numberOfButtons > lines)
            {
                _stackPanel.Children.RemoveRange(lines, numberOfButtons - lines);              
            }
            else if (numberOfButtons < lines)
            {
                
                for (var i = numberOfButtons; i < lines; i++)
                {
                    var button = createButton();
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
