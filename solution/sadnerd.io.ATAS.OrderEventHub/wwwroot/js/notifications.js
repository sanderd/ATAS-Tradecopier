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
            maxNotificationsDisplay: 5,
            browserSeverities: ['Warning', 'Error', 'Critical'],
            soundSeverities: ['Warning', 'Error', 'Critical'],
            bellSeverities: ['Warning', 'Error']
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
            info: this.generateTone(800, 0.2, 0.05),
            warning: this.generateTone(600, 0.4, 0.1),
            error: this.generateTone(400, 0.6, 0.15),
            critical: this.generateTone(300, 0.8, 0.2)
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
            
            // Normalize all loaded notifications to Pascal case
            this.notifications = (notifications || []).map(n => ({
                Id: n.Id || n.id,
                Title: n.Title || n.title,
                Message: n.Message || n.message,
                Severity: this.normalizeSeverity(n.Severity || n.severity),
                Timestamp: n.Timestamp || n.timestamp,
                Source: n.Source || n.source,
                Metadata: n.Metadata || n.metadata || {}
            }));
            
            this.updateUI();
        } catch (err) {
            console.error("Failed to load existing notifications:", err);
        }
    }

    handleNotification(notification) {
        // Ensure notification has required properties (handle both Pascal and camel case)
        const title = notification.Title || notification.title;
        const message = notification.Message || notification.message;
        const severity = notification.Severity || notification.severity;
        const timestamp = notification.Timestamp || notification.timestamp;
        const source = notification.Source || notification.source;
        const metadata = notification.Metadata || notification.metadata;
        const id = notification.Id || notification.id;

        if (!title || !message) {
            console.warn("Received invalid notification:", notification);
            return;
        }

        // Normalize notification object to Pascal case for consistency
        const normalizedNotification = {
            Id: id,
            Title: title,
            Message: message,
            Severity: this.normalizeSeverity(severity),
            Timestamp: timestamp,
            Source: source,
            Metadata: metadata || {}
        };

        this.notifications.unshift(normalizedNotification);
        
        // Keep only recent notifications in memory
        if (this.notifications.length > 100) {
            this.notifications = this.notifications.slice(0, 100);
        }

        this.updateUI();
        this.playSound(normalizedNotification.Severity);
        this.showBrowserNotification(normalizedNotification);

        // Dispatch custom event for standalone notification page
        window.dispatchEvent(new CustomEvent('notificationReceived', { detail: normalizedNotification }));
    }

    normalizeSeverity(severity) {
        // Ensure Severity is a string
        if (typeof severity === 'number') {
            // Convert enum number to string
            const severityMap = { 0: 'Info', 1: 'Warning', 2: 'Error', 3: 'Critical' };
            return severityMap[severity] || 'Info';
        } else if (!severity) {
            return 'Info';
        }
        return severity;
    }

    updateUI() {
        this.updateBadge();
        this.updateDropdown();
    }

    updateBadge() {
        const badge = document.getElementById('notification-badge');
        if (!badge) return;

        // Count notifications based on bell severity settings
        const count = this.notifications.filter(notification => {
            const severity = notification.Severity || 'Info';
            return this.settings.bellSeverities.includes(severity);
        }).length;

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

        // Filter notifications based on bell severity settings
        const bellNotifications = this.notifications.filter(notification => {
            const severity = notification.Severity || 'Info';
            return this.settings.bellSeverities.includes(severity);
        });

        if (bellNotifications.length === 0) {
            container.innerHTML = '<div class="dropdown-item-text text-muted text-center">No notifications</div>';
            return;
        }

        const recentNotifications = bellNotifications.slice(0, this.settings.maxNotificationsDisplay);
        const html = recentNotifications.map(notification => {
            // Defensive checks for notification properties
            const severity = notification.Severity || 'Info';
            const title = this.escapeHtml(notification.Title || 'Notification');
            const message = this.escapeHtml(notification.Message || 'No message');
            const timestamp = notification.Timestamp || new Date().toISOString();
            
            return `
                <div class="dropdown-item notification-item severity-${severity.toLowerCase()}">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1 min-w-0">
                            <h6 class="dropdown-header mb-1 text-${this.getSeverityColor(severity)}" title="${title}">${title}</h6>
                            <p class="mb-1 small">${message}</p>
                            <small class="text-muted">${this.formatTimestamp(timestamp)}</small>
                        </div>
                        <span class="badge bg-${this.getSeverityColor(severity)} flex-shrink-0">${severity}</span>
                    </div>
                </div>
            `;
        }).join('');

        container.innerHTML = html;

        // Add "View All" link if there are more notifications
        if (bellNotifications.length > this.settings.maxNotificationsDisplay) {
            container.innerHTML += `
                <div class="dropdown-divider"></div>
                <a class="dropdown-item text-center text-primary" href="/Notifications">
                    View All (${bellNotifications.length})
                </a>
            `;
        }
    }

    // Helper function to escape HTML
    escapeHtml(unsafe) {
        if (unsafe == null) return '';
        return unsafe.toString()
             .replace(/&/g, "&amp;")
             .replace(/</g, "&lt;")
             .replace(/>/g, "&gt;")
             .replace(/"/g, "&quot;")
             .replace(/'/g, "&#039;");
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

        // Check if this severity should trigger sound notifications
        if (!this.settings.soundSeverities.includes(severity)) return;

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

        // Check if this severity should trigger browser notifications
        const severity = notification.Severity || 'Info';
        if (!this.settings.browserSeverities.includes(severity)) return;

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