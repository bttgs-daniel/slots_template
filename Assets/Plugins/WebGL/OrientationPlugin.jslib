mergeInto(LibraryManager.library, {
    RequestLandscapeOrientation: function() {
        // Try to lock orientation to landscape
        if (screen.orientation && screen.orientation.lock) {
            screen.orientation.lock('landscape').then(function() {
                console.log('Orientation locked to landscape');
            }).catch(function(error) {
                console.log('Orientation lock failed:', error);
            });
        }
        
        // Fallback for browsers that don't support orientation.lock
        if (window.matchMedia("(orientation: portrait)").matches) {
            // Try to trigger fullscreen which often forces landscape on mobile
            var elem = document.documentElement;
            if (elem.requestFullscreen) {
                elem.requestFullscreen();
            } else if (elem.webkitRequestFullscreen) {
                elem.webkitRequestFullscreen();
            } else if (elem.mozRequestFullScreen) {
                elem.mozRequestFullScreen();
            } else if (elem.msRequestFullscreen) {
                elem.msRequestFullscreen();
            }
        }
    },
    
    IsPortraitOrientation: function() {
        return window.innerHeight > window.innerWidth;
    },
    
    ShowOrientationWarning: function() {
        var warning = document.getElementById('orientation-warning');
        if (!warning) {
            warning = document.createElement('div');
            warning.id = 'orientation-warning';
            warning.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(26, 26, 46, 0.98);
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                z-index: 9999;
                color: white;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
                text-align: center;
                padding: 20px;
            `;
            
            warning.innerHTML = `
                <svg width="100" height="100" viewBox="0 0 100 100" style="animation: rotate 2s infinite ease-in-out;">
                    <style>
                        @keyframes rotate {
                            0% { transform: rotate(0deg); }
                            50% { transform: rotate(90deg); }
                            100% { transform: rotate(90deg); }
                        }
                    </style>
                    <rect x="35" y="20" width="30" height="60" rx="5" fill="none" stroke="white" stroke-width="3"/>
                    <circle cx="50" cy="70" r="3" fill="white"/>
                    <path d="M 70 50 L 80 40 L 80 60 Z" fill="white"/>
                </svg>
                <h2 style="margin-top: 30px; font-size: 24px; font-weight: 600;">Please Rotate Your Device</h2>
                <p style="margin-top: 10px; font-size: 16px; opacity: 0.8; max-width: 300px;">
                    This game is best played in landscape mode. Please rotate your device to continue.
                </p>
            `;
            
            document.body.appendChild(warning);
        } else {
            warning.style.display = 'flex';
        }
    },
    
    HideOrientationWarning: function() {
        var warning = document.getElementById('orientation-warning');
        if (warning) {
            warning.style.display = 'none';
        }
    }
});