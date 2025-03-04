namespace RebalancedMoons
{
    using System.Collections.Generic;
    using UnityEngine;
    using HarmonyLib;

    internal class MoldBlockerLogic
    {
        [HarmonyPatch(typeof(MoldSpreadManager), "GenerateMold")]
        [HarmonyPostfix]
        private static void PostSpreadMold()
        {
            Plugin.Logger.LogDebug("[MoldBlocker] Starting mold spread check...");

            Transform[] moldContainer = Object.FindAnyObjectByType<MoldSpreadManager>()?.moldContainer.GetComponentsInChildren<Transform>();
            MoldDenialPoint[] moldDenialPoints = GameObject.FindObjectsByType<MoldDenialPoint>(FindObjectsSortMode.None);

            if (moldContainer == null)
            {
                Plugin.Logger.LogWarning("[MoldBlocker] moldContainer is null!");
                return;
            }

            Plugin.Logger.LogDebug($"[MoldBlocker] Found {moldContainer.Length} mold objects.");
            Plugin.Logger.LogDebug($"[MoldBlocker] Found {moldDenialPoints.Length} mold denial points.");

            float gridCellSize = 10f;
            SpatialGrid spatialGrid = new SpatialGrid(gridCellSize);

            // Populate the grid with mold denial points
            foreach (MoldDenialPoint denialPoint in moldDenialPoints)
            {
                spatialGrid.AddObject(denialPoint.gameObject);
                Plugin.Logger.LogDebug($"[MoldBlocker] Added denial point at {denialPoint.transform.position}");
            }

            // Check all mold (weeds)
            foreach (Transform weed in moldContainer)
            {
                if (weed == null) continue;

                Vector3 weedPos = weed.position;
                List<GameObject> nearbyDenialPoints = spatialGrid.GetNearbyObjects(weedPos);

                Plugin.Logger.LogDebug($"[MoldBlocker] Checking weed at {weedPos}. Found {nearbyDenialPoints.Count} nearby denial points.");

                // Check for mold near the ship using OverlapSphere
                Collider[] moldColliders = Physics.OverlapSphere(StartOfRound.Instance.elevatorTransform.position, 25f, 65536);

                if (moldColliders.Length > 0)  // Only run if found
                {
                    foreach (var moldCollider in moldColliders)
                    {
                        float distanceToShip = (StartOfRound.Instance.elevatorTransform.position - moldCollider.transform.position).sqrMagnitude;
                        Plugin.Logger.LogDebug($"[MoldBlocker] Killing weed near ship at {moldCollider.transform.position} (Distance: {Mathf.Sqrt(distanceToShip)})");

                        // Kill the weed at this position (prevents redundant calls)
                        ModNetworkHandler.Instance.KillWeedServerRpc(moldCollider.transform.position);
                        break; // Stop checking after the first valid moldCollider
                    }
                }

                if (weed == null) continue; // Check if weed is still valid before continuing

                foreach (GameObject denialPoint in nearbyDenialPoints)
                {
                    float distanceSqr = (denialPoint.transform.position - weedPos).sqrMagnitude;
                    if (distanceSqr < 100f) // 10^2 to avoid sqrt
                    {
                        Plugin.Logger.LogDebug($"[MoldBlocker] Killing weed at {weedPos} (Distance: {Mathf.Sqrt(distanceSqr)})");
                        ModNetworkHandler.Instance.KillWeedServerRpc(weedPos);
                        break; // Stop checking after finding one valid denial point
                    }
                }
            }
        }

        public class SpatialGrid
        {
            private float cellSize;
            private Dictionary<Vector2Int, List<GameObject>> grid = new Dictionary<Vector2Int, List<GameObject>>();

            public SpatialGrid(float cellSize)
            {
                this.cellSize = cellSize;
                Plugin.Logger.LogDebug($"[SpatialGrid] Initialized with cell size {cellSize}");
            }

            private Vector2Int GetCell(Vector3 position)
            {
                return new Vector2Int(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.z / cellSize));
            }

            public void AddObject(GameObject obj)
            {
                Vector2Int cell = GetCell(obj.transform.position);
                if (!grid.ContainsKey(cell))
                {
                    grid[cell] = new List<GameObject>();
                    Plugin.Logger.LogDebug($"[SpatialGrid] Created new cell {cell}");
                }
                grid[cell].Add(obj);
                Plugin.Logger.LogDebug($"[SpatialGrid] Added object {obj.name} to cell {cell}");
            }

            public List<GameObject> GetNearbyObjects(Vector3 position)
            {
                Vector2Int cell = GetCell(position);
                List<GameObject> nearbyObjects = new List<GameObject>();

                Plugin.Logger.LogDebug($"[SpatialGrid] Looking for nearby objects at cell {cell}");

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int neighborCell = new Vector2Int(cell.x + x, cell.y + y);
                        if (grid.ContainsKey(neighborCell))
                        {
                            nearbyObjects.AddRange(grid[neighborCell]);
                            Plugin.Logger.LogDebug($"[SpatialGrid] Found {grid[neighborCell].Count} objects in cell {neighborCell}");
                        }
                    }
                }
                return nearbyObjects;
            }
        }
    }
}
