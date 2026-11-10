using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SudokuWPF
{
    internal static class SudokuNative
    {
        private const string DllName = "Sudoku.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GenerateNewBoard([In, Out] int[,] board, int n, int m);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool HasConflict([In, Out] int[,] board, int n, int m, int row, int col);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool IsBoardFullySolved([In, Out] int[,] board, int n, int m);
    }

    public partial class MainWindow : Window
    {
        private int[,] _board = new int[9, 9];
        private Button[,] _uiCells = new Button[9, 9];
        private int _triesRemaining = 3;
        private int _selectedRow = -1;

        private int _selectedCol = -1;

        private Button _selectedButton = null;



        public MainWindow()
        {
            InitializeComponent();

            BuildBoardUI();
            StartNewGame();

        }

        private void BuildBoardUI()
        {
           for (int r = 0; r < 9; r++)
           {
               for (int c = 0; c < 9; c++)
                 {
                    var btn = new Button
                    {
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.White,
                        BorderBrush = Brushes.LightGray,
                        Tag = new Point(r, c)
                    };

                    btn.Click += Cell_Click;
                    _uiCells[r, c] = btn;
                    int blockRow = r / 3;
                    int blockCol = c / 3;
                    string blockName = $"Block_{blockRow}_{blockCol}";
                    if (LogicalTreeHelper.FindLogicalNode(this, blockName) is UniformGrid targetBlock)
                    {
                        targetBlock.Children.Add(btn);
                   }
               }
           }
        }

        private void StartNewGame()
        { 
            _triesRemaining = 3;
            StatusText.Text = $"Tries remaining: {_triesRemaining}";
            StatusText.Foreground = Brushes.Black;
            ClearSelection();
            SudokuNative.GenerateNewBoard(_board, 9, 9);

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int val = _board[r, c];
                    _isGiven[r, c] = val != 0;
                    _uiCells[r, c].Content = val == 0 ? "" : val.ToString();
                    if (val != 0)
                    {
                        _uiCells[r, c].Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)); // Light gray
                        _uiCells[r, c].Foreground = Brushes.DarkSlateGray;
                        _uiCells[r, c].IsEnabled = true; 
                    }
                    else
                    {
                        _uiCells[r, c].Background = Brushes.White;
                        _uiCells[r, c].Foreground = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    }
                }
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedBtn && clickedBtn.Tag is Point pt)
            {
                int r = (int)pt.X;
                int c = (int)pt.Y;


                HighlightSelectedButton(clickedBtn, r, c);
            }
        }
        private void HighlightSelectedButton(Button btn, int r, int c)

        {
            if (_selectedButton != null)
            {

                int oldR = _selectedRow;



                int oldC = _selectedCol;

                _selectedButton.Background = (_board[oldR, oldC] != 0 && IsPreFilled(oldR, oldC))
                    ? new SolidColorBrush(Color.FromRgb(229, 231, 235))

                    : Brushes.White;
            }


            _selectedRow = r;
            _selectedCol = c;

            _selectedButton = btn;



            btn.Background = new SolidColorBrush(Color.FromRgb(254, 240, 138));
        }


        private bool[,] _isGiven = new bool[9, 9];

        private bool IsPreFilled(int r, int c) => _isGiven[r, c];

        private void NumberPad_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == -1 || IsPreFilled(_selectedRow, _selectedCol)) return;
            if (sender is Button numBtn && int.TryParse(numBtn.Content.ToString(), out int val))
            {
                int r = _selectedRow;

                int c = _selectedCol;
                int previous = _board[r, c];
                _board[r, c] = val;

                _uiCells[r, c].Content = val.ToString();

                bool hasError = SudokuNative.HasConflict(_board, 9, 9, r, c);

                if (hasError)
                {
                    _triesRemaining--;

                    _board[r, c] = previous;

                    _uiCells[r, c].Content = "";

                    StatusText.Text = $"Invalid move! Tries remaining: {_triesRemaining}";

                    StatusText.Foreground = Brushes.Red;

                    if (_triesRemaining <= 0)
                    {
                        MessageBox.Show("Game Over! Restarting...", "Sudoku");

                        StatusText.Foreground = Brushes.DarkRed;
                        StartNewGame();
                    }
                }
                else if (SudokuNative.IsBoardFullySolved(_board, 9, 9))

             {
                    MessageBox.Show("You win! Congratulations!", "Sudoku");
                    StatusText.Foreground = Brushes.Green;
                StartNewGame();
                }
                else
                {
                    StatusText.Text = $"Tries remaining: {_triesRemaining}";
                    StatusText.Foreground = Brushes.Black;
                }
            }

        }


        private void ClearCell_Click(object sender, RoutedEventArgs e)
        {
           if (_selectedRow != -1 && _selectedCol != -1)
            {
                
                if (!IsPreFilled(_selectedRow, _selectedCol))
               {
                    _board[_selectedRow, _selectedCol] = 0;
                    _uiCells[_selectedRow, _selectedCol].Content = "";
                  }
            }
        }

        private void ClearSelection()
        {
            _selectedRow = -1;

            _selectedCol = -1;
            _selectedButton = null;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}