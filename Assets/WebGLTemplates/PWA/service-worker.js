const CACHE_NAME = 'slottest-pwa-v1.0.0';
const urlsToCache = [
  './',
  './index.html',
  './Build/Build.loader.js',
  './Build/Build.framework.js',
  './Build/Build.wasm',
  './Build/Build.data',
  './TemplateData/style.css',
  './TemplateData/favicon.ico',
  './TemplateData/fullscreen-button.png',
  './TemplateData/progress-bar-empty-dark.png',
  './TemplateData/progress-bar-empty-light.png',
  './TemplateData/progress-bar-full-dark.png',
  './TemplateData/progress-bar-full-light.png',
  './TemplateData/unity-logo-dark.png',
  './TemplateData/unity-logo-light.png',
  './TemplateData/webgl-logo.png',
  './manifest.json'
];

// Install event - cache resources
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('Opened cache');
        // Try to cache all URLs, but don't fail installation if some fail
        return Promise.allSettled(
          urlsToCache.map(url => 
            cache.add(url).catch(err => {
              console.warn(`Failed to cache ${url}:`, err);
            })
          )
        );
      })
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME && cacheName.startsWith('slottest-pwa-')) {
            console.log('Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - serve from cache when offline
self.addEventListener('fetch', event => {
  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  event.respondWith(
    caches.match(event.request)
      .then(response => {
        // Cache hit - return response
        if (response) {
          return response;
        }

        // Clone the request
        const fetchRequest = event.request.clone();

        return fetch(fetchRequest).then(response => {
          // Check if valid response
          if (!response || response.status !== 200 || response.type !== 'basic') {
            return response;
          }

          // Clone the response
          const responseToCache = response.clone();

          // Don't cache if it's a Unity streaming asset or large file
          const url = new URL(event.request.url);
          const shouldCache = !url.pathname.includes('/StreamingAssets/') &&
                             !url.pathname.includes('.unityweb') &&
                             !url.pathname.endsWith('.mp4') &&
                             !url.pathname.endsWith('.webm');

          if (shouldCache) {
            caches.open(CACHE_NAME)
              .then(cache => {
                cache.put(event.request, responseToCache);
              });
          }

          return response;
        }).catch(() => {
          // Offline fallback for navigation requests
          if (event.request.mode === 'navigate') {
            return caches.match('./index.html');
          }
          
          // Return a simple offline response for other requests
          return new Response('Offline - resource not cached', {
            status: 503,
            statusText: 'Service Unavailable',
            headers: new Headers({
              'Content-Type': 'text/plain'
            })
          });
        });
      })
  );
});

// Handle messages from the client
self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  
  if (event.data && event.data.type === 'CACHE_ASSETS') {
    // Cache additional assets on demand
    const assets = event.data.assets;
    caches.open(CACHE_NAME).then(cache => {
      cache.addAll(assets);
    });
  }
});

// Background sync for saving game progress
self.addEventListener('sync', event => {
  if (event.tag === 'sync-game-progress') {
    event.waitUntil(syncGameProgress());
  }
});

async function syncGameProgress() {
  try {
    // This would sync saved game data when connection is restored
    const response = await fetch('/api/sync-progress', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        // Game progress data would go here
        timestamp: Date.now()
      })
    });
    return response;
  } catch (error) {
    console.error('Failed to sync game progress:', error);
    throw error;
  }
}