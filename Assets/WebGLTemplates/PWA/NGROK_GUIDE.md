# 🚀 Using Ngrok with Unity WebGL PWA

## The Problem
Ngrok doesn't handle Unity's `.gz` compressed files correctly by default, causing MIME type errors.

## ✅ The Solution

### Step 1: Use the Unity Server (Required!)
Don't use Python's SimpleHTTPServer. Use our custom Unity server instead:

```bash
cd Builds/WebGLPWA
python3 unity-server.py
```

This server properly sets:
- `Content-Encoding: gzip` for compressed files
- Correct MIME types (`application/wasm`, `application/javascript`, etc.)
- CORS headers for WebAssembly

### Step 2: Start Ngrok
In a new terminal:

```bash
ngrok http 8000
```

### Step 3: Access Your Game
Use the HTTPS URL provided by ngrok (e.g., `https://abc123.ngrok-free.app`)

---

## 🎮 Alternative Solutions

### Option 1: Use Localhost for Testing
Skip ngrok entirely for local testing:
```bash
python3 unity-server.py
# Open http://localhost:8000
```

### Option 2: Deploy to Free Hosting
For sharing with others, deploy to:

**Netlify (Easiest):**
1. Drag & drop your build folder to [netlify.com](https://netlify.com)
2. Headers are auto-configured via `netlify.toml`

**Vercel:**
```bash
npm i -g vercel
cd Builds/WebGLPWA
vercel
```

**Surge.sh:**
```bash
npm i -g surge
cd Builds/WebGLPWA
surge
```

### Option 3: Use LocalTunnel Instead of Ngrok
LocalTunnel handles headers better:
```bash
npm install -g localtunnel
python3 unity-server.py
lt --port 8000
```

### Option 4: Build Without Compression
In Unity:
1. **File → Build Settings → Player Settings**
2. **Publishing Settings → Compression Format**
3. Set to **Disabled**
4. Rebuild

This creates larger files but avoids compression issues entirely.

---

## 🔍 Troubleshooting

### Still Getting MIME Errors?
1. Make sure you're using `unity-server.py`, NOT `python -m http.server`
2. Clear browser cache (Ctrl+Shift+R or Cmd+Shift+R)
3. Check browser console for specific error messages

### Test Headers
```bash
curl -I https://your-ngrok-url.ngrok-free.app/Build/WebGLPWA.wasm.gz
```

Should show:
```
Content-Type: application/wasm
Content-Encoding: gzip
```

### Mobile Testing
For mobile testing without ngrok:
1. Connect device to same WiFi
2. Find your local IP: `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
3. Access: `http://YOUR_LOCAL_IP:8000`

---

## 📝 Quick Commands Reference

```bash
# Start Unity server
python3 unity-server.py

# Start on different port
python3 unity-server.py 8080

# Start ngrok
ngrok http 8000

# Start localtunnel (alternative)
lt --port 8000 --subdomain myslotgame

# Deploy to Netlify
# Just drag & drop folder to netlify.com

# Deploy to Vercel
vercel --prod

# Deploy to Surge
surge --domain myslotgame.surge.sh
```

---

## ⚡ Best Practice
For development: Use `unity-server.py` + localhost
For sharing: Deploy to Netlify/Vercel (free & reliable)
For demos: Use localtunnel instead of ngrok