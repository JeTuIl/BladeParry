using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

namespace BladeParry.Editor.Localization
{
    /// <summary>
    /// Editor window to analyze scenes for TextMeshProUGUI to localize and scripts for display strings.
    /// Menu: Tools > Localization > Analyze for localization
    /// </summary>
    public class LocalizationAnalyzerWindow : EditorWindow
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ScriptsFolder = "Assets/Scripts";
        private const string DefaultReportPath = "Assets/Localization/LocalizationReport.json";

        private Vector2 _scrollScene;
        private Vector2 _scrollScripts;
        private Vector2 _scrollReport;
        private int _tab;
        private readonly List<TmpEntry> _sceneEntries = new List<TmpEntry>();
        private readonly List<ScriptStringEntry> _scriptEntries = new List<ScriptStringEntry>();
        private string _sceneSearchFolder = ScenesFolder;
        private bool _sceneAnalysisDirty = true;
        private bool _scriptAnalysisDirty = true;
        private string _reportPath = DefaultReportPath;
        private int _selectedCollectionIndex;
        private int _sourceLocaleIndex;
        private GUIContent[] _collectionOptions = Array.Empty<GUIContent>();
        private GUIContent[] _localeOptions = Array.Empty<GUIContent>();

        [MenuItem("Tools/Localization/Analyze for localization")]
        public static void ShowWindow()
        {
            var w = GetWindow<LocalizationAnalyzerWindow>("Localization Analyzer");
            w.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            _tab = GUILayout.Toolbar(_tab, new[] { "Scenes (TMP)", "Scripts (display strings)", "Report & fill" });
            GUILayout.Space(4);

            if (_tab == 0)
                DrawSceneTab();
            else if (_tab == 1)
                DrawScriptsTab();
            else
                DrawReportAndFillTab();
        }

        #region Scene tab: TextMeshPro in scenes

        private void DrawSceneTab()
        {
            EditorGUILayout.BeginHorizontal();
            _sceneSearchFolder = EditorGUILayout.TextField("Scenes folder", _sceneSearchFolder);
            if (GUILayout.Button("Analyze scenes", GUILayout.Width(120)))
            {
                AnalyzeScenes();
                _sceneAnalysisDirty = false;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "Finds all TextMeshProUGUI components in every scene under the folder. " +
                "Use this list to hook up localization keys for each TMP that shows user-facing text.",
                MessageType.Info);

            if (_sceneEntries.Count == 0 && !_sceneAnalysisDirty)
                EditorGUILayout.LabelField("No TMP components found. Click \"Analyze scenes\" or add scenes under the folder.");
            else if (_sceneEntries.Count > 0)
            {
                if (GUILayout.Button("Copy CSV to clipboard"))
                    CopySceneEntriesToClipboard();
                _scrollScene = EditorGUILayout.BeginScrollView(_scrollScene);
                DrawSceneTableHeader();
                foreach (var e in _sceneEntries)
                    DrawSceneRow(e);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSceneTableHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Scene", EditorStyles.boldLabel, GUILayout.Width(180));
            GUILayout.Label("GameObject path", EditorStyles.boldLabel, GUILayout.MinWidth(200));
            GUILayout.Label("Current text", EditorStyles.boldLabel, GUILayout.Width(220));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneRow(TmpEntry e)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileNameWithoutExtension(e.ScenePath), GUILayout.Width(180));
            GUILayout.Label(e.GameObjectPath, GUILayout.MinWidth(200));
            var preview = e.CurrentText ?? "";
            if (preview.Length > 50)
                preview = preview.Substring(0, 47) + "...";
            preview = preview.Replace("\n", " ");
            GUILayout.Label(preview, GUILayout.Width(220));
            EditorGUILayout.EndHorizontal();
        }

        private void CopySceneEntriesToClipboard()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Scene,GameObjectPath,CurrentText");
            foreach (var e in _sceneEntries)
            {
                var text = (e.CurrentText ?? "").Replace("\"", "\"\"").Replace("\r", "").Replace("\n", " ");
                sb.AppendLine($"\"{Path.GetFileNameWithoutExtension(e.ScenePath)}\",\"{e.GameObjectPath}\",\"{text}\"");
            }
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }

        private void AnalyzeScenes()
        {
            _sceneEntries.Clear();
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { _sceneSearchFolder });
            if (sceneGuids.Length == 0)
            {
                Debug.LogWarning($"LocalizationAnalyzer: No scenes found under '{_sceneSearchFolder}'.");
                return;
            }

            string currentScenePath = SceneManager.GetActiveScene().path;
            bool hadOpenScene = !string.IsNullOrEmpty(currentScenePath);

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(scenePath))
                    continue;
                AnalyzeOneScene(scenePath);
            }

            if (hadOpenScene && !string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            Debug.Log($"LocalizationAnalyzer: Found {_sceneEntries.Count} TextMeshProUGUI in {sceneGuids.Length} scene(s).");
        }

        private void AnalyzeOneScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
                return;

            TMPro.TMP_Text[] tmpComponents = UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMPro.TMP_Text tmp in tmpComponents)
            {
                if (tmp == null)
                    continue;
                string path = GetHierarchyPath(tmp.transform);
                _sceneEntries.Add(new TmpEntry
                {
                    ScenePath = scenePath,
                    GameObjectPath = path,
                    CurrentText = tmp.text
                });
            }
        }

        private static string GetHierarchyPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private struct TmpEntry
        {
            public string ScenePath;
            public string GameObjectPath;
            public string CurrentText;
        }

        #endregion

        #region Scripts tab: display strings in code

        private void DrawScriptsTab()
        {
            if (GUILayout.Button("Analyze Scripts", GUILayout.Width(120)))
            {
                AnalyzeScripts();
                _scriptAnalysisDirty = false;
            }
            EditorGUILayout.HelpBox(
                "Scans C# files under Assets/Scripts for strings that are displayed on screen: .text = ..., string.Format(..., ShowAt(\"...\", ...), etc. " +
                "Excludes Editor and Debug-only code.",
                MessageType.Info);

            if (_scriptEntries.Count == 0 && !_scriptAnalysisDirty)
                EditorGUILayout.LabelField("No display strings found. Click \"Analyze Scripts\".");
            else if (_scriptEntries.Count > 0)
            {
                if (GUILayout.Button("Copy report to clipboard"))
                    CopyScriptEntriesToClipboard();
                _scrollScripts = EditorGUILayout.BeginScrollView(_scrollScripts);
                DrawScriptTableHeader();
                foreach (var e in _scriptEntries)
                    DrawScriptRow(e);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawScriptTableHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("File:Line", EditorStyles.boldLabel, GUILayout.Width(180));
            GUILayout.Label("Kind", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Snippet / String", EditorStyles.boldLabel, GUILayout.MinWidth(200));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScriptRow(ScriptStringEntry e)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{e.File}:{e.Line}", EditorStyles.miniLabel, GUILayout.Width(180));
            GUILayout.Label(e.Kind, EditorStyles.miniLabel, GUILayout.Width(100));
            var snippet = e.Snippet ?? "";
            if (snippet.Length > 80)
                snippet = snippet.Substring(0, 77) + "...";
            GUILayout.Label(snippet, EditorStyles.wordWrappedLabel, GUILayout.MinWidth(200));
            EditorGUILayout.EndHorizontal();
        }

        private void CopyScriptEntriesToClipboard()
        {
            var sb = new StringBuilder();
            sb.AppendLine("File\tLine\tKind\tSnippet");
            foreach (var e in _scriptEntries)
            {
                var snippet = (e.Snippet ?? "").Replace("\t", " ").Replace("\r", "").Replace("\n", " ");
                sb.AppendLine($"{e.File}\t{e.Line}\t{e.Kind}\t{snippet}");
            }
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }

        private void AnalyzeScripts()
        {
            _scriptEntries.Clear();
            string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
            if (!Directory.Exists(scriptsPath))
            {
                Debug.LogWarning("LocalizationAnalyzer: Assets/Scripts folder not found.");
                return;
            }

            string dataPath = Application.dataPath.Replace("\\", "/");
            string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
            foreach (string fullPath in csFiles)
            {
                string normalizedFull = fullPath.Replace("\\", "/");
                if (normalizedFull.Contains("/Editor/"))
                    continue;
                string relativePath = "Assets" + normalizedFull.Substring(dataPath.Length);
                AnalyzeOneScript(fullPath, relativePath);
            }

            Debug.Log($"LocalizationAnalyzer: Found {_scriptEntries.Count} display-string usages in scripts.");
        }

        private void AnalyzeOneScript(string fullPath, string relativePath)
        {
            string content = File.ReadAllText(fullPath);
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            // .text = "literal"
            var regexTextLiteral = new Regex(@"\.(text|SetText)\s*=\s*@?""([^""]*(?:""""[^""]*)*)""", RegexOptions.Compiled);
            // string.Format("...", ...) or $"..."
            var regexFormat = new Regex(@"string\.Format\s*\(\s*@?""([^""]*(?:""""[^""]*)*)""", RegexOptions.Compiled);
            // ShowAt("...", ...) or similar method with first string arg
            var regexShowAt = new Regex(@"(Show(?:Parry|PerfectParry|Miss|Combo|At)|SetMessage)\s*\(\s*@?""([^""]*(?:""""[^""]*)*)""", RegexOptions.Compiled);
            // "Fight!" style literal on same line as .text
            var regexFight = new Regex(@"""([^""]+)""\s*\)?\s*;", RegexOptions.Compiled);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;

                if (line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("*"))
                    continue;
                if (line.Contains("Debug.Log") && line.Contains(".text"))
                    continue;

                Match m;
                m = regexTextLiteral.Match(line);
                if (m.Success)
                {
                    _scriptEntries.Add(new ScriptStringEntry
                    {
                        File = relativePath,
                        Line = lineNum,
                        Kind = ".text/SetText",
                        Snippet = Unescape(m.Groups[2].Value)
                    });
                    continue;
                }

                m = regexFormat.Match(line);
                if (m.Success)
                {
                    _scriptEntries.Add(new ScriptStringEntry
                    {
                        File = relativePath,
                        Line = lineNum,
                        Kind = "string.Format",
                        Snippet = Unescape(m.Groups[1].Value)
                    });
                    continue;
                }

                m = regexShowAt.Match(line);
                if (m.Success)
                {
                    _scriptEntries.Add(new ScriptStringEntry
                    {
                        File = relativePath,
                        Line = lineNum,
                        Kind = "Display method",
                        Snippet = Unescape(m.Groups[2].Value)
                    });
                    continue;
                }

                if (line.Contains("countdownText.text") || line.Contains("summaryText.text") || line.Contains("tmp.text") ||
                    line.Contains("numericText.text") || line.Contains("directionLabel.text") || line.Contains(".text ="))
                {
                    m = Regex.Match(line, @"""([^""]+)""");
                    if (m.Success)
                    {
                        _scriptEntries.Add(new ScriptStringEntry
                        {
                            File = relativePath,
                            Line = lineNum,
                            Kind = ".text literal",
                            Snippet = m.Groups[1].Value
                        });
                    }
                    else if (line.Contains(".text =") && line.Contains(".ToString()"))
                    {
                        _scriptEntries.Add(new ScriptStringEntry
                        {
                            File = relativePath,
                            Line = lineNum,
                            Kind = "Enum/ToString (localize if user-facing)",
                            Snippet = line.Trim()
                        });
                    }
                }

                // SerializeField format strings used for display (e.g. numericFormat)
                if (line.Contains("numericFormat") || line.Contains("Format\""))
                {
                    m = Regex.Match(line, @"(?:numericFormat|string)\s*=\s*@?""([^""]*)""");
                    if (m.Success && (m.Groups[1].Value.Contains("{0}") || m.Groups[1].Value.Contains("{")))
                    {
                        _scriptEntries.Add(new ScriptStringEntry
                        {
                            File = relativePath,
                            Line = lineNum,
                            Kind = "Format string (SerializeField)",
                            Snippet = m.Groups[1].Value
                        });
                    }
                }
            }

            // Multi-line string.Format (e.g. FightSummaryDisplay): find block after "string.Format("
            var formatBlockStart = new Regex(@"string\.Format\s*\(", RegexOptions.Compiled);
            var quotedLine = new Regex(@"@?""((?:[^""\\]|\\.)*)""", RegexOptions.Compiled);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!formatBlockStart.IsMatch(lines[i]))
                    continue;
                var sb = new StringBuilder();
                for (int j = i + 1; j < lines.Length && j < i + 20; j++)
                {
                    string l = lines[j];
                    Match q = quotedLine.Match(l);
                    while (q.Success)
                    {
                        sb.Append(q.Groups[1].Value.Replace("\\n", "\n"));
                        q = q.NextMatch();
                    }
                    if (l.TrimEnd().EndsWith(");"))
                        break;
                }
                if (sb.Length > 0)
                {
                    string fullFormat = sb.ToString();
                    if (fullFormat.Contains("{0}") && !_scriptEntries.Exists(e => e.Line == i + 1 && e.Kind == "string.Format"))
                        _scriptEntries.Add(new ScriptStringEntry
                        {
                            File = relativePath,
                            Line = i + 1,
                            Kind = "string.Format (multi-line)",
                            Snippet = fullFormat.Length > 120 ? fullFormat.Substring(0, 117) + "..." : fullFormat
                        });
                }
            }
        }

        private static string Unescape(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\"\"", "\"");
        }

        private struct ScriptStringEntry
        {
            public string File;
            public int Line;
            public string Kind;
            public string Snippet;
        }

        #endregion

        #region Report & fill tab

        private void DrawReportAndFillTab()
        {
            _scrollReport = EditorGUILayout.BeginScrollView(_scrollReport);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Export report", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Run \"Analyze scenes\" and/or \"Analyze Scripts\" in the other tabs first. Then click \"Write report document\" to save a JSON report with suggested keys.",
                MessageType.Info);
            if (GUILayout.Button("Write report document...", GUILayout.Width(180)))
            {
                string path = EditorUtility.SaveFilePanel("Save localization report", "Assets/Localization", "LocalizationReport", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    string assetPath = path.Replace("\\", "/");
                    if (assetPath.StartsWith(Application.dataPath.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase))
                        assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                    WriteReportDocument(assetPath);
                }
            }
            bool hasData = _sceneEntries.Count > 0 || _scriptEntries.Count > 0;
            if (!hasData)
                EditorGUILayout.LabelField("No data to export. Analyze scenes or scripts first.", EditorStyles.helpBox);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Fill table from report", EditorStyles.boldLabel);
            _reportPath = EditorGUILayout.TextField("Report path", _reportPath);

            RefreshCollectionAndLocaleOptions();
            if (_collectionOptions.Length > 0)
            {
                int newCollection = EditorGUILayout.Popup(new GUIContent("String Table Collection"), _selectedCollectionIndex, _collectionOptions);
                if (newCollection != _selectedCollectionIndex)
                {
                    _selectedCollectionIndex = newCollection;
                    _localeOptions = Array.Empty<GUIContent>();
                    _sourceLocaleIndex = 0;
                }
            }
            else
                EditorGUILayout.LabelField("No String Table Collections found.", EditorStyles.helpBox);

            if (_localeOptions.Length > 0)
            {
                _sourceLocaleIndex = EditorGUILayout.Popup(new GUIContent("Source locale (initial value)"), _sourceLocaleIndex, _localeOptions);
            }

            if (GUILayout.Button("Fill table from report", GUILayout.Width(160)))
                FillTableFromReport();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Scene TMP localization", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Adds Localize String Event to each TMP in the report (except countdown and fight summary, which are set by script). Uses the report path above.",
                MessageType.Info);
            if (GUILayout.Button("Apply scene localization from report", GUILayout.Width(260)))
                ApplySceneLocalizationFromReport();
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Applies localization to the currently opened scene only: finds every TextMeshProUGUI, adds Localize String Event with a generated key (SceneName_GameObjectPath), and skips countdown/summary TMPs.",
                MessageType.Info);
            if (GUILayout.Button("Apply localization to opened scene", GUILayout.Width(260)))
                ApplyLocalizationToOpenedScene();
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Removes Localize String Event from TMPs that are driven by PerfectParryComboDisplay (combo count). Use this if that TMP incorrectly shows \"No translation found\".",
                MessageType.Info);
            if (GUILayout.Button("Remove localization from PerfectParryComboDisplay TMPs", GUILayout.Width(320)))
                RemoveLocalizeStringEventFromPerfectParryComboDisplayTmp();

            EditorGUILayout.EndScrollView();
        }

        private void RefreshCollectionAndLocaleOptions()
        {
            var collections = LocalizationEditorSettings.GetStringTableCollections();
            if (collections == null || collections.Count == 0)
            {
                _collectionOptions = Array.Empty<GUIContent>();
                _localeOptions = Array.Empty<GUIContent>();
                return;
            }
            if (_collectionOptions.Length != collections.Count)
            {
                _collectionOptions = collections.Select(c => new GUIContent(c.TableCollectionName)).ToArray();
                if (_selectedCollectionIndex >= _collectionOptions.Length)
                    _selectedCollectionIndex = 0;
            }
            if (_collectionOptions.Length == 0)
                return;
            var selectedCollection = collections[_selectedCollectionIndex];
            var tables = selectedCollection.StringTables;
            if (tables == null || tables.Count == 0)
            {
                _localeOptions = Array.Empty<GUIContent>();
                return;
            }
            _localeOptions = tables.Select(t => new GUIContent(t.LocaleIdentifier.CultureInfo?.Name ?? t.LocaleIdentifier.Code)).ToArray();
            if (_sourceLocaleIndex >= _localeOptions.Length)
                _sourceLocaleIndex = 0;
        }

        private void WriteReportDocument(string assetPath)
        {
            LocalizationReportData data = BuildReportData();
            if (data.sceneEntries.Count == 0 && data.scriptEntries.Count == 0)
            {
                Debug.LogWarning("LocalizationAnalyzer: No entries to write. Run Analyze scenes and/or Analyze Scripts first.");
                return;
            }
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fullPath = assetPath;
            if (!Path.IsPathRooted(fullPath))
                fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            fullPath = Path.GetFullPath(fullPath);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, json, Encoding.UTF8);
            AssetDatabase.Refresh();
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
                EditorGUIUtility.PingObject(asset);
            Debug.Log($"LocalizationAnalyzer: Wrote report to {assetPath} ({data.sceneEntries.Count} scene, {data.scriptEntries.Count} script entries).");
        }

        private void FillTableFromReport()
        {
            string path = _reportPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("LocalizationAnalyzer: Report path is empty.");
                return;
            }
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Application.dataPath, "..", path);
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"LocalizationAnalyzer: Report file not found: {path}");
                return;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            LocalizationReportData data;
            try
            {
                data = JsonUtility.FromJson<LocalizationReportData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"LocalizationAnalyzer: Failed to parse report JSON: {e.Message}");
                return;
            }
            if (data == null)
            {
                Debug.LogError("LocalizationAnalyzer: Report data is null.");
                return;
            }
            var collections = LocalizationEditorSettings.GetStringTableCollections();
            if (collections == null || _selectedCollectionIndex < 0 || _selectedCollectionIndex >= collections.Count)
            {
                Debug.LogWarning("LocalizationAnalyzer: No valid String Table Collection selected.");
                return;
            }
            var collection = collections[_selectedCollectionIndex];
            var tables = collection.StringTables;
            if (tables == null || tables.Count == 0)
            {
                Debug.LogWarning("LocalizationAnalyzer: Selected collection has no locale tables.");
                return;
            }
            int sourceIndex = _sourceLocaleIndex >= 0 && _sourceLocaleIndex < tables.Count ? _sourceLocaleIndex : 0;
            var sourceTable = tables[sourceIndex];
            var sharedData = collection.SharedData;
            if (sharedData == null)
            {
                Debug.LogError("LocalizationAnalyzer: Collection SharedData is null.");
                return;
            }
            int added = 0;
            var usedKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in data.sceneEntries ?? new List<SceneEntryReport>())
            {
                string key = string.IsNullOrEmpty(e.suggestedKey) ? GetSuggestedKeyForScene(e.scenePath, e.gameObjectPath, usedKeys) : e.suggestedKey;
                long keyId = GetOrCreateKeyId(sharedData, key);
                sourceTable.AddEntry(keyId, e.currentText ?? "");
                added++;
            }
            foreach (var e in data.scriptEntries ?? new List<ScriptEntryReport>())
            {
                string key = string.IsNullOrEmpty(e.suggestedKey) ? GetSuggestedKeyForScript(e.file, e.line, e.kind, e.snippet, usedKeys) : e.suggestedKey;
                long keyId = GetOrCreateKeyId(sharedData, key);
                sourceTable.AddEntry(keyId, e.snippet ?? "");
                added++;
            }
            EditorUtility.SetDirty(sharedData);
            foreach (var t in tables)
                EditorUtility.SetDirty(t);
            AssetDatabase.SaveAssets();
            Debug.Log($"LocalizationAnalyzer: Filled table with {added} entries (source locale: {sourceTable.LocaleIdentifier.Code}).");
        }

        private const string TableNameForScenes = "BladeParry_LocalizationTable";
        private static readonly string[] SkipKeysForSceneTmp = new[]
        {
            "FightingScene_Canvas_Container_Mask_CountDown",
            "FightingScene_Canvas_Container_EndGame_SummaryBackground_SummartText"
        };

        private void ApplySceneLocalizationFromReport()
        {
            string path = _reportPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("LocalizationAnalyzer: Report path is empty.");
                return;
            }
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Application.dataPath, "..", path);
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"LocalizationAnalyzer: Report file not found: {path}");
                return;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            LocalizationReportData data;
            try
            {
                data = JsonUtility.FromJson<LocalizationReportData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"LocalizationAnalyzer: Failed to parse report: {e.Message}");
                return;
            }
            if (data?.sceneEntries == null || data.sceneEntries.Count == 0)
            {
                Debug.LogWarning("LocalizationAnalyzer: No scene entries in report.");
                return;
            }
            var skipSet = new HashSet<string>(SkipKeysForSceneTmp, StringComparer.Ordinal);
            var byScene = new Dictionary<string, List<SceneEntryReport>>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in data.sceneEntries)
            {
                if (string.IsNullOrEmpty(e.scenePath) || string.IsNullOrEmpty(e.suggestedKey) || skipSet.Contains(e.suggestedKey))
                    continue;
                string scenePath = e.scenePath.Replace("\\", "/");
                if (!scenePath.StartsWith("Assets/"))
                    scenePath = "Assets/" + scenePath;
                if (!byScene.TryGetValue(scenePath, out var list))
                {
                    list = new List<SceneEntryReport>();
                    byScene[scenePath] = list;
                }
                list.Add(e);
            }
            string currentScenePath = SceneManager.GetActiveScene().path;
            bool hadOpenScene = !string.IsNullOrEmpty(currentScenePath);
            int applied = 0;
            foreach (var kv in byScene)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(kv.Key, UnityEditor.SceneManagement.OpenSceneMode.Single);
                UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
                var skipTmpInScene = GetTmpDrivenByPerfectParryComboDisplay(scene);
                foreach (var e in kv.Value)
                {
                    Transform t = FindTransformByPath(scene, e.gameObjectPath);
                    if (t == null)
                    {
                        Debug.LogWarning($"LocalizationAnalyzer: Could not find GameObject at path '{e.gameObjectPath}' in {kv.Key}");
                        continue;
                    }
                    var tmp = t.GetComponent<TMPro.TMP_Text>();
                    if (tmp == null)
                        tmp = t.GetComponentInChildren<TMPro.TMP_Text>(true);
                    if (tmp == null)
                    {
                        Debug.LogWarning($"LocalizationAnalyzer: No TMP_Text on '{e.gameObjectPath}' in {kv.Key}");
                        continue;
                    }
                    if (skipTmpInScene.Contains(tmp))
                        continue;
                    var evt = t.GetComponent<LocalizeStringEvent>();
                    if (evt == null)
                        evt = t.gameObject.AddComponent<LocalizeStringEvent>();
                    evt.StringReference.SetReference(TableNameForScenes, e.suggestedKey);
                    evt.OnUpdateString.RemoveAllListeners();
                    evt.OnUpdateString.AddListener(s => { if (tmp != null) tmp.text = s ?? ""; });
                    evt.RefreshString();
                    applied++;
                }
                EditorSceneManager.SaveOpenScenes();
            }
            if (hadOpenScene && !string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            Debug.Log($"LocalizationAnalyzer: Applied Localize String Event to {applied} TMP(s) across {byScene.Count} scene(s).");
        }

        private void ApplyLocalizationToOpenedScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("LocalizationAnalyzer: No saved scene is open. Save and open a scene first.");
                return;
            }
            var allTmp = UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var skipSet = new HashSet<string>(SkipKeysForSceneTmp, StringComparer.Ordinal);
            var skipTmpDrivenByScript = GetTmpDrivenByPerfectParryComboDisplay(scene);
            var usedKeys = new HashSet<string>(StringComparer.Ordinal);
            int applied = 0;
            foreach (var tmp in allTmp)
            {
                if (tmp == null || tmp.gameObject.scene != scene)
                    continue;
                if (skipTmpDrivenByScript.Contains(tmp))
                    continue;
                string gameObjectPath = GetHierarchyPath(tmp.transform);
                string suggestedKey = GetSuggestedKeyForScene(scene.path, gameObjectPath, usedKeys);
                if (skipSet.Contains(suggestedKey))
                    continue;
                Transform t = tmp.transform;
                var evt = t.GetComponent<LocalizeStringEvent>();
                if (evt == null)
                    evt = t.gameObject.AddComponent<LocalizeStringEvent>();
                evt.StringReference.SetReference(TableNameForScenes, suggestedKey);
                evt.OnUpdateString.RemoveAllListeners();
                evt.OnUpdateString.AddListener(s => { if (tmp != null) tmp.text = s ?? ""; });
                evt.RefreshString();
                applied++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"LocalizationAnalyzer: Applied Localize String Event to {applied} TMP(s) in the opened scene '{scene.path}'.");
        }

        private static HashSet<TMPro.TMP_Text> GetTmpDrivenByPerfectParryComboDisplay(UnityEngine.SceneManagement.Scene scene)
        {
            var set = new HashSet<TMPro.TMP_Text>();
            var comboDisplays = UnityEngine.Object.FindObjectsByType<PerfectParryComboDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var comp in comboDisplays)
            {
                if (comp == null || comp.gameObject.scene != scene)
                    continue;
                var so = new SerializedObject(comp);
                var textProp = so.FindProperty("text");
                if (textProp != null && textProp.objectReferenceValue is TMPro.TMP_Text tmp)
                    set.Add(tmp);
            }
            return set;
        }

        private void RemoveLocalizeStringEventFromPerfectParryComboDisplayTmp()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("LocalizationAnalyzer: No saved scene is open.");
                return;
            }
            Type gameObjectLocalizerType = Type.GetType("UnityEngine.Localization.PropertyVariants.GameObjectLocalizer, Unity.Localization");
            var tmpSet = GetTmpDrivenByPerfectParryComboDisplay(scene);
            int removedEvt = 0;
            int removedLocalizer = 0;
            foreach (var tmp in tmpSet)
            {
                if (tmp == null)
                    continue;
                var evt = tmp.GetComponent<LocalizeStringEvent>();
                if (evt != null)
                {
                    UnityEngine.Object.DestroyImmediate(evt);
                    removedEvt++;
                }
                if (gameObjectLocalizerType != null)
                {
                    var localizer = tmp.GetComponent(gameObjectLocalizerType);
                    if (localizer != null)
                    {
                        UnityEngine.Object.DestroyImmediate(localizer);
                        removedLocalizer++;
                    }
                }
            }
            var comboDisplays = UnityEngine.Object.FindObjectsByType<PerfectParryComboDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var comp in comboDisplays)
            {
                if (comp == null || comp.gameObject.scene != scene)
                    continue;
                if (gameObjectLocalizerType != null)
                {
                    var localizer = comp.GetComponent(gameObjectLocalizerType);
                    if (localizer != null)
                    {
                        UnityEngine.Object.DestroyImmediate(localizer);
                        removedLocalizer++;
                    }
                }
            }
            if (removedEvt > 0 || removedLocalizer > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"LocalizationAnalyzer: Removed {removedEvt} Localize String Event(s) and {removedLocalizer} GameObjectLocalizer(s) from PerfectParryComboDisplay-related objects.");
        }

        private static Transform FindTransformByPath(UnityEngine.SceneManagement.Scene scene, string gameObjectPath)
        {
            if (string.IsNullOrEmpty(gameObjectPath))
                return null;
            string[] parts = gameObjectPath.Split('/');
            if (parts.Length == 0)
                return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0])
                    continue;
                if (parts.Length == 1)
                    return root.transform;
                string rest = string.Join("/", parts, 1, parts.Length - 1);
                Transform child = root.transform.Find(rest);
                if (child != null)
                    return child;
            }
            return null;
        }

        private static long GetOrCreateKeyId(SharedTableData sharedData, string key)
        {
            if (sharedData.Entries != null)
            {
                foreach (var e in sharedData.Entries)
                {
                    if (e.Key == key)
                        return e.Id;
                }
            }
            var newEntry = sharedData.AddKey(key);
            return newEntry != null ? newEntry.Id : 0;
        }

        #endregion

        #region Report model and key generation

        [Serializable]
        public class LocalizationReportData
        {
            public List<SceneEntryReport> sceneEntries = new List<SceneEntryReport>();
            public List<ScriptEntryReport> scriptEntries = new List<ScriptEntryReport>();
        }

        [Serializable]
        public class SceneEntryReport
        {
            public string scenePath;
            public string gameObjectPath;
            public string currentText;
            public string suggestedKey;
        }

        [Serializable]
        public class ScriptEntryReport
        {
            public string file;
            public int line;
            public string kind;
            public string snippet;
            public string suggestedKey;
        }

        private static string SanitizeKey(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "Key";
            var sb = new StringBuilder();
            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else if (c == '/' || c == ' ' || c == '.' || c == '-')
                    sb.Append('_');
            }
            if (sb.Length == 0)
                return "Key";
            return sb.ToString();
        }

        private static string GetSuggestedKeyForScene(string scenePath, string gameObjectPath, HashSet<string> usedKeys)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            string pathPart = (gameObjectPath ?? "").Replace("/", "_");
            string baseKey = SanitizeKey(sceneName + "_" + pathPart);
            if (string.IsNullOrEmpty(baseKey))
                baseKey = "Scene_Text";
            return EnsureUniqueKey(baseKey, usedKeys);
        }

        private static string GetSuggestedKeyForScript(string file, int line, string kind, string snippet, HashSet<string> usedKeys)
        {
            string fileBase = Path.GetFileNameWithoutExtension(file ?? "Script");
            string human = GetHumanReadableKeyFromSnippet(snippet);
            string baseKey = !string.IsNullOrEmpty(human) ? SanitizeKey(human) : SanitizeKey(fileBase + "_" + line);
            if (string.IsNullOrEmpty(baseKey))
                baseKey = "Script_" + line;
            return EnsureUniqueKey(baseKey, usedKeys);
        }

        private static string GetHumanReadableKeyFromSnippet(string snippet)
        {
            if (string.IsNullOrEmpty(snippet))
                return null;
            string t = snippet.Trim();
            if (t.StartsWith("Fight!", StringComparison.OrdinalIgnoreCase))
                return "Gameplay_Fight";
            if (t.StartsWith("PARRY!", StringComparison.OrdinalIgnoreCase))
                return "UI_Parry";
            if (t.StartsWith("Perfect", StringComparison.OrdinalIgnoreCase))
                return "UI_PerfectParry";
            if (t.StartsWith("MISS", StringComparison.OrdinalIgnoreCase))
                return "UI_Miss";
            if (t.StartsWith("COMBO!", StringComparison.OrdinalIgnoreCase))
                return "UI_Combo";
            if (t.Contains("Duree du combat") || t.Contains("Parades parfaites"))
                return "UI_FightSummary";
            if (t.Contains("{0}") && t.Contains("{1}"))
                return "UI_LifebarFormat";
            return null;
        }

        private static string EnsureUniqueKey(string baseKey, HashSet<string> usedKeys)
        {
            string key = baseKey;
            int n = 1;
            while (usedKeys.Contains(key))
                key = baseKey + "_" + (n++);
            usedKeys.Add(key);
            return key;
        }

        private LocalizationReportData BuildReportData()
        {
            var usedKeys = new HashSet<string>(StringComparer.Ordinal);
            var data = new LocalizationReportData();

            foreach (TmpEntry e in _sceneEntries)
            {
                data.sceneEntries.Add(new SceneEntryReport
                {
                    scenePath = e.ScenePath,
                    gameObjectPath = e.GameObjectPath,
                    currentText = e.CurrentText ?? "",
                    suggestedKey = GetSuggestedKeyForScene(e.ScenePath, e.GameObjectPath, usedKeys)
                });
            }

            foreach (ScriptStringEntry e in _scriptEntries)
            {
                data.scriptEntries.Add(new ScriptEntryReport
                {
                    file = e.File,
                    line = e.Line,
                    kind = e.Kind,
                    snippet = e.Snippet ?? "",
                    suggestedKey = GetSuggestedKeyForScript(e.File, e.Line, e.Kind, e.Snippet, usedKeys)
                });
            }

            return data;
        }

        #endregion
    }
}
