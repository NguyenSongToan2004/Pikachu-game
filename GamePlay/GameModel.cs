using GamePlay;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class GameModel
{
    private int[,] table;
    private int width, height;
    private List<Point> path;


    //MERGE CODE 2
    public Dictionary<int, List<Point>> PossibleMatches { get; private set; }
    private static readonly int[] rowDirs = { -1, 1, 0, 0 };  // Các hướng: lên, xuống, trái, phải
    private static readonly int[] colDirs = { 0, 0, -1, 1 };  // Các hướng: lên, xuống, trái, phải


    public int Width { get => width; }
    public int Height { get => height; }

    public GameModel(int _width, int _height, int _numOfType)
    {
        width = _width;
        height = _height;
        table = new int[height, width];

        //MERGE CODE
        PossibleMatches = new Dictionary<int, List<Point>>();


        ConfigTable();
        InitializeBoard(_numOfType);


    }


    //MERGE CODE
    private void ConfigTable()
    {
        int count = 0;
        List<Point> list = new List<Point>();
        for (int i = 0; i < height; i++)
        {
            if (i == 0 || i == height - 1)
            {
                for (int j = 0; j < width; j++)
                {
                    table[i, j] = 0;
                    Point p = new Point(i, j);
                    list.Add(p);
                    count++;
                }
            }
            else
            {
                table[i, 0] = 0;
                Point p = new Point(i, 0);
                list.Add(p);
                count++;

                p = new Point(i, width - 1);
                list.Add(p);
                table[i, width - 1] = 0;
                count++;
            }
            String s = "";
            foreach (Point p in list)
            {
                s += p.ToString() + " || ";
            }
        }
    }



    private void InitializeBoard(int _numOfType)
    {
        // Sử dụng để lưu các ô đã sinh ra pokemon
        HashSet<int> cellIndex = new HashSet<int>();
        Random random = new Random();

        int pairCount = (width * height / 2) - (((width * 2) + (height - 2) * 2) / 2);


        for (int i = 0; i < pairCount; i++)
        {
            // Sinh ngẫu nhiên 1 loại pokemon (1 -> _numOfType)
            int typeOfPokemon = random.Next(1, _numOfType + 1);
            if (!PossibleMatches.ContainsKey(typeOfPokemon))
            {
                PossibleMatches[typeOfPokemon] = new List<Point>();
            }


            // Sinh ô thứ 1
            int cell1 = random.Next(0, width * height);
            while (cellIndex.Contains(cell1) || !IsOutBoundary(cell1))    //MERGE CODE

                cell1 = random.Next(0, width * height);
            table[cell1 / width, cell1 % width] = typeOfPokemon;

            PossibleMatches[typeOfPokemon].Add(new Point(cell1 / width, cell1 % width)); //MERGE CODE 2

            cellIndex.Add(cell1);

            // Sinh ô thứ 2
            int cell2 = random.Next(0, width * height);
            while (cellIndex.Contains(cell2) || !IsOutBoundary(cell2))    //MERGE CODE

                cell2 = random.Next(0, width * height);
            table[cell2 / width, cell2 % width] = typeOfPokemon;

            PossibleMatches[typeOfPokemon].Add(new Point(cell2 / width, cell2 % width)); //MERGE CODE 2

            cellIndex.Add(cell2);
        }
    }

    //MERGE CODE
    private bool IsOutBoundary(int cell)
    {
        int row = cell / width;  // Xác định hàng
        int col = cell % width;  // Xác định cột

        // Kiểm tra nếu ô nằm ở biên
        if (row == 0 || row == height - 1 || col == 0 || col == width - 1)
        {
            //System.Windows.MessageBox.Show("row : " + row + "|| col : " + col);
            return false; // Nằm trên biên
        }
        return true; // Nằm bên trong bảng
    }


    public int GetCell(int row, int col)
    {
        return table[row, col];
    }

    public bool IsSameType(int row1, int col1, int row2, int col2)
    {
        return table[row1, col1] == table[row2, col2];
    }

    public void RemovePair(int row1, int col1, int row2, int col2)
    {
        RemovePairPointFromPossibleMatches(row1, col1, row2, col2);        //MERGE CODE
        table[row1, col1] = 0;
        table[row2, col2] = 0;
    }

   



    //==========================MERGE CODE 2========================================
    private void RemovePairPointFromPossibleMatches(int row1, int col1, int row2, int col2)
    {
        int type = table[row1, col1];
        if (PossibleMatches.ContainsKey(type))
        {
            PossibleMatches[type].RemoveAll(p => (p.X == row1 && p.Y == col1) || (p.X == row2 && p.Y == col2));
            if (!(PossibleMatches[type].Count > 0))
            {
                PossibleMatches.Remove(type);
            }
        }
    }


    private int countShuffle = 0;
    public void ShuffleBoard()
    {
        // Thu thập tất cả các Pokémon còn lại
        List<int> remainingPokemon = new List<int>();
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (table[r, c] != 0)
                    remainingPokemon.Add(table[r, c]);
            }
        }

        // Xáo trộn danh sách Pokémon
        Random random = new Random();
        for (int i = remainingPokemon.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (remainingPokemon[i], remainingPokemon[j]) = (remainingPokemon[j], remainingPokemon[i]);
        }

        // Đặt lại Pokémon vào bảng, giữ nguyên vị trí các ô trống
        int index = 0;
        PossibleMatches.Clear();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (table[i, j] != 0)
                {
                    table[i, j] = remainingPokemon[index];

                    if (!PossibleMatches.ContainsKey(table[i, j]))
                    {
                        PossibleMatches[table[i, j]] = new List<Point>();
                    }
                    PossibleMatches[table[i, j]].Add(new Point(i, j));

                    index++;
                }
            }
        }

        // Kiểm tra nếu không có cặp hợp lệ, thì xáo trộn lại
        if (!HasValidPairs() && countShuffle < 10)
        {
            countShuffle++;
            ShuffleBoard(); // Gọi lại phương thức shuffle nếu không tìm thấy cặp hợp lệ
        }
        countShuffle = 0;
    }

    public bool HasValidPairs()
    {
        foreach (var entry in PossibleMatches)
        {
            var points = entry.Value; // Danh sách tọa độ của loại Pokémon hiện tại

            // Chỉ kiểm tra mỗi cặp (i, j) một lần với i < j
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    var p1 = points[i];
                    var p2 = points[j];

                    // Nếu hai tọa độ có thể kết nối được, trả về true
                    if (CanConnect(p1.X, p1.Y, p2.X, p2.Y))
                    {
                        return true;
                    }
                }
            }
        }
        return false;  // Không tìm thấy cặp hợp lệ
    }


    //==========================MERGE CODE========================================


    public bool CanConnect(int x1, int y1, int x2, int y2)
    {
        Point p1 = new Point(x1, y1);
        Point p2 = new Point(x2, y2);
        if (table[p1.X, p1.Y] != table[p2.X, p2.Y] || table[p1.X, p1.Y] == 0)
            return false;

        return BFSCheckPath(table, p1, p2) == true ? true : BFSCheckPath(table, p2, p1);
    }

    bool BFSCheckPath(int[,] board, Point start, Point end)
    {
        path = new List<Point>();
        int rows = height;
        int cols = width;
        var directions = new Point[] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
        // System.Windows.MessageBox.Show("DirCOunt : " + directions.Count());

        // BFS Queue: Mỗi phần tử lưu (tọa độ hiện tại, số lần rẽ, hướng di chuyển)
        var queue = new Queue<(Point, int, Point)>();
        var visited = new bool[rows, cols];
        //System.Windows.MessageBox.Show("DirCOunt lan 1 : " + directions.Count());
        Stack<Point> stack = new Stack<Point>();

        foreach (var dir in directions)
        {
            queue.Enqueue((start, 0, dir));
        }

        int finalTurn = -1;
        Point finalPoint = new Point(1000, 1000);
        while (queue.Count > 0)
        {
            var (current, turns, direction) = queue.Dequeue();

            finalPoint = current;
            finalTurn = turns;

            if (turns > 2)
                continue;

            if (turns == 1)
            {
                // Vector hướng từ current đến end
                var targetDirection = new Point(end.X - current.X, end.Y - current.Y);

                if (IsOppositeDirection(direction, targetDirection))
                    continue;
            }

            else if (turns == 2)
            {
                if (current.X != end.X && current.Y != end.Y)
                    continue;
            }

            if (!visited[current.X, current.Y] && turns <= 2)
                path.Add(current);

            visited[current.X, current.Y] = true;

            foreach (var dir in directions)
            {
                var next = new Point(current.X + dir.X, current.Y + dir.Y);

                if (IsValidMove(board, visited, next, rows, cols))
                {
                    int newTurns = (dir != direction) ? turns + 1 : turns;
                    queue.Enqueue((next, newTurns, dir));
                }

                if (next == end && turns <= 2)
                {
                    path.Add(end);
                    string s = "|| ";
                    foreach (Point p in stack)
                    {
                        s += p + " || ";
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetLinePaths(Canvas canvas)
    {
        // Kiểm tra nếu danh sách điểm rỗng
        if (path == null || path.Count == 0 || canvas == null) return;

        // Duyệt qua danh sách các điểm và reset trạng thái của từng ô
        foreach (var point in path)
        {
            // Kiểm tra nếu điểm nằm trong phạm vi của bảng
            if (point.X >= 0 && point.X < table.GetLength(0) && point.Y >= 0 && point.Y < table.GetLength(1))
            {
                // Đặt lại trạng thái của ô, ví dụ: reset về 0 (không có đường)
                table[(int)point.X, (int)point.Y] = 0;

                // Ẩn ô tương ứng trên giao diện (nếu tìm thấy)
                var cell = canvas.Children.OfType<System.Windows.UIElement>()
                    .FirstOrDefault(c => c is Border border &&
                                         border.Tag is Point p &&
                                         (int)p.X == (int)point.X &&
                                         (int)p.Y == (int)point.Y);
                if (cell != null)
                {
                    cell.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }
    }




    //private void FindFinalPath()
    //{
    //    Point end = path[path.Count - 1];
    //    Point start = path[0];
    //    List<Point> finalPath = new List<Point>();
    //    Point temp = end;
    //    path.Reverse();
    //    int turn = 0;
    //    string previousDirection = "None";

    //    Point pointSample = new Point(100, 100);
    //    Point pointDelete = pointSample;
    //    int[] checkArr = new int[path.Count()];
    //    for (int i = 0; i < path.Count; i++)
    //    {
    //        checkArr[i] = 1;
    //    }
    //    int indexArr = 0;
    //    do
    //    {
    //        turn = 0;
    //        //if (pointDelete != pointSample)
    //        //{
    //        //    path.Remove(pointDelete);
    //        //    // System.Windows.MessageBox.Show($"Lap lai {++count} lan || Da xoa {pointDelete}");
    //        //}
    //        pointDelete = pointSample;
    //        foreach (Point p in path)
    //        {
    //            if (p != start && p != end)
    //            {
    //                if (p.X == temp.X)
    //                {
    //                    if (Math.Abs(p.Y - temp.Y) == 1)
    //                    {
    //                        if (GetDirection(temp, p) != previousDirection)
    //                            turn++;
    //                        if (turn > 2)
    //                        {
    //                            // turn--;
    //                            // pointDelete = p;
    //                            checkArr[indexArr] = 0;
    //                            turn--;
    //                            indexArr++;
    //                            continue;
    //                        }
    //                        previousDirection = GetDirection(temp, p);
    //                        finalPath.Add(p);
    //                        temp = p;
    //                    }
    //                }
    //                else if (p.Y == temp.Y)
    //                {
    //                    if (Math.Abs(p.X - temp.X) == 1)
    //                    {
    //                        if (GetDirection(temp, p) != previousDirection)
    //                            turn++;
    //                        if (turn > 2)
    //                        {
    //                            // turn--;
    //                            // pointDelete = p;
    //                            checkArr[indexArr] = 0;
    //                            turn--;
    //                            indexArr++;
    //                            continue;
    //                        }
    //                        previousDirection = GetDirection(temp, p);
    //                        finalPath.Add(p);
    //                        temp = p;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                finalPath.Add(p);
    //                temp = p;
    //            }
    //            indexArr++;
    //        }
    //    } while (turn > 2);

    //    if (finalPath.Count == 2 && path.Count > 2)
    //    {
    //        string s = "";
    //        foreach (Point p in path)
    //        {
    //            s += $"{p} ||";
    //        }
    //        // System.Windows.MessageBox.Show("Duong di final : " + s);
    //    }
    //    path.Clear();
    //    indexArr = 0;
    //    foreach (Point p in finalPath)
    //    {
    //        if (checkArr[indexArr] == 1)
    //        {
    //            path.Add(p);
    //        }
    //        indexArr++;
    //    }
    //    //path = finalPath;
    //    // System.Windows.MessageBox.Show("So luong point : " + path.Count());
    //}
    List<Point> FindOptimalPath(int[,] board, List<Point> listPoint, int maxTurns)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        Point start = listPoint[0];
        Point end = listPoint[listPoint.Count() - 1];

        var directions = new Point[]
        {
    new Point(0, 1),  // Right
    new Point(0, -1), // Left
    new Point(1, 0),  // Down
    new Point(-1, 0)  // Up
        };

        // BFS Queue: Mỗi phần tử là (tọa độ hiện tại, số lần rẽ, hướng di chuyển, lộ trình)
        var queue = new Queue<(Point current, int turns, Point direction, List<Point> path)>();
        var visited = new Dictionary<(Point, int, Point), bool>();

        // Khởi tạo hàng đợi
        foreach (var dir in directions)
        {
            queue.Enqueue((start, 0, dir, new List<Point> { start }));
        }

        while (queue.Count > 0)
        {
            var (current, turns, direction, path) = queue.Dequeue();

            // Nếu đạt đến điểm kết thúc với số lần rẽ <= maxTurns
            if (current == end && turns <= maxTurns)
            {
                path.Add(end);
                return path;
            }

            // Đánh dấu ô hiện tại với trạng thái số lần rẽ và hướng
            var state = (current, turns, direction);
            if (visited.ContainsKey(state))
                continue;
            visited[state] = true;

            // Duyệt qua các hướng di chuyển
            foreach (var dir in directions)
            {
                var next = new Point(current.X + dir.X, current.Y + dir.Y);

                // Kiểm tra tính hợp lệ của bước di chuyển
                if (IsValidMove(board, next, rows, cols))
                {
                    int newTurns = (dir != direction) ? turns + 1 : turns;

                    // Nếu số lần rẽ vượt quá giới hạn, bỏ qua
                    if (newTurns > maxTurns)
                        continue;

                    // Thêm vào hàng đợi
                    var newPath = new List<Point>(path) { next };
                    queue.Enqueue((next, newTurns, dir, newPath));
                }
            }
        }

        // Không tìm được đường đi hợp lệ
        return new List<Point>();
    }

    public bool ShowPath(Canvas canvas, int cellSize)
    {
        // Kiểm tra nếu không có đủ điểm để vẽ
        path = FindOptimalPath(table, path, 2);
        if (path == null || path.Count < 2 || canvas == null) return false;
        // System.Windows.MessageBox.Show("So luong con lai : " + path.Count());

        for (int i = 0; i < path.Count - 1; i++)
        {
            var start = path[i];
            var end = path[i + 1];

            // Kiểm tra xem đường có phải là đường xiên
            if (start.X != end.X && start.Y != end.Y)
            {
                continue; // Bỏ qua nếu cả hàng (X) và cột (Y) đều thay đổi
            }

            double startX = start.Y * cellSize + cellSize / 2.0;
            double startY = start.X * cellSize + cellSize / 2.0;
            double endX = end.Y * cellSize + cellSize / 2.0;
            double endY = end.X * cellSize + cellSize / 2.0;

            Line line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 5,
                Tag = "PathLine" // Đặt Tag để nhận diện
            };

            canvas.Children.Add(line);
        }
        return true;
    }


    private string GetDirection(Point from, Point to)
    {
        if (from.X == to.X)
        {
            if (to.Y > from.Y) return "Right";
            if (to.Y < from.Y) return "Left";
        }
        else if (from.Y == to.Y)
        {
            if (to.X > from.X) return "Down";
            if (to.X < from.X) return "Up";
        }
        return "None"; // Không liền kề hoặc không cùng hàng/cột
    }

    private bool IsOppositeDirection(Point dir1, Point dir2)
    {
        // Chuẩn hóa hướng để so sánh (chỉ giữ -1, 0, 1)
        dir1 = NormalizeDirection(dir1);
        dir2 = NormalizeDirection(dir2);

        // Hai hướng ngược chiều nếu tích vô hướng của chúng là -1
        return dir1.X * dir2.X + dir1.Y * dir2.Y == -1;
    }

    private Point NormalizeDirection(Point dir)
    {
        // Chuẩn hóa vector để có giá trị -1, 0, 1
        int x = dir.X == 0 ? 0 : dir.X / Math.Abs(dir.X);
        int y = dir.Y == 0 ? 0 : dir.Y / Math.Abs(dir.Y);
        return new Point(x, y);
    }

    public void ClearPaths(Canvas canvas)
    {
        if (canvas == null) return;

        // Lấy tất cả các phần tử có Tag là "PathLine"
        var linesToRemove = canvas.Children.OfType<Line>().Where(line => line.Tag?.ToString() == "PathLine").ToList();

        // Xóa các phần tử này khỏi Canvas
        foreach (var line in linesToRemove)
        {
            canvas.Children.Remove(line);
        }
    }
    bool IsValidMove(int[,] board, Point point, int rows, int cols)
    {
        return point.X >= 0 && point.X < rows &&
               point.Y >= 0 && point.Y < cols &&
               board[point.X, point.Y] == 0; // 0 đại diện cho ô trống
    }

    bool IsValidMove(int[,] board, bool[,] visited, Point next, int rows, int cols)
    {
        return next.X >= 0 && next.X < rows &&
               next.Y >= 0 && next.Y < cols &&
               !visited[next.X, next.Y] &&
               board[next.X, next.Y] == 0; // Ô trống
    }

}

