# Server Configuration for Unity WebGL PWA

## The Gzip Error Solution

The error you're seeing happens because Unity builds compressed files (.gz) but the server doesn't tell the browser they're compressed. Here are solutions for different hosting platforms:

---

## 1. Local Testing (Python)

Create a file `server.py` in your build folder:

```python
#!/usr/bin/env python3
import http.server
import socketserver
import os

class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_my_headers()
        super().end_headers()

    def send_my_headers(self):
        # Add Content-Encoding for .gz files
        if self.path.endswith('.gz'):
            self.send_header("Content-Encoding", "gzip")
        # Add correct MIME types
        if self.path.endswith('.wasm') or self.path.endswith('.wasm.gz'):
            self.send_header("Content-Type", "application/wasm")
        elif self.path.endswith('.js.gz'):
            self.send_header("Content-Type", "application/javascript")
        elif self.path.endswith('.data.gz'):
            self.send_header("Content-Type", "application/octet-stream")

PORT = 8000
with socketserver.TCPServer(("", PORT), MyHTTPRequestHandler) as httpd:
    print(f"Server running at http://localhost:{PORT}/")
    httpd.serve_forever()
```

Run with: `python3 server.py`

---

## 2. Netlify

Create `netlify.toml` in your build folder:

```toml
[[headers]]
  for = "*.data.gz"
  [headers.values]
    Content-Type = "application/octet-stream"
    Content-Encoding = "gzip"

[[headers]]
  for = "*.wasm.gz"
  [headers.values]
    Content-Type = "application/wasm"
    Content-Encoding = "gzip"

[[headers]]
  for = "*.js.gz"
  [headers.values]
    Content-Type = "application/javascript"
    Content-Encoding = "gzip"

[[headers]]
  for = "*.symbols.json.gz"
  [headers.values]
    Content-Type = "application/json"
    Content-Encoding = "gzip"
```

---

## 3. Vercel

Create `vercel.json` in your build folder:

```json
{
  "headers": [
    {
      "source": "**/*.data.gz",
      "headers": [
        { "key": "Content-Type", "value": "application/octet-stream" },
        { "key": "Content-Encoding", "value": "gzip" }
      ]
    },
    {
      "source": "**/*.wasm.gz",
      "headers": [
        { "key": "Content-Type", "value": "application/wasm" },
        { "key": "Content-Encoding", "value": "gzip" }
      ]
    },
    {
      "source": "**/*.js.gz",
      "headers": [
        { "key": "Content-Type", "value": "application/javascript" },
        { "key": "Content-Encoding", "value": "gzip" }
      ]
    }
  ]
}
```

---

## 4. Firebase Hosting

Create `firebase.json` in your build folder:

```json
{
  "hosting": {
    "public": ".",
    "headers": [
      {
        "source": "**/*.@(js|wasm|data|symbols.json).gz",
        "headers": [
          { "key": "Content-Encoding", "value": "gzip" }
        ]
      }
    ]
  }
}
```

---

## 5. GitHub Pages

GitHub Pages doesn't support custom headers. Use this workaround:

1. Build with **Decompression Fallback** enabled in Unity
2. Or use the uncompressed build option

---

## 6. Nginx

Add to your nginx config:

```nginx
location ~ \.(data|wasm|js|symbols\.json)\.gz$ {
    add_header Content-Encoding gzip;
    
    # Set correct MIME types
    location ~ \.data\.gz$ {
        add_header Content-Type application/octet-stream;
    }
    location ~ \.wasm\.gz$ {
        add_header Content-Type application/wasm;
    }
    location ~ \.js\.gz$ {
        add_header Content-Type application/javascript;
    }
}
```

---

## 7. Apache (using .htaccess)

The `.htaccess` file is already included in the PWA template and will work automatically on Apache servers.

---

## Alternative: Build Without Compression

If server configuration is not possible, rebuild in Unity with:
1. Go to **File → Build Settings → Player Settings**
2. Under **Publishing Settings**
3. Set **Compression Format** to **Disabled**
4. Enable **Decompression Fallback**

This creates larger files but avoids the server configuration requirement.