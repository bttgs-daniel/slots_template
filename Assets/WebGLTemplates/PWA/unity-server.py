#!/usr/bin/env python3
"""
Unity WebGL PWA Server with proper gzip handling
Serves Unity WebGL builds with correct headers for compressed files
"""

import http.server
import socketserver
import os
import mimetypes
from urllib.parse import urlparse
import gzip

class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        # Set headers before sending them
        self.send_unity_headers()
        super().end_headers()
    
    def send_unity_headers(self):
        path = self.path.lower()
        
        # CORS headers for WebAssembly
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        
        # Handle gzipped Unity files
        if path.endswith('.gz'):
            # Important: Set Content-Encoding for gzipped files
            self.send_header("Content-Encoding", "gzip")
            
            # Set the correct Content-Type based on the original file
            if '.wasm.gz' in path:
                self.send_header("Content-Type", "application/wasm")
            elif '.js.gz' in path:
                self.send_header("Content-Type", "application/javascript")
            elif '.data.gz' in path:
                self.send_header("Content-Type", "application/octet-stream")
            elif '.symbols.json.gz' in path:
                self.send_header("Content-Type", "application/json")
            else:
                # Default for other gzipped files
                self.send_header("Content-Type", "application/gzip")
        
        # Handle Brotli compressed files
        elif path.endswith('.br'):
            self.send_header("Content-Encoding", "br")
            
            if '.wasm.br' in path:
                self.send_header("Content-Type", "application/wasm")
            elif '.js.br' in path:
                self.send_header("Content-Type", "application/javascript")
            elif '.data.br' in path:
                self.send_header("Content-Type", "application/octet-stream")
        
        # Standard Unity WebGL files
        elif path.endswith('.wasm'):
            self.send_header("Content-Type", "application/wasm")
        elif path.endswith('.js'):
            self.send_header("Content-Type", "application/javascript")
        elif path.endswith('.json'):
            self.send_header("Content-Type", "application/json")
        
        # PWA specific files
        if 'service-worker.js' in path:
            self.send_header("Cache-Control", "no-cache, no-store, must-revalidate")
        elif 'manifest.json' in path:
            self.send_header("Content-Type", "application/manifest+json")
    
    def do_OPTIONS(self):
        """Handle preflight CORS requests"""
        self.send_response(200)
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.end_headers()
    
    def guess_type(self, path):
        """Override to handle Unity WebGL specific files"""
        mimetype = super().guess_type(path)
        
        # Fix for Unity WebGL files
        if path.endswith('.unityweb'):
            return ('application/octet-stream', None)
        elif path.endswith('.wasm'):
            return ('application/wasm', None)
        
        return mimetype

def run_server(port=8000, directory=None):
    """Run the Unity WebGL server"""
    
    if directory:
        os.chdir(directory)
    
    # Configure MIME types
    mimetypes.init()
    mimetypes.add_type('application/wasm', '.wasm')
    mimetypes.add_type('application/javascript', '.js')
    mimetypes.add_type('application/octet-stream', '.data')
    
    Handler = UnityWebGLHandler
    
    print(f"""
╔════════════════════════════════════════════════════════╗
║         Unity WebGL PWA Development Server             ║
╠════════════════════════════════════════════════════════╣
║                                                        ║
║  Server running at:                                    ║
║  → http://localhost:{port}/                              ║
║  → http://127.0.0.1:{port}/                              ║
║                                                        ║
║  Network access:                                       ║""")
    
    # Try to get local network IP
    import socket
    try:
        hostname = socket.gethostname()
        local_ip = socket.gethostbyname(hostname)
        if local_ip.startswith('127.'):
            # Try alternative method
            s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            s.connect(("8.8.8.8", 80))
            local_ip = s.getsockname()[0]
            s.close()
        print(f"║  → http://{local_ip}:{port}/                           ║")
    except:
        print(f"║  → (Could not determine local IP)                     ║")
    
    print(f"""║                                                        ║
║  Features:                                             ║
║  ✓ Proper gzip/brotli compression headers             ║
║  ✓ CORS enabled for WebAssembly                       ║
║  ✓ PWA manifest and service worker support            ║
║  ✓ Unity WebGL MIME types configured                  ║
║                                                        ║
║  Press Ctrl+C to stop the server                      ║
╚════════════════════════════════════════════════════════╝
    """)
    
    with socketserver.TCPServer(("", port), Handler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\n\nServer stopped.")
            return

if __name__ == "__main__":
    import sys
    
    # Check for port argument
    port = 8000
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            print(f"Invalid port: {sys.argv[1]}")
            print("Usage: python unity-server.py [port]")
            sys.exit(1)
    
    try:
        run_server(port)
    except OSError as e:
        if "Address already in use" in str(e):
            print(f"\n❌ Port {port} is already in use!")
            print(f"   Try a different port: python unity-server.py 8080")
        else:
            raise