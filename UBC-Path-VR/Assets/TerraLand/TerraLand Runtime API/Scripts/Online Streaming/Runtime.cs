using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MEC;

namespace TerraLand
{
    public class Runtime : MonoBehaviour
    {
        public enum gridSizeEnumList
        {
            _1 = 1,
            _2x2 = 2,
            _4x4 = 4,
            _8x8 = 8,
            _16x16 = 16,
            _32x32 = 32,
            _64x64 = 64
        }

        // Main Settings
        public gridSizeEnumList terrainGridSize = gridSizeEnumList._8x8;
        public string latitudeUser = ""; // 27.98582
        public string longitudeUser = ""; // 86.9236
        public float areaSize = 25f;
        public int heightmapResolution = 1024;
        public int imageResolution = 1024;
        public float elevationExaggeration = 1.25f;
        public int smoothIterations = 1;
        public bool farTerrain = true;
        public int farTerrainHeightmapResolution = 512;
        public int farTerrainImageResolution = 1024;
        public float areaSizeFarMultiplier = 4f;


        // Performance Settings
        public float heightmapPixelError = 10f;
        public float farTerrainQuality = 10f;
        public int cellSize = 64;
        public int concurrentTasks = 4;
        public float elevationDelay = 0.5f;
        public float imageryDelay = 0.5f;

        // Advanced Settings
        public bool elevationOnly = false;
        public bool fastStartBuild = true;
        public bool showTileOnFinish = true;
        public bool progressiveTexturing = true;
        public bool spiralGeneration = true;
        public bool delayedLOD = false;
        [HideInInspector] public bool IsCustomGeoServer = false;
        [HideInInspector] public bool progressiveGeneration = false;
        [HideInInspector] public float terrainDistance;
        [HideInInspector] public float terrainCurvator;
        public float farTerrainBelowHeight = 100f;
        [HideInInspector] public int farTerrainCellSize;
        public bool stitchTerrainTiles = true;
        [Range(5, 100)] public int levelSmooth = 5;
        [Range(1, 7)] public int power = 1;
        public bool trend = false;
        public int stitchDistance = 4;
        public float stitchDelay = 0.25f;

        //TODO: User Geo-Server
        [HideInInspector] public string dataBasePath = "C:/Users/Amir/Desktop/GeoServer"; //public string dataBasePath = "http://terraunity.com/freedownload/TerraLand_GeoServer";

        public static bool initialRunInBackground;


        // Menu Settings Parameters
        // Main Settings
        public static string terrainGridSizeMenu;
        public static string latitudeMenu;
        public static string longitudeMenu;
        public static float areaSizeMenu;
        public static int heightmapResolutionMenu;
        public static int imageResolutionMenu;
        public static float elevationExaggerationMenu;
        public static int smoothIterationsMenu;
        public static bool farTerrainMenu;
        public static int farTerrainHeightmapResolutionMenu;
        public static int farTerrainImageResolutionMenu;
        public static float areaSizeFarMultiplierMenu;

        // Performance Settings
        public static float heightmapPixelErrorMenu;
        public static float farTerrainQualityMenu;
        public static int cellSizeMenu;
        public static int concurrentTasksMenu;
        public static float elevationDelayMenu;
        public static float imageryDelayMenu;

        // Advanced Settings
        public static bool elevationOnlyMenu;
        public static bool fastStartBuildMenu;
        public static bool showTileOnFinishMenu;
        public static bool progressiveTexturingMenu;
        public static bool spiralGenerationMenu;
        public static bool delayedLODMenu;
        public static float farTerrainBelowHeightMenu;
        public static bool stitchTerrainTilesMenu;
        public static int levelSmoothMenu;
        public static int powerMenu;
        public static int stitchDistanceMenu;
        public static float stitchDelayMenu;


        #region multithreading variables

        int maxThreads = 50;
        private int numThreads;
        private int _count;

        private bool m_HasLoaded = false;

        private List<Action> _actions = new List<Action>();
        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
        private List<Action> _currentActions = new List<Action>();

        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        #endregion


        //public WorldElevation.Terrain_ImageServer mapserviceTerrain = null;

        void Start()
        {
#if UNITY_EDITOR
            initialRunInBackground = UnityEditor.PlayerSettings.runInBackground;
            UnityEditor.PlayerSettings.runInBackground = true;
#endif

            if (!MainMenu.latitude.Equals(""))
                SetFromMenu();

            terrainDistance = (areaSize * 1000f) / 3f; //2f
            farTerrainCellSize = cellSize;

            terrainCurvator = 0.00001f;

            int tileResolution = (heightmapResolution / (int)terrainGridSize);

            if (cellSize > tileResolution)
                cellSize = tileResolution;

            if (farTerrainCellSize > farTerrainHeightmapResolution)
                farTerrainCellSize = farTerrainHeightmapResolution;

            m_HasLoaded = true;

            //#if UNITY_EDITOR
            //ConnectionsManager.SetAsyncConnections();
            //#else
            ConnectionsManagerRuntime.SetAsyncConnections();
            //#endif

            //mapserviceTerrain = new WorldElevation.Terrain_ImageServer();
            TerraLandRuntime.Initialize();

            progressiveGeneration = false;
        }

        void Update()
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
        }

        public void SetFromMenu()
        {
            // Main Settings
            terrainGridSize = (gridSizeEnumList)Enum.Parse(typeof(gridSizeEnumList), terrainGridSizeMenu);
            latitudeUser = latitudeMenu;
            longitudeUser = longitudeMenu;
            areaSize = areaSizeMenu;
            heightmapResolution = heightmapResolutionMenu;
            imageResolution = imageResolutionMenu;
            elevationExaggeration = elevationExaggerationMenu;
            smoothIterations = smoothIterationsMenu;
            farTerrain = farTerrainMenu;
            farTerrainHeightmapResolution = farTerrainHeightmapResolutionMenu;
            farTerrainImageResolution = farTerrainImageResolutionMenu;
            areaSizeFarMultiplier = areaSizeFarMultiplierMenu;


            // Performance Settings
            heightmapPixelError = heightmapPixelErrorMenu;
            farTerrainQuality = farTerrainQualityMenu;
            cellSize = cellSizeMenu;
            concurrentTasks = concurrentTasksMenu;
            elevationDelay = elevationDelayMenu;
            imageryDelay = imageryDelayMenu;


            // Advanced Settings
            elevationOnly = elevationOnlyMenu;
            fastStartBuild = fastStartBuildMenu;
            showTileOnFinish = showTileOnFinishMenu;
            progressiveTexturing = progressiveTexturingMenu;
            spiralGeneration = spiralGenerationMenu;
            delayedLOD = delayedLODMenu;
            farTerrainBelowHeight = farTerrainBelowHeightMenu;
            stitchTerrainTiles = stitchTerrainTilesMenu;
            levelSmooth = levelSmoothMenu;
            power = powerMenu;
            stitchDistance = stitchDistanceMenu;
            stitchDelay = stitchDelayMenu;
        }

        public void LoadTerrainHeights()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.SmoothAllHeights();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFDynamic(0));
                });
            });
        }

        public void LoadTerrainHeightsFAR()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.SmoothFarTerrain();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFFAR());
                });
            });
        }

        public void LoadTerrainHeightsNORTH(int i)
        {
            RunAsync(() =>
            {
            //TerraLandRuntime.SmoothNORTH(i);

            if (i == (int)terrainGridSize)
                    TerraLandRuntime.SmoothNORTH(i);

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromNORTH());

                //                //Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(i));
                //
                //
                //                //if(i == (int)terrainGridSize - 1)
                //                if(i == (int)terrainGridSize)
                //                {
                ////                    if(InfiniteTerrain.inProgressWest)
                ////                    {
                ////                        print("Moving North West");
                ////
                ////                        for(int x = 0; x < (int)terrainGridSize - 1; x++)
                ////                            Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                ////                    }
                ////                    else if(InfiniteTerrain.inProgressEast)
                ////                    {
                ////                        for(int x = 0; x < (int)terrainGridSize - 1; x++)
                ////                            Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                ////                    }
                ////                    else
                ////                    {
                //                        for(int x = 0; x < (int)terrainGridSize; x++)
                //                            Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                //                    //}
                //                }
            });
            });
        }

        public void LoadTerrainHeightsSOUTH(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLandRuntime.SmoothSOUTH();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromSOUTH());
                });
            });
        }

        public void LoadTerrainHeightsEAST(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLandRuntime.SmoothEAST();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromEAST());
                });
            });
        }

        public void LoadTerrainHeightsWEST(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLandRuntime.SmoothWEST();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromWEST());
                });
            });
        }

        private IEnumerator<float> HeightsFromNORTH()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + (elevationDelay * 2);
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromSOUTH()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFSOUTH(InfiniteTerrain.southIndex + x));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromEAST()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFEAST(InfiniteTerrain.eastIndex + (x * (int)terrainGridSize)));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromWEST()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLandRuntime.LoadTerrainHeightsFromTIFFWEST(InfiniteTerrain.westIndex + (x * (int)terrainGridSize)));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        public void GetHeightmaps()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevation();
            });
        }

        public void GetHeightmapFAR()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevationFAR();
            });
        }

        public void GetHeightmapsNORTH(int index)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevationNORTH(index);
            });
        }

        public void GetHeightmapsSOUTH()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevationSOUTH();
            });
        }

        public void GetHeightmapsEAST()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevationEAST();
            });
        }

        public void GetHeightmapsWEST()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoElevationWEST();
            });
        }

        //    public void ServerConnectHeightmap ()
        //    {
        //        RunAsync(()=>
        //        {
        //            TerraLandRuntime.ElevationDownload();
        //
        //            QueueOnMainThread(()=>
        //            {
        //                GenerateTerrainHeights();
        //            });
        //        });
        //    }

        public void ServerConnectHeightmap(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    TerraLandRuntime.LoadHeights(i);
                });
            });
        }

        public void ServerConnectHeightmapFAR()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownloadFAR();

                QueueOnMainThread(() =>
                {
                    TerraLandRuntime.LoadHeightsFAR();
                });
            });
        }

        public void ServerConnectHeightmapNORTH(int index)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownloadNORTH(index);

                QueueOnMainThread(() =>
                {
                //                //if(!InfiniteTerrain.inProgressSouth)
                //                TerraLandRuntime.LoadHeightsNORTH(index);

                if (!InfiniteTerrain.inProgressSouth)
                    {
                        if (TerraLandRuntime.northCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLandRuntime.LoadHeightsNORTH(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLandRuntime.northCounter = 0;
                    //InfiniteTerrain.northTerrains.Clear();
                }

                });
            });
        }

        public void ServerConnectHeightmapSOUTH(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownloadSOUTH(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressNorth)
                    {
                        if (TerraLandRuntime.southCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLandRuntime.LoadHeightsSOUTH(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLandRuntime.southCounter = 0;
                    //InfiniteTerrain.southTerrains.Clear();
                }
                });
            });
        }

        public void ServerConnectHeightmapEAST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownloadEAST(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressWest)
                    {
                        if (TerraLandRuntime.eastCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLandRuntime.LoadHeightsEAST(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLandRuntime.eastCounter = 0;
                    //InfiniteTerrain.eastTerrains.Clear();
                }
                });
            });
        }

        public void ServerConnectHeightmapWEST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ElevationDownloadWEST(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressEast)
                    {
                        if (TerraLandRuntime.westCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLandRuntime.LoadHeightsWEST(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLandRuntime.westCounter = 0;
                    //InfiniteTerrain.westTerrains.Clear();
                }
                });
            });
        }

        public void GenerateTerrainHeights()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.TiffData(TerraLandRuntime.fileNameTerrainData);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntime.tiffData, TerraLandRuntime.tiffWidth, TerraLandRuntime.tiffLength);
                });
            });
        }

        public void FinalizeTerrainHeights(float[,] data, int width, int height)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.SmoothHeights(data, width, height);

                QueueOnMainThread(() =>
                {
                    TerraLandRuntime.FinalizeHeights();
                });
            });
        }

        public void TerrainFromRAW()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.RawData(TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntime.rawData, TerraLandRuntime.m_Width, TerraLandRuntime.m_Height);
                });
            });
        }

        public void TerrainFromTIFF()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.TiffData(TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntime.tiffData, TerraLandRuntime.tiffWidth, TerraLandRuntime.tiffLength);
                });
            });
        }

        public void TerrainFromASCII()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.AsciiData(TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLandRuntime.asciiData, TerraLandRuntime.nCols, TerraLandRuntime.nRows);
                });
            });
        }

        public void ApplyElevationData()
        {
            IEnumerable<string> names = Directory.GetFiles(TerraLandRuntime.dataBasePathElevation, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".asc")
                    || s.EndsWith(".raw")
                    || s.EndsWith(".tif"));

            if (names.ToArray().Length == 0)
                UnityEngine.Debug.LogError("NO AVILABLE DATA - No elevation data is available in selected folder.");
            else
            {
                TerraLandRuntime.geoDataPathElevation = names.ToArray()[0];

                if (TerraLandRuntime.geoDataPathElevation.EndsWith(".asc") || TerraLandRuntime.geoDataPathElevation.EndsWith(".raw") || TerraLandRuntime.geoDataPathElevation.EndsWith(".tif"))
                {
                    String[] pathParts = TerraLandRuntime.geoDataPathElevation.Split(char.Parse("."));
                    TerraLandRuntime.geoDataExtensionElevation = pathParts[pathParts.Length - 1];

                    if (TerraLandRuntime.geoDataExtensionElevation.Equals("raw"))
                    {
                        RunAsync(() =>
                        {
                            TerraLandRuntime.GetElevationFileInfo();

                            QueueOnMainThread(() =>
                            {
                                TerraLandRuntime.ApplyOfflineTerrain();
                            });
                        });
                    }
                }
                else
                    UnityEngine.Debug.LogError("UNKNOWN FORMAT - There are no valid ASCII, RAW or Tiff files in selected folder.");
            }
        }

        public void ApplyImageData()
        {
            TerraLandRuntime.GetFolderInfo(TerraLandRuntime.dataBasePathImagery);

            if (TerraLandRuntime.totalImagesDataBase == 0)
            {
                TerraLandRuntime.geoImagesOK = false;
                UnityEngine.Debug.LogError("There are no images in data base!");
            }
            else
                TerraLandRuntime.geoImagesOK = true;

            if (TerraLandRuntime.terrainChunks > TerraLandRuntime.totalImagesDataBase)
            {
                TerraLandRuntime.geoImagesOK = false;
                UnityEngine.Debug.LogError("No sufficient images to texture terrains. Select a lower Grid Size for terrains");
            }
            else
                TerraLandRuntime.geoImagesOK = true;

            if (TerraLandRuntime.geoImagesOK)
            {
                Vector2Int imageDimensions = ImageUtils.GetJpegImageSize(TerraLandRuntime.geoImageNames[0]);
                TerraLandRuntime.imageWidth = imageDimensions.x;
                TerraLandRuntime.imageHeight = imageDimensions.y;
                imageResolution = TerraLandRuntime.imageWidth;

                for (int i = 0; i < TerraLandRuntime.geoImageNames.Length; i++)
                {
                    TerraLandRuntime.images.Add(new Texture2D(TerraLandRuntime.imageWidth, TerraLandRuntime.imageHeight, TextureFormat.RGB24, true, true));
                    TerraLandRuntime.images[i].wrapMode = TextureWrapMode.Clamp;
                }

                RunAsync(() =>
                {
                    TerraLandRuntime.imageBytes = new List<byte[]>();

                    for (int i = 0; i < TerraLandRuntime.geoImageNames.Length; i++)
                        TerraLandRuntime.DownloadImageData(TerraLandRuntime.geoImageNames[i]);

                    QueueOnMainThread(() =>
                    {
                        Timing.RunCoroutine(TerraLandRuntime.FillImages(TerraLandRuntime.totalImagesDataBase));
                    });
                });
            }
        }

        public void GetSatelliteImages()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImagery();
            });
        }

        public void GetSatelliteImagesFAR()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImageryFAR();
            });
        }

        public void GetSatelliteImagesNORTH()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImageryNORTH();
            });
        }

        public void GetSatelliteImagesSOUTH()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImagerySOUTH();
            });
        }

        public void GetSatelliteImagesEAST()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImageryEAST();
            });
        }

        public void GetSatelliteImagesWEST()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ServerInfoImageryWEST();
            });
        }

        public void ServerConnectImagery(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloader(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLandRuntime.allBlack)
                    {
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");
                        TerraLandRuntime.imageDownloadingStarted = false;
                        return;
                    }

                    if (progressiveTexturing)
                        Timing.RunCoroutine(TerraLandRuntime.FillImage(i));
                    else
                    {
                        if (TerraLandRuntime.downloadedImageIndex == TerraLandRuntime.totalImages)
                            Timing.RunCoroutine(TerraLandRuntime.FillImages(TerraLandRuntime.totalImages));
                    }
                });
            });
        }

        public void ServerConnectImageryFAR()
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloaderFAR();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLandRuntime.FillImageFAR());
                });
            });
        }

        public void ServerConnectImageryNORTH(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloaderNORTH(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLandRuntime.FillImageNORTH(i));
                });
            });
        }

        public void ServerConnectImagerySOUTH(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloaderSOUTH(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLandRuntime.FillImageSOUTH(i));
                });
            });
        }

        public void ServerConnectImageryEAST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloaderEAST(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLandRuntime.FillImageEAST(i));
                });
            });
        }

        public void ServerConnectImageryWEST(int i)
        {
            RunAsync(() =>
            {
                TerraLandRuntime.ImageDownloaderWEST(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLandRuntime.FillImageWEST(i));
                });
            });
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

        private void UnloadResources()
        {
            UnloadAllAssets();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void UnloadAllAssets()
        {
            try
            {
                Destroy(TerraLandRuntime.terrain);
                Destroy(TerraLandRuntime.firstTerrain);
                Destroy(TerraLandRuntime.secondaryTerrain);
                Destroy(TerraLandRuntime.currentTerrain);
                Destroy(TerraLandRuntime.farImage);
                Destroy(TerraLandRuntime.data);

                TerraLandRuntime.webClientTerrain = null;
                TerraLandRuntime.tiffData = null;
                TerraLandRuntime.tiffDataASCII = null;
                TerraLandRuntime.tiffDataFAR = null;
                TerraLandRuntime.tiffDataASCIIFAR = null;
                TerraLandRuntime.finalHeights = null;
                TerraLandRuntime.heightmapCell = null;
                TerraLandRuntime.heightmapCellSec = null;
                TerraLandRuntime.heightmapCellFar = null;
                TerraLandRuntime.rawData = null;
                TerraLandRuntime.webClientImage = null;
                TerraLandRuntime.smData = null;
                TerraLandRuntime.farImageBytes = null;
                TerraLandRuntime.asciiData = null;
                TerraLandRuntime.tiffDataFar = null;
                TerraLandRuntime._terrainDict = null;
                TerraLandRuntime.heights = null;
                TerraLandRuntime.secondHeights = null;

                for (int i = 0; i < TerraLandRuntime.croppedTerrains.Count; i++)
                    Destroy(TerraLandRuntime.croppedTerrains[i]);

                for (int i = 0; i < TerraLandRuntime.images.Count; i++)
                    Destroy(TerraLandRuntime.images[i]);

                for (int i = 0; i < TerraLandRuntime.imageBytes.Count; i++)
                    TerraLandRuntime.imageBytes[i] = null;

                for (int i = 0; i < TerraLandRuntime.tiffDataDynamic.Count; i++)
                    TerraLandRuntime.tiffDataDynamic[i] = null;

                for (int i = 0; i < TerraLandRuntime._terrains.Length; i++)
                    Destroy(TerraLandRuntime._terrains[i]);

                if (TerraLandRuntime.stitchingTerrainsList != null)
                {
                    for (int i = 0; i < TerraLandRuntime.stitchingTerrainsList.Count; i++)
                        Destroy(TerraLandRuntime.stitchingTerrainsList[i]);
                }
            }
            catch { }
        }

        public void OnDisable()
        {
            UnloadResources();
        }
    }
}

