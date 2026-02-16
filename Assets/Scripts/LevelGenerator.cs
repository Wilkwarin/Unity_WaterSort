using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generation Parameters")]
    public int numberOfColors = 3;
    public int emptyBottles = 2;

    [Header("Difficulty Progression")]
    public bool useProgression = true;
    public int baseColors = 3;
    public int maxColors = 8;
    public int levelsPerColorIncrease = 5;

    [Header("Solver Settings")]
    public int maxSolverDepth = 20;
    public int maxSolverStates = 2000;

    [Header("Color Palette")]
    public Color[] availableColors = new Color[]
    {
        new Color(1f, 0f, 0.439f),      // FF0070
        new Color(1f, 0.647f, 0f),      // FFA500
        new Color(1f, 0.922f, 0.016f),  // FFEB04
        new Color(0.486f, 0.988f, 0f),  // 7CFC00
        new Color(0f, 1f, 1f),          // 00FFFF
        new Color(0.502f, 0f, 1f),      // 8000FF
        new Color(0.984f, 0f, 0.992f),  // FB00FD
        new Color(0.659f, 0.161f, 0.667f), // A829AA
        new Color(0.502f, 0.251f, 0f),  // 804000
        new Color(0.5f, 0.5f, 0.5f)     // 808080
    };

    [Header("References")]
    public LevelManager levelManager;

    public LevelData GenerateLevel()
    {
        if (useProgression)
        {
            int levelNumber = levelManager.currentLevelIndex;
            int cycleIndex = levelNumber % 5;
            numberOfColors = 6 + cycleIndex;
            emptyBottles = 2;
        }


        List<BottleData> bottles = null;
        int attempts = 0;
        const int MAX_ATTEMPTS = 30;

        while (attempts < MAX_ATTEMPTS)
        {
            attempts++;

            List<Color> selectedColors = SelectRandomColors(numberOfColors);
            bottles = CreateShuffledBottles(selectedColors);

            for (int i = 0; i < emptyBottles; i++)
                bottles.Add(new BottleData());

            if (IsSolvable(bottles))
                break;

            bottles = null;
        }

        if (bottles == null)
            bottles = CreateTrivialLevel();

        return ConvertToLevelData(bottles);
    }

    bool IsSolvable(List<BottleData> bottles)
    {
        GameState initialState = new GameState(bottles);

        if (initialState.IsSolved())
            return false;

        Queue<GameState> queue = new Queue<GameState>();
        HashSet<string> visited = new HashSet<string>();

        queue.Enqueue(initialState);
        visited.Add(initialState.GetHash());

        int statesChecked = 0;

        while (queue.Count > 0 && statesChecked < maxSolverStates)
        {
            statesChecked++;
            GameState current = queue.Dequeue();

            if (current.depth > maxSolverDepth)
                continue;

            List<GameState> nextStates = current.GetPossibleMoves();

            foreach (GameState next in nextStates)
            {
                string hash = next.GetHash();

                if (visited.Contains(hash))
                    continue;

                visited.Add(hash);

                if (next.IsSolved())
                    return true;

                queue.Enqueue(next);
            }
        }

        return false;
    }

    List<BottleData> CreateTrivialLevel()
    {
        List<Color> colors = SelectRandomColors(numberOfColors);
        List<BottleData> bottles = new List<BottleData>();

        for (int i = 0; i < numberOfColors; i++)
        {
            BottleData bottle = new BottleData();
            for (int layer = 0; layer < 4; layer++)
            {
                int colorIndex = (i + layer) % numberOfColors;
                bottle.colors.Add(colors[colorIndex]);
            }
            bottles.Add(bottle);
        }

        for (int i = 0; i < emptyBottles; i++)
            bottles.Add(new BottleData());

        return bottles;
    }

    List<BottleData> CreateShuffledBottles(List<Color> colors)
    {
        List<Color> pool = new List<Color>();

        foreach (Color c in colors)
            for (int i = 0; i < 4; i++)
                pool.Add(c);

        List<BottleData> bottles = new List<BottleData>();

        for (int bottleIdx = 0; bottleIdx < colors.Count; bottleIdx++)
        {
            BottleData bottle = new BottleData();
            Color previousColor = Color.clear;

            for (int layer = 0; layer < 4; layer++)
            {
                List<int> validIndices = new List<int>();

                for (int i = 0; i < pool.Count; i++)
                    if (pool[i] != previousColor)
                        validIndices.Add(i);

                if (validIndices.Count == 0)
                    for (int i = 0; i < pool.Count; i++)
                        validIndices.Add(i);

                int chosenIndex = validIndices[Random.Range(0, validIndices.Count)];
                Color chosenColor = pool[chosenIndex];

                bottle.colors.Add(chosenColor);
                pool.RemoveAt(chosenIndex);

                previousColor = chosenColor;
            }

            bottles.Add(bottle);
        }

        return bottles;
    }

    List<Color> SelectRandomColors(int count)
    {
        List<Color> result = new List<Color>();
        List<Color> palette = new List<Color>(availableColors);

        for (int i = 0; i < count && palette.Count > 0; i++)
        {
            int idx = Random.Range(0, palette.Count);
            result.Add(palette[idx]);
            palette.RemoveAt(idx);
        }

        return result;
    }

    LevelData ConvertToLevelData(List<BottleData> bottles)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.levelNumber = levelManager.currentLevelIndex + 1;
        level.levelName = $"Level {level.levelNumber}";
        level.bottles = new LevelData.BottleConfiguration[bottles.Count];

        for (int i = 0; i < bottles.Count; i++)
        {
            level.bottles[i] = new LevelData.BottleConfiguration
            {
                numberOfColors = bottles[i].colors.Count,
                colors = new Color[4]
            };

            for (int j = 0; j < 4; j++)
                level.bottles[i].colors[j] =
                    j < bottles[i].colors.Count ? bottles[i].colors[j] : Color.clear;
        }

        return level;
    }

    class BottleData
    {
        public List<Color> colors = new List<Color>();

        public BottleData Clone()
        {
            BottleData copy = new BottleData();
            copy.colors = new List<Color>(this.colors);
            return copy;
        }
    }

    class GameState
    {
        public List<BottleData> bottles;
        public int depth;

        public GameState(List<BottleData> initialBottles, int currentDepth = 0)
        {
            bottles = new List<BottleData>();
            foreach (var b in initialBottles)
                bottles.Add(b.Clone());

            depth = currentDepth;
        }

        public bool IsSolved()
        {
            foreach (var bottle in bottles)
            {
                if (bottle.colors.Count == 0) continue;
                if (bottle.colors.Count != 4) return false;

                Color first = bottle.colors[0];
                for (int i = 1; i < 4; i++)
                    if (bottle.colors[i] != first)
                        return false;
            }
            return true;
        }

        public List<GameState> GetPossibleMoves()
        {
            List<GameState> moves = new List<GameState>();

            for (int from = 0; from < bottles.Count; from++)
            {
                if (bottles[from].colors.Count == 0) continue;

                Color topColor = bottles[from].colors[^1];

                for (int to = 0; to < bottles.Count; to++)
                {
                    if (from == to) continue;
                    if (bottles[to].colors.Count >= 4) continue;

                    if (bottles[to].colors.Count == 0 ||
                        bottles[to].colors[^1] == topColor)
                    {
                        GameState newState = Clone();
                        newState.Pour(from, to);
                        newState.depth = depth + 1;
                        moves.Add(newState);
                    }
                }
            }

            return moves;
        }

        void Pour(int from, int to)
        {
            Color topColor = bottles[from].colors[^1];

            int countInSource = 0;
            for (int i = bottles[from].colors.Count - 1; i >= 0; i--)
            {
                if (bottles[from].colors[i] == topColor)
                    countInSource++;
                else
                    break;
            }

            int spaceInTarget = 4 - bottles[to].colors.Count;
            int toPour = Mathf.Min(countInSource, spaceInTarget);

            for (int i = 0; i < toPour; i++)
            {
                bottles[from].colors.RemoveAt(bottles[from].colors.Count - 1);
                bottles[to].colors.Add(topColor);
            }
        }

        GameState Clone()
        {
            return new GameState(this.bottles, this.depth);
        }

        public string GetHash()
        {
            List<string> bottleHashes = new List<string>();

            foreach (var bottle in bottles)
            {
                if (bottle.colors.Count == 0)
                    bottleHashes.Add("E");
                else
                    bottleHashes.Add(string.Join("", bottle.colors.Select(c => c.GetHashCode())));
            }

            bottleHashes.Sort();
            return string.Join("|", bottleHashes);
        }
    }
}
