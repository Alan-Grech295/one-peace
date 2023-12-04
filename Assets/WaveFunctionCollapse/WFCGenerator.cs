using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

using Pair = System.Tuple<int, int>;

public class WFCGenerator : MonoBehaviour
{
    class ChoiceList : IEnumerable<Pair>, ICloneable
    {
        public Dictionary<int, int> choiceDict;

        public int Count { get { return choiceDict.Count; } }

        public int this[int key]
        {
            get
            {
                return choiceDict[key];
            }

            set
            {
                choiceDict[key] = value;
            }
        }

        public ChoiceList()
        {
            choiceDict = new Dictionary<int, int>();
        }

        public ChoiceList(params Pair[] tuples)
        {
            choiceDict = new Dictionary<int, int>();
            foreach (var tup in tuples)
            {
                choiceDict[tup.Item1] = tup.Item2;
            }
        }

        public ChoiceList(int[] keys, int defaultVal = 1)
        {
            choiceDict = new Dictionary<int, int>();
            foreach (var key in keys)
            {
                choiceDict[key] = defaultVal;
            }
        }

        public ChoiceList Union(ChoiceList choiceList, bool min = false)
        {
            foreach (var kvp in choiceList.choiceDict)
            {
                if (choiceDict.ContainsKey(kvp.Key))
                {
                    if (min)
                        choiceDict[kvp.Key] = Mathf.Min(kvp.Value, choiceDict[kvp.Key]);
                    else
                        choiceDict[kvp.Key] = Mathf.Max(kvp.Value, choiceDict[kvp.Key]);
                }
                else
                {
                    choiceDict[kvp.Key] = kvp.Value;
                }
            }

            return this;
        }

        public ChoiceList Intersect(ChoiceList choiceList, bool min = false)
        {
            KeyValuePair<int, int>[] keysCopy = (KeyValuePair<int, int>[])choiceDict.ToArray().Clone();

            foreach (var kvp in keysCopy)
            {
                if (!choiceList.Contains(kvp.Key))
                {
                    choiceDict.Remove(kvp.Key);
                }
                else
                {
                    if (min)
                        choiceDict[kvp.Key] = Mathf.Min(kvp.Value, choiceList.choiceDict[kvp.Key]);
                    else
                        choiceDict[kvp.Key] = Mathf.Max(kvp.Value, choiceList.choiceDict[kvp.Key]);
                }
            }

            return this;
        }

        public Pair[] ToArray()
        {
            return choiceDict.Select(x => new Pair(x.Key, x.Value)).ToArray();
        }

        public bool Contains(int key)
        {
            return choiceDict.ContainsKey(key);
        }

        public int TotalWeight()
        {
            return choiceDict.Values.Sum();
        }

        public IEnumerator<Pair> GetEnumerator()
        {
            return ((IEnumerable<Pair>)ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            ChoiceList copy = new ChoiceList();
            copy.choiceDict = choiceDict.ToDictionary(entry => entry.Key, entry => entry.Value);
            return copy;
        }
    }

    [Serializable]
    public class TilemapOptions
    {
        public string name;
        public Tilemap tilemap;
        public List<Sprite> spritesToPlace;
    }

    public enum Direction
    {
        UP = 0,
        DOWN = 1,
        LEFT = 2,
        RIGHT = 3,
    }

    //public Tilemap tilemap;
    public TilemapOptions[] tilemapOptions;
    public Tilemap palette;
    public Sprite[] wallSprites;

    public bool generate = false;

    private List<int> spriteIndexToTilemap = new List<int>();

    public delegate void OnCompletedEvent();
    public event OnCompletedEvent OnCompleted;
    
    public delegate void OnProgressEvent(float progress);
    public event OnProgressEvent OnProgress;

    // Valid sprites [Directions] [Sprite Index] 
    List<ChoiceList[]> spriteRules = new List<ChoiceList[]>();
    List<Sprite> sprites = new List<Sprite>();
    Dictionary<Sprite, int> spriteToIndex;
    BoundsInt paletteBounds;

    static Vector2Int[] dirs = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
    };

    public int maxGenerationSize = 400;
    public int maxTries = 10;

    // Used for async generation
    private List<Vector3Int>[] tilePositions;
    private List<int>[] tiles;
    private Sprite[,] tilemapSprites;
    private Vector3 cellSize;
    private System.Random random = new System.Random();
    private int emptySpriteIndex = -1;

    void Start()
    {
        palette.CompressBounds();
        paletteBounds = palette.cellBounds;

        // New system
        spriteToIndex = new Dictionary<Sprite, int>();

        for (int y = paletteBounds.yMin; y <= paletteBounds.yMax; y++)
        {
            for (int x = paletteBounds.xMin; x <= paletteBounds.xMax; x++)
            {
                Sprite sprite = palette.GetSprite(new Vector3Int(x, y, 0));
                if (sprite == null) continue;

                if (!spriteToIndex.ContainsKey(sprite))
                {
                    spriteToIndex.Add(sprite, spriteRules.Count);
                    spriteRules.Add(new ChoiceList[]
                    {
                        new ChoiceList(),
                        new ChoiceList(),
                        new ChoiceList(),
                        new ChoiceList(),
                    });
                    sprites.Add(sprite);
                    spriteIndexToTilemap.Add(GetTilemapIndex(sprite));
                }

                int spriteIndex = spriteToIndex[sprite];

                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int dir = dirs[i];
                    Vector2Int newPos = new Vector2Int(x, y) + dir;
                    Sprite neighbourSprite = palette.GetSprite((Vector3Int)newPos);
                    if (neighbourSprite == null) continue;

                    if (!spriteToIndex.ContainsKey(neighbourSprite))
                    {
                        spriteToIndex.Add(neighbourSprite, spriteRules.Count);
                        spriteRules.Add(new ChoiceList[]
                        {
                            new ChoiceList(),
                            new ChoiceList(),
                            new ChoiceList(),
                            new ChoiceList(),
                        });
                        sprites.Add(neighbourSprite);
                        spriteIndexToTilemap.Add(GetTilemapIndex(neighbourSprite));
                    }

                    int neighbourSpriteIndex = spriteToIndex[neighbourSprite];

                    if (!spriteRules[spriteIndex][i].Contains(neighbourSpriteIndex))
                        spriteRules[spriteIndex][i][neighbourSpriteIndex] = 0;

                    spriteRules[spriteIndex][i][neighbourSpriteIndex]++;
                }
            }
        }

        for (int i = 0; i < spriteRules.Count; i++)
        {
            string ruleString = $"Sprite {sprites[i].name} rules:\n";
            foreach (Direction d in Enum.GetValues(typeof(Direction)))
            {
                ruleString += d.ToString() + "\n";
                foreach (var n in spriteRules[i][(int)d])
                {
                    ruleString += sprites[n.Item1].name + ", ";
                }
                ruleString += "\n";
            }
        }

        tilePositions = new List<Vector3Int>[tilemapOptions.Length];
        for (int i = 0; i < tilePositions.Length; i++)
            tilePositions[i] = new List<Vector3Int>(maxGenerationSize / tilemapOptions.Length);

        tiles = new List<int>[tilemapOptions.Length];
        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = new List<int>(maxGenerationSize / tilemapOptions.Length);

        GenerateWall();

        //Generate(initialBounds);
        Stopwatch sw = Stopwatch.StartNew();
        Bounds expanded = GameManager.Instance.mapBounds;
        expanded.Expand(50);
        ProgressiveGenerate(expanded);
        sw.Stop();
        //StartCoroutine(GenerateCoroutine(initialBounds));
    }

    private int GetTilemapIndex(Sprite sprite)
    {
        for (int i = 0; i < tilemapOptions.Length; i++)
        {
            if (tilemapOptions[i].spritesToPlace.Contains(sprite))
                return i;
        }

        return tilemapOptions.Length - 1;
    }

    void GenerateWall()
    {
        Tilemap tilemapToPlace = tilemapOptions.Where(opt => opt.spritesToPlace.Contains(wallSprites[0])).ToArray()[0].tilemap;
        TileBase wallTile = new UnityEngine.Tilemaps.Tile()
        {
            sprite = wallSprites[UnityEngine.Random.Range(0, wallSprites.Length)]
        };

        for (int x = (int)GameManager.Instance.mapBounds.min.x; x < GameManager.Instance.mapBounds.max.x; x++)
        {
            tilemapToPlace.SetTile(new Vector3Int(x, (int)GameManager.Instance.mapBounds.min.y, 0) + ToVec3Int(GameManager.Instance.mapBounds.center), wallTile);
            tilemapToPlace.SetTile(new Vector3Int(x, (int)GameManager.Instance.mapBounds.max.y, 0) + ToVec3Int(GameManager.Instance.mapBounds.center), wallTile);
        }

        for (int y = (int)GameManager.Instance.mapBounds.min.y; y < GameManager.Instance.mapBounds.max.y; y++)
        {
            tilemapToPlace.SetTile(new Vector3Int((int)GameManager.Instance.mapBounds.min.x, y, 0) + ToVec3Int(GameManager.Instance.mapBounds.center), wallTile);
            tilemapToPlace.SetTile(new Vector3Int((int)GameManager.Instance.mapBounds.max.y, y, 0) + ToVec3Int(GameManager.Instance.mapBounds.center), wallTile);
        }

        tilemapToPlace.RefreshAllTiles();
    }

    Vector3Int ToVec3Int(Vector3 vec)
    {
        return new Vector3Int(
            Mathf.RoundToInt(vec.x),
            Mathf.RoundToInt(vec.y),
            Mathf.RoundToInt(vec.z)
          );
    }

    public void ProgressiveGenerate(Bounds bounds)
    {
        int xCount = Mathf.CeilToInt(bounds.size.x / tilemapOptions[0].tilemap.layoutGrid.cellSize.x);
        int yCount = Mathf.CeilToInt(bounds.size.y / tilemapOptions[0].tilemap.layoutGrid.cellSize.y);

        if (xCount * yCount > maxGenerationSize)
        {
            StartCoroutine(ProgressiveGenerateCoroutine(bounds, xCount, yCount));
        }
        else
        {
            GenerateAsync(bounds);
        }
    }

    Task GenerateAsync(Bounds bounds, Action callback = null)
    {
        cellSize = tilemapOptions[0].tilemap.layoutGrid.cellSize;

        int xCount = Mathf.CeilToInt(bounds.size.x / cellSize.x);
        int yCount = Mathf.CeilToInt(bounds.size.y / cellSize.y);

        if (tilemapSprites == null || tilemapSprites.GetLength(0) < xCount + 2 || tilemapSprites.GetLength(1) < yCount + 2)
        {
            tilemapSprites = new Sprite[xCount + 2, yCount + 2];
        }

        Vector3Int tilemapBottomLeft = RoundToInt3(Divide(bounds.center - bounds.extents, cellSize));
        tilemapBottomLeft.z = 0;

        for (int y = -1; y < yCount + 1; y++)
        {
            for (int x = -1; x < xCount + 1; x++)
            {
                Vector2Int localPos = new Vector2Int(x, y);
                Vector3Int worldPos = (Vector3Int)localPos + tilemapBottomLeft;
                tilemapSprites[x + 1, y + 1] = null;
                foreach (TilemapOptions opt in tilemapOptions)
                {
                    Sprite sprite = opt.tilemap.GetSprite(worldPos);
                    if(sprite != null)
                    {
                        tilemapSprites[x + 1, y + 1] = sprite;
                        break;
                    }
                }
            }
        }

        return Task.Run(() =>
        {
            Generate(bounds);
        }).ContinueWith((t) =>
        {
            if (callback != null) callback();
        });
    }

    IEnumerator ProgressiveGenerateCoroutine(Bounds bounds, int xCount, int yCount)
    {
        //boundsList.Clear();
        int maxSideSize = Mathf.FloorToInt(Mathf.Sqrt(maxGenerationSize));

        Vector3 boundsBL = (Vector2)(bounds.center - bounds.extents);

        bool completed = false;

        int totalGenerations = Mathf.RoundToInt((bounds.size.x / (maxSideSize - 1)) * (bounds.size.y / (maxSideSize - 1)));

        for (int y = 0, i = 0; y < bounds.size.y; y += maxSideSize - 1)
        {
            for (int x = 0; x < bounds.size.x; x += maxSideSize - 1, i++)
            {
                completed = false;
                int xSize = Math.Min(maxSideSize, (int)bounds.size.x - x);
                int ySize = Math.Min(maxSideSize, (int)bounds.size.y - y);
                Bounds sectionBounds = new Bounds(new Vector3(x + xSize / 2, y + ySize / 2, 0) + boundsBL, new Vector3(xSize, ySize, 0));
                GenerateAsync(sectionBounds, () => { completed = true; });
                yield return new WaitUntil(() => completed);
                for(int j = 0; j < tilePositions.Length; j++)
                {
                    tilemapOptions[j].tilemap.SetTiles(tilePositions[j].ToArray(), tiles[j].Select(index =>
                    {
                        return new UnityEngine.Tilemaps.Tile()
                        {
                            sprite = sprites[index]
                        };
                    }).ToArray());
                }

                float progress = (float)i / totalGenerations;

                OnProgress?.Invoke(progress);
            }
        }

        foreach (TilemapOptions opt in tilemapOptions)
            opt.tilemap.RefreshAllTiles();

        AstarPath.active.UpdateGraphs(bounds);
        OnCompleted?.Invoke();
    }

    public void Generate(Bounds bounds)
    {
        foreach (var pos in tilePositions)
            pos.Clear();

        foreach (var tile in tiles)
            tile.Clear();

        //Stopwatch sw = Stopwatch.StartNew();
        int xCount = Mathf.CeilToInt(bounds.size.x / cellSize.x);
        int yCount = Mathf.CeilToInt(bounds.size.y / cellSize.y);

        Vector3Int tilemapBottomLeft = RoundToInt3(Divide(bounds.center - bounds.extents, cellSize));
        tilemapBottomLeft.z = 0;

        // 2D Array of lists of possible tiles
        // It is expanded by 1 to accomodate the neighbour tiles
        ChoiceList[,] wfcMap = new ChoiceList[xCount + 2, yCount + 2];
        bool[,] collapsed = new bool[xCount, yCount];

        // List of tuple of square positions and entropy
        Dictionary<Vector2Int, int> entropyList = new Dictionary<Vector2Int, int>();

        int[] allTilePos = Enumerable.Range(0, spriteRules.Count).ToArray();

        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                wfcMap[x + 1, y + 1] = new ChoiceList(allTilePos);
                entropyList[new Vector2Int(x, y)] = allTilePos.Length;
            }
        }

        for (int y = -1; y < yCount + 1; y++)
        {
            for (int x = -1; x < xCount + 1; x++)
            {
                Vector2Int localPos = new Vector2Int(x, y);
                //Vector3Int worldPos = (Vector3Int)localPos + tilemapBottomLeft;
                Sprite tileSprite = tilemapSprites[x + 1, y + 1];// tilemap.GetSprite(worldPos);
                if (tileSprite == null || !spriteToIndex.ContainsKey(tileSprite)) continue;

                CollapseSpecific(localPos, spriteToIndex[tileSprite], ref wfcMap, ref entropyList, ref collapsed);
            }
        }

        // Creating copies
        ChoiceList[,] wfcMapCopy = wfcMap.Clone() as ChoiceList[,];
        Dictionary<Vector2Int, int> entropyListCopy = entropyList.ToDictionary(entry => entry.Key, entry => entry.Value);
        bool[,] collapsedCopy = collapsed.Clone() as bool[,];

        for (int i = 0; i < maxTries; i++)
        {
            bool failed = false;
            while (entropyList.Count > 0)
            {
                if (!Collapse(ref wfcMap, ref entropyList, ref collapsed, tilemapBottomLeft, ref tilePositions, ref tiles, i < maxTries - 1) && i < maxTries - 1)
                {
                    failed = true;
                    break;
                }
            }

            if (!failed)
                return;

            // Reset map
            foreach (var pos in tilePositions)
                pos.Clear();

            foreach (var tile in tiles)
                tile.Clear();
            wfcMap = wfcMapCopy.Clone() as ChoiceList[,];
            entropyList = entropyListCopy.ToDictionary(entry => entry.Key, entry => entry.Value);
            collapsed = collapsedCopy.Clone() as bool[,];
        }

        //sw.Stop();
        //UnityEngine.Debug.Log($"Generated map in {sw.ElapsedMilliseconds}ms");
    }

    private bool Collapse(ref ChoiceList[,] wfcMap, ref Dictionary<Vector2Int, int> entropyList, ref bool[,] collapsed, Vector3Int tilemapBottomLeft, ref List<Vector3Int>[] tilePositions, ref List<int>[] tiles, bool failfast = true)
    {
        if (entropyList.Count == 0) return true;

        // Choosing tile to collapse
        Vector2Int collapsePos = entropyList.Aggregate((l, r) =>
        {
            if(l.Value == r.Value)
                return random.Next(100) < 50 ? l : r;

            return l.Value < r.Value ? l : r;
        }).Key;

        Vector3Int worldPos = tilemapBottomLeft + (Vector3Int)collapsePos;

        Pair[] availableTiles = wfcMap[collapsePos.x + 1, collapsePos.y + 1].ToArray().OrderBy(x => x.Item2).ToArray();
        int totalWeight = wfcMap[collapsePos.x + 1, collapsePos.y + 1].TotalWeight();

        bool hasOptions = true;
        if (availableTiles.Length == 0)
        {
            if (failfast)
                return false;
            //UnityEngine.Debug.LogError("No available tiles to place");
            hasOptions = false;
        }

        int rand = random.Next(totalWeight);
        int spriteIndex = 0;
        foreach (Pair p in availableTiles)
        {
            rand -= p.Item2;

            if (rand <= 0)
            {
                spriteIndex = p.Item1;
                break;
            }
        }

        if(spriteIndex != emptySpriteIndex)
        {
            tilePositions[spriteIndexToTilemap[spriteIndex]].Add(worldPos);
            tiles[spriteIndexToTilemap[spriteIndex]].Add(spriteIndex);
        }
        
        CollapseSpecific(collapsePos, spriteIndex, ref wfcMap, ref entropyList, ref collapsed);
        return hasOptions;
    }

    // Collapses a tile with a specific sprite
    // Note: It does not draw it 
    void CollapseSpecific(Vector2Int localTilePos, int spriteIndex, ref ChoiceList[,] wfcMap, ref Dictionary<Vector2Int, int> entropyList, ref bool[,] collapsed)
    {
        if (entropyList.ContainsKey(localTilePos))
            entropyList.Remove(localTilePos);

        wfcMap[localTilePos.x + 1, localTilePos.y + 1] = new ChoiceList(new int[] { spriteIndex });

        bool[,] marked = new bool[wfcMap.GetLength(0) - 2, wfcMap.GetLength(1) - 2];

        if (InBounds(localTilePos, wfcMap.GetLength(0) - 2, wfcMap.GetLength(1) - 2))
        {
            marked[localTilePos.x, localTilePos.y] = true;
            collapsed[localTilePos.x, localTilePos.y] = true;
        }

        Propogate(localTilePos, ref wfcMap, ref entropyList, collapsed, ref marked);
    }

    private void Propogate(Vector2Int pos, ref ChoiceList[,] wfcMap, ref Dictionary<Vector2Int, int> entropyList, in bool[,] collapsed, ref bool[,] marked)
    {
        if (wfcMap[pos.x + 1, pos.y + 1].Count == 0)
            return;

        ChoiceList[] availTiles = new ChoiceList[]
        {
            new ChoiceList(), new ChoiceList(),
            new ChoiceList(), new ChoiceList()
        };

        List<Vector2Int> propogateChildren = new List<Vector2Int>();

        foreach (Pair tile in wfcMap[pos.x + 1, pos.y + 1])
        {
            for (int i = 0; i < dirs.Length; i++)
            {
                availTiles[i].Union(spriteRules[tile.Item1][i]);
            }
        }

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector2Int dir = dirs[i];
            Vector2Int newPos = pos + dir;
            if (!InBounds(newPos, wfcMap.GetLength(0) - 2, wfcMap.GetLength(1) - 2) || collapsed[newPos.x, newPos.y] || marked[newPos.x, newPos.y]) continue;

            wfcMap[newPos.x + 1, newPos.y + 1].Intersect(availTiles[i]);
            marked[newPos.x, newPos.y] = true;

            if (entropyList.ContainsKey(newPos))
            {
                int pastEntropy = entropyList[newPos];
                int currentEntropy = wfcMap[newPos.x + 1, newPos.y + 1].Count;

                entropyList[newPos] = currentEntropy;

                if (pastEntropy != currentEntropy)
                {
                    propogateChildren.Add(newPos);
                }
            }
        }

        foreach (Vector2Int childPos in propogateChildren)
        {
            Propogate(childPos, ref wfcMap, ref entropyList, collapsed, ref marked);
        }
    }

    static bool InBounds(Vector2Int pos, Vector2Int max)
    {
        return pos.x >= 0 && pos.x < max.x && pos.y >= 0 && pos.y < max.y;
    }

    static bool InBounds(Vector2Int pos, int maxX, int maxY)
    {
        return InBounds(pos, new Vector2Int(maxX, maxY));
    }

    static Vector3Int RoundToInt3(Vector3 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    static Vector3 Divide(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    private void OnDrawGizmos()
    {
        if(GameManager.Instance != null)
            Gizmos.DrawWireCube(GameManager.Instance.mapBounds.center, GameManager.Instance.mapBounds.size);

        //Gizmos.color = Color.green;

        //foreach(Bounds b in boundsList)
        //{
        //    Gizmos.DrawWireCube(b.center, b.size);
        //}
    }
}
