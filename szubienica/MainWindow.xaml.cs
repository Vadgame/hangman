using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace szubienica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] splittedEntry = {"default", "entry"};
        private string category = "Default Category";
        private int[] wordsStartIndex;
        private byte columns = 20;
        private byte rows = 5;
        private string alphabet = "abcdefghijklmnoprstuwxyz";
        private bool isRightAltClicked = false;
        private string imageSource = "images/image";
        private byte mistakes = 0;

        private string specialLettersLatin = "acelnoszx";
        private string specialLettersPolish = "ąćęłńóśżź";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            categoryTextBlock.Text = "Category: " + category;

            int wordsLength = splittedEntry.Length;
            wordsStartIndex = new int[wordsLength];
            int firstLine = StartLine(wordsLength);
            int lastLine = firstLine + wordsLength - 1;

            int line = 1;
            int actualWord = -1;
            int freeSpaceBeforeWordLength;
            int modulo;
            if (firstLine == line) actualWord++;
            for (int i = 1; i < rows * columns + 1; i++)
            {
                TextBlock window = new TextBlock();
                window.Margin = new Thickness(2);
                window.Name = "window" + i;

                modulo = i % columns;

                if (line == firstLine)
                {
                    freeSpaceBeforeWordLength = (columns - splittedEntry[actualWord].Length) / 2;
                    if (modulo == 1) wordsStartIndex[actualWord] = i + freeSpaceBeforeWordLength;
                    if ((freeSpaceBeforeWordLength < modulo && modulo <= splittedEntry[actualWord].Length + freeSpaceBeforeWordLength) || splittedEntry[actualWord].Length == columns)
                    {
                        window.Background = Brushes.Gray;
                        window.FontSize = 24;
                        window.TextAlignment = TextAlignment.Center;
                    }
                    else window.Background = Brushes.LightGray;
                }
                else window.Background = Brushes.LightGray;
                windowsSpaceUniformGrid.Children.Add(window);
                if (modulo == 0)
                {
                    line++;
                    if (line > firstLine && line <= lastLine)
                    {
                        firstLine = line;
                    }
                    if (firstLine == line) actualWord++;
                }
            }
        }

        private int StartLine(int wordsLength)
        {
            switch (wordsLength)
            {
                case 1:
                    return 3;
                case 2:
                case 3:
                    return 2;
                case 4:
                case 5:
                    return 1;
            }
            // make exception
            throw new NotImplementedException();
        }

        private void LoadData()
        {
            // change it to relative and make exceptions
            try
            {
                using (StreamReader file = new StreamReader(@"C:\Programs\szubienica\szubienica\data.json"))
                {
                    // change 
                    string json = file.ReadToEnd();
                    List<Data> items = JsonConvert.DeserializeObject<List<Data>>(json);
                    Data drawedData = DrawEntry(items);
                    splittedEntry = drawedData.entry;
                    category = drawedData.category;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't load file with entries!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // make second exception wrong data file format
        }

        private Data DrawEntry(List<Data> items)
        {
            Random random = new Random();
            return items[random.Next(0, items.Count())];
        }

        private void ShowLettersInWindows(List<List<int>> indexes, string letter)
        {
            List<string> windowsNames = new List<string>();
            for (int i = 0; i < wordsStartIndex.Length; i++)
            {
                foreach (int index in indexes[i])
                {
                    windowsNames.Add("window" + (wordsStartIndex[i] + index));
                }
            }
            List<TextBlock> windowsToShow = windowsSpaceUniformGrid.Children.OfType<TextBlock>().ToList().Where(window => windowsNames.Contains(window.Name.ToString())).ToList();
            foreach (TextBlock window in windowsToShow)
            {
                window.Text = letter.ToUpper();
            }
        }

        private void WhenEntryContainsLetter(string letter)
        {
            List<List<int>> wordsIndexes = new List<List<int>>();
            foreach (string entryWord in splittedEntry)
            {
                List<int> characterIndexes = new List<int>();
                if (entryWord.Contains(letter))
                {
                    int lastCharacterIndex = -1;

                    for (int i = 0; i < entryWord.Length; i++)
                    {
                        if (i > lastCharacterIndex)
                        {
                            lastCharacterIndex = entryWord.IndexOf(letter, i);
                            if (lastCharacterIndex == -1) break;
                            characterIndexes.Add(lastCharacterIndex);
                        }
                    }
                }
                wordsIndexes.Add(characterIndexes);
            }
            ShowLettersInWindows(wordsIndexes, letter);
            CheckWin();
        }

        private void CheckWin()
        {
            foreach (TextBlock window in windowsSpaceUniformGrid.Children)
            {
                if (window.Background == Brushes.Gray && window.Text == string.Empty) return;
            }
            WonGame();
        }

        private void WonGame()
        {
            MessageBox.Show("You Won!");
            RestartGame();
        }

        private void RestartGame()
        {
            throw new NotImplementedException();
        }

        private void IsEntryContainsLetter(string letter)
        {
            foreach (string word in splittedEntry)
            {
                if (word.Contains(letter))
                {
                    WhenEntryContainsLetter(letter);
                    return;
                }
            }
            MadeMistake();
        }

        private void MadeMistake()
        {
            mistakes++;
            gallowsImage.Source = new BitmapImage(new Uri(imageSource + mistakes + ".png", UriKind.Relative));
            if (mistakes > 5) LostGame();
        }

        private void LostGame()
        {
            MessageBox.Show("You Lost!");
            RestartGame();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            string clickedLetter = e.Key.ToString().ToLower();
            if (e.Key == Key.RightAlt) isRightAltClicked = true;
            else if (alphabet.Contains(clickedLetter)) 
            {
                if (isRightAltClicked) CheckIsPolishLetterClicked(clickedLetter);
                else IsEntryContainsLetter(clickedLetter);
            }
        }

        private void CheckIsPolishLetterClicked(string clickedLetter)
        {
            for (int i = 0; i < specialLettersLatin.Length; i++)
            {
                if (clickedLetter == specialLettersLatin[i].ToString())
                {
                    IsEntryContainsLetter(specialLettersPolish[i].ToString());
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightAlt)
            {
                isRightAltClicked = false;
            }
        }
    }
}
