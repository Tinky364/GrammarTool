using System;
using System.Collections.Generic;
using System.Linq;
using Grammar.Editor;
using Grammar.Helper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grammar.Core
{
    [AddComponentMenu("ObjGenerator")]
    public class ObjGenerator : ObjWord
    {
        [SerializeField] private bool canSpawnSameTileMoreThanOnce = false;
        [SerializeField, Min(0)] private int maxTileObjectCount = 0;
        [SerializeField] private SurfaceChose surfaceChose;
        [SerializeField] private bool randomRotationAtStart = true;
        [SerializeField, Tooltip("Type -1 for random index.")]
        private int firstTileObjectPrefabIndex = -1;
        [SerializeField, Tooltip("Type -1 for random index.")]
        private int lastTileObjectPrefabIndex = -1;

        [ReadOnly] public float totalHeight = 0;

        [SerializeField] private ObjTile[] tileObjectPrefabs;

        private int properSurfaceCount = 0;
        private bool[] usedPrefabs;

        private List<ProperTileObject> xPosSurface;
        private List<ProperTileObject> zNegSurface;
        private List<ProperTileObject> xNegSurface;
        private List<ProperTileObject> zPosSurface;
        private List<ProperTileObject> yPosSurface;
        private List<ProperTileObject> yNegSurface;
        private Dictionary<SurfaceDirection, List<ProperTileObject>> surfaces;

        private List<SurfaceDirection> availableSurfaceDirections;

        [HideInInspector] public List<ObjTile> spawnedTileObjects;

        public override void OnSpawn()
        {
            spawnedTileObjects = new List<ObjTile>();

            surfaces = new Dictionary<SurfaceDirection, List<ProperTileObject>>()
            {
                {SurfaceDirection.XPos, xPosSurface = new List<ProperTileObject>()},
                {SurfaceDirection.ZNeg, zNegSurface = new List<ProperTileObject>()},
                {SurfaceDirection.XNeg, xNegSurface = new List<ProperTileObject>()},
                {SurfaceDirection.ZPos, zPosSurface = new List<ProperTileObject>()},
                {SurfaceDirection.YPos, yPosSurface = new List<ProperTileObject>()},
                {SurfaceDirection.YNeg, yNegSurface = new List<ProperTileObject>()}
            };

            StartGeneration();
        }

        public void StartGeneration()
        {
            Generate();
        }
        public void StartGeneration(int tileObjectCount)
        {
            maxTileObjectCount = tileObjectCount;
            Generate();
        }
        private void Generate()
        {
            // initialize usedPrefabs if needed
            if (!canSpawnSameTileMoreThanOnce)
                usedPrefabs = new bool[tileObjectPrefabs.Length];
            //

            // first tile object index
            int prefabIndex = firstTileObjectPrefabIndex;
            if (prefabIndex == -1)
                prefabIndex = Random.Range(0, tileObjectPrefabs.Length);
            //

            // spawn the first tile object
            ObjTile tileObj = Instantiate(tileObjectPrefabs[prefabIndex], transform);
            tileObj.name = tileObjectPrefabs[prefabIndex].name;
            if (randomRotationAtStart) tileObj.transform.Rotate(0, Random.Range(0, 4) * 90, 0);
            spawnedTileObjects.Add(tileObj);
            //

            // start spawning. every iteration spawn one tile object
            for (int i = 1; i < maxTileObjectCount; i++)
            {
                InitializeLoop(); // Reset every iteration

                // calculate the height of the generated structure
                if (surfaceChose == SurfaceChose.Y)
                    totalHeight += tileObj.Size.y;
                else
                {
                    if (tileObj.Size.y > totalHeight)
                        totalHeight = tileObj.Size.y;
                }
                //

                if (!canSpawnSameTileMoreThanOnce) usedPrefabs[prefabIndex] = true;

                // find proper tile objects for the last spawned tile object
                for (int j = 0; j < tileObjectPrefabs.Length; j++)
                {
                    // if cannot able to spawn same tile object type more than once,
                    // skip adding those to proper tile objects
                    if (!canSpawnSameTileMoreThanOnce) 
                    {
                        if (usedPrefabs[j]) continue;
                    }
                    //

                    if (lastTileObjectPrefabIndex != -1) // last object is not random
                    {
                        if (i == maxTileObjectCount - 1) // if last iteration
                        {
                            if (j != lastTileObjectPrefabIndex) continue; // spawn only the chosen last object
                        }
                        else // if not last iteration
                        {
                            if (j == lastTileObjectPrefabIndex) continue; // do not spawn last object
                        }
                    }

                    AddProperTileObjectsToLists(tileObj, tileObjectPrefabs[j], j);
                }
                //

                // if there is no proper tile object that can be spawned, then stop iteration
                if (properSurfaceCount <= 0) break; 
                //

                // choose one proper tile object that can be spawned, and spawn it
                ProperTileObject properTileObject = SelectRandomProperTileObject(ref prefabIndex);
                tileObj = SpawnAndPlaceTileObject(tileObj, properTileObject);
                spawnedTileObjects.Add(tileObj);
                //
            }
        }

        private void AddProperTileObjectsToLists(ObjTile spawnedTileObj, ObjTile prefabTileObj, int prefabTileObjIndex)
        {
            foreach (var pairSpawned in spawnedTileObj.surfaceIdData.faceIdDict)
            {
                if (spawnedTileObj.surfaceCheck[(int) pairSpawned.Key])
                    continue;
                if (pairSpawned.Value == 0)
                    continue;
                if (!availableSurfaceDirections.Contains(pairSpawned.Key))
                    continue;

                foreach (var pairRelated in prefabTileObj.surfaceIdData.faceIdDict)
                {
                    if (pairSpawned.Value != pairRelated.Value)
                        continue;

                    switch (pairSpawned.Key)
                    {
                        case SurfaceDirection.XPos:
                            xPosSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        case SurfaceDirection.ZNeg:
                            zNegSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        case SurfaceDirection.XNeg:
                            xNegSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        case SurfaceDirection.ZPos:
                            zPosSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        case SurfaceDirection.YPos:
                            if (pairRelated.Key == SurfaceDirection.YPos)
                                continue;
                            yPosSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        case SurfaceDirection.YNeg:
                            if (pairRelated.Key == SurfaceDirection.YNeg)
                                continue;
                            yNegSurface.Add(new ProperTileObject(prefabTileObj, pairRelated.Key, prefabTileObjIndex, SurfaceDirection.Null));
                            properSurfaceCount++;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private ProperTileObject SelectRandomProperTileObject(ref int prefabIndex)
        {
            ProperTileObject properTileObject = new ProperTileObject();
            int index = 0;
            bool found = false;
            while (!found)
            {
                index = Random.Range(0, availableSurfaceDirections.Count);
                var keyValuePair = surfaces.FirstOrDefault(x => x.Key == availableSurfaceDirections[index]);
                if (keyValuePair.Value.Count > 0)
                {
                    int elementIndex = Random.Range(0, keyValuePair.Value.Count);
                    properTileObject = keyValuePair.Value[elementIndex];
                    prefabIndex = properTileObject.prefabIndex;
                    properTileObject.placeNextToSD = availableSurfaceDirections[index];
                    found = true;
                }
                else
                {
                    availableSurfaceDirections.RemoveAt(index);
                }
            }

            return properTileObject;
        }

        private ObjTile SpawnAndPlaceTileObject(ObjTile placeNextTo, ProperTileObject properTileObj)
        {
            ObjTile spawned = Instantiate(properTileObj.prefab, transform);
            spawned.name = properTileObj.prefab.name;
            spawned.surfaceCheck[(int) properTileObj.properSurface] = true;
            spawned.Place(placeNextTo, properTileObj.placeNextToSD, properTileObj.properSurface, surfaceChose);
            return spawned;
        }

        private void InitializeLoop()
        {
            properSurfaceCount = 0;
            xPosSurface.Clear();
            xNegSurface.Clear();
            zPosSurface.Clear();
            zNegSurface.Clear();
            yPosSurface.Clear();
            yNegSurface.Clear();
            availableSurfaceDirections = SurfaceDirectionHelper.AvailableSurfaceDirections(surfaceChose);
        }
    }
}