class NotificationManager {
    constructor() {
        this.connection = null;
        this.notifications = [];
        this.settings = this.loadSettings();
        this.audioContext = null;
        this.soundBuffers = {};
        
        this.init();
    }

    loadSettings() {
        const defaultSettings = {
            soundEnabled: true,
            browserNotificationsEnabled: true,
            maxNotificationsDisplay: 5
        };
        
        const saved = localStorage.getItem('notificationSettings');
        return saved ? { ...defaultSettings, ...JSON.parse(saved) } : defaultSettings;
    }

    saveSettings() {
        localStorage.setItem('notificationSettings', JSON.stringify(this.settings));
    }

    async init() {
        await this.initSignalR();
        await this.initAudio();
        await this.requestBrowserNotificationPermission();
        this.setupEventHandlers();
        await this.loadExistingNotifications();
    }

    async initSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationhub")
            .withAutomaticReconnect()
            .build();

        this.connection.on("NotificationReceived", (notification) => {
            this.handleNotification(notification);
        });

        this.connection.onreconnected(() => {
            console.log("SignalR reconnected");
        });

        try {
            await this.connection.start();
            console.log("SignalR connected");
        } catch (err) {
            console.error("SignalR connection failed:", err);
        }
    }

    async initAudio() {
        if (this.settings.soundEnabled) {
            try {
                // Initialize AudioContext on user interaction
                document.addEventListener('click', this.initAudioContext.bind(this), { once: true });
            } catch (err) {
                console.warn("Audio initialization failed:", err);
            }
        }
    }

    async initAudioContext() {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            await this.loadSounds();
        }
    }

    async loadSounds() {
        const sounds = {
            info: this.generateTone(800, 0.1, 0.05),
            warning: this.generateTone(600, 0.2, 0.1),
            error: this.generateTone(400, 0.3, 0.15),
            critical: this.generateTone(300, 0.5, 0.2)
        };

        for (const [name, buffer] of Object.entries(sounds)) {
            this.soundBuffers[name] = buffer;
        }
    }

    generateTone(frequency, duration, volume) {
        const sampleRate = this.audioContext.sampleRate;
        const frameCount = sampleRate * duration;
        const buffer = this.audioContext.createBuffer(1, frameCount, sampleRate);
        const channelData = buffer.getChannelData(0);

        for (let i = 0; i < frameCount; i++) {
            channelData[i] = Math.sin(2 * Math.PI * frequency * i / sampleRate) * volume;
        }

        return buffer;
    }

    async requestBrowserNotificationPermission() {
        if (this.settings.browserNotificationsEnabled && 'Notification' in window) {
            if (Notification.permission === 'default') {
                await Notification.requestPermission();
            }
        }
    }

    setupEventHandlers() {
        // Clear notifications button
        document.getElementById('clear-notifications-btn')?.addEventListener('click', () => {
            this.clearNotifications();
        });

        // Notification settings (will be implemented in standalone page)
        document.addEventListener('notificationSettingsChanged', (e) => {
            this.settings = { ...this.settings, ...e.detail };
            this.saveSettings();
        });
    }

    async loadExistingNotifications() {
        try {
            const response = await fetch('/api/notifications?count=20');
            const notifications = await response.json();
            this.notifications = notifications || [];
            this.updateUI();
        } catch (err) {
            console.error("Failed to load existing notifications:", err);
        }
    }

    handleNotification(notification) {
        // Ensure notification has required properties
        if (!notification || !notification.Title || !notification.Message) {
            console.warn("Received invalid notification:", notification);
            return;
        }

        // Ensure Severity is a string
        if (typeof notification.Severity === 'number') {
            // Convert enum number to string
            const severityMap = { 0: 'Info', 1: 'Warning', 2: 'Error', 3: 'Critical' };
            notification.Severity = severityMap[notification.Severity] || 'Info';
        } else if (!notification.Severity) {
            notification.Severity = 'Info';
        }

        this.notifications.unshift(notification);
        
        // Keep only recent notifications in memory
        if (this.notifications.length > 100) {
            this.notifications = this.notifications.slice(0, 100);
        }

        this.updateUI();
        this.playSound(notification.Severity);
        this.showBrowserNotification(notification);

        // Dispatch custom event for standalone notification page
        window.dispatchEvent(new CustomEvent('notificationReceived', { detail: notification }));
    }

    updateUI() {
        this.updateBadge();
        this.updateDropdown();
    }

    updateBadge() {
        const badge = document.getElementById('notification-badge');
        if (!badge) return;

        const count = this.notifications.length;
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count.toString();
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    }

    updateDropdown() {
        const container = document.getElementById('notification-list');
        if (!container) return;

        if (this.notifications.length === 0) {
            container.innerHTML = '<div class="dropdown-item-text text-muted text-center">No notifications</div>';
            return;
        }

        const recentNotifications = this.notifications.slice(0, this.settings.maxNotificationsDisplay);
        const html = recentNotifications.map(notification => {
            // Defensive checks for notification properties
            const severity = notification.Severity || 'Info';
            const title = notification.Title || 'Notification';
            const message = notification.Message || 'No message';
            const timestamp = notification.Timestamp || new Date().toISOString();
            
            return `
                <div class="dropdown-item notification-item severity-${severity.toLowerCase()}">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">
                            <h6 class="dropdown-header mb-1 text-${this.getSeverityColor(severity)}">${title}</h6>
                            <p class="mb-1 small">${message}</p>
                            <small class="text-muted">${this.formatTimestamp(timestamp)}</small>
                        </div>
                        <span class="badge bg-${this.getSeverityColor(severity)}">${severity}</span>
                    </div>
                </div>
            `;
        }).join('');

        container.innerHTML = html;

        // Add "View All" link if there are more notifications
        if (this.notifications.length > this.settings.maxNotificationsDisplay) {
            container.innerHTML += `
                <div class="dropdown-divider"></div>
                <a class="dropdown-item text-center text-primary" href="/Notifications">
                    View All (${this.notifications.length})
                </a>
            `;
        }
    }

    getSeverityColor(severity) {
        const colors = {
            'Info': 'primary',
            'Warning': 'warning',
            'Error': 'danger',
            'Critical': 'danger'
        };
        return colors[severity] || 'secondary';
    }

    formatTimestamp(timestamp) {
        try {
            const date = new Date(timestamp);
            const now = new Date();
            const diffMs = now - date;
            const diffMins = Math.floor(diffMs / 60000);
            const diffHours = Math.floor(diffMs / 3600000);
            const diffDays = Math.floor(diffMs / 86400000);

            if (diffMins < 1) return 'Just now';
            if (diffMins < 60) return `${diffMins}m ago`;
            if (diffHours < 24) return `${diffHours}h ago`;
            if (diffDays < 7) return `${diffDays}d ago`;
            
            return date.toLocaleDateString();
        } catch (err) {
            return 'Unknown time';
        }
    }

    playSound(severity) {
        if (!this.settings.soundEnabled || !this.audioContext || !this.soundBuffers) return;

        try {
            const soundName = (severity || 'info').toLowerCase();
            const buffer = this.soundBuffers[soundName] || this.soundBuffers['info'];
            
            if (buffer) {
                const source = this.audioContext.createBufferSource();
                source.buffer = buffer;
                source.connect(this.audioContext.destination);
                source.start();
            }
        } catch (err) {
            console.warn("Failed to play notification sound:", err);
        }
    }

    showBrowserNotification(notification) {
        if (!this.settings.browserNotificationsEnabled || 
            !('Notification' in window) || 
            Notification.permission !== 'granted') {
            return;
        }

        try {
            const title = notification.Title || 'Notification';
            const message = notification.Message || 'No message';
            
            const browserNotification = new Notification(title, {
                body: message,
                icon: '/favicon.ico',
                tag: notification.Id,
                badge: '/favicon.ico'
            });

            // Auto-close after 5 seconds
            setTimeout(() => browserNotification.close(), 5000);

            browserNotification.addEventListener('click', () => {
                window.focus();
                browserNotification.close();
            });
        } catch (err) {
            console.warn("Failed to show browser notification:", err);
        }
    }

    async clearNotifications() {
        try {
            await fetch('/api/notifications', { method: 'DELETE' });
            this.notifications = [];
            this.updateUI();
        } catch (err) {
            console.error("Failed to clear notifications:", err);
        }
    }

    // Public methods for testing
    async sendTestNotification(severity = 'Info') {
        try {
            await fetch('/api/testing/notifications/send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ severity })
            });
        } catch (err) {
            console.error("Failed to send test notification:", err);
        }
    }

    updateSettings(newSettings) {
        this.settings = { ...this.settings, ...newSettings };
        this.saveSettings();
        
        if (newSettings.soundEnabled !== undefined) {
            if (newSettings.soundEnabled && !this.audioContext) {
                this.initAudioContext();
            }
        }
    }
}

// Initialize notification manager when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.notificationManager = new NotificationManager();
});