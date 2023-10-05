using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CaterpillarControlSystem
{
    private char[][] planetMap;
    private Stack<string> commandLog;
    private Stack<string> redoLog;
    private List<(int X, int Y)> segments;
    private int headX, headY;
    private int caterpillarLength;
    private const int maxCaterpillarLength = 5;
    private const int radarRadius = 5;

    public CaterpillarControlSystem()
    {
        InitializePlanetMap();
        commandLog = new Stack<string>();
        redoLog = new Stack<string>();
        segments = new List<(int X, int Y)>();
        caterpillarLength = 2;

        // Initialize segments based on initial caterpillar length
        for (int i = 0; i < caterpillarLength; i++)
        {
            segments.Add((14 - i, 29));
        }

        headX = 14; // Initial head position
        headY = 29; // Initial head position
    }

    private void InitializePlanetMap()
    {
        planetMap = new[]
        {
            "$*********$**********$********".ToCharArray(),
            "***$*******B*************#****".ToCharArray(),
            "************************#*****".ToCharArray(),
            "***#**************************".ToCharArray(),
            "**$*************************#*".ToCharArray(),
            "$$***#************************".ToCharArray(),
            "**************$***************".ToCharArray(),
            "**********$*********$*****#***".ToCharArray(),
            "********************$*******$*".ToCharArray(),
            "*********#****$***************".ToCharArray(),
            "**B*********$*****************".ToCharArray(),
            "*************$$****B**********".ToCharArray(),
            "****$************************B".ToCharArray(),
            "**********************#*******".ToCharArray(),
            "***********************$***B**".ToCharArray(),
            "********$***$*****************".ToCharArray(),
            "************$*****************".ToCharArray(),
            "*********$********************".ToCharArray(),
            "*********************#********".ToCharArray(),
            "*******$**********************".ToCharArray(),
            "*#***$****************#*******".ToCharArray(),
            "****#****$****$********B******".ToCharArray(),
            "***#**$********************$**".ToCharArray(),
            "***************#**************".ToCharArray(),
            "***********$******************".ToCharArray(),
            "****B****#******B*************".ToCharArray(),
            "***$***************$*****B****".ToCharArray(),
            "**********$*********#*$*******".ToCharArray(),
            "**************#********B******".ToCharArray(),
            "s**********$*********#*B******".ToCharArray()
        };
    }

    private void DisplayRadarImage()
    {
        int radarSize = radarRadius * 2 + 1;
        char[][] radar = new char[radarSize][];

        for (int i = 0; i < radarSize; i++)
        {
            radar[i] = new char[radarSize];
            for (int j = 0; j < radarSize; j++)
            {
                int radarX = headX - radarRadius + j;
                int radarY = headY - radarRadius + i;

                if (IsInsidePlanetMap(radarX, radarY))
                {
                    char c = planetMap[radarY][radarX];
                    radar[i][j] = c;
                }
                else
                {
                    radar[i][j] = ' ';
                }
            }
        }

        // Place the caterpillar head, tail, and segments on the radar
        radar[radarRadius][radarRadius] = 'H'; // Head

        if (caterpillarLength >= 2)
        {
            int tailRadarX = headX - segments.Last().X + radarRadius;
            int tailRadarY = headY - segments.Last().Y + radarRadius;

            if (tailRadarX >= 0 && tailRadarX < radarSize && tailRadarY >= 0 && tailRadarY < radarSize)
            {
                radar[tailRadarY][tailRadarX] = 'T'; // Tail
            }

            foreach (var segment in segments.Skip(1).Take(caterpillarLength - 2))
            {
                int segmentX = segment.X;
                int segmentY = segment.Y;

                int segmentRadarX = headX - segmentX + radarRadius;
                int segmentRadarY = headY - segmentY + radarRadius;

                if (segmentRadarX >= 0 && segmentRadarX < radarSize && segmentRadarY >= 0 && segmentRadarY < radarSize)
                {
                    radar[segmentRadarY][segmentRadarX] = 'O'; // Segment
                }
            }
        }

        // Display the radar image
        for (int i = 0; i < radarSize; i++)
        {
            for (int j = 0; j < radarSize; j++)
            {
                Console.Write(radar[i][j]);
            }
            Console.WriteLine();
        }
    }

    private void ExecuteCommand(string command)
    {
        int newX = headX;
        int newY = headY;

        // Store the previous state for undo/redo
        string previousState = SerializeState();

        switch (command)
        {
            case "U":
                newY--;
                break;
            case "D":
                newY++;
                break;
            case "L":
                newX--;
                break;
            case "R":
                newX++;
                break;
            case "Undo":
                Undo();
                return;
            case "Redo":
                Redo();
                return;
            default:
                break;
        }

        if (IsInsidePlanetMap(newX, newY) && !IsObstacle(newX, newY))
        {
            MoveCaterpillar(newX, newY);
            LogCommand(command, previousState);
            CheckInteractions();
        }
    }

    private void MoveCaterpillar(int newX, int newY)
    {
        // Move segments relative to the head
        segments.Insert(0, (newX, newY));

        // Trim caterpillar length if it exceeds the maximum
        if (segments.Count > caterpillarLength)
        {
            segments.RemoveAt(caterpillarLength);
        }

        // Check if the head collects spice or boosters
        if (planetMap[newY][newX] == 'B' && caterpillarLength < maxCaterpillarLength)
        {
            caterpillarLength++;
            planetMap[newY][newX] = '*';
        }
        else if (planetMap[newY][newX] == '$')
        {
            planetMap[newY][newX] = '*';
        }

        headX = newX; // Update head position
        headY = newY; // Update head position
    }

    private void LogCommand(string command, string previousState)
    {
        // Log the command and previous state for undo/redo
        commandLog.Push(command);
        redoLog.Clear();
        redoLog.Push(previousState);
    }

    private void CheckInteractions()
    {
        // Check for interactions and disintegration logic (same as before)
        // ...
    }

    private bool IsInsidePlanetMap(int x, int y)
    {
        return x >= 0 && x < planetMap[0].Length && y >= 0 && y < planetMap.Length;
    }

    private bool IsObstacle(int x, int y)
    {
        return planetMap[y][x] == '#';
    }

    private string SerializeState()
    {
        // Serialize the current state for undo/redo
        return $"{headX},{headY};{string.Join(";", segments)};{caterpillarLength}";
    }

    private void DeserializeState(string state)
    {
        // Deserialize the state for undo/redo
        var stateParts = state.Split(';');
        var headParts = stateParts[0].Split(',');
        headX = int.Parse(headParts[0]);
        headY = int.Parse(headParts[1]);

        var segmentParts = stateParts[1].Split(';');
        segments.Clear();
        foreach (var segment in segmentParts)
        {
            var segmentCoords = segment.Split(',');
            segments.Add((int.Parse(segmentCoords[0]), int.Parse(segmentCoords[1])));
        }

        caterpillarLength = int.Parse(stateParts[2]);
    }

    public void Undo()
    {
        if (commandLog.Count > 0)
        {
            redoLog.Push(SerializeState());
            DeserializeState(commandLog.Pop());
        }
    }

    public void Redo()
    {
        if (redoLog.Count > 0)
        {
            commandLog.Push(SerializeState());
            DeserializeState(redoLog.Pop());
        }
    }

    public void Run()
    {
        Console.WriteLine("Planet Mission Control Station - Caterpillar Control System");
        Console.WriteLine("Use the following commands to control the caterpillar:");
        Console.WriteLine("U - Move up");
        Console.WriteLine("D - Move down");
        Console.WriteLine("L - Move left");
        Console.WriteLine("R - Move right");
        Console.WriteLine("Enter commands (Example., 'L 4' to move Left 4 steps):");

        while (caterpillarLength > 0)
        {
            Console.WriteLine("\nCurrent Planet Map:");
            DisplayRadarImage();

            Console.Write("\nEnter a command: ");
            string input = Console.ReadLine().Trim();

            if (!string.IsNullOrEmpty(input))
            {
                string[] parts = input.Split(' ');

                if (parts.Length == 1 && (parts[0] == "Undo" || parts[0] == "Redo"))
                {
                    if (parts[0] == "Undo")
                    {
                        Undo();
                    }
                    else
                    {
                        Redo();
                    }
                }
                else if (parts.Length == 2)
                {
                    string command = parts[0];
                    if (int.TryParse(parts[1], out int steps))
                    {
                        for (int i = 0; i < steps; i++)
                        {
                            ExecuteCommand(command);
                            if (caterpillarLength == 0)
                            {
                                Console.WriteLine("\nCaterpillar disintegrated!");
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid command format. Please use 'U/D/L/R <steps>'");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command format. Please use 'U/D/L/R <steps>'");
                }
            }
        }

        Console.WriteLine("\nMission complete. Caterpillar control system shut down.");
    }

    public static void Main(string[] args)
    {
        CaterpillarControlSystem controlSystem = new CaterpillarControlSystem();
        controlSystem.Run();
    }
}

