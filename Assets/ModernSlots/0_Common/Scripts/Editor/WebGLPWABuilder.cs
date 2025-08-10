using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

namespace Mkey
{
    public class WebGLPWABuilder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        [MenuItem("Build/Build WebGL PWA")]
        public static void BuildWebGLPWA()
        {
            // Configure WebGL build settings for PWA
            // Disable compression for local development with http-server
            // For production, change to WebGLCompressionFormat.Gzip or WebGLCompressionFormat.Brotli
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.memorySize = 512; // Set appropriate memory size
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            PlayerSettings.WebGL.template = "PROJECT:PWA"; // Use our PWA template
            
            // Configure player settings
            PlayerSettings.defaultWebScreenWidth = 1920;
            PlayerSettings.defaultWebScreenHeight = 1080;
            PlayerSettings.runInBackground = true;
            
            // Configure quality settings for web
            // QualitySettings.SetQualityLevel(2, true); // Medium quality for better performance
            
            // Set up build scenes
            List<string> scenePaths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenePaths.Add(scene.path);
                }
            }
            
            if (scenePaths.Count == 0)
            {
                // Add default scenes if none are configured
                scenePaths.Add("Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Scenes_ModernS1/1_Lobby.unity");
                scenePaths.Add("Assets/ModernSlots/1_ModernSuitsSlot_skin#1/Scenes_ModernS1/2_Slot_3X5.unity");
            }
            
            // Build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenePaths.ToArray(),
                locationPathName = "Builds/WebGLPWA",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            
            // Perform the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"WebGL PWA build succeeded: {summary.totalSize / 1048576} MB");
                
                // Post-process the build
                PostProcessPWABuild("Builds/WebGLPWA");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("WebGL PWA build failed");
            }
        }
        
        private static void PostProcessPWABuild(string buildPath)
        {
            // Create icons directory
            string iconsPath = Path.Combine(buildPath, "icons");
            if (!Directory.Exists(iconsPath))
            {
                Directory.CreateDirectory(iconsPath);
                Debug.Log($"Created icons directory at: {iconsPath}");
            }
            
            // Create screenshots directory
            string screenshotsPath = Path.Combine(buildPath, "screenshots");
            if (!Directory.Exists(screenshotsPath))
            {
                Directory.CreateDirectory(screenshotsPath);
                Debug.Log($"Created screenshots directory at: {screenshotsPath}");
            }
            
            // Copy service worker and manifest to build directory
            string templatePath = Application.dataPath + "/WebGLTemplates/PWA";
            
            if (File.Exists(Path.Combine(templatePath, "service-worker.js")))
            {
                File.Copy(
                    Path.Combine(templatePath, "service-worker.js"),
                    Path.Combine(buildPath, "service-worker.js"),
                    true
                );
            }
            
            if (File.Exists(Path.Combine(templatePath, "manifest.json")))
            {
                File.Copy(
                    Path.Combine(templatePath, "manifest.json"),
                    Path.Combine(buildPath, "manifest.json"),
                    true
                );
            }
            
            // Copy .htaccess for Apache servers
            if (File.Exists(Path.Combine(templatePath, ".htaccess")))
            {
                File.Copy(
                    Path.Combine(templatePath, ".htaccess"),
                    Path.Combine(buildPath, ".htaccess"),
                    true
                );
            }
            
            // Create server configuration files for different platforms
            CreateServerConfigs(buildPath);
            
            // Create a simple deployment info file
            string deployInfo = @"=== Unity WebGL PWA Deployment Guide ===

1. ICON GENERATION:
   - Add your game icons to the 'icons' folder
   - Required sizes: 72x72, 96x96, 128x128, 144x144, 152x152, 192x192, 384x384, 512x512
   - Format: PNG with transparency
   - You can use online tools like https://realfavicongenerator.net/

2. SCREENSHOTS:
   - Add game screenshots to the 'screenshots' folder
   - Recommended: 1920x1080 PNG files
   - Name them: game1.png, game2.png, etc.

3. HTTPS DEPLOYMENT:
   PWAs require HTTPS. Deploy to one of these services:
   - Netlify (https://netlify.com) - Drag & drop deployment
   - Vercel (https://vercel.com) - Simple deployment
   - GitHub Pages (https://pages.github.com)
   - Firebase Hosting (https://firebase.google.com/products/hosting)

4. DEPLOYMENT STEPS:
   a. Generate and add icons to the icons/ folder
   b. Add screenshots to the screenshots/ folder
   c. Test locally with: python3 -m http.server 8000
   d. Deploy to HTTPS hosting service
   e. Test PWA installation on mobile devices

5. TESTING PWA FEATURES:
   - Open Chrome DevTools > Application tab
   - Check 'Manifest' section for manifest validation
   - Check 'Service Workers' for SW registration
   - Use Lighthouse audit for PWA score

6. COMPRESSION (OPTIONAL):
   For better performance, consider using Brotli compression:
   - Most modern hosting services handle this automatically
   - Or use Unity's built-in Brotli compression in build settings

7. UPDATING YOUR PWA:
   - Update version in manifest.json
   - Update CACHE_NAME in service-worker.js
   - Deploy changes
   - Users will see update prompt automatically

For more info: https://web.dev/progressive-web-apps/
";
            
            File.WriteAllText(Path.Combine(buildPath, "DEPLOYMENT_GUIDE.txt"), deployInfo);
            
            Debug.Log($"PWA build post-processing complete!");
            Debug.Log($"Build location: {Path.GetFullPath(buildPath)}");
            Debug.Log($"Next steps: Read DEPLOYMENT_GUIDE.txt in the build folder");
            
            // Open build folder
            EditorUtility.RevealInFinder(buildPath);
        }
        
        private static void CreateServerConfigs(string buildPath)
        {
            string templatePath = Application.dataPath + "/WebGLTemplates/PWA";
            
            // Create netlify.toml
            string netlifyConfig = @"[[headers]]
  for = ""*.data.gz""
  [headers.values]
    Content-Type = ""application/octet-stream""
    Content-Encoding = ""gzip""

[[headers]]
  for = ""*.wasm.gz""
  [headers.values]
    Content-Type = ""application/wasm""
    Content-Encoding = ""gzip""

[[headers]]
  for = ""*.js.gz""
  [headers.values]
    Content-Type = ""application/javascript""
    Content-Encoding = ""gzip""

[[headers]]
  for = ""*.symbols.json.gz""
  [headers.values]
    Content-Type = ""application/json""
    Content-Encoding = ""gzip""";
            
            File.WriteAllText(Path.Combine(buildPath, "netlify.toml"), netlifyConfig);
            
            // Create vercel.json
            string vercelConfig = @"{
  ""headers"": [
    {
      ""source"": ""**/*.data.gz"",
      ""headers"": [
        { ""key"": ""Content-Type"", ""value"": ""application/octet-stream"" },
        { ""key"": ""Content-Encoding"", ""value"": ""gzip"" }
      ]
    },
    {
      ""source"": ""**/*.wasm.gz"",
      ""headers"": [
        { ""key"": ""Content-Type"", ""value"": ""application/wasm"" },
        { ""key"": ""Content-Encoding"", ""value"": ""gzip"" }
      ]
    },
    {
      ""source"": ""**/*.js.gz"",
      ""headers"": [
        { ""key"": ""Content-Type"", ""value"": ""application/javascript"" },
        { ""key"": ""Content-Encoding"", ""value"": ""gzip"" }
      ]
    }
  ]
}";
            
            File.WriteAllText(Path.Combine(buildPath, "vercel.json"), vercelConfig);
            
            // Copy the Unity server script
            string unityServerPath = Path.Combine(templatePath, "unity-server.py");
            if (File.Exists(unityServerPath))
            {
                File.Copy(unityServerPath, Path.Combine(buildPath, "unity-server.py"), true);
            }
            else
            {
                // Create a fallback Python server if template doesn't exist
                string pythonServer = @"#!/usr/bin/env python3
import http.server
import socketserver
import os

class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_my_headers()
        super().end_headers()

    def send_my_headers(self):
        if self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
        if self.path.endswith('.wasm') or self.path.endswith('.wasm.gz'):
            self.send_header('Content-Type', 'application/wasm')
        elif self.path.endswith('.js.gz'):
            self.send_header('Content-Type', 'application/javascript')
        elif self.path.endswith('.data.gz'):
            self.send_header('Content-Type', 'application/octet-stream')

PORT = 8000
print(f'Server running at http://localhost:{PORT}/')
print('Press Ctrl+C to stop')

with socketserver.TCPServer(('', PORT), MyHTTPRequestHandler) as httpd:
    httpd.serve_forever()";
                
                File.WriteAllText(Path.Combine(buildPath, "server.py"), pythonServer);
            }
            
            Debug.Log("Created server configuration files for Netlify, Vercel, and local Python testing");
        }
        
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                Debug.Log("Preprocessing WebGL PWA build...");
                
                // Ensure PWA template exists
                string templatePath = Application.dataPath + "/WebGLTemplates/PWA";
                if (!Directory.Exists(templatePath))
                {
                    Debug.LogError("PWA template not found! Please ensure PWA template exists at Assets/WebGLTemplates/PWA/");
                }
            }
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                Debug.Log("Postprocessing WebGL PWA build...");
                
                // Additional post-processing can be added here
                if (report.summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"Build completed successfully!");
                    Debug.Log($"Total size: {report.summary.totalSize / 1048576} MB");
                    Debug.Log($"Build time: {report.summary.totalTime.TotalSeconds} seconds");
                }
            }
        }
    }
    
    [InitializeOnLoad]
    public static class WebGLPWASettings
    {
        static WebGLPWASettings()
        {
            // Set recommended WebGL settings on editor load
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                // Disabled compression for local development with http-server
                // For production, change to WebGLCompressionFormat.Gzip or WebGLCompressionFormat.Brotli
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            }
        }
    }
}