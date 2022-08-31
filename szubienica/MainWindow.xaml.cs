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
        // Default entry and category
        private string[] splittedEntry = {"default", "entry"};
        private string category = "Default Category";

        // UniformGird size
        private byte columns = 20;
        private byte rows = 5;

        // Indexes of windows in which word of entry start
        private int[] wordsStartIndex;
        
        private string imageSource = "images/image";
        private byte mistakes = 0;
        private int points = 0;

        // If true alt is clicked else is not
        private bool isRightAltClicked = false;

        // Letter used in keydown
        private string alphabet = "abcdefghijklmnoprstuwxyz";
        private string specialLettersLatin = "acelnoszx";
        private string specialLettersPolish = "ąćęłńóśżź";

        // Windows background colors
        private string darkGray = "#85857e";
        private string lightGray = "#cfcfc4";

        public MainWindow()
        {
            InitializeComponent();
            // Load category and data from .json file
            LoadData();
            categoryTextBlock.Text = "Category: " + category;
            pointsTextBlock.Text = "Points: " + points;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateUniformGrid();
        }

        private void CreateUniformGrid()
        {
            int wordsAmount = splittedEntry.Length;
            wordsStartIndex = new int[wordsAmount];
            int firstLine = StartLine(wordsAmount);
            int freeSpaceBeforeWordLength;
            int actualWord = -1;
            bool finish = false;
            for (int r = 0; r < rows; r++)
            {
                if (r + 1 == firstLine && !finish)
                {
                    actualWord++;
                    firstLine++;
                    freeSpaceBeforeWordLength = (columns - splittedEntry[actualWord].Length) / 2;
                    wordsStartIndex[actualWord] = r * 20 + freeSpaceBeforeWordLength + 1;
                    for (int b = 0; b < freeSpaceBeforeWordLength; b++)
                    {
                        windowsSpaceUniformGrid.Children.Add(CreateWindow(r * 20 + b + 1, lightGray));
                    }
                    for (int n = 0; n < splittedEntry[actualWord].Length; n++)
                    {
                        TextBlock window = CreateWindow(r * 20 + n + 1 + freeSpaceBeforeWordLength, darkGray);
                        window.TextAlignment = TextAlignment.Center;
                        windowsSpaceUniformGrid.Children.Add(window);
                    }
                    for (int a = 0; a < columns - freeSpaceBeforeWordLength - splittedEntry[actualWord].Length; a++)
                    {
                        windowsSpaceUniformGrid.Children.Add(
                            CreateWindow(r * 20 + a + 1 + freeSpaceBeforeWordLength + splittedEntry[actualWord].Length, lightGray)
                            );
                    }
                    if (actualWord == splittedEntry.Length - 1) finish = true;
                }
                else for (int c = 0; c < columns; c++) windowsSpaceUniformGrid.Children.Add(CreateWindow(r * 20 + c + 1, lightGray));
            }
        }

        private TextBlock CreateWindow(int index, string color)
        {
            // Set window properties
            TextBlock window = new TextBlock();
            window.Margin = new Thickness(2);
            window.Name = "window" + index;
            window.Background = new BrushConverter().ConvertFrom(color) as Brush;
            return window;
        }

        private int StartLine(int wordsAmount)
        {
            try
            {
                switch (wordsAmount)
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
                throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't determine start line!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
        }

        private void LoadData()
        {
            try
            {
                using (StreamReader file = new StreamReader("../../../data.json"))
                {
                    string json = file.ReadToEnd();
                    List<Data> items = JsonConvert.DeserializeObject<List<Data>>(json);
                    Data drawedData = DrawEntry(items);
                    splittedEntry = drawedData.entry;
                    category = drawedData.category;
                }
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("Couldn't find and load file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (JsonReaderException)
            {
                MessageBox.Show("Couldn't parse json file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("File with data is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            List<TextBlock> windowsToShow = windowsSpaceUniformGrid.Children.OfType<TextBlock>().ToList().Where(
                window => windowsNames.Contains(window.Name.ToString())).ToList(
                );
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
                if (window.Background.ToString() == (new BrushConverter().ConvertFrom(darkGray) as Brush).ToString() 
                    && window.Text == string.Empty) return;
            }
            WonGame();
        }

        private void WonGame()
        {
            MessageBox.Show("You Won!");
            points++;
            pointsTextBlock.Text = "Points: " + points;
            RestartGame();
        }

        private void RestartGame()
        {
            LoadData();
            categoryTextBlock.Text = "Category: " + category;
            windowsSpaceUniformGrid.Children.Clear();
            CreateUniformGrid();
            mistakes = 0;
            gallowsImage.Source = new BitmapImage(new Uri(imageSource + mistakes + ".png", UriKind.Relative));
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
