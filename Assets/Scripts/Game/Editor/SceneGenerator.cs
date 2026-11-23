using System.IO;
using FlatIronFields.Game;
using FlatIronFields.Input;
using FlatIronFields.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace FlatIronFields.Game.Editor
{
    public static class SceneGenerator
    {
        private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private static TMP_FontAsset uiFont;

        [MenuItem("Flat Iron Fields/Generate Scenes")]
        public static void GenerateAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            uiFont = EnsureTmpFont();

            CreateBootScene();
            CreateMainMenuScene();
            CreateFarmScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Flat Iron Fields: generated Boot, MainMenu, and Farm scenes.");
        }

        private static TMP_FontAsset EnsureTmpFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (font != null)
            {
                return font;
            }

            string editorFolder = Path.GetDirectoryName(EditorApplication.applicationPath);
            if (string.IsNullOrEmpty(editorFolder))
            {
                Debug.LogWarning("Flat Iron Fields: could not locate the Unity editor folder for TMP Essential Resources.");
                return null;
            }

            string package = Path.Combine(
                editorFolder,
                "Data",
                "Resources",
                "PackageManager",
                "BuiltInPackages",
                "com.unity.ugui",
                "Package Resources",
                "TMP Essential Resources.unitypackage");

            if (File.Exists(package))
            {
                AssetDatabase.ImportPackage(package, false);
                AssetDatabase.Refresh();
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            }

            if (font == null)
            {
                Debug.LogWarning("Flat Iron Fields: TMP Essential Resources were not imported; UI text may not render.");
            }

            return font;
        }

        private static void CreateBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(scene);
            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameController>();
            bootstrap.AddComponent<InputRouter>();
            bootstrap.AddComponent<SceneLoader>();
            ConfigureAudio(bootstrap.AddComponent<AudioService>());
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(scene);
            AddGlobalVolume(scene);
            Canvas canvas = AddCanvas(scene, "MainMenuCanvas");

            TextMeshProUGUI title = CreateText(canvas.transform, "Title", "Flat Iron Fields", 48);
            title.rectTransform.anchoredPosition = new Vector2(0f, 160f);

            GameObject buttonObject = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvas.transform, false);
            Button button = buttonObject.GetComponent<Button>();
            SceneLoader loader = buttonObject.AddComponent<SceneLoader>();
            UnityEventTools.AddPersistentListener(button.onClick, loader.LoadFarm);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 60f);

            TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", "Start Farming", 24);
            label.rectTransform.anchoredPosition = Vector2.zero;

            GameObject audio = new GameObject("AudioService");
            ConfigureAudio(audio.AddComponent<AudioService>());

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        private static void CreateFarmScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = AddCamera(scene);
            Tilemap tilemap = AddFarmTilemap(scene);
            Volume volume = AddGlobalVolume(scene);
            Canvas canvas = AddCanvas(scene, "FarmCanvas");

            UIManager ui = new GameObject("UIManager", typeof(RectTransform)).AddComponent<UIManager>();
            ui.transform.SetParent(canvas.transform, false);
            ui.DayText = CreateText(canvas.transform, "Day", "Day: 1", 20);
            ui.SeasonText = CreateText(canvas.transform, "Season", "Season: Spring", 20);
            ui.WeatherText = CreateText(canvas.transform, "Weather", "Weather: Sunny", 20);
            ui.CoinsText = CreateText(canvas.transform, "Coins", "Coins: 100", 20);
            ui.XpText = CreateText(canvas.transform, "XP", "XP: 0", 20);
            ui.LevelText = CreateText(canvas.transform, "Level", "Level: 1", 20);
            ui.SkillPointsText = CreateText(canvas.transform, "SkillPoints", "Skill Points: 0", 20);
            ui.CropStatusText = CreateText(canvas.transform, "CropStatus", "Crop: none", 18);
            ui.TractorStatusText = CreateText(canvas.transform, "TractorStatus", "Tractor: none", 18);
            ui.MessageText = CreateText(canvas.transform, "Messages", string.Empty, 18);

            GameObject panel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            panel.SetActive(false);
            ui.MenuPanel = panel;
            ui.MenuText = CreateText(panel.transform, "MenuText", string.Empty, 22);

            GameObject systems = new GameObject("GameBootstrap");
            GameController controller = systems.AddComponent<GameController>();
            systems.AddComponent<InputRouter>();
            controller.UI = ui;
            controller.Input = systems.GetComponent<InputRouter>();
            controller.Audio = ConfigureAudio(systems.AddComponent<AudioService>());

            SeasonalPaletteSystem palette = systems.AddComponent<SeasonalPaletteSystem>();
            palette.TargetCamera = camera;
            palette.Tilemaps = new[] { tilemap };
            palette.GlobalVolume = volume;
            palette.UI = ui;
            controller.Palette = palette;

            HudController hud = systems.AddComponent<HudController>();
            hud.Manager = ui;

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Farm.unity");
        }

        private static Tilemap AddFarmTilemap(Scene scene)
        {
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Tiles");
            Tile soil = CreateTile("SoilTile", new Color(0.45f, 0.30f, 0.18f, 1f));
            Tile grass = CreateTile("GrassTile", new Color(0.32f, 0.52f, 0.24f, 1f));

            var gridObject = new GameObject("FarmGrid", typeof(Grid));
            SceneManager.MoveGameObjectToScene(gridObject, scene);
            var tilemapObject = new GameObject("PlotTilemap", typeof(Tilemap), typeof(TilemapRenderer));
            tilemapObject.transform.SetParent(gridObject.transform, false);

            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = -1;
            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    bool plot = y == 0 && x < 5;
                    tilemap.SetTile(new Vector3Int(x, y, 0), plot ? soil : grass);
                }
            }
            return tilemap;
        }

        private static Tile CreateTile(string name, Color color)
        {
            string path = "Assets/Art/Tiles/" + name + ".asset";
            Tile existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (existing != null) { return existing; }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                name = name + "Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) { pixels[i] = color; }
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, "Assets/Art/Tiles/" + name + "Texture.asset");

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = name + "Sprite";
            AssetDatabase.AddObjectToAsset(sprite, texture);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = sprite;
            tile.color = Color.white;
            AssetDatabase.CreateAsset(tile, path);
            EditorUtility.SetDirty(texture);
            AssetDatabase.SaveAssets();
            return tile;
        }

        private static AudioService ConfigureAudio(AudioService audio)
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(PhaseTwoAssetGenerator.MixerPath);
            audio.Mixer = mixer;
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(PhaseTwoAssetGenerator.MixerPath);
            if (assets != null)
            {
                foreach (UnityEngine.Object asset in assets)
                {
                    AudioMixerGroup group = asset as AudioMixerGroup;
                    if (group == null) { continue; }
                    if (group.name == "Music") { audio.MusicGroup = group; }
                    if (group.name == "Sfx") { audio.SfxGroup = group; }
                }
            }
            return audio;
        }

        private static Volume AddGlobalVolume(Scene scene)
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PhaseTwoAssetGenerator.ProfilePath);
            GameObject volumeObject = new GameObject("GlobalVolume", typeof(Volume));
            SceneManager.MoveGameObjectToScene(volumeObject, scene);
            Volume volume = volumeObject.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
            return volume;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) { return; }
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string name = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Camera AddCamera(Scene scene)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.42f, 0.55f, 0.28f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            return camera;
        }

        private static Canvas AddCanvas(Scene scene, string name)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            SceneManager.MoveGameObjectToScene(canvasObject, scene);

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            return canvas;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string value, int fontSize)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(420f, 40f);
            if (uiFont != null)
            {
                text.font = uiFont;
                text.fontSharedMaterial = uiFont.material;
            }
            return text;
        }
    }
}
