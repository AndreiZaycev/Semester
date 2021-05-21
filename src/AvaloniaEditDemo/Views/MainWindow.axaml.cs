using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Native;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.Rendering;
using Arithm;
using System.Xml;
using AvaloniaEdit.Highlighting.Xshd;


namespace AvaloniaEditDemo.Views
{
    using Pair = KeyValuePair<int, IControl>;

    public class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private CompletionWindow _completionWindow;
        private OverloadInsightWindow _insightWindow;
        private Button _runButton;
        private Button _openFileButton;
        private Button _createFileButton;
        private Button _saveFileButton;
        private TextBox _console;
        private StackPanel _stackPanel;
        private ScrollViewer _scrollViewer;

        public MainWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextChanged += _textEditor_TextChanged;
            _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            _textEditor.SyntaxHighlighting.MainRuleSet.Name = "print";
            _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy();

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
            var but = new Button() { Height = 16.8, Margin = Thickness.Parse("0,0"), Width = 20, Background = Brush.Parse("Yellow") };
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

        public void _runControlBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _console.Text = "";
            bool flag = false;
            int line = 0;
            var cnt = 0;
            foreach (Button button in _stackPanel.Children)
            {               
                if (button.Background == Brush.Parse("Purple"))
                {
                    if (cnt == counterOfBreakpoint)
                    {
                        flag = true;
                        counterOfBreakpoint++;
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }
                line++;              
            }
            if (!flag)
            {
                try
                {
                    string text = _textEditor.Text;
                    var dictionary = Arithm.Interpreter.run(Arithm.Main.parse(text)).Item3;
                    foreach (var keys in dictionary.Values)
                    {
                        _console.Text += keys + "\r\n";
                    }

                }
                catch (Exception)
                {
                    _console.Text = "Syntax error";
                }
            }
            else
            {
                try
                {
                    string textToExecute = "";
                    string[] lines = _textEditor.Text.Split("\r\n");
                    for (var counter = 0; counter < line; counter++)
                    {
                        textToExecute += lines[counter];
                    }
                    var dictionary = Arithm.Interpreter.run(Arithm.Main.parse(textToExecute)).Item2;
                    foreach (var keys in dictionary.Keys)
                    {
                        _console.Text += $"{keys} = {dictionary[keys]}\r\n";
                    }
                }
                catch (Exception)
                {
                    _console.Text = "Syntax error";
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
                    _textEditor.Text = text;
                }
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
                    var button = new Button() { Height = 16.8, Width = 20, Background = Brush.Parse("Yellow"), Margin = Thickness.Parse("0,0") };
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
        void textEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {

                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            _insightWindow?.Hide();
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
            string path = await sw.ShowAsync(this);
            try
            {
                System.IO.File.WriteAllText(path, _textEditor.Text);
                _completionWindow.Show();
            }
            catch (Exception)
            { }
        }

        class ElementGenerator : VisualLineElementGenerator, IComparer<Pair>
        {
            public List<Pair> controls = new List<Pair>();

            /// <summary>
            /// Gets the first interested offset using binary search
            /// </summary>
            /// <returns>The first interested offset.</returns>
            /// <param name="startOffset">Start offset.</param>
            public override int GetFirstInterestedOffset(int startOffset)
            {
                int pos = controls.BinarySearch(new Pair(startOffset, null), this);
                if (pos < 0)
                    pos = ~pos;
                if (pos < controls.Count)
                    return controls[pos].Key;
                else
                    return -1;
            }

            public override VisualLineElement ConstructElement(int offset)
            {
                int pos = controls.BinarySearch(new Pair(offset, null), this);
                if (pos >= 0)
                    return new InlineObjectElement(0, controls[pos].Value);
                else
                    return null;
            }

            int IComparer<Pair>.Compare(Pair x, Pair y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
    }         
}
