using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
namespace RebalancedMoons
{
    internal class MoldBlockerLogic
    {

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void PostSpreadMold()
        {
            Transform[] moldContainer = Object.FindAnyObjectByType<MoldSpreadManager>()?.moldContainer.GetComponentsInChildren<Transform>();

            MoldDenialPoint[] moldDenialPoints = GameObject.FindObjectsByType<MoldDenialPoint>(sortMode: FindObjectsSortMode.None);

            float gridCellSize = 10f; 
            SpatialGrid spatialGrid = new SpatialGrid(gridCellSize);

            // population
            foreach (MoldDenialPoint denialPoint in moldDenialPoints)
            {
                spatialGrid.AddObject(denialPoint.gameObject);
            }

            // go through all the shit
            foreach (Transform weed in moldContainer)
            {
                if (weed == null) continue;

                Vector3 weedPos = weed.position;
                List<GameObject> nearbyDenialPoints = spatialGrid.GetNearbyObjects(weedPos);

                foreach (GameObject denialPoint in nearbyDenialPoints)
                {
                    if ((denialPoint.transform.position - weedPos).sqrMagnitude < 100f) // 10^2 
                    {
                        ModNetworkHandler.Instance.KillWeedServerRpc(weedPos);
                        break; // weed found in cell range
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
                }
                grid[cell].Add(obj);
            }

            public List<GameObject> GetNearbyObjects(Vector3 position)
            {
                Vector2Int cell = GetCell(position);
                List<GameObject> nearbyObjects = new List<GameObject>();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int neighborCell = new Vector2Int(cell.x + x, cell.y + y);
                        if (grid.ContainsKey(neighborCell))
                        {
                            nearbyObjects.AddRange(grid[neighborCell]);
                        }
                    }
                }
                return nearbyObjects;
            }
        }
    }
}
