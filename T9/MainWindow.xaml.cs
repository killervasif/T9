using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace T9
{
    
    public partial class MainWindow : Window
    {
        private bool isProcessing;
        private bool isBackSpacePressed;
        public ObservableCollection<string> SuitableWords { get; set; }
        public List<string> Words { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SuitableWords = new();

            if (File.Exists(@"..\..\..\Words.json"))
            {
                var json = File.ReadAllText(@"..\..\..\Words.json");

                Words = JsonSerializer.Deserialize<List<string>>(json)!;
            }

            if (Words is null)
                Words = new();
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                SuitableWords.Clear();
                return;
            }

            if (isProcessing || isBackSpacePressed)
                return;

            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    SuitableWords.Clear();

                    foreach (var word in Words)
                    {
                        if (word.ToLower().StartsWith(txt.Text.ToLower()))
                            SuitableWords.Add(word);
                    }

                    isProcessing = true;
                    if (SuitableWords.Count > 0)
                    {
                        var FilledWord = SuitableWords[0];

                        var startIndex = txt.Text.Length;
                        var lenght = FilledWord.Length - txt.Text.Length;

                        txt.Text += FilledWord.Remove(0, startIndex);
                        txt.Select(startIndex, lenght);
                    }
                    isProcessing = false;
                });
            });
        }

        private void Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
                isBackSpacePressed = true;
            else
                isBackSpacePressed = false;

            var key = e.Key;

            bool isNumPudPressed = key switch
            {
                Key.NumPad0 => true,
                Key.NumPad1 => true,
                Key.NumPad2 => true,
                Key.NumPad3 => true,
                Key.NumPad4 => true,
                Key.NumPad5 => true,
                Key.NumPad6 => true,
                Key.NumPad7 => true,
                Key.NumPad8 => true,
                Key.NumPad9 => true,
                _ => false
            };



            if (SuitableWords.Count == 0)
                return;

            var index = List.SelectedIndex;


            isProcessing = true;
            switch (e.Key)
            {
                case Key.Up:
                    if (index != 0)
                        index--;
                    else
                        index = SuitableWords.Count - 1;


                    txt.Text = SuitableWords[index];
                    List.SelectedIndex = index;
                    break;

                case Key.Down:
                    if (index != SuitableWords.Count - 1)
                        index++;
                    else
                        index = 0;
                    txt.Text = SuitableWords[index];
                    List.SelectedIndex = index;
                    break;
                default:
                    break;
            }
            isProcessing = false;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!Words.Contains(txt.Text))
            {
                Words.Add(txt.Text);

                Task.Run(() =>
                {
                    var jsonStr = JsonSerializer.Serialize(Words);

                    File.WriteAllText(@"..\..\..\Words.json", jsonStr);
                    MessageBox.Show("Added Succesfully");
                });
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView list)
            {
                isProcessing = true;
                txt.Text = list.SelectedItem as string;
                SuitableWords.Clear();
                isProcessing = false;
            }
        }
    }
}
