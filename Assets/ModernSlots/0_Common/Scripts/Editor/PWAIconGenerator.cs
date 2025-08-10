using UnityEngine;
using UnityEditor;
using System.IO;

namespace Mkey
{
    public class PWAIconGenerator : EditorWindow
    {
        private Texture2D sourceIcon;
        private string outputPath = "Assets/WebGLTemplates/PWA/icons";
        
        [MenuItem("Build/PWA Icon Generator")]
        public static void ShowWindow()
        {
            GetWindow<PWAIconGenerator>("PWA Icon Generator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("PWA Icon Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "Select a source image (512x512 or larger recommended) to generate all PWA icon sizes.",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            sourceIcon = (Texture2D)EditorGUILayout.ObjectField(
                "Source Icon",
                sourceIcon,
                typeof(Texture2D),
                false
            );
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Output Path:", outputPath);
            
            if (GUILayout.Button("Select Output Folder"))
            {
                string path = EditorUtility.SaveFolderPanel(
                    "Select Icon Output Folder",
                    Application.dataPath,
                    ""
                );
                
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        outputPath = path;
                    }
                }
            }
            
            GUILayout.Space(20);
            
            GUI.enabled = sourceIcon != null;
            
            if (GUILayout.Button("Generate PWA Icons", GUILayout.Height(40)))
            {
                GenerateIcons();
            }
            
            GUI.enabled = true;
            
            GUILayout.Space(20);
            
            EditorGUILayout.HelpBox(
                "Required icon sizes for PWA:\n" +
                "• 16x16, 32x32 (Favicon)\n" +
                "• 72x72, 96x96, 128x128, 144x144, 152x152 (Mobile)\n" +
                "• 192x192, 384x384, 512x512 (PWA Manifest)",
                MessageType.None
            );
        }
        
        private void GenerateIcons()
        {
            if (sourceIcon == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a source icon first.", "OK");
                return;
            }
            
            // Icon sizes needed for PWA
            int[] sizes = { 16, 32, 72, 96, 128, 144, 152, 192, 384, 512 };
            
            // Ensure output directory exists
            string fullOutputPath = outputPath;
            if (!fullOutputPath.StartsWith(Application.dataPath))
            {
                fullOutputPath = Application.dataPath + "/" + outputPath.Replace("Assets/", "");
            }
            
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }
            
            // Make the source texture readable
            string assetPath = AssetDatabase.GetAssetPath(sourceIcon);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
            
            try
            {
                foreach (int size in sizes)
                {
                    Texture2D resizedIcon = ResizeTexture(sourceIcon, size, size);
                    byte[] pngData = resizedIcon.EncodeToPNG();
                    
                    string fileName = $"icon-{size}x{size}.png";
                    string filePath = Path.Combine(fullOutputPath, fileName);
                    
                    File.WriteAllBytes(filePath, pngData);
                    
                    // Clean up temporary texture
                    DestroyImmediate(resizedIcon);
                    
                    Debug.Log($"Generated icon: {fileName}");
                }
                
                // Also create special icons for iOS and shortcuts
                GenerateSpecialIcon(sourceIcon, fullOutputPath, "apple-touch-icon.png", 180);
                GenerateSpecialIcon(sourceIcon, fullOutputPath, "suits-96x96.png", 96);
                GenerateSpecialIcon(sourceIcon, fullOutputPath, "neon-96x96.png", 96);
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog(
                    "Success",
                    $"PWA icons generated successfully!\nLocation: {outputPath}",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Failed to generate icons: {e.Message}",
                    "OK"
                );
            }
        }
        
        private void GenerateSpecialIcon(Texture2D source, string outputPath, string fileName, int size)
        {
            Texture2D resizedIcon = ResizeTexture(source, size, size);
            byte[] pngData = resizedIcon.EncodeToPNG();
            string filePath = Path.Combine(outputPath, fileName);
            File.WriteAllBytes(filePath, pngData);
            DestroyImmediate(resizedIcon);
            Debug.Log($"Generated special icon: {fileName}");
        }
        
        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            // Create a temporary RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 24);
            RenderTexture.active = rt;
            
            // Copy source texture to the RenderTexture with scaling
            Graphics.Blit(source, rt);
            
            // Create output texture
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            
            // Read pixels from RenderTexture
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            
            // Clean up
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return result;
        }
    }
    
    public class PWAManifestEditor : EditorWindow
    {
        private string appName = "SlotTest";
        private string shortName = "SlotTest";
        private string description = "Modern slot machine game";
        private string themeColor = "#16213e";
        private string backgroundColor = "#1a1a2e";
        
        [MenuItem("Build/PWA Manifest Editor")]
        public static void ShowWindow()
        {
            GetWindow<PWAManifestEditor>("PWA Manifest Editor");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("PWA Manifest Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            appName = EditorGUILayout.TextField("App Name", appName);
            shortName = EditorGUILayout.TextField("Short Name", shortName);
            description = EditorGUILayout.TextField("Description", description);
            themeColor = EditorGUILayout.TextField("Theme Color", themeColor);
            backgroundColor = EditorGUILayout.TextField("Background Color", backgroundColor);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Update Manifest", GUILayout.Height(40)))
            {
                UpdateManifest();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Manifest File"))
            {
                string manifestPath = Application.dataPath + "/WebGLTemplates/PWA/manifest.json";
                if (File.Exists(manifestPath))
                {
                    EditorUtility.OpenWithDefaultApp(manifestPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Manifest file not found!", "OK");
                }
            }
        }
        
        private void UpdateManifest()
        {
            string manifestPath = Application.dataPath + "/WebGLTemplates/PWA/manifest.json";
            
            if (File.Exists(manifestPath))
            {
                string content = File.ReadAllText(manifestPath);
                
                // Simple string replacements (in production, use proper JSON parsing)
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    "\"name\":\\s*\"[^\"]*\"",
                    $"\"name\": \"{appName}\""
                );
                
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    "\"short_name\":\\s*\"[^\"]*\"",
                    $"\"short_name\": \"{shortName}\""
                );
                
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    "\"description\":\\s*\"[^\"]*\"",
                    $"\"description\": \"{description}\""
                );
                
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    "\"theme_color\":\\s*\"[^\"]*\"",
                    $"\"theme_color\": \"{themeColor}\""
                );
                
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    "\"background_color\":\\s*\"[^\"]*\"",
                    $"\"background_color\": \"{backgroundColor}\""
                );
                
                File.WriteAllText(manifestPath, content);
                
                EditorUtility.DisplayDialog("Success", "Manifest updated successfully!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Manifest file not found!", "OK");
            }
        }
    }
}