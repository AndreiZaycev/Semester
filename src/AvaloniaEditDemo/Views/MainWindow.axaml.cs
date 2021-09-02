using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
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
        private readonly Button _runButton;
        private readonly Button _openFileButton;
        private readonly Button _saveFileButton;
        private readonly TextBox _console;
        private readonly StackPanel _stackPanel;
        private readonly TextBlock _executionStatus;
        private bool isSuccessfulRun = true;
        private readonly Button _stopButton;
        public MainWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("Editor");
            _executionStatus = this.FindControl<TextBlock>("executionStatus");
            _stopButton = this.FindControl<Button>("Stop");
            _stackPanel = this.FindControl<StackPanel>("stackPanel");
            _runButton = this.FindControl<Button>("Run");
            _saveFileButton = this.FindControl<Button>("SaveFile");
            _openFileButton = this.FindControl<Button>("OpenFile");
            _console = this.FindControl<TextBox>("console");
            _textEditor.TextChanged += TextEditor_TextChanged;
            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            _stopButton.Click += StopButton_Click;
            _runButton.Click += RunControlBtn_Click;
            _saveFileButton.Click += SaveFileButton_Click;
            _openFileButton.Click += OpenControlBtn_Click;
            // here I add 2, because in the editor's text for some reason the first line has a space of 2 pixels on top
            // regardless of font
            var button = CreateButton();
            _stackPanel.Children.Add(button);
            // syntax highlighting 
            using StreamReader s =
            new(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "/highlighter.xshd");
            using XmlTextReader reader = new(s);
            _textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        private void InitializeComponent() { AvaloniaXamlLoader.Load(this); }
        private int lineOfLinearDebugEnd;
        private int caretLineBeforeChanging = 1;
        private string textBeforeCaretChanging = "";
        private bool firstButton = true;
        private string openedFile = null;
        private int previousLine = 0;
        private static Dicts dicts = new(
            variablesDictionary: new Dictionary<string, string>(),
            interpretedDictionary: new Dictionary<AST.VName, AST.Expression>());
        private int currentLine = 0;   
        private void StopAndRunChangingRoles(bool isRunEnabled)
        {
            if (isRunEnabled) { _stopButton.IsVisible = false; _runButton.IsEnabled = true; }
            else { _stopButton.IsVisible = true; _runButton.IsEnabled = false; }
        }
        private static Microsoft.FSharp.Collections.FSharpList<AST.Stmt> GetAST(string text)
        {
            if (text == "") return Microsoft.FSharp.Collections.FSharpList<AST.Stmt>.Empty;          
            else return parse(text);            
        }
        private Button CreateButton()
        {
            var button = new Button() { Height = _textEditor.TextArea.TextView.DefaultLineHeight, Margin = Thickness.Parse("0,0"), Width = _stackPanel.Width, Background = Brushes.Yellow };
            if (firstButton) { button.Height += 2; firstButton = false; }        
            button.Click += but_Click;
            void but_Click(object sender, RoutedEventArgs e)
            {
                if (button.Background == Brushes.Yellow) button.Background = Brushes.Green;               
                else button.Background = Brushes.Yellow;                              
            }
            return button;
        }    
        private void SendMessageToConsole(string exception)
        {
            _console.Text = exception;
            _executionStatus.Background = Brushes.Red;
            StopAndRunChangingRoles(true);
        }
        private (int, bool) FindLineNewBreakpoint(bool hasBreakpoint)
        {
            var line = 0;
            bool findRedBreakpoint = false;
            var lineOfFirstGreenBreakpoint = -1;
            foreach (Button button in _stackPanel.Children)
            {
                if (lineOfFirstGreenBreakpoint == -1 && line >= lineOfLinearDebugEnd && button.Background == Brushes.Green) lineOfFirstGreenBreakpoint = line;
                if (findRedBreakpoint)
                {
                    if (button.Background == Brushes.Green)
                    {
                        button.Background = Brushes.Red;
                        hasBreakpoint = true;
                        lineOfLinearDebugEnd = line;
                        break;
                    }
                }
                else if (button.Background == Brushes.Red)
                {
                    button.Background = Brushes.Green;
                    previousLine = line;
                    lineOfLinearDebugEnd = line;
                    findRedBreakpoint = true;
                }               
                line += 1;
            }
            if (!findRedBreakpoint && lineOfFirstGreenBreakpoint != -1) 
            {
                ((Button)_stackPanel.Children.ElementAt(lineOfFirstGreenBreakpoint)).Background = Brushes.Red;
                lineOfLinearDebugEnd = lineOfFirstGreenBreakpoint;
                return (lineOfFirstGreenBreakpoint, true);
            }
            else return (line, hasBreakpoint);
        }      
        private void SetExecutionBegin(bool runAllCode)
        {
            _console.Text = "";
            isSuccessfulRun = true;
            _executionStatus.Background = Brushes.Yellow;
            if (runAllCode) _console.Text = "Execution started.";           
            else _console.Text = "Debug started.";
        }
        private void RunControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            void ExecuteCodeWithBreakpoint()
            {
                StopAndRunChangingRoles(false);
                if (isSuccessfulRun)
                {
                    if (previousLine >= currentLine)
                    {
                        previousLine = 0;
                        dicts = new(
                            variablesDictionary: new Dictionary<string, string>(),
                            interpretedDictionary: new Dictionary<AST.VName, AST.Expression>());

                    }
                    string[] lines = _textEditor.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    var textToExecute = string.Join(" ", lines[previousLine..currentLine]);
                    if (string.Join(" ", lines[previousLine..currentLine]).Trim() == "")
                    {
                        StopAndRunChangingRoles(true);
                        _executionStatus.Background = Brushes.Green;
                        _console.Text = $"Local variables are empty.";
                    }
                    else
                    {
                        var task = new Task<Dicts>(() => { return runVariables(dicts, GetAST(textToExecute)); });
                        task.ContinueWith(t =>
                            Dispatcher.UIThread.Post(() =>
                            {
                                try
                                {
                                    if (t.Result.VariablesDictionary.Count == 0) _console.Text = $"Local variables are empty.";
                                    else
                                    {
                                        _console.Text = $"Local variables{ Environment.NewLine }";
                                        foreach ((string keys, string values) in t.Result.VariablesDictionary)
                                            _console.Text += $"{keys} = {values}{Environment.NewLine}";
                                        if (isSuccessfulRun) _executionStatus.Background = Brushes.Green;
                                    }
                                }
                                catch (Exception exception) 
                                {
                                    isSuccessfulRun = false;
                                    SendMessageToConsole(exception.Message); 
                                }
                                StopAndRunChangingRoles(true);
                            }
                        ));
                        task.Start();
                    }
                }
            }
            void ExecuteAllCode(string text)
            {
                StopAndRunChangingRoles(false);
                var task = new Task<string>(() => { return runPrint(parse(text)); });
                task.ContinueWith(t =>
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            _console.Text = t.Result;
                            if (isSuccessfulRun) { _executionStatus.Background = Brushes.Green; _console.Text += "Execution finished."; }      
                        }
                        catch (Exception exception) 
                        {
                            isSuccessfulRun = false;
                            SendMessageToConsole(exception.Message); 
                        }
                        StopAndRunChangingRoles(true);
                    }));
                task.Start();
            }
            if (_textEditor.Text.Trim() == "") { _executionStatus.Background = Brushes.Green; _console.Text = "Your code is empty!"; }
            else
            {
                var hasBreakpoints = false;
                (currentLine, hasBreakpoints) = FindLineNewBreakpoint(hasBreakpoints);
                var isLineOfLinearDebugInRange = lineOfLinearDebugEnd > _stackPanel.Children.Count;
                var isCurrentBreakpointDeleted = ((Button)_stackPanel.Children.ElementAt(lineOfLinearDebugEnd)).Background != Brushes.Red;
                if (!hasBreakpoints && (isLineOfLinearDebugInRange || isCurrentBreakpointDeleted || isSuccessfulRun))
                {
                    _runButton.Content = "Run";
                    SetExecutionBegin(true);
                    foreach (Button buts in _stackPanel.Children)
                    {
                        if (buts.Background == Brushes.Green || buts.Background == Brushes.Red) buts.Background = Brushes.Yellow;
                    }
                    ExecuteAllCode(_textEditor.Text);                    
                    lineOfLinearDebugEnd = 0;
                    isSuccessfulRun = true;
                }
                else
                {
                    _runButton.Content = "Continue";
                    if (isSuccessfulRun)
                    {
                        SetExecutionBegin(false);
                        ExecuteCodeWithBreakpoint();                       
                    }
                    else
                    {
                        foreach (Button buts in _stackPanel.Children) { if (buts.Background == Brushes.Red) buts.Background = Brushes.Green; }
                        isSuccessfulRun = true;
                        _console.Text = "Debug dropped.";
                        _executionStatus.Background = Brushes.White;
                        lineOfLinearDebugEnd = 0;
                        _runButton.Content = "Run";
                    }
                }                
                previousLine = currentLine;
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            var currentText = _textEditor.Text;
            _textEditor.Text = "";
            RunControlBtn_Click(this, e);
            _textEditor.Text = currentText;
            _executionStatus.Background = Brushes.White;
            _console.Text = "Execution stopped.";
            _runButton.Content = "Run";
            lineOfLinearDebugEnd = 0;
            StopAndRunChangingRoles(true);
        }         
        async private void OpenControlBtn_Click(object sender, RoutedEventArgs e)
        {
            var openedFileDialog = new OpenFileDialog();
            openedFileDialog.Filters.Add(new FileDialogFilter() { Name = "Txt", Extensions = { "txt" } });
            var path = await openedFileDialog.ShowAsync(this);
            if (path != null && path.Length > 0) { _textEditor.Text = File.ReadAllText(path[0]); openedFile = path[0]; }
        }
        async private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { InitialFileName = openedFile };
            var path = await saveFileDialog.ShowAsync(this);
            if (path != null) File.WriteAllText(path, _textEditor.Text);
        }
        private void AddChildrensRange(int begin, int count)
        {  
            var ab = _stackPanel.Children.GetRange(begin, _stackPanel.Children.Count - begin);
            _stackPanel.Children.RemoveRange(begin, _stackPanel.Children.Count - begin);
            for (var i = 0; i < count; i++)
            {
                var button = CreateButton();
                _stackPanel.Children.Add(button);
            }
            _stackPanel.Children.AddRange(ab);
        }
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (textBeforeCaretChanging != _textEditor.Text)
            {
                var lines = _textEditor.LineCount;
                int numberOfButtons = _stackPanel.Children.Count;
                if (_textEditor.TextArea.Caret.Line == lines)
                {
                    if (numberOfButtons > lines) _stackPanel.Children.RemoveRange(lines, numberOfButtons - lines);
                    else if (numberOfButtons < lines)
                    {
                        for (var i = numberOfButtons; i < lines; i++)
                        {
                            var button = CreateButton();
                            _stackPanel.Children.Add(button);
                        }
                    }
                    caretLineBeforeChanging = _textEditor.TextArea.Caret.Line;
                }
                else if (numberOfButtons < lines) 
                {                    
                    if (_textEditor.TextArea.Caret.Line >= caretLineBeforeChanging) AddChildrensRange(caretLineBeforeChanging, lines - numberOfButtons);
                }
                else _stackPanel.Children.RemoveRange(_textEditor.TextArea.Caret.Line, _stackPanel.Children.Count - lines);                        
            }
            caretLineBeforeChanging = _textEditor.TextArea.Caret.Line;
            textBeforeCaretChanging = _textEditor.Text;    
        }
        public void TextEditor_TextChanged(object sender, EventArgs e)
        {
            Caret_PositionChanged(this, e);
        }
    }         
}
