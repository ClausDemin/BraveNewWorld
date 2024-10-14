using System;

namespace BraveNewWorld
{
    public static class Program
    {
        public static void Main()
        {
            const string MapFolder = "map";
            const string MapFilename = "Map.txt";

            const char PlayerSymbol = 'o';
            const char TreasureSymbol = '*';

            int endGamePauseMs = 1000;

            int playerPositionX = 1;
            int playerPositionY = 1;

            int playerScore = 0;


            Console.CursorVisible = false;

            string mapFilePath = FindFile(MapFolder, MapFilename);
            char[,] map = ParseFile(mapFilePath);

            PlacePlayer(map, playerPositionX, playerPositionY, PlayerSymbol);

            int treasuresCount = PlaceTreasure(map, TreasureSymbol);

            int playerScoreWidgetX = map.GetLength(0);
            int playerScoreWidgetY = 0;

            DrawMap(map);
            DrawTreasures(map, TreasureSymbol);
            DrawPlayer(playerPositionX, playerPositionY, PlayerSymbol);

            while (treasuresCount > 0)
            {
                if (Console.KeyAvailable)
                {
                    HandlePlayerInput
                    (
                        ref playerPositionX,
                        ref playerPositionY,
                        map, PlayerSymbol,
                        TreasureSymbol,
                        ref playerScore,
                        ref treasuresCount
                    );
                }
                DrawPlayerScore(playerScoreWidgetX, playerScoreWidgetY, playerScore);
            };

            Thread.Sleep(endGamePauseMs);

            Console.Clear();
            PrintWonMessage(playerScore);
        }

        private static void DrawMap(char[,] map, ConsoleColor wallCollor = ConsoleColor.DarkBlue)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = wallCollor;

            for (int i = 0; i < map.GetLength(1); i++)
            {
                for (int j = 0; j < map.GetLength(0); j++)
                {
                    Console.Write(map[j, i]);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = defaultColor;
        }

        private static void DrawTreasures(char[,] map, char treasureSymbol, ConsoleColor treasuresColor = ConsoleColor.Cyan)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            Console.ForegroundColor = treasuresColor;

            for (int i = 0; i < map.GetLength(1); i++)
            {
                for (int j = 0; j < map.GetLength(0); j++)
                {
                    if (map[j, i] == treasureSymbol)
                    {
                        Console.SetCursorPosition(j, i);
                        Console.Write(treasureSymbol);
                    }
                }
            }

            Console.SetCursorPosition(map.GetLength(1), map.GetLength(0));
            Console.ForegroundColor = defaultColor;
        }

        private static void DrawPlayer(int x, int y, char playerSymbol, ConsoleColor playerColor = ConsoleColor.Yellow)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = playerColor;

            Console.SetCursorPosition(x, y);
            Console.Write(playerSymbol);

            Console.ForegroundColor = defaultColor;
        }

        private static void DrawPlayerScore(int x, int y, int playerScore, ConsoleColor scoreColor = ConsoleColor.Green)
        {
            int currentX = Console.CursorLeft;
            int currentY = Console.CursorTop;

            ConsoleColor defaultColor = Console.ForegroundColor;

            string score = $" Score: {playerScore}";

            Console.ForegroundColor = scoreColor;
            Console.SetCursorPosition(x, y);
            Console.Write(score);

            Console.ForegroundColor = defaultColor;
            Console.SetCursorPosition(currentX, currentY);
        }

        private static char[,] ParseFile(string fileDirectory)
        {
            string[] mapStrings = null;
            char[,] mapData = null;

            if (File.Exists(fileDirectory))
            {
                mapStrings = File.ReadAllLines(fileDirectory);
            }

            mapData = ToCharArray(mapStrings);

            return mapData;
        }

        private static char[,] ToCharArray(string[] lines)
        {
            char[,] result = new char[GetMaxStringLength(lines), lines.Length];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = lines[j][i];
                }
            }

            return result;
        }

        private static int PlaceTreasure(char[,] map, char treasureSymbol)
        {
            int treasuresCount = 0;

            for (int i = 0; i < map.GetLength(1); i++)
            {
                for (int j = 0; j < map.GetLength(0); j++)
                {
                    if (char.IsWhiteSpace(map[j, i]))
                    {
                        map[j, i] = treasureSymbol;
                        treasuresCount++;
                    }
                }
            }

            return treasuresCount;
        }

        private static void PlacePlayer(char[,] map, int x, int y, char playerSymbol)
        {
            if (char.IsWhiteSpace(map[x, y]))
            {
                map[x, y] = playerSymbol;
            }
            else throw new ArgumentOutOfRangeException();
        }

        private static int GetMaxStringLength(string[] lines)
        {
            int maxStringLength = int.MinValue;

            foreach (var line in lines)
            {
                if (line.Length > maxStringLength)
                {
                    maxStringLength = line.Length;
                }
            }

            return maxStringLength;
        }

        private static string FindFile(string folder, string filename, string topLevelFolder = "BraveNewWorld")
        {
            string[] queue = null;
            string[] visited = null;

            string directory = string.Empty;
            directory = Directory.GetCurrentDirectory();

            queue = Enqueue(queue, directory);
            visited = Enqueue(visited, directory);

            DirectoryInfo directoryInfo = new DirectoryInfo(directory);


            while
            (
                (directoryInfo.Name == folder) == false &&
                (directoryInfo.Name == topLevelFolder) == false
            )
            {
                foreach (var subDirectory in Directory.GetDirectories(directory))
                {
                    if (Contains(visited, subDirectory) == false)
                    {
                        queue = Enqueue(queue, subDirectory);
                    }
                }

                if (GetLastElementIndex(queue) + 1 == 0)
                {
                    directoryInfo = Directory.GetParent(directory);
                    directory = directoryInfo.FullName;
                }
                else
                {
                    directory = Dequeue(ref queue);
                    directoryInfo = new DirectoryInfo(directory);

                    if (Contains(visited, directory) == false)
                    {
                        visited = Enqueue(visited, directory);
                    }
                }
            }

            if (directoryInfo.Name == topLevelFolder)
            {
                foreach (var subDirectory in directoryInfo.GetDirectories())
                {
                    if (subDirectory.Name == folder)
                    {
                        directory = subDirectory.FullName;
                    }
                }
            }

            return directory + $"\\{filename}";
        }

        private static void HandlePlayerInput
        (
            ref int x,
            ref int y,
            char[,] map,
            char playerSymbol,
            char treasureSymbol,
            ref int playerScore,
            ref int treasuresCount,
            ConsoleKey upMovement = ConsoleKey.UpArrow,
            ConsoleKey downMovement = ConsoleKey.DownArrow,
            ConsoleKey leftMovement = ConsoleKey.LeftArrow,
            ConsoleKey rightMovement = ConsoleKey.RightArrow

        )
        {

            ConsoleKey command = Console.ReadKey(true).Key;

            if (command == upMovement)
            {
                if (IsMovementAvailable(map, x, y - 1))
                {
                    ClearPreviousPosition(x, y, map);
                    MoveDirection(x, y - 1, map, playerSymbol, treasureSymbol, ref playerScore, ref treasuresCount);
                    --y;
                }
            }
            else if (command == downMovement)
            {
                if (IsMovementAvailable(map, x, y + 1))
                {
                    ClearPreviousPosition(x, y, map);
                    MoveDirection(x, y + 1, map, playerSymbol, treasureSymbol, ref playerScore, ref treasuresCount);
                    ++y;
                }
            }
            else if (command == leftMovement)
            {
                if (IsMovementAvailable(map, x - 1, y))
                {
                    ClearPreviousPosition(x, y, map);
                    MoveDirection(x - 1, y, map, playerSymbol, treasureSymbol, ref playerScore, ref treasuresCount);
                    --x;
                }

            }
            else if (command == rightMovement)
            {
                if (IsMovementAvailable(map, x + 1, y))
                {
                    ClearPreviousPosition(x, y, map);
                    MoveDirection(x + 1, y, map, playerSymbol, treasureSymbol, ref playerScore, ref treasuresCount);
                    ++x;
                }
            }
        }

        private static void ClearPreviousPosition(int x, int y, char[,] map, char voidSymbol = ' ')
        {
            map[x, y] = voidSymbol;

            Console.SetCursorPosition(x, y);
            Console.Write(' ');
        }

        private static void MoveDirection
        (
            int x,
            int y,
            char[,] map,
            char playerSymbol,
            char treasureSymbol,
            ref int playerScore,
            ref int treasuresCount,
            int collectebleCost = 100
        )
        {
            if (map[x, y] == treasureSymbol)
            {
                CollectTreasure(ref playerScore, ref treasuresCount, collectebleCost);
            }
            map[x, y] = playerSymbol;

            Console.SetCursorPosition(x, y);
            Console.Write(playerSymbol);
        }

        private static bool IsMovementAvailable(char[,] map, int x, int y, char wallSymbol = '#')
        {
            return ((map[x, y] == wallSymbol) == false) && InBounds(map, x, y);
        }

        private static bool InBounds(char[,] map, int x, int y)
        {
            return x > 0 && x < map.GetLength(0) && y > 0 && y < map.GetLength(1);
        }

        private static void CollectTreasure(ref int playerScore, ref int treasuresCount, int treasureCost)
        {
            playerScore += treasureCost;
            --treasuresCount;
        }

        private static string[] Enqueue(string[] queue, string value)
        {
            if (IsExpandNeeded(queue))
            {
                queue = Expand(queue);
            }

            int freeSpaceIndex = GetLastElementIndex(queue) + 1;

            queue[freeSpaceIndex] = value;

            return queue;
        }

        private static string Dequeue(ref string[] queue, float fillPercentageToCollapse = 0.25f)
        {
            string result = string.Empty;

            float fillPercentage = 1f;

            if (IsArrayNotInitialized(queue) == false)
            {
                result = queue[0];
                queue[0] = null;

                int LastElementIndex = GetLastElementIndex(queue);

                if (LastElementIndex > 0)
                {
                    ShiftArrayToLeft(queue);
                    fillPercentage = (float)LastElementIndex / queue.Length;

                    if (fillPercentage <= fillPercentageToCollapse)
                    {
                        queue = Collapse(queue);
                    }
                }
            }

            return result;
        }

        private static string[] Expand(string[] strings, int growthRate = 2)
        {
            string[] result;

            if (IsArrayNotInitialized(strings))
            {
                result = new string[growthRate];
            }
            else
            {
                result = new string[strings.Length * growthRate];

                for (int i = 0; i < strings.Length; i++)
                {
                    result[i] = strings[i];
                }
            }

            return result;
        }

        private static string[] Collapse(string[] strings, int compressionRate = 2)
        {
            string[] result = new string[0];

            if (IsArrayNotInitialized(strings) == false)
            {
                result = new string[strings.Length / compressionRate];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = strings[i];
                }
            }

            return result;
        }

        private static int GetLastElementIndex(string[] strings, int notInitializedOrEmptyCode = -1)
        {
            int result = 0;

            if (IsArrayNotInitialized(strings))
            {
                result = notInitializedOrEmptyCode;

                return result;
            }
            else
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    if (string.IsNullOrEmpty(strings[i]))
                    {
                        if (i == strings.Length - 1 || string.IsNullOrEmpty(strings[i + 1]))
                        {
                            return i - 1;
                        }
                    }
                }
            }

            return strings.Length - 1;
        }

        private static void ShiftArrayToLeft(string[] strings, int index = 0)
        {
            if (IsArrayNotInitialized(strings) == false)
            {
                int lastElementIndex = GetLastElementIndex(strings);

                string buffer = strings[index];

                for (int i = index; i < lastElementIndex; i++)
                {
                    strings[i] = strings[i + 1];
                }

                strings[lastElementIndex] = buffer;
            }
        }

        private static bool IsArrayNotInitialized(string[] strings)
        {
            return strings == null || strings.Length == 0;
        }

        private static bool IsExpandNeeded(string[] strings)
        {
            return IsArrayNotInitialized(strings) || (GetLastElementIndex(strings) + 1) >= strings.Length;
        }

        private static bool Contains(string[] strings, string query)
        {
            foreach (string line in strings)
            {
                if (line == query) return true;
            }

            return false;
        }

        private static void PrintWonMessage(int playerScore)
        {
            string message = $"You win! your score: {playerScore}";

            Console.WriteLine(message);
        }
    }
}
