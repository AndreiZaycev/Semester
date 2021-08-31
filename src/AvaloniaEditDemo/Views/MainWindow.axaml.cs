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
            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            _executionStatus = this.FindControl<TextBlock>("executionStatus");

            _stackPanel = this.FindControl<StackPanel>("stackPanel");

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
        private int lineOfLinearDebugEnd;
        private int caretLineBeforeChanging = 1;
        private string textBeforeCaretChanging = "";
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

        private void stopExecuteBreakpoints()
        {
            foreach (Button buts in _stackPanel.Children)
            {
                if (buts.Background == Brush.Parse("Red"))
                {
                    buts.Background = Brush.Parse("Green");
                }
            }
        }

        private Microsoft.FSharp.Collections.FSharpList<AST.Stmt> getAST(string text)
        {
            if (text == "")
            {
                return Microsoft.FSharp.Collections.FSharpList<AST.Stmt>.Empty;
            }
            else
            {
                return parse(text);
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
            bool findRedBreakpoint = false;
                foreach (Button button in _stackPanel.Children)
                {
                    if (findRedBreakpoint)
                    {
                        if (button.Background == Brush.Parse("Green"))
                        {
                            button.Background = Brush.Parse("Red");
                            hasBreakpoint = true;
                            lineOfLinearDebugEnd = line;
                            break;
                        }
                    }
                    else
                    {
                        if (button.Background == Brush.Parse("Red"))
                        {
                            previousLine = line;
                            lineOfLinearDebugEnd = line;
                            findRedBreakpoint = true;
                        }
                    }
                    line += 1;
                }
                if (!findRedBreakpoint)
                {
                    line = lineOfLinearDebugEnd;
                    if (lineOfLinearDebugEnd <= _stackPanel.Children.Count)
                    {
                        foreach (Button button in _stackPanel.Children.GetRange(lineOfLinearDebugEnd, _stackPanel.Children.Count - lineOfLinearDebugEnd))
                        {
                            if (button.Background == Brush.Parse("Green"))
                            {
                                button.Background = Brush.Parse("Red");
                                hasBreakpoint = true;
                                lineOfLinearDebugEnd = line;
                                break;
                            }
                            line += 1;
                        }
                    }
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
            if (isSuccessfulRun)
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
                if (string.Join(" ", lines[previousLine .. currentLine]).Trim() == "") 
                {
                    _executionStatus.Background = Brushes.Green;
                    _console.Text = $"Local variables are empty";
                }
                else
                {
                    var parsedText = getAST(textToExecute);
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
                                    _console.Text = $"Local variables are empty";
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
        }
        
        private void setExecutionBegin()
        {
            _console.Text = "";
            isSuccessfulRun = true;
            _executionStatus.Background = Brushes.Yellow;
            _console.Text = "Execution started.";
        }

        private void _runControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_textEditor.Text.Trim() == "") 
            {
                _executionStatus.Background = Brushes.Green;
                _console.Text = "Execution finished.";
            }
            else
            {             
                var hasBreakpoints = false;
                (currentLine, hasBreakpoints) = findLineNewBreakpoint(hasBreakpoints);
                if (!hasBreakpoints && (lineOfLinearDebugEnd > _stackPanel.Children.Count || (((Button)_stackPanel.Children.ElementAt(lineOfLinearDebugEnd)).Background != Brush.Parse("Red") || isSuccessfulRun)))
                {
                    _runButton.Content = "Run";
                    setExecutionBegin();
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
                    lineOfLinearDebugEnd = 0;
                    isSuccessfulRun = true;
                }
                else
                {
                    _runButton.Content = "Continue";
                    if (isSuccessfulRun)
                    {
                        setExecutionBegin();
                        try
                        {  
                            executeCodeWithBreakpoint();
                        }
                        catch (Exception exception)
                        {
                            sendMessageToConsole(exception.Message);
                        } 
                    }
                    else
                    {
                        stopExecuteBreakpoints();
                        counterOfBreakpoint = 0;
                        isSuccessfulRun = true;
                        _console.Text = "Debug dropped";
                        _executionStatus.Background = Brushes.White;
                        lineOfLinearDebugEnd = 0;
                        _runButton.Content = "Run";
                    }
                }
            }
            previousLine = currentLine;
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

        private void addChildrensRange(int begin, int count)
        {  
            var ab = _stackPanel.Children.GetRange(begin, _stackPanel.Children.Count - begin);
            _stackPanel.Children.RemoveRange(begin, _stackPanel.Children.Count - begin);
            for (var i = 0; i < count; i++)
            {
                var button = createButton();
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
                    if (numberOfButtons > lines)
                    {
                        _stackPanel.Children.RemoveRange(lines, numberOfButtons - lines);
                    }
                    else if (numberOfButtons < lines)
                    {

                        for (var i = numberOfButtons; i < lines; i++)
                        {
                            var button = createButton();
                            _stackPanel.Children.Add(button);
                        }
                    }
                    caretLineBeforeChanging = _textEditor.TextArea.Caret.Line;
                }
                else if (numberOfButtons < lines) 
                {                    
                    if (_textEditor.TextArea.Caret.Line >= caretLineBeforeChanging)
                    {
                        addChildrensRange(caretLineBeforeChanging, lines - numberOfButtons);
                    }
                }
                else
                {
                    {
                        var num = _stackPanel.Children.Count - lines;
                        _stackPanel.Children.RemoveRange(_textEditor.TextArea.Caret.Line, num);
                    }
                }
            }
            caretLineBeforeChanging = _textEditor.TextArea.Caret.Line;
            textBeforeCaretChanging = _textEditor.Text;    
        }

        public void _textEditor_TextChanged(object sender, EventArgs e)
        {
            Caret_PositionChanged(this, e);
        }
    }         
}
