using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MEC;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerraLand
{
    [DefaultExecutionOrder(-100)]
    public class RuntimeOffline : MonoBehaviour
    {
        public bool showStartingLocation = false;
        [Range(33, 1025)] public int previewHeightmapResolution = 1024;
        [Range(32, 4096)] public int previewSatelliteImageResolution = 1024;
        public bool reloadMap = false;
        public static int startingTileIndex;
        public static int startingTileRow;
        public static int startingTileColumn;
        public bool startFromCenter = false;
        public static string centerTileCoordsTLBR;

        public GameObject player;
        public static double centerLatitude;
        public static double centerLongitude;

        public static float areaSize;
        public static string locationName, serverInfoPath;
        public static string globalHeightmapPath, globalHeightmapPath2, globalSatelliteImagePath, globalSatelliteImagePath2;
        public static double top, left, bottom, right;
        public static double latExtent, lonExtent;

        public float sizeExaggeration = 10f;
        public static float exaggeratedWorldSize;
        public static int heightmapResolution;

        public bool drawInstanced = false;
        private static bool drawInstancedState = false;

        public bool circularLOD = true;
        [Range(1f, 200f)] public float heightmapPixelError = 5f;
        [Range(1f, 200f)] public float heightmapPixelErrorFurthest = 100f;
        [Range(1, 8)] public int centerLayersCount = 1;
        public int smoothIterations = 1;
        public static bool farTerrain = false;
        public int cellSize = 64;
        //private static bool IsCustomGeoServer = false;
        public bool delayedLOD = true;
        public static int concurrentTasks = 1;
        [HideInInspector] public bool spiralGeneration = false;
        public bool showTileOnFinish = true;
        public static int imageResolution;
        //private bool progressiveTexturing = true;
        public float elevationExaggeration = 1;

        //public bool stitchTerrainTiles = true;

        [HideInInspector] public bool stitchTerrainTiles = false;
        [HideInInspector] [Range(5, 100)] public int levelSmooth = 5;
        [HideInInspector] [Range(1, 7)] public int power = 1;
        [HideInInspector] [Range(1, 32)] public int stitchDistance = 1;

        public string serverPath = "C:/Users/Amir/Desktop/TerraLand_GeoServer"; // "http://terraunity.com/freedownload/TerraLand_GeoServer";
        public static string dataBasePath;
        public bool projectRootPath = true;

        //android Build
        public bool androidBuild = false;
        //android Build

        public bool elevationOnly = false;
        [HideInInspector] public bool progressiveGeneration = false;

        //private int counterNorth = 0;
        //private int counterSouth = 0;
        //private int counterEast = 0;
        //private int counterWest = 0;

        [HideInInspector] public float terrainDistance = 1000f;

        public bool terrainColliders = false;
        public bool fastStartBuild = true;

        public static bool tiledElevation;

        [Range(4, 32)] public int activeTilesGrid = 4;
        public static int totalTiles;
        public static int dataBaseTiles;
        public static int dataBaseGrid;
        public static int padStartX;
        public static int padStartY;
        public static int padEndX;
        public static int padEndY;

        public static string[] elevationTileNames;
        public static string[] imageryTileNames;
        public static string[] normalTileNames;

        public float delayBetweenConnections;
        public float elevationDelay = 0.5f;
        public float imageryDelay = 0.5f;
        //public float stitchDelay = 0.25f;

        public Material terrainMaterial;
        public bool enableDetailTextures = true;
        public Texture2D detailTexture;
        public Texture2D detailNormal;
        //public Texture2D detailNormalFar;
        [Range(0, 100)] public float detailBlending = 25f;
        public float detailTileSize = 25f;

        [HideInInspector] public bool asyncImageLoading = true;

#if UNITY_EDITOR
        private TextureImporter imageImport;
#endif

        private Texture2D[] detailTextures;
        private SplatPrototype[] terrainTextures;
        private SplatPrototype currentSplatPrototye;
        private List<Terrain> terrains;
        private int startIndex;
        private int texturesNO;
        private int length;
        private float[,,] smData;
        private int index;
        private int filteredIndex;
        private String[] pathParts;
        public static int northCounter = 0;
        public static int southCounter = 0;
        public static int eastCounter = 0;
        public static int westCounter = 0;

        public bool enableSplatting;
        public Texture2D layer1Albedo;
        public Texture2D layer1Normal;
        public int tiling1;
        public Texture2D layer2Albedo;
        public Texture2D layer2Normal;
        public int tiling2;
        public Texture2D layer3Albedo;
        public Texture2D layer3Normal;
        public int tiling3;
        public Texture2D layer4Albedo;
        public Texture2D layer4Normal;
        public int tiling4;

        private IEnumerable<string> imageryNames;
        private IEnumerable<string> normalNames;
        [HideInInspector] public bool normalsAvailable;
        [HideInInspector] public bool imageryAvailable;

        private static string serverError;
        public Text notificationText;

        public static Timing timing;
        public static bool updatingSurfaceNORTH;
        public static bool updatingSurfaceSOUTH;
        public static bool updatingSurfaceEAST;
        public static bool updatingSurfaceWEST;

        public static float hiddenTerrainsBelowUnits = 100000f;

        public StreamingAssetsManager streamingAssets;
        [HideInInspector] public List<Terrain> processedTiles;

        public float activeDistance = 10000f;
        public bool isStreamingAssets = false;

        public static float worldPositionOffsetX;
        public static float worldPositionOffsetY;

        public static bool isGeoReferenced;
        private static string sceneName;

        #region multithreading variables

        int maxThreads = 50;
        private int numThreads;
        private int _count;

        private bool m_HasLoaded = false;

        private List<Action> _actions;
        private List<DelayedQueueItem> _delayed;

        private List<DelayedQueueItem> _currentDelayed;
        private List<Action> _currentActions;

        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        #endregion


        void Start()
        {
            showStartingLocation = false;

            if (Application.isPlaying)
            {
                _actions = new List<Action>();
                _delayed = new List<DelayedQueueItem>();
                _currentDelayed = new List<DelayedQueueItem>();
                _currentActions = new List<Action>();
                m_HasLoaded = true;

                SetupServer();
                TerraLandRuntimeOffline.Initialize();
                //OfflineStreaming.Initialize();
            }
        }

        private void CheckDetailTextures()
        {
#if UNITY_EDITOR
            detailTextures = new Texture2D[2] { detailTexture, detailNormal };

            foreach (Texture2D currentImage in detailTextures)
            {
                imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(currentImage)) as TextureImporter;

                if (imageImport != null && !imageImport.isReadable)
                {
                    imageImport.isReadable = true;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
                }
            }
#endif
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (m_HasLoaded == false)
                    Start();

                lock (_actions)
                {
                    _currentActions.Clear();
                    _currentActions.AddRange(_actions);
                    _actions.Clear();
                }

                foreach (var a in _currentActions)
                    a();

                lock (_delayed)
                {
                    _currentDelayed.Clear();
                    _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));

                    foreach (var item in _currentDelayed)
                        _delayed.Remove(item);
                }

                foreach (var delayed in _currentDelayed)
                    delayed.action();

                if (GameObject.Find("Movement Effects") != null && timing == null)
                    timing = GameObject.Find("Movement Effects").GetComponent<Timing>();

                if (TerraLandRuntimeOffline.worldIsGenerated)
                {
                    if (enableDetailTextures)
                        AddDetailTexturesToTerrains();

                    //if(timing.UpdateCoroutines == 0)
                    //{
                    //    if (updatingSurfaceNORTH)
                    //    {
                    //        TerraLandRuntimeOffline.ManageNeighborings("North");
                    //        updatingSurfaceNORTH = false;
                    //    }
                    //    else if (updatingSurfaceSOUTH)
                    //    {
                    //        TerraLandRuntimeOffline.ManageNeighborings("South");
                    //        updatingSurfaceSOUTH = false;
                    //    }
                    //    else if (updatingSurfaceEAST)
                    //    {
                    //        TerraLandRuntimeOffline.ManageNeighborings("East");
                    //        updatingSurfaceEAST = false;
                    //    }
                    //    else if (updatingSurfaceWEST)
                    //    {
                    //        TerraLandRuntimeOffline.ManageNeighborings("West");
                    //        updatingSurfaceWEST = false;
                    //    }
                    //}
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (showStartingLocation)
            {
                SetupServer();

                if (isGeoReferenced)
                    EditorApplication.ExecuteMenuItem("Tools/TerraUnity/TerraLand/Refresh");
                else
                    EditorUtility.DisplayDialog("NON GEO-REFERENCED SERVER", "This option is not available for non geo-referenced servers!", "Ok");

                showStartingLocation = false;
            }

            if (activeTilesGrid % 2 != 0) activeTilesGrid = activeTilesGrid + 1;
            if (!Mathf.IsPowerOfTwo(previewHeightmapResolution)) previewHeightmapResolution = Mathf.ClosestPowerOfTwo(previewHeightmapResolution) + 1;
            if (!Mathf.IsPowerOfTwo(previewSatelliteImageResolution)) previewSatelliteImageResolution = Mathf.ClosestPowerOfTwo(previewSatelliteImageResolution);

            if (drawInstancedState != drawInstanced)
            {
                //f (drawInstanced) EditorUtility.DisplayDialog("ALWAYS INCLUDED SHADERS", "Make sure to add Nature/Terrain/Standard shader in Project Settings => Graphics => Always Included Shaders list\n\nAnd under Shader Stripping select Keep All in Instancing Variants drop down menu.", "Ok");
                if (drawInstanced) Debug.Log("ALWAYS INCLUDED SHADERS, Make sure to add Nature/Terrain/Standard shader in Project Settings => Graphics => Always Included Shaders list\n\nAnd under Shader Stripping select Keep All in Instancing Variants drop down menu.");
                drawInstancedState = drawInstanced;
            }
        }
#endif

        public void SetupServer()
        {
            sceneName = SceneManager.GetActiveScene().name;

            TerraLandRuntimeOffline.runTime = this;
            InfiniteTerrainOffline.runTime = this;
            OfflineStreaming.runtime = this;

            progressiveGeneration = false;
            spiralGeneration = false;
            concurrentTasks = 1;
            stitchDistance = 1;
            asyncImageLoading = true;

            //android Build
            if (androidBuild)
            {
#if UNITY_EDITOR
                if (projectRootPath)
                    dataBasePath = Application.dataPath.Replace("Assets", "") + serverPath;
                else
                    dataBasePath = serverPath;
#else
            dataBasePath = Application.persistentDataPath + "/" + serverPath;
#endif
            }
            else
            {
                if (projectRootPath)
                {
#if UNITY_EDITOR
                    dataBasePath = Application.dataPath.Replace("Assets", "") + serverPath;
#else
                dataBasePath = Application.dataPath + "/" + serverPath;
#endif
                }
                else
                    dataBasePath = serverPath;
            }
            //android Build

            if (!Directory.Exists(dataBasePath))
            {
                //serverError = "Server Directory Not Found!\n\nDownload sample servers from links in ReadMe file next to scene file\nDataBasePath parameter in RuntimeOffline script must be typed correctly\nApplication will quit now";
                serverError = "Server Directory Not Found! " + dataBasePath;
                Debug.LogError(serverError);
                notificationText.text = serverError;
                StartCoroutine(StopApplication());
                return;
            }
            else
            {
                serverError = "";
                notificationText.text = serverError;
            }

            TerraLandRuntimeOffline.dataBasePathElevation = dataBasePath + "/Elevation/";
            TerraLandRuntimeOffline.dataBasePathImagery = dataBasePath + "/Imagery/"; // "/Imagery/512/64/"; // 1 4 16 64 256 1024
            TerraLandRuntimeOffline.dataBasePathNormals = dataBasePath + "/Normals/";

            if (!Directory.Exists(TerraLandRuntimeOffline.dataBasePathElevation))
            {
                serverError = "Server's Elevation Directory Not Found!\n\nElevation directory in server is not available\nApplication will quit now";
                UnityEngine.Debug.LogError(serverError);
                notificationText.text = serverError;
                StartCoroutine(StopApplication());
                return;
            }
            else
            {
                serverError = "";
                notificationText.text = serverError;
            }

            if (!elevationOnly && !Directory.Exists(TerraLandRuntimeOffline.dataBasePathImagery))
            {
                serverError = "Server's Imagery Directory Not Found!\n\nImagery directory in server is not available but texturing is activated!\nApplication will quit now";
                UnityEngine.Debug.LogError(serverError);
                notificationText.text = serverError;
                StartCoroutine(StopApplication());
                return;
            }
            else
            {
                serverError = "";
                notificationText.text = serverError;
            }

            IEnumerable<string> elevationNames = Directory.GetFiles(TerraLandRuntimeOffline.dataBasePathElevation, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".asc")
                    || s.EndsWith(".raw")
                    || s.EndsWith(".tif"));

            if (Directory.Exists(TerraLandRuntimeOffline.dataBasePathImagery))
            {
                imageryNames = Directory.GetFiles(TerraLandRuntimeOffline.dataBasePathImagery, "*.*", SearchOption.AllDirectories)
                .Where
                (
                    s => s.EndsWith(".jpg")
                    || s.EndsWith(".png")
                    || s.EndsWith(".gif")
                    || s.EndsWith(".bmp")
                    || s.EndsWith(".tga")
                    || s.EndsWith(".psd")
                    || s.EndsWith(".tiff")
                    || s.EndsWith(".iff")
                    || s.EndsWith(".pict")
                );

                imageryAvailable = true;
            }
            else
                imageryAvailable = false;

            if (Directory.Exists(TerraLandRuntimeOffline.dataBasePathNormals))
            {
                normalNames = Directory.GetFiles(TerraLandRuntimeOffline.dataBasePathNormals, "*.*", SearchOption.AllDirectories)
                .Where
                (
                    s => s.EndsWith(".jpg")
                    || s.EndsWith(".png")
                    || s.EndsWith(".gif")
                    || s.EndsWith(".bmp")
                    || s.EndsWith(".tga")
                    || s.EndsWith(".psd")
                    || s.EndsWith(".tiff")
                    || s.EndsWith(".iff")
                    || s.EndsWith(".pict")
                );

                normalsAvailable = true;
            }
            else
                normalsAvailable = false;

            string infoFilePathT = Path.GetFullPath(dataBasePath) + "/Info/Terrain Info.tlps";

            if (File.Exists(infoFilePathT))
            {
                ServerInfo.GetServerCoords(dataBasePath, out locationName, out serverInfoPath, out globalHeightmapPath, out globalHeightmapPath2, out globalSatelliteImagePath, out globalSatelliteImagePath2, out top, out left, out bottom, out right, out latExtent, out lonExtent, out areaSize);

                if (PlayerPrefs.HasKey(sceneName + "_TileCenterLat"))
                    centerLatitude = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLat"));
                else
                    centerLatitude = bottom + ((top - bottom) * 0.5d);

                if (PlayerPrefs.HasKey(sceneName + "_TileCenterLon"))
                    centerLongitude = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLon"));
                else
                    centerLongitude = left + ((right - left) * 0.5d);

                isGeoReferenced = true;
            }
            else
            {
                /// If server is not geo-referenced then manually set a world size in kilometers. You can change it to your actual area size
                /// In World Machine demo, the world size is considered to be 480km2, so the calculation of the final area size in scene is as follows
                areaSize = 480 / sizeExaggeration;
                isGeoReferenced = false;
            }

            dataBaseTiles = elevationNames.ToArray().Length;
            dataBaseGrid = (int)(Mathf.Sqrt(dataBaseTiles));

            elevationTileNames = new string[dataBaseTiles];
            elevationTileNames = elevationNames.ToArray();
            elevationTileNames = TerraLandRuntimeOffline.LogicalComparer(elevationTileNames);

            if (imageryAvailable)
            {
                imageryTileNames = new string[dataBaseTiles];
                imageryTileNames = imageryNames.ToArray();
                imageryTileNames = TerraLandRuntimeOffline.LogicalComparer(imageryTileNames);

                if (imageryTileNames.Length == 0)
                {
                    elevationOnly = true;
                    enableDetailTextures = false;
                }
            }

            if (normalsAvailable && normalNames.ToArray().Length > 0)
            {
                normalTileNames = new string[dataBaseTiles];
                normalTileNames = normalNames.ToArray();
                normalTileNames = TerraLandRuntimeOffline.LogicalComparer(normalTileNames);
            }

            if (activeTilesGrid > dataBaseGrid)
                activeTilesGrid = dataBaseGrid;

            if (heightmapPixelErrorFurthest < heightmapPixelError)
                heightmapPixelErrorFurthest = heightmapPixelError;

            if (activeTilesGrid <= 4)
                centerLayersCount = 1;

            if (dataBaseTiles > 1)
                tiledElevation = true;
            else
                tiledElevation = false;

            totalTiles = (int)(Mathf.Pow(activeTilesGrid, 2));

            processedTiles = new List<Terrain>();

            if (tiledElevation)
                TerraLandRuntimeOffline.terrainChunks = activeTilesGrid;
            else
                TerraLandRuntimeOffline.terrainChunks = dataBaseTiles;

            TerraLandRuntimeOffline.croppedTerrains = new List<Terrain>();
            TerraLandRuntimeOffline.spiralIndex = new List<int>();
            TerraLandRuntimeOffline.spiralCell = new List<Vector2>();
            TerraLandRuntimeOffline.images = new List<Texture2D>();
            TerraLandRuntimeOffline.imageBytes = new List<byte[]>();

            terrainDistance = 1000000f;

            exaggeratedWorldSize = areaSize * sizeExaggeration;
            float normalizedPercentage = 1f - (((float)dataBaseGrid - (float)activeTilesGrid) / (float)dataBaseGrid);
            TerraLandRuntimeOffline.areaSizeLat = exaggeratedWorldSize * normalizedPercentage;
            TerraLandRuntimeOffline.areaSizeLon = exaggeratedWorldSize * normalizedPercentage;
            TerraLandRuntimeOffline.terrainSizeNewX = TerraLandRuntimeOffline.areaSizeLon * 1000f;
            TerraLandRuntimeOffline.terrainSizeNewY = 100;
            TerraLandRuntimeOffline.terrainSizeNewZ = TerraLandRuntimeOffline.areaSizeLat * 1000f;
            TerraLandRuntimeOffline.terrainSizeFactor = TerraLandRuntimeOffline.areaSizeLat / TerraLandRuntimeOffline.areaSizeLon;
            TerraLandRuntimeOffline.generatedTerrainsCount = 0;
            TerraLandRuntimeOffline.taskIndex = concurrentTasks;
            TerraLandRuntimeOffline.concurrentUpdates = 0;

            if (PlayerPrefs.HasKey(sceneName + "_TileRow"))
                startingTileRow = PlayerPrefs.GetInt(sceneName + "_TileRow");
            else
                startingTileRow = dataBaseGrid / 2;

            if (PlayerPrefs.HasKey(sceneName + "_TileColumn"))
                startingTileColumn = PlayerPrefs.GetInt(sceneName + "_TileColumn");
            else
                startingTileColumn = dataBaseGrid / 2;

            if (PlayerPrefs.HasKey(sceneName + "_TileIndex"))
                startingTileIndex = PlayerPrefs.GetInt(sceneName + "_TileIndex");
            else
                startingTileIndex = ((startingTileRow - 1) * dataBaseGrid) + startingTileColumn;

            // If in corners, offset center tile to be in bounds range
            if (startingTileRow < activeTilesGrid / 2)
                startingTileRow = (activeTilesGrid / 2);

            if (startingTileColumn < activeTilesGrid / 2)
                startingTileColumn = (activeTilesGrid / 2);

            if (startingTileRow > dataBaseGrid - (activeTilesGrid / 2))
                startingTileRow = dataBaseGrid - (activeTilesGrid / 2);

            if (startingTileColumn > dataBaseGrid - (activeTilesGrid / 2))
                startingTileColumn = dataBaseGrid - (activeTilesGrid / 2);

            padStartX = startingTileRow - (activeTilesGrid / 2); // (dataBaseGrid - activeTilesGrid) / 2
            padStartY = startingTileColumn - (activeTilesGrid / 2); // padStartX
            padEndX = dataBaseGrid - (padStartX + activeTilesGrid); // dataBaseGrid - (padStartX + activeTilesGrid)
            padEndY = dataBaseGrid - (padStartY + activeTilesGrid); // padEndX

            int centerTile = dataBaseGrid / 2;
            int tilesFromCenterX = centerTile - startingTileColumn;
            int tilesFromCenterY = centerTile - startingTileRow;
            float worldSize = exaggeratedWorldSize * 1000f;
            float tileWorldSize = worldSize / dataBaseGrid;

            worldPositionOffsetX = tilesFromCenterX * tileWorldSize;
            worldPositionOffsetY = -(tilesFromCenterY * tileWorldSize);

            TerraLandRuntimeOffline.elevationNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];
            TerraLandRuntimeOffline.imageryNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];

            if (normalsAvailable)
                TerraLandRuntimeOffline.normalNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];

            GetElevationInfo();

            if (!elevationOnly)
                GetImageryInfo();

            CheckDetailTextures();

            if (streamingAssets != null && streamingAssets.enabled && streamingAssets.gameObject.activeSelf)
                isStreamingAssets = true;
            else
                isStreamingAssets = false;
        }

        private IEnumerator StopApplication()
        {
            yield return new WaitForSeconds(10);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public void GetElevationInfo()
        {
            if (dataBaseTiles == 0)
            {
                UnityEngine.Debug.LogError("NO AVILABLE DATA - No elevation data is available in selected folder.");
                return;
            }
            else
            {
                index = 0;
                filteredIndex = 0;

                for (int i = 0; i < dataBaseGrid; i++)
                {
                    for (int j = 0; j < dataBaseGrid; j++)
                    {
                        if (i > padStartX - 1 && i < (dataBaseGrid - padEndX) && j > padStartY - 1 && j < (dataBaseGrid - padEndY))
                        {
                            TerraLandRuntimeOffline.elevationNames[index] = elevationTileNames[filteredIndex];
                            index++;
                        }

                        filteredIndex++;
                    }
                }

                if (TerraLandRuntimeOffline.elevationNames[0].EndsWith(".asc") || TerraLandRuntimeOffline.elevationNames[0].EndsWith(".raw") || TerraLandRuntimeOffline.elevationNames[0].EndsWith(".tif"))
                {
                    pathParts = TerraLandRuntimeOffline.elevationNames[0].Split(char.Parse("."));
                    TerraLandRuntimeOffline.geoDataExtensionElevation = pathParts[pathParts.Length - 1];

                    TerraLandRuntimeOffline.GetElevationFileInfo();
                    TerraLandRuntimeOffline.tileResolution = Mathf.ClosestPowerOfTwo(heightmapResolution / dataBaseGrid) + 1;
                    TerraLandRuntimeOffline.heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt(TerraLandRuntimeOffline.terrainChunks);

                    if (cellSize > TerraLandRuntimeOffline.tileResolution)
                        cellSize = TerraLandRuntimeOffline.tileResolution - 1;
                }
                else
                {
                    UnityEngine.Debug.LogError("UNKNOWN FORMAT - There are no valid ASCII, RAW or Tiff files in selected folder.");
                    return;
                }
            }
        }

        public void GetImageryInfo()
        {
            index = 0;
            filteredIndex = 0;

            for (int i = 0; i < dataBaseGrid; i++)
            {
                for (int j = 0; j < dataBaseGrid; j++)
                {
                    if (i > padStartX - 1 && i < (dataBaseGrid - padEndX) && j > padStartY - 1 && j < (dataBaseGrid - padEndY))
                    {
                        TerraLandRuntimeOffline.imageryNames[index] = imageryTileNames[filteredIndex];

                        if (normalsAvailable)
                            TerraLandRuntimeOffline.normalNames[index] = normalTileNames[filteredIndex];

                        index++;
                    }

                    filteredIndex++;
                }
            }

            TerraLandRuntimeOffline.totalImagesDataBase = dataBaseTiles;

            if (TerraLandRuntimeOffline.terrainChunks > 1)
            {
                TerraLandRuntimeOffline.multipleTerrainsTiling = true;

                if (tiledElevation)
                    TerraLandRuntimeOffline.imagesPerTerrain = (int)((float)activeTilesGrid / (float)TerraLandRuntimeOffline.terrainChunks);
                else
                    TerraLandRuntimeOffline.imagesPerTerrain = (int)((float)TerraLandRuntimeOffline.totalImagesDataBase / (float)TerraLandRuntimeOffline.terrainChunks);

                TerraLandRuntimeOffline.tileGrid = (int)(Mathf.Sqrt((float)TerraLandRuntimeOffline.imagesPerTerrain));

                //if(!allDatabase)
                TerraLandRuntimeOffline.splitSizeFinal = activeTilesGrid;
                //else
                //TerraLandRuntimeOffline.splitSizeFinal = (int)Mathf.Sqrt(TerraLandRuntimeOffline.terrainChunks);

                TerraLandRuntimeOffline.totalImages = (int)(Mathf.Pow(TerraLandRuntimeOffline.gridPerTerrain, 2)) * TerraLandRuntimeOffline.terrainChunks;
                TerraLandRuntimeOffline.chunkImageResolution = (RuntimeOffline.imageResolution * (int)Mathf.Sqrt(TerraLandRuntimeOffline.totalImages)) / (int)Mathf.Sqrt((float)TerraLandRuntimeOffline.terrainChunks);
            }
            else
            {
                TerraLandRuntimeOffline.multipleTerrainsTiling = false;
                TerraLandRuntimeOffline.tileGrid = (int)(Mathf.Sqrt((float)TerraLandRuntimeOffline.totalImagesDataBase));
                TerraLandRuntimeOffline.terrainSizeX = TerraLandRuntimeOffline.terrainSizeNewX;
                TerraLandRuntimeOffline.terrainSizeY = TerraLandRuntimeOffline.terrainSizeNewZ;
            }

            if (TerraLandRuntimeOffline.totalImagesDataBase == 0)
            {
                TerraLandRuntimeOffline.geoImagesOK = false;
                UnityEngine.Debug.LogError("There are no images in data base!");
                return;
            }
            else
                TerraLandRuntimeOffline.geoImagesOK = true;

            if (TerraLandRuntimeOffline.terrainChunks > TerraLandRuntimeOffline.totalImagesDataBase)
            {
                TerraLandRuntimeOffline.geoImagesOK = false;
                UnityEngine.Debug.LogError("No sufficient images to texture terrains. Select a lower Grid Size for terrains");
                return;
            }
            else
                TerraLandRuntimeOffline.geoImagesOK = true;

            if (TerraLandRuntimeOffline.geoImagesOK)
            {
                //android Build
                Vector2Int imageDimensions = ImageUtils.GetJpegImageSize(TerraLandRuntimeOffline.imageryNames[0]);
                TerraLandRuntimeOffline.imageWidth = imageDimensions.x;
                TerraLandRuntimeOffline.imageHeight = imageDimensions.y;
                imageResolution = TerraLandRuntimeOffline.imageWidth;

                //using (Image sourceImage = Image.FromFile(TerraLandRuntimeOffline.imageryNames[0]))
                //{
                //    TerraLandRuntimeOffline.imageWidth = sourceImage.Width;
                //    TerraLandRuntimeOffline.imageHeight = sourceImage.Height;
                //    imageResolution = TerraLandRuntimeOffline.imageWidth;
                //}
                //android Build

                //for(int i = 0; i < totalTiles; i++)
                //{
                //    TerraLandRuntimeOffline.images.Add(new Texture2D(TerraLandRuntimeOffline.imageWidth, TerraLandRuntimeOffline.imageHeight, TextureFormat.RGB24, true, true));
                //    TerraLandRuntimeOffline.images[i].wrapMode = TextureWrapMode.Clamp;
                //    TerraLandRuntimeOffline.images[i].name = (i + 1).ToString();
                //    TerraLandRuntimeOffline.LoadImageData(TerraLandRuntimeOffline.imageryNames[i]);
                //}
            }
        }

        private void AddDetailTexturesToTerrains()
        {
            terrains = TerraLandRuntimeOffline.croppedTerrains;

            foreach (Terrain t in terrains)
                AddDetailTextures(t, detailBlending);
        }

        private void AddDetailTextures(Terrain terrain, float blend)
        {
            int startIndex = 0;

#if UNITY_2018_3_OR_NEWER
            try
            {
                if (terrain.terrainData.terrainLayers != null && terrain.terrainData.terrainLayers.Length > 0)
                    startIndex = terrain.terrainData.terrainLayers.Length;
                else
                    startIndex = 0;
            }
            catch
            {
                startIndex = 0;
            }

            TerrainLayer[] terrainLayers = new TerrainLayer[startIndex + 1];
#else
        startIndex = terrain.terrainData.splatPrototypes.Length;
        SplatPrototype[] terrainTextures = new SplatPrototype[startIndex + 1];
#endif

            for (int i = 0; i < startIndex + 1; i++)
            {
                try
                {
                    if (i < startIndex)
                    {
#if UNITY_2018_3_OR_NEWER
                        TerrainLayer currentLayer = terrain.terrainData.terrainLayers[i];

                        terrainLayers[i] = new TerrainLayer();
                        if (currentLayer.diffuseTexture != null) terrainLayers[i].diffuseTexture = currentLayer.diffuseTexture;

                        if (detailNormal != null)
                        {
                            terrainLayers[i].normalMapTexture = detailNormal;
                            terrainLayers[i].normalMapTexture.Apply();
                        }

                        terrainLayers[i].tileSize = new Vector2(currentLayer.tileSize.x, currentLayer.tileSize.y);
                        terrainLayers[i].tileOffset = new Vector2(currentLayer.tileOffset.x, currentLayer.tileOffset.y);
                    }
                    else
                    {
                        terrainLayers[i] = new TerrainLayer();
                        if (detailTexture != null) terrainLayers[i].diffuseTexture = detailTexture;

                        if (detailNormal != null)
                        {
                            terrainLayers[i].normalMapTexture = detailNormal;
                            terrainLayers[i].normalMapTexture.Apply();
                        }

                        if (!farTerrain)
                            terrainLayers[i].tileSize = new Vector2(detailTileSize, detailTileSize);
                        else
                            terrainLayers[i].tileSize = new Vector2(detailTileSize * 200f, detailTileSize * 200f);

                        terrainLayers[i].tileOffset = Vector2.zero;
                    }
#else
                    SplatPrototype currentSplatPrototye = terrain.terrainData.splatPrototypes[i];

                    terrainTextures[i] = new SplatPrototype();
                    if(currentSplatPrototye.texture != null) terrainTextures[i].texture = currentSplatPrototye.texture;

                    if(detailNormal != null)
                    {
                        terrainTextures[i].normalMap = detailNormal;
                        terrainTextures[i].normalMap.Apply();
                    }

                    terrainTextures[i].tileSize = new Vector2(currentSplatPrototye.tileSize.x, currentSplatPrototye.tileSize.y);
                    terrainTextures[i].tileOffset = new Vector2(currentSplatPrototye.tileOffset.x, currentSplatPrototye.tileOffset.y);
                }
                else
                {
                    terrainTextures[i] = new SplatPrototype();
                    if(detailTexture != null) terrainTextures[i].texture = detailTexture;

                    if(detailNormal != null)
                    {
                        terrainTextures[i].normalMap = detailNormal;
                        terrainTextures[i].normalMap.Apply();
                    }

                    if(!farTerrain)
                        terrainTextures[i].tileSize = new Vector2(detailTileSize, detailTileSize);
                    else
                        terrainTextures[i].tileSize = new Vector2(detailTileSize * 200f, detailTileSize * 200f);

                    terrainTextures[i].tileOffset = Vector2.zero;
                }
#endif
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }

#if UNITY_2018_3_OR_NEWER
            terrain.terrainData.terrainLayers = terrainLayers;
#else
        terrain.terrainData.splatPrototypes = terrainTextures;
#endif

            length = terrain.terrainData.alphamapResolution;
            smData = new float[length, length, startIndex + 1];

            try
            {
                for (int y = 0; y < length; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        if (startIndex + 1 > 1)
                        {
                            smData[y, z, 0] = 1f - (blend / 100f);
                            smData[y, z, 1] = blend / 100f;
                        }
                        else
                            smData[y, z, 0] = 1f;
                    }
                }

                terrain.terrainData.SetAlphamaps(0, 0, smData);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            terrain.terrainData.RefreshPrototypes();
            terrain.Flush();

            smData = null;

#if UNITY_2018_3_OR_NEWER
            terrainLayers = null;
#else
        terrainTextures = null;
#endif

            enableDetailTextures = false;
        }

        public void ApplyElevationData()
        {
            TerraLandRuntimeOffline.ApplyOfflineTerrain();
        }

        public void TerrainFromRAW()
        {
            RunAsync(() =>
            {
            //TerraLandRuntimeOffline.rawData = new List<float[,]>();
            TerraLandRuntimeOffline.rawData.Clear();

                if (tiledElevation)
                {
                    for (int i = 0; i < totalTiles; i++)
                        TerraLandRuntimeOffline.RawData(TerraLandRuntimeOffline.elevationNames[i], i);
                }
                else
                    TerraLandRuntimeOffline.RawData(TerraLandRuntimeOffline.elevationNames[0], 0);

                QueueOnMainThread(() =>
                {
                    if (tiledElevation)
                    {
                        for (int i = 0; i < totalTiles; i++)
                            FinalizeTerrainHeights(TerraLandRuntimeOffline.rawData[i], TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, i);
                    }
                    else
                        FinalizeTerrainHeights(TerraLandRuntimeOffline.rawData[0], TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, 0);

                //FinalizeTerrainHeights(null, TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, 0);
            });
            });
        }

        public void TerrainFromRAW(int index)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.RawData(TerraLandRuntimeOffline.elevationNames[index], index);
            });
        }

        public void FinalizeTerrainFromRAW()
        {
            QueueOnMainThread(() =>
            {
                if (tiledElevation)
                {
                    for (int i = 0; i < totalTiles; i++)
                        FinalizeTerrainHeights(TerraLandRuntimeOffline.rawData[i], TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, i);
                }
                else
                    FinalizeTerrainHeights(TerraLandRuntimeOffline.rawData[0], TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, 0);

            //FinalizeTerrainHeights(null, TerraLandRuntimeOffline.m_Width, TerraLandRuntimeOffline.m_Height, 0);
        });
        }

        public void TerrainFromTIFF()
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.TiffData(TerraLandRuntimeOffline.elevationNames[0]);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntimeOffline.tiffData, TerraLandRuntimeOffline.tiffWidth, TerraLandRuntimeOffline.tiffLength, 0);
                });
            });
        }

        public void TerrainFromASCII()
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.AsciiData(TerraLandRuntimeOffline.elevationNames[0]);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntimeOffline.asciiData, TerraLandRuntimeOffline.nCols, TerraLandRuntimeOffline.nRows, 0);
                });
            });
        }

        public void FinalizeTerrainHeights(float[,] data, int width, int height, int index)
        {
            //TerraLandRuntimeOffline.SmoothHeights(TerraLandRuntimeOffline.rawData[index], width, height, index);
            //TerraLandRuntimeOffline.SmoothHeights(data, width, height, index);

            if (smoothIterations > 0)
                TerraLandRuntimeOffline.FinalizeSmooth(data, width, height, smoothIterations, TerraLandRuntimeOffline.smoothBlendIndex, TerraLandRuntimeOffline.smoothBlend);

            if (index == totalTiles - 1)
            {
                if (!tiledElevation)
                    TerraLandRuntimeOffline.CalculateResampleHeightmapsGeoServer(index);

                FinalizeHeights();
            }
        }

        public void FinalizeHeights()
        {
            QueueOnMainThread(() =>
            {
                TerraLandRuntimeOffline.FinalizeHeights();
            });
        }


        public void ServerConnectHeightmapNORTH(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    northCounter++;

                    if (northCounter == activeTilesGrid)
                        LoadTerrainHeightsNORTH("North");
                    else
                        Timing.RunCoroutine(ConnectTileNORTH());
                });
            });
        }

        private IEnumerator<float> ConnectTileNORTH()
        {
            if (InfiniteTerrainOffline.northTerrains.Count > 0)
            {
                yield return Timing.WaitForSeconds(delayBetweenConnections);

                try
                {
                    if (InfiniteTerrainOffline.inProgressWest && northCounter != 0)
                        ServerConnectHeightmapNORTH(TerraLandRuntimeOffline.northIndices[northCounter]);
                    else if (InfiniteTerrainOffline.inProgressEast && northCounter != (activeTilesGrid - 1))
                        ServerConnectHeightmapNORTH(TerraLandRuntimeOffline.northIndices[northCounter]);
                    else
                        ServerConnectHeightmapNORTH(TerraLandRuntimeOffline.northIndices[northCounter]);
                }
                catch { }
            }
        }

        public void ServerConnectHeightmapSOUTH(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    southCounter++;

                    if (southCounter == activeTilesGrid)
                        LoadTerrainHeightsSOUTH("South");
                    else
                        Timing.RunCoroutine(ConnectTileSOUTH());
                });
            });
        }

        private IEnumerator<float> ConnectTileSOUTH()
        {
            if (InfiniteTerrainOffline.southTerrains.Count > 0)
            {
                yield return Timing.WaitForSeconds(delayBetweenConnections);

                try
                {
                    if (InfiniteTerrainOffline.inProgressWest && southCounter != 0)
                        ServerConnectHeightmapSOUTH(TerraLandRuntimeOffline.southIndices[southCounter]);
                    else if (InfiniteTerrainOffline.inProgressEast && southCounter != (activeTilesGrid - 1))
                        ServerConnectHeightmapSOUTH(TerraLandRuntimeOffline.southIndices[southCounter]);
                    else
                        ServerConnectHeightmapSOUTH(TerraLandRuntimeOffline.southIndices[southCounter]);
                }
                catch { }
            }
        }

        public void ServerConnectHeightmapEAST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    eastCounter++;

                    if (eastCounter == activeTilesGrid)
                        LoadTerrainHeightsEAST("East");
                    else
                        Timing.RunCoroutine(ConnectTileEAST());
                });
            });
        }

        private IEnumerator<float> ConnectTileEAST()
        {
            if (InfiniteTerrainOffline.eastTerrains.Count > 0)
            {
                yield return Timing.WaitForSeconds(delayBetweenConnections);

                try
                {
                    if (InfiniteTerrainOffline.inProgressNorth && eastCounter != 0)
                        ServerConnectHeightmapEAST(TerraLandRuntimeOffline.eastIndices[eastCounter]);
                    else if (InfiniteTerrainOffline.inProgressSouth && eastCounter != (activeTilesGrid - 1))
                        ServerConnectHeightmapEAST(TerraLandRuntimeOffline.eastIndices[eastCounter]);
                    else
                        ServerConnectHeightmapEAST(TerraLandRuntimeOffline.eastIndices[eastCounter]);
                }
                catch { }
            }
        }

        public void ServerConnectHeightmapWEST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    westCounter++;

                    if (westCounter == activeTilesGrid)
                        LoadTerrainHeightsWEST("West");
                    else
                        Timing.RunCoroutine(ConnectTileWEST());
                });
            });
        }

        private IEnumerator<float> ConnectTileWEST()
        {
            if (InfiniteTerrainOffline.westTerrains.Count > 0)
            {
                yield return Timing.WaitForSeconds(delayBetweenConnections);

                try
                {
                    if (InfiniteTerrainOffline.inProgressNorth && westCounter != 0)
                        ServerConnectHeightmapWEST(TerraLandRuntimeOffline.westIndices[westCounter]);
                    else if (InfiniteTerrainOffline.inProgressSouth && westCounter != (activeTilesGrid - 1))
                        ServerConnectHeightmapWEST(TerraLandRuntimeOffline.westIndices[westCounter]);
                    else
                        ServerConnectHeightmapWEST(TerraLandRuntimeOffline.westIndices[westCounter]);
                }
                catch { }
            }
        }


        public void LoadTerrainHeightsNORTH(string dir)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.SmoothNORTH();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntimeOffline.LoadTerrainHeightsNORTH(dir));

                //print(InfiniteTerrainOffline.northTerrains.Count);

                //if (InfiniteTerrainOffline.northTerrains.Count == 0)
                //TerraLandRuntimeOffline.ManageNeighborings(dir);
            });
            });
        }

        public void LoadTerrainHeightsSOUTH(string dir)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.SmoothSOUTH();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntimeOffline.LoadTerrainHeightsSOUTH(dir));
                });
            });
        }

        public void LoadTerrainHeightsEAST(string dir)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.SmoothEAST();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntimeOffline.LoadTerrainHeightsEAST(dir));
                });
            });
        }

        public void LoadTerrainHeightsWEST(string dir)
        {
            RunAsync(() =>
            {
                TerraLandRuntimeOffline.SmoothWEST();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntimeOffline.LoadTerrainHeightsWEST(dir));
                });
            });
        }

        public void ApplyImageData()
        {
            if (TerraLandRuntimeOffline.geoImagesOK)
                StartCoroutine(TerraLandRuntimeOffline.FillImagesFAST());
            //Timing.RunCoroutine(TerraLandRuntimeOffline.FillImages(totalTiles));
        }

        public void ServerConnectImagery(int i, string dir)
        {
            //StartCoroutine(TerraLandRuntimeOffline.FillImageFAST(i, dir));
        }

        public void ServerConnectImagery(string dir)
        {
            StartCoroutine(TerraLandRuntimeOffline.FillImageFAST(dir));
        }

        public void SendNewTiles(List<Terrain> tiles)
        {
            //StartCoroutine(streamingAssets.ClearNewTileAssets(tiles));

            //if (isStreamingAssets)
            //streamingAssets.ClearNewTileAssets(tiles);
        }

        public void SendProcessedTiles(List<Terrain> tiles)
        {
            if (isStreamingAssets)
                StartCoroutine(streamingAssets.PopulateTiles(tiles));
        }

        #region multithreading functions

        protected void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        protected void QueueOnMainThread(Action action, float time)
        {
            if (time != 0)
            {
                lock (_delayed)
                {
                    _delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                lock (_actions)
                {
                    _actions.Add(action);
                }
            }
        }

        protected Thread RunAsync(Action a)
        {
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }

            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }

        private void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }
        }

        #endregion
    }
}

