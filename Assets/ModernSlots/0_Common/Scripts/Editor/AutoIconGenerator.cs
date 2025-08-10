using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Mkey
{
    public class AutoIconGenerator : EditorWindow
    {
        private static readonly string[] ICON_SEARCH_PATHS = {
            "Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Sprites_ModernS1/LobbyScreen/PNG/GameIcons/Icon Modern Suits.png",
            "Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Sprites_ModernS1/LobbyScreen/PNG/Modern Icon.png",
            "Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Sprites_ModernS1/LobbyScreen/PNG/GameIcons/Icon Game.png",
            "Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Sprites_ModernS1/GUI/PNG Icons/Icon Diamond.png",
            "Assets/ModernSlots/2_ModernNeonSlot_skin#2/Sprites_ModernS2/LobbyScreen/PNG/GameIcons/Icon Modern Neon.png"
        };
        
        [MenuItem("Build/Auto Generate PWA Icons")]
        public static void GenerateIconsAutomatically()
        {
            Texture2D sourceIcon = FindBestIcon();
            
            if (sourceIcon == null)
            {
                EditorUtility.DisplayDialog(
                    "No Icon Found",
                    "Could not find a suitable icon in the project.\nPlease use the PWA Icon Generator to select an icon manually.",
                    "OK"
                );
                PWAIconGenerator.ShowWindow();
                return;
            }
            
            string outputPath = Application.dataPath + "/WebGLTemplates/PWA/icons";
            
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                Debug.Log($"Created icons directory at: {outputPath}");
            }
            
            GenerateAllIcons(sourceIcon, outputPath);
            
            EditorUtility.DisplayDialog(
                "Icons Generated!",
                $"PWA icons have been generated from:\n{AssetDatabase.GetAssetPath(sourceIcon)}\n\nLocation: {outputPath}",
                "Great!"
            );
        }
        
        private static Texture2D FindBestIcon()
        {
            // Try to find the best available icon
            foreach (string path in ICON_SEARCH_PATHS)
            {
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (icon != null)
                {
                    Debug.Log($"Found icon at: {path}");
                    return icon;
                }
            }
            
            // If specific icons not found, search for any suitable icon
            string[] guids = AssetDatabase.FindAssets("t:texture2D Icon", new[] { "Assets/ModernSlots" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                
                if (icon != null && icon.width >= 256 && icon.height >= 256)
                {
                    // Check if it contains slot-related keywords
                    string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
                    if (fileName.Contains("modern") || fileName.Contains("slot") || 
                        fileName.Contains("game") || fileName.Contains("icon"))
                    {
                        Debug.Log($"Using icon: {path}");
                        return icon;
                    }
                }
            }
            
            return null;
        }
        
        private static void GenerateAllIcons(Texture2D sourceIcon, string outputPath)
        {
            // Make the source texture readable
            string assetPath = AssetDatabase.GetAssetPath(sourceIcon);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            bool wasReadable = false;
            if (importer != null && !importer.isReadable)
            {
                wasReadable = importer.isReadable;
                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
            
            // Icon sizes needed for PWA
            int[] sizes = { 16, 32, 72, 96, 128, 144, 152, 192, 384, 512 };
            
            try
            {
                foreach (int size in sizes)
                {
                    Texture2D resizedIcon = ResizeTexture(sourceIcon, size, size);
                    
                    // Add background color for better visibility
                    Texture2D iconWithBg = AddBackground(resizedIcon, new Color(0.1f, 0.13f, 0.24f, 1f)); // Dark blue background
                    
                    byte[] pngData = iconWithBg.EncodeToPNG();
                    
                    string fileName = $"icon-{size}x{size}.png";
                    string filePath = Path.Combine(outputPath, fileName);
                    
                    File.WriteAllBytes(filePath, pngData);
                    
                    // Clean up temporary textures
                    DestroyImmediate(resizedIcon);
                    DestroyImmediate(iconWithBg);
                    
                    Debug.Log($"Generated icon: {fileName}");
                }
                
                // Also create special icons
                GenerateSpecialIcon(sourceIcon, outputPath, "apple-touch-icon.png", 180);
                GenerateSpecialIcon(sourceIcon, outputPath, "favicon.png", 32);
                
                // Create themed icons for shortcuts
                GenerateThemedIcon(sourceIcon, outputPath, "suits-96x96.png", 96, new Color(0.4f, 0.2f, 0.5f, 1f));
                GenerateThemedIcon(sourceIcon, outputPath, "neon-96x96.png", 96, new Color(0.2f, 0.5f, 0.9f, 1f));
                
                AssetDatabase.Refresh();
                
                Debug.Log($"Successfully generated all PWA icons at: {outputPath}");
            }
            finally
            {
                // Restore original import settings
                if (importer != null && !wasReadable)
                {
                    importer.isReadable = wasReadable;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
        
        private static void GenerateSpecialIcon(Texture2D source, string outputPath, string fileName, int size)
        {
            Texture2D resizedIcon = ResizeTexture(source, size, size);
            Texture2D iconWithBg = AddBackground(resizedIcon, new Color(0.1f, 0.13f, 0.24f, 1f));
            byte[] pngData = iconWithBg.EncodeToPNG();
            string filePath = Path.Combine(outputPath, fileName);
            File.WriteAllBytes(filePath, pngData);
            DestroyImmediate(resizedIcon);
            DestroyImmediate(iconWithBg);
            Debug.Log($"Generated special icon: {fileName}");
        }
        
        private static void GenerateThemedIcon(Texture2D source, string outputPath, string fileName, int size, Color bgColor)
        {
            Texture2D resizedIcon = ResizeTexture(source, size, size);
            Texture2D iconWithBg = AddBackground(resizedIcon, bgColor);
            byte[] pngData = iconWithBg.EncodeToPNG();
            string filePath = Path.Combine(outputPath, fileName);
            File.WriteAllBytes(filePath, pngData);
            DestroyImmediate(resizedIcon);
            DestroyImmediate(iconWithBg);
            Debug.Log($"Generated themed icon: {fileName}");
        }
        
        private static Texture2D AddBackground(Texture2D icon, Color backgroundColor)
        {
            Texture2D result = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, false);
            
            for (int y = 0; y < icon.height; y++)
            {
                for (int x = 0; x < icon.width; x++)
                {
                    Color pixelColor = icon.GetPixel(x, y);
                    
                    // Blend icon with background based on alpha
                    Color finalColor = Color.Lerp(backgroundColor, pixelColor, pixelColor.a);
                    finalColor.a = 1f; // Ensure full opacity
                    
                    result.SetPixel(x, y, finalColor);
                }
            }
            
            result.Apply();
            return result;
        }
        
        private static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 24);
            RenderTexture.active = rt;
            
            // Use bilinear filtering for better quality
            source.filterMode = FilterMode.Bilinear;
            
            Graphics.Blit(source, rt);
            
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return result;
        }
    }
}