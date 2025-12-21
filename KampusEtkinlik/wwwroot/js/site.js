// Scroll fonksiyonu - belirli miktar kadar
window.scrollElement = (element, amount) => {
    if (element) {
        element.scrollLeft += amount;
    }
};

// Scroll fonksiyonu - tam genişlik kadar (4 kart)
window.scrollElementByWidth = (element, forward = false) => {
    if (element) {
        const scrollAmount = element.clientWidth;
        element.scrollLeft += forward ? scrollAmount : -scrollAmount;
    }
};

// Chat widget scroll to bottom
window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Membership Cache - SessionStorage operations for club membership statuses
window.membershipCache = {
    get: function(key) {
        try {
            return sessionStorage.getItem(key);
        } catch (e) {
            console.error('SessionStorage read error:', e);
            return null;
        }
    },
    set: function(key, value) {
        try {
            sessionStorage.setItem(key, value);
            return true;
        } catch (e) {
            console.error('SessionStorage write error:', e);
            return false;
        }
    },
    remove: function(key) {
        try {
            sessionStorage.removeItem(key);
            return true;
        } catch (e) {
            console.error('SessionStorage remove error:', e);
            return false;
        }
    }
};

// Theme Interop - Dark/Light mode management
window.themeInterop = {
    getTheme: function() {
        try {
            return localStorage.getItem('theme-preference');
        } catch (e) {
            console.error('Theme read error:', e);
            return null;
        }
    },
    setTheme: function(theme) {
        try {
            document.documentElement.setAttribute('data-theme', theme);
            return true;
        } catch (e) {
            console.error('Theme set error:', e);
            return false;
        }
    },
    saveTheme: function(theme) {
        try {
            localStorage.setItem('theme-preference', theme);
            return true;
        } catch (e) {
            console.error('Theme save error:', e);
            return false;
        }
    }
};

// Initialize theme on page load (before Blazor loads)
(function() {
    try {
        var savedTheme = localStorage.getItem('theme-preference');
        if (savedTheme === 'dark' || savedTheme === 'light') {
            document.documentElement.setAttribute('data-theme', savedTheme);
        }
    } catch (e) {
        console.error('Theme init error:', e);
    }
})();
