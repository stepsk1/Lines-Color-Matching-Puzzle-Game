using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Linije_Filip_Milosavljevic_65_2019
{
    public partial class Form1 : Form
    {
        IDatabase db;

        private SoundPlayer impactSoundPlayer;
        private SoundPlayer gameOverSoundPlayer;
        private SoundPlayer gameStartSoundPlayer;

        Random random = new Random();

        private const int EMPTY = -1;

        private const int n = 9;
        private const int cellSize = 60;

        private const int boardMarginTop = 150;
        private const int boardMarginLeft = 100;

        private const int startX = 100;
        private const int startY = 100;

        private const int previewMarginTop = 40;
        private const int previewMarginLeft = 260;

        private const int lineThickness = 1;

        private const int windowSizeHeight = 800;
        private const int windowSizeWidth = 800;

        private int[,] board;
        private int[] preview;
        private Queue<(int x, int y)> bfsQueue = new Queue<(int x, int y)>();

        private Point currentPosition;

        private int score;
        private const int minMatch = 5;
        private int elapsedTime;

        private string path = "Slike/";

        string[] colorToImageMap = new string[]
            {
                "Braon.bmp",
                "Crvena.bmp",
                "Ljubicasta.bmp",
                "Plava.bmp",
                "SvetloPlava.bmp",
                "Zelena.bmp",
                "Borda.png",
                "Zelenkasta.png"
            };

        public Form1()
        {
            InitializeComponent();

            db = new Database();

            InitForm();
            InitBoard();
            InitPreview();
        }

        private void InitForm()
        {
            currentPosition = new Point(-1, -1);

            this.Width = windowSizeWidth;
            this.Height = windowSizeHeight;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Text = "Linije";
            this.ShowIcon = false;
            this.ControlBox = false;

            buttonNewGame.Location = new Point(this.ClientSize.Width - buttonNewGame.Width - 20, previewMarginTop);
            buttonEndGame.Location = new Point(this.ClientSize.Width - buttonEndGame.Width - 20, this.ClientSize.Height - buttonEndGame.Height - previewMarginTop);

            labelSakupio.Location = new Point(20, previewMarginTop);
            labelScore.Location = new Point(50 + labelSakupio.Width, previewMarginTop);

            labelNajbolje.Location = new Point(20, previewMarginTop + 20);
            labelBestScore.Location = new Point(50 + labelNajbolje.Width, previewMarginTop + 20);

            labelTime.Location = new Point(20, previewMarginTop + 40);
            labelTimeValue.Location = new Point(30 + labelTime.Width, previewMarginTop + 40);

            labelTime.Visible = false;
            labelTimeValue.Visible = false;

            labelScore.Visible = false;
            labelBestScore.Visible = false;

            impactSoundPlayer = new SoundPlayer("Zvuk/impact.wav");
            gameOverSoundPlayer = new SoundPlayer("Zvuk/gameOver.wav");
            gameStartSoundPlayer = new SoundPlayer("Zvuk/start.wav");
        }

        private void InitBoard()
        {
            ClearBoard(tableLayoutPanelBoard);

            tableLayoutPanelBoard.RowCount = n;
            tableLayoutPanelBoard.ColumnCount = n;

            tableLayoutPanelBoard.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanelBoard.Margin = new Padding(0);
            tableLayoutPanelBoard.Padding = new Padding(0);

            tableLayoutPanelBoard.Location = new Point(boardMarginLeft, boardMarginTop);
            tableLayoutPanelBoard.Size = new Size(n * cellSize + 20, n * cellSize + 20);

            board = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    PictureBox cell = new PictureBox()
                    {
                        Size = new Size(cellSize, cellSize),
                        BorderStyle = BorderStyle.None,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = Image.FromFile(path + "Prazno.bmp"),
                        Margin = new Padding(0),
                        Padding = new Padding(0)
                    };

                    cell.Click += cellClicked;
                    board[i, j] = EMPTY;

                    tableLayoutPanelBoard.Controls.Add(cell, i, j);
                }
            }

            for (int i = 0; i < n; i++)
            {
                tableLayoutPanelBoard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, cellSize));
                tableLayoutPanelBoard.RowStyles.Add(new RowStyle(SizeType.Absolute, cellSize));
            }
        }
        private void cellClicked(object sender, EventArgs e)
        {
            PictureBox clickedCell = sender as PictureBox;

            int x = tableLayoutPanelBoard.GetPositionFromControl(clickedCell).Column;
            int y = tableLayoutPanelBoard.GetPositionFromControl(clickedCell).Row;

            // first click can't be empty
            if (currentPosition.X == -1 && currentPosition.Y == -1 && board[x, y] == EMPTY) return;

            // first click has to be colored ball
            if (currentPosition.X == -1 && currentPosition.Y == -1 && board[x, y] != EMPTY)
            {
                currentPosition = new Point(x, y);

                clickedCell.Image = Image.FromFile("ClickedSlike/" + colorToImageMap[board[x, y]]);

                //labelScore.Text = $"id: {board[x, y]} color: {colorToImageMap[board[x, y]]} x: {x} y: {y}";
                return;
            }

            // second click has to be empty field
            if (currentPosition.X != -1 && currentPosition.Y != -1 && board[x, y] == EMPTY)
            {
                if (IsValidPath(x, y) == true)
                {
                    impactSoundPlayer.Play();
                    //labelBestScore.Text = $"VALID x: {x} y: {y} board[x, y]: {board[x,y]}";

                    // swap cells
                    int temp = board[x, y];
                    board[x, y] = board[currentPosition.X, currentPosition.Y];
                    board[currentPosition.X, currentPosition.Y] = temp;

                    // swap images
                    PictureBox currentCell = tableLayoutPanelBoard.GetControlFromPosition(currentPosition.X, currentPosition.Y) as PictureBox;
                    PictureBox targetCell = tableLayoutPanelBoard.GetControlFromPosition(x, y) as PictureBox;

                    currentCell.Image = Image.FromFile(path + "Prazno.bmp");
                    targetCell.Image = Image.FromFile(path + colorToImageMap[board[x, y]]);

                    // restart current position
                    currentPosition.X = -1;
                    currentPosition.Y = -1;

                    // add from the preview panel balls at random positions
                    HashSet<(int, int)> pairs = GetRandomPairs(3);
                    int i = 0;

                    foreach ((int x, int y) pair in pairs)
                    {
                        PictureBox targetPreviewCell = tableLayoutPanelBoard.GetControlFromPosition(pair.x, pair.y) as PictureBox;

                        // update board value and cell image
                        targetPreviewCell.Image = Image.FromFile(path + colorToImageMap[preview[i]]);
                        board[pair.x, pair.y] = preview[i];

                        i++;
                    }

                    CheckGameEnd();

                    // create new preview fields
                    CreatePreview();

                    // fresh teh que
                    bfsQueue.Clear();

                    CheckScore();
                    return;
                }
                else
                {
                    //labelBestScore.Text = $"NOT VALID x: {x} y: {y} board[x, y]: {board[x, y]}";
                    return;
                }
            }

            // if second click is ball, change current to ball
            if (currentPosition.X != -1 && currentPosition.Y != -1 && board[x, y] != EMPTY)
            {

                PictureBox previousClicked = tableLayoutPanelBoard.GetControlFromPosition(currentPosition.X, currentPosition.Y) as PictureBox;
                previousClicked.Image = Image.FromFile("Slike/" + colorToImageMap[board[currentPosition.X, currentPosition.Y]]);

                currentPosition = new Point(x, y);

                clickedCell = tableLayoutPanelBoard.GetControlFromPosition(currentPosition.X, currentPosition.Y) as PictureBox;
                clickedCell.Image = Image.FromFile("ClickedSlike/" + colorToImageMap[board[currentPosition.X, currentPosition.Y]]);

                //labelScore.Text = $"> id: {board[x, y]} color: {colorToImageMap[board[x, y]]} x: {x} y: {y}";
                return;
            }

        }

        private bool IsValidPath(int endX, int endY)
        {
            // Looking for a path from currentPosition to (v, k)
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((currentPosition.X, currentPosition.Y));

            bool found = false;
            (int, int) target = (endX, endY);
            bool[,] visited = new bool[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    visited[i, j] = false;
                }
            }

            visited[currentPosition.X, currentPosition.Y] = true;

            while (!found && queue.Count > 0)
            {
                var tmp = queue.Dequeue();
                if (tmp == target)
                {
                    found = true;
                }
                else
                {
                    // Cell left
                    if (tmp.Item2 - 1 >= 0 && board[tmp.Item1, tmp.Item2 - 1] == EMPTY && !visited[tmp.Item1, tmp.Item2 - 1])
                    {
                        queue.Enqueue((tmp.Item1, tmp.Item2 - 1));
                        visited[tmp.Item1, tmp.Item2 - 1] = true;
                    }

                    // Cell right
                    if (tmp.Item2 + 1 < 9 && board[tmp.Item1, tmp.Item2 + 1] == EMPTY && !visited[tmp.Item1, tmp.Item2 + 1])
                    {
                        queue.Enqueue((tmp.Item1, tmp.Item2 + 1));
                        visited[tmp.Item1, tmp.Item2 + 1] = true;
                    }

                    // Cell top
                    if (tmp.Item1 - 1 >= 0 && board[tmp.Item1 - 1, tmp.Item2] == EMPTY && !visited[tmp.Item1 - 1, tmp.Item2])
                    {
                        queue.Enqueue((tmp.Item1 - 1, tmp.Item2));
                        visited[tmp.Item1 - 1, tmp.Item2] = true;
                    }

                    // Cell bottom
                    if (tmp.Item1 + 1 < 9 && board[tmp.Item1 + 1, tmp.Item2] == EMPTY && !visited[tmp.Item1 + 1, tmp.Item2])
                    {
                        queue.Enqueue((tmp.Item1 + 1, tmp.Item2));
                        visited[tmp.Item1 + 1, tmp.Item2] = true;
                    }
                }
            }

            return found;
        }

        private void InitPreview()
        {
            ClearBoard(tableLayoutPanelPreview);

            tableLayoutPanelPreview.RowCount = 1;
            tableLayoutPanelPreview.ColumnCount = 3;

            tableLayoutPanelPreview.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanelPreview.Margin = new Padding(0);
            tableLayoutPanelPreview.Padding = new Padding(0);

            tableLayoutPanelPreview.Location = new Point(previewMarginLeft, previewMarginTop);
            tableLayoutPanelPreview.Size = new Size(3 * cellSize + 9, cellSize + 5);

            preview = new int[3];

            for (int i = 0; i < 3; i++)
            {
                PictureBox cell = new PictureBox()
                {
                    Size = new Size(cellSize, cellSize),
                    BorderStyle = BorderStyle.None,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Image.FromFile(path + "Prazno.bmp"),
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                preview[i] = EMPTY;

                tableLayoutPanelPreview.Controls.Add(cell, i, 0);
            }

            for (int i = 0; i < 3; i++)
            {
                tableLayoutPanelPreview.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, cellSize));
            }
        }

        private void CreatePreview()
        {
            for (int i = 0; i < 3; i++)
            {
                int id = random.Next(colorToImageMap.Length);

                PictureBox targetCell = tableLayoutPanelPreview.GetControlFromPosition(i, 0) as PictureBox;
                targetCell.Image = Image.FromFile(path + colorToImageMap[id]);

                preview[i] = id;
                targetCell.Tag = colorToImageMap[id];
            }
        }

        private void CreateBoard()
        {
            HashSet<(int, int)> pairs = GetRandomPairs(3);

            foreach ((int x, int y) pair in pairs)
            {
                int id = random.Next(colorToImageMap.Length);

                PictureBox targetCell = tableLayoutPanelBoard.GetControlFromPosition(pair.x, pair.y) as PictureBox;
                targetCell.Image = Image.FromFile(path + colorToImageMap[id]);

                board[pair.x, pair.y] = id;
            }
        }

        private void ClearBoardAndPreview()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (board[i, j] != EMPTY)
                    {
                        PictureBox targetCell = tableLayoutPanelBoard.GetControlFromPosition(i, j) as PictureBox;
                        targetCell.Image = Image.FromFile(path + "Prazno.bmp");

                        board[i, j] = EMPTY;
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                PictureBox targetCell = tableLayoutPanelBoard.GetControlFromPosition(i, 0) as PictureBox;
                targetCell.Image = Image.FromFile(path + "Prazno.bmp");

                preview[i] = EMPTY;
            }
        }

        private HashSet<(int, int)> GetRandomPairs(int numOfPairs)
        {
            HashSet<(int, int)> pairs = new HashSet<(int, int)>();

            while (pairs.Count < numOfPairs)
            {
                int x = random.Next(n);
                int y = random.Next(n);

                if (board[x, y] == EMPTY)
                {
                    var pair = (x, y);

                    if (pairs.Contains(pair) == false)
                        pairs.Add(pair);
                }
            }

            return pairs;
        }

        private void ClearBoard(TableLayoutPanel panel)
        {
            panel.Controls.Clear();
            panel.ColumnStyles.Clear();
            panel.RowStyles.Clear();
        }

        private void buttonKraj_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Da li ste sigurni da želite da napustite?", "Kraj Igre Linije", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //gameOverSoundPlayer.Play();
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private bool IsBoardFull()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (board[i, j] == EMPTY)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CheckScore()
        {
            List<Point> horizontalMatches = GetMatchedCoordinates(true);
            List<Point> verticalMatches = GetMatchedCoordinates(false);
            List<Point> diagonalUpMatches = GetDiagonalMatches(true);
            List<Point> diagonalDownMatches = GetDiagonalMatches(false);

            List<Point> allMatches = horizontalMatches.Concat(verticalMatches)
                                                      .Concat(diagonalUpMatches)
                                                      .Concat(diagonalDownMatches)
                                                      .ToList();

            if (allMatches.Count > 0)
            {
                foreach (Point match in allMatches)
                {
                    if (board[match.X, match.Y] != EMPTY)
                    {
                        score++;

                        PictureBox targetCell = tableLayoutPanelBoard.GetControlFromPosition(match.X, match.Y) as PictureBox;
                        targetCell.Image = Image.FromFile(path + "Prazno.bmp");
                        board[match.X, match.Y] = EMPTY;
                    }
                }

                labelScore.Text = score + "";
            }
        }

        private List<Point> GetMatchedCoordinates(bool horizontal)
        {
            List<Point> matchedPoints = new List<Point>();

            for (int i = 0; i < n; i++)
            {
                int colorID = EMPTY;
                List<Point> matches = new List<Point>();

                for (int j = 0; j < n; j++)
                {
                    int x = horizontal ? i : j;
                    int y = horizontal ? j : i;

                    if (board[x, y] != EMPTY && colorID == EMPTY)
                    {
                        colorID = board[x, y];
                    }

                    if (colorID != EMPTY && board[x, y] == colorID)
                    {
                        matches.Add(new Point(x, y));
                    }
                    else
                    {
                        if (matches.Count >= minMatch && matches.Count <= n)
                        {
                            matchedPoints.AddRange(matches);
                        }

                        colorID = EMPTY;
                        matches.Clear();
                    }
                }

                if (matches.Count >= minMatch && matches.Count <= n)
                {
                    matchedPoints.AddRange(matches);
                }
            }

            return matchedPoints;
        }

        private List<Point> GetDiagonalMatches(bool up)
        {
            List<Point> matchedPoints = new List<Point>();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int colorID = board[i, j];
                    List<Point> matches = new List<Point>();

                    int dx = up ? -1 : 1;
                    int dy = 1;

                    for (int k = 0; k < minMatch; k++)
                    {
                        int x = i + (k * dx);
                        int y = j + (k * dy);

                        if (x >= 0 && x < n && y >= 0 && y < n && board[x, y] == colorID)
                        {
                            matches.Add(new Point(x, y));
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (matches.Count >= minMatch)
                    {
                        matchedPoints.AddRange(matches);
                    }
                }
            }

            return matchedPoints;
        }

        private bool NoMoreMovesAvailable()
        {
            int emptyCells = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (board[i, j] == EMPTY)
                    {
                        emptyCells++;

                        if (emptyCells > 2) 
                            return false;
                    }
                }
            }

            return true;
        }

        private void CheckGameEnd()
        {

            if (IsBoardFull() == true || NoMoreMovesAvailable() == true )
            {
                timer1.Stop();

                if ( score > 0 ) db.InsertScore(score, elapsedTime);

                gameOverSoundPlayer.Play();

                DialogResult result = MessageBox.Show($"Igra je gotova, Vaš rezultat je: {labelScore.Text} za {labelTimeValue.Text}\nDa li želite ponovo da igrate?", "Kraj Igre Linije", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    StartNewGame();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            ClearBoardAndPreview();

            labelScore.Visible = true;
            labelBestScore.Visible = true;

            labelTime.Visible = true;
            labelTimeValue.Visible = true;

            score = 0;

            labelScore.Text = score + "";

            Score bestScore = db.GetBestScore();

            if ( bestScore == null)
                labelBestScore.Text = "Nema Podataka";
            else 
                labelBestScore.Text = $"{bestScore.score} ({bestScore.time}s)";

            CreatePreview();
            CreateBoard();

            elapsedTime = -1;
            timer1.Start();

            gameStartSoundPlayer.Play();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            elapsedTime++;

            labelTimeValue.Text = elapsedTime + " s";
        }
    }
}