using System;
using System.Collections.Generic;

namespace SpawnSystem
{
    public class SpawnLocationFinder
    {
        private readonly int _mapSize;
        private readonly int _spawnPoints;
        private readonly int _spawnSpread;
        private readonly int[,] _map;
        
        private int _biggestPocket = -1;
        private List<HashSet<Tuple<int, int>>> _pockets;
        private List<Tuple<int, int>> _spawnLocations;
        private bool[,] _visited;

        #region Getters
        // Realistically not needed after sending to Danyal
        public List<HashSet<Tuple<int, int>>> GetPockets() => _pockets;
        public bool[,] GetVisited() => _visited;
        public int getBiggestPocketID => _biggestPocket;

        #endregion
        
        /**
         * Search through a 2D array and find pockets where the player can spawn
         * using depth first search and from this we find the biggest pocket and
         * find a random spawn location to spawn our colonists at.
         */
        public SpawnLocationFinder(int[,] map, int mapSize = 64, int spawnPoints = 3, int spawnSpread = 2)
        {
            _map = map;
            _mapSize = mapSize;
            _spawnPoints = spawnPoints;
            _spawnSpread = spawnSpread;
            _visited = new bool[_mapSize, _mapSize];
            _pockets = new List<HashSet<Tuple<int, int>>>();
            _spawnLocations = new List<Tuple<int, int>>();
        }
        
        public List<Tuple<int,int>> GenerateSpawnPoints()
        {
            FindPockets();
            IdentifyLargestPocket();
            var origin = SelectRandomSpawnOrigin();
            SetSpawnLocations(origin);
            return _spawnLocations;
        }

        #region Helper Methods
        
        private bool InBounds(int x, int y) => InBounds(_mapSize, _mapSize, x, y);

        private static bool InBounds(int maxX, int maxY, int x, int y) => (x >= 0) && (x < maxX) &&
                                                                          (y >= 0) && (y < maxY);

        #endregion
        
        #region Algorithm Methods
        private void FindPockets()
        {
            for (int i = 0; i < _mapSize; i++)
            {
                for (int j = 0; j < _mapSize; j++)
                {
                    if (_map[i, j] == 0 && !_visited[i, j])
                    {
                        var newPocket = new HashSet<Tuple<int, int>>();
                        DepthFirstSearch(_map, _visited, i, j, newPocket);
                        _pockets.Add(newPocket);
                    }
                }
            }
        }

        private void IdentifyLargestPocket()
        {
            // Find largest pocket
            int largestCount = 0;
            for (int g = 0; g < _pockets.Count; g++)
            {
                if (_pockets[g].Count > largestCount)
                {
                    largestCount = _pockets[g].Count;
                    _biggestPocket = g;
                }
            }
            if (_biggestPocket == -1) throw new Exception("Unable to locate spawnable pocket!");
        }

        private Tuple<int,int> SelectRandomSpawnOrigin()
        {
            // Pick random location within pocket
            var rand = new Random();
            var t = rand.Next(_pockets[_biggestPocket].Count);
            var index = 0;
            Tuple<int, int> chosenLocation = null;

            // Find specific Vector2 and save x,y
            foreach (var p in _pockets[_biggestPocket])
            {
                if (index == t)
                {
                    chosenLocation = p;
                    break;
                }
                index++;
            }

            if (chosenLocation == null) throw new Exception("Unable to find spawn origin point in pocket.");
            return chosenLocation;
        }

        private void SetSpawnLocations(Tuple<int,int> origin)
        {
            // Find random spawn points around origin
            for (int i = 0; i < _spawnPoints; i++)
            {
                _spawnLocations.Add(GetRandomNearByPoint(origin.Item1, origin.Item2, _pockets[_biggestPocket], _spawnLocations));
            }
        }
        
        private Tuple<int, int> GetRandomNearByPoint(int x, int y, HashSet<Tuple<int, int>> pocketLocations, List<Tuple<int,int>> spawnLocations)
        {
            int ranX = -1, ranY = -1;
            var foundRandomLocationNearby = false;
            while (!foundRandomLocationNearby)
            {
                var r = new Random();
                ranX = r.Next(x - _spawnSpread, x + _spawnSpread);
                ranY = r.Next(y - _spawnSpread, y + _spawnSpread);
                if (InBounds(ranX, ranY) 
                    && pocketLocations.Contains(new Tuple<int,int>(ranX,ranY))
                    && !spawnLocations.Contains(new Tuple<int, int>(ranX,ranY))) foundRandomLocationNearby = true;
            }
            return new Tuple<int, int>(ranX,ranY);
        }
        
        private static void DepthFirstSearch(int[,] map, bool[,] visited, int i, int j, HashSet<Tuple<int, int>> pocket)
        {
            try
            {
                if (i < 0 || i >= map.GetLength(0)) return;
                if (j < 0 || j >= map.GetLength(1)) return;
                if (map[i, j] != 0 || visited[i, j]) return;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception("Index out of range inside DFS. Possible map size mismatch");
            }
            

            visited[i, j] = true;
            pocket.Add(new Tuple<int, int>(i, j));
            
            DepthFirstSearch(map, visited, i + 1, j, pocket);
            DepthFirstSearch(map, visited, i - 1, j, pocket);
            DepthFirstSearch(map, visited, i, j + 1, pocket);
            DepthFirstSearch(map, visited, i, j - 1, pocket);
        }
        
        #endregion
    }
}
