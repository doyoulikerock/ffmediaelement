﻿#define ENABLE_MY_TITLEBAR

namespace Unosquare.FFME.Windows.Sample.ViewModels
{
    using Common;
    using Foundation;
    using System;
#if !ENABLE_MY_TITLEBAR
    using System.Diagnostics;
#endif
    using System.IO;
    using System.Windows;
    using System.Windows.Shell;

    /// <summary>
    /// Represents the application-wide view model.
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public sealed class RootViewModel : ViewModelBase
    {
        private string m_WindowTitle = string.Empty;
        private string m_NotificationMessage = string.Empty;
        private double m_PlaybackProgress;
        private TaskbarItemProgressState m_PlaybackProgressState;
        private bool m_IsPlaylistPanelOpen = App.IsInDesignMode;
        private bool m_IsPropertiesPanelOpen = App.IsInDesignMode;
        private bool m_IsApplicationLoaded = App.IsInDesignMode;
        private MediaElement m_MediaElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootViewModel"/> class.
        /// </summary>
        public RootViewModel()
        {
            // Set and create an app data directory
            WindowTitle = "Application Loading . . .";
            AppVersion = typeof(RootViewModel).Assembly.GetName().Version.ToString();
            AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);
            if (Directory.Exists(AppDataDirectory) == false)
                Directory.CreateDirectory(AppDataDirectory);

            // Attached ViewModel Initialization
            Playlist = new PlaylistViewModel(this);
            Controller = new ControllerViewModel(this);
        }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public static string ProductName => "Unosquare FFME-Play";

        /// <summary>
        /// Gets the playlist ViewModel.
        /// </summary>
        public PlaylistViewModel Playlist { get; }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public ControllerViewModel Controller { get; }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        public string AppDataDirectory { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string WindowTitle
        {
            get => m_WindowTitle;
            private set => SetProperty(ref m_WindowTitle, value);
        }

        /// <summary>
        /// Gets or sets the notification message to be displayed.
        /// </summary>
        public string NotificationMessage
        {
            get
            {
                return m_NotificationMessage;
            }
            set
            {
                m_NotificationMessage = value;
                NotifyPropertyChanged(nameof(NotificationMessage));
            }
        }

        /// <summary>
        /// Gets or sets the playback progress.
        /// </summary>
        public double PlaybackProgress
        {
            get
            {
                return m_PlaybackProgress;
            }
            set
            {
                m_PlaybackProgress = value;
                NotifyPropertyChanged(nameof(PlaybackProgress));
            }
        }

        /// <summary>
        /// Gets or sets the state of the playback progress.
        /// </summary>
        public TaskbarItemProgressState PlaybackProgressState
        {
            get
            {
                return m_PlaybackProgressState;
            }
            set
            {
                m_PlaybackProgressState = value;
                NotifyPropertyChanged(nameof(PlaybackProgressState));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is playlist panel open.
        /// </summary>
        public bool IsPlaylistPanelOpen
        {
            get => m_IsPlaylistPanelOpen;
            set => SetProperty(ref m_IsPlaylistPanelOpen, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is properties panel open.
        /// </summary>
        public bool IsPropertiesPanelOpen
        {
            get => m_IsPropertiesPanelOpen;
            set => SetProperty(ref m_IsPropertiesPanelOpen, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is application loaded.
        /// </summary>
        public bool IsApplicationLoaded
        {
            get => m_IsApplicationLoaded;
            set => SetProperty(ref m_IsApplicationLoaded, value);
        }

        /// <summary>
        /// Provides access to application-wide commands.
        /// </summary>
        public AppCommands Commands { get; } = new AppCommands();

        /// <summary>
        /// Gets the media element hosted by the main window.
        /// </summary>
        public MediaElement MediaElement
        {
            get
            {
                if (m_MediaElement == null)
                    m_MediaElement = (Application.Current.MainWindow as MainWindow)?.Media;

                return m_MediaElement;
            }
        }

        /// <summary>
        /// Provides access to the current media options.
        /// This is an unsupported usage of media options.
        /// </summary>
        public MediaOptions CurrentMediaOptions { get; set; }

        /// <summary>
        /// Called when application has finished loading.
        /// </summary>
        internal void OnApplicationLoaded()
        {
            if (IsApplicationLoaded)
                return;

            Playlist.OnApplicationLoaded();
            Controller.OnApplicationLoaded();

            var m = MediaElement;
            m.WhenChanged(UpdateWindowTitle,
                nameof(m.IsOpen),
                nameof(m.IsOpening),
                nameof(m.MediaState),
                nameof(m.Source));

            m.MediaOpened += (s, e) =>
            {
                // Reset the Zoom
                Controller.MediaElementZoom = 1d;

                // Update the Controls
                Playlist.IsInOpenMode = false;
                IsPlaylistPanelOpen = false;
                Playlist.OpenMediaSource = e.Info.MediaSource;
            };

            IsPlaylistPanelOpen = true;
            IsApplicationLoaded = true;
        }

        /// <summary>
        /// Updates the window title according to the current state.
        /// </summary>
        private void UpdateWindowTitle()
        {
            var m = MediaElement;
            var title = m?.Source?.ToString() ?? "(No media loaded)";
            var state = m?.MediaState.ToString();

            if (m?.IsOpen ?? false)
            {
                foreach (var kvp in m.Metadata)
                {
                    if (!kvp.Key.Equals("title", StringComparison.OrdinalIgnoreCase))
                        continue;

                    title = kvp.Value;
                    break;
                }
            }
            else if (m?.IsOpening ?? false)
            {
                state = "Opening . . .";
            }
            else
            {
                title = "(No media loaded)";
                state = "Ready";
            }

#if ENABLE_MY_TITLEBAR
            WindowTitle = MainWindow.MyTitle;
#else
            WindowTitle = $"{title} - {state} - FFME Player v{AppVersion} "
                + $"FFmpeg {Library.FFmpegVersionInfo} ({(Debugger.IsAttached ? "Debug" : "Release")})";
#endif
        }
    }
}
