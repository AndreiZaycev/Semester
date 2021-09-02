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
using Avalonia.Input;

namespace AvaloniaEditDemo.Views
{
    public class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private Button _runButton;
        private Button _openFileButton;
        private Button _saveFileButton;
        private TextBox _console;
        private StackPanel _stackPanel;
        private readonly TextBlock _executionStatus;
        private bool isSuccessfulRun = true;
        private Button _stopButton;

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

            _stopButton = this.FindControl<Button>("Stop");
            _stopButton.Click += _stopButton_Click;
            _stopButton.IsVisible = false;

            _stackPanel = this.FindControl<StackPanel>("stackPanel");

            _runButton = this.FindControl<Button>("Run");
            _runButton.Click += _runControlBtn_Click;

            _saveFileButton = this.FindControl<Button>("SaveFile");
            _saveFileButton.Click += _saveFileButton_Click;

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
        bool alreadyPushButton = false;
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
            _stopButton.IsVisible = true;
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
                    _stopButton.IsVisible = false;
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
                    _console.Text = $"Local variables are empty.";
                }
                else
                {  
                    _stopButton.IsVisible = true;             
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
                                    _console.Text = $"Local variables are empty.";
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
                            _stopButton.IsVisible = false;          
                        }

                    ));                
                    task.Start();
                    
                }
            }
        }
        
        private void setExecutionBegin(bool runAllCode)
        {
            _console.Text = "";
            isSuccessfulRun = true;
            _executionStatus.Background = Brushes.Yellow;
            if (runAllCode)
            {
                _console.Text = "Execution started.";
            }
            else
            {
                _console.Text = "Debug started.";
            }
        }

        private void mainRunCode()
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
                    setExecutionBegin(true);
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
                        setExecutionBegin(false);
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
                        isSuccessfulRun = true;
                        _console.Text = "Debug dropped.";
                        _executionStatus.Background = Brushes.White;
                        lineOfLinearDebugEnd = 0;
                        _runButton.Content = "Run";
                    }
                }
            }
            previousLine = currentLine;
        }

        private void _runControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            alreadyPushButton = false;
            if (_executionStatus.Background == Brushes.Yellow)
            { 
                var txt = _console.Text;
                var debugMessage = "If you want to continue, you should wait until the code is executed or stop debugging.";
                if (_console.Text == "Debug started." || _console.Text == debugMessage)
                {
                    _console.Text = debugMessage;
                }
                else
                {
                    _console.Text = "the previous code has not been executed yet, are you sure you want to continue? \n press 'y' if you want to continue, 'n' if not.";
                }
                _console.KeyUp += _console_KeyUp;
                void _console_KeyUp(object sender, KeyEventArgs e)
                {
                    if (!alreadyPushButton)
                    {
                        if (e.Key == Key.Y)
                        {
                            alreadyPushButton = true;
                            mainRunCode();
                        }
                        else if (e.Key != Key.N)
                        {
                            _console.Text = "press 'y' if you want to continue, 'n' if not.";
                        }
                        else
                        {
                            alreadyPushButton = true;
                            _console.Text = txt.Replace("started", "continued");
                        }
                    }
                }
            }
            else
            {
                mainRunCode();
            } 
        }

        private void _stopButton_Click(object sender, RoutedEventArgs e)
        {
            var currentText = _textEditor.Text;
            _textEditor.Text = "";
            mainRunCode();
            _textEditor.Text = currentText;
            _executionStatus.Background = Brushes.White;
            _console.Text = "Execution stopped.";
            _stopButton.IsVisible = false;
            _runButton.Content = "Run";
            lineOfLinearDebugEnd = 0;
        }
          
        async private void _openControlBtn_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filters.Add(new FileDialogFilter() { Name = "Txt", Extensions = { "txt" } });
            var path = await ofd.ShowAsync(this);
            if (path != null && path.Length > 0)
            {
                _textEditor.Text = File.ReadAllText(path[0]);
                openedFile = path[0];
            }
        }
        async private void _saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialFileName = openedFile;
            var path = await saveFileDialog.ShowAsync(this);
            if (path != null) File.WriteAllText(path, _textEditor.Text);
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
