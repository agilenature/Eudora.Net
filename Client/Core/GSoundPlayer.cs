using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Eudora.Net.Core
{
    /// <summary>
    /// Nothing more than a thin wrapper around a path to a sound file for GUI-binding purposes.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    public class Sound : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private string _Path = string.Empty;
        public string Path
        {
            get => _Path;
            set => SetField(ref _Path, value, nameof(Path));
        }

        private bool _IsSystemSound = false;
        public bool IsSystemSound
        {
            get => _IsSystemSound;
            set => SetField(ref _IsSystemSound, value, nameof(IsSystemSound));
        }

        public Sound(string name, string path, bool isSystemSound = false)
        {
            Name = name;
            Path = path;
            IsSystemSound = isSystemSound;
        }
    }


    /// <summary>
    /// Decrypts Apollo-era NASA communications. No, just kidding. It plays sounds. 
    /// </summary>
    public class GSoundPlayer
    {
        public static GSoundPlayer Instance;

        private static readonly string DataRoot = string.Empty;

        // This was a trick to get Visual Studio to load the right reference for MediaPlayer.
        // Leaving it here but commented out for future reference.
        //System.Windows.Media.Imaging.BitmapCacheOption cacheOption = BitmapCacheOption.OnLoad;

        /// <summary>
        /// A databindable list of sounds the app knows about
        /// </summary>
        public ObservableCollection<Sound> Sounds { get; set; } = new ObservableCollection<Sound>();

        /// <summary>
        /// NYI: playback queueing
        /// </summary>
        private Queue<Sound> SoundQueue { get; set; } = new Queue<Sound>();
        
        /// <summary>
        /// The player proper
        /// </summary>
        private MediaPlayer Player  = new MediaPlayer();

        /// <summary>
        /// Keeps the universe from exploding
        /// </summary>
        private object SoundsSerializationLocker = new object();

        /// <summary>
        /// A list of supported sound formats. These are checked against on LoadSounds
        /// </summary>
        private List<string> SoundExtensions = new List<string>();


        ////////////////////////////////////////////////////////////////////////
        #region Internal
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsSoundFile(string path)
        {
            if (!Path.HasExtension(path))
            {
                return false;
            }

            string extension = Path.GetExtension(path);
            if (!SoundExtensions.Contains(extension))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fires when async load is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            PlayLoadedSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlayLoadedSound()
        {
            Player.Play();
        }

        /// <summary>
        /// To the user/caller there's no difference between the sound files in \Sounds
        /// and the system sounds. They're all presented in the same list. This alternate
        /// Play call is strictly an internal matter.
        /// </summary>
        /// <param name="sound"></param>
        private void PlaySystemSound(Sound sound)
        {
            switch(sound.Name)
            {
                case "System: Beep":
                    SystemSounds.Beep.Play();
                    break;

                case "System: Asterisk":
                    SystemSounds.Asterisk.Play();
                    break;

                case "System: Question":
                    SystemSounds.Question.Play();
                    break;

                case "System: Exclamation":
                    SystemSounds.Exclamation.Play();
                    break;

                case "System: Hand":
                    SystemSounds.Hand.Play();
                    break;
                default:
                    Logger.NewEvent(LogEvent.EventCategory.Warning,
                        "Unknown system sound requested: " + sound.Name);
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        #endregion Internal
        ////////////////////////////////////////////////////////////////////////
        


        ////////////////////////////////////////////////////////////////////////
        #region Construction & Initialization
        ////////////////////////////////////////////////////////////////////////

        static GSoundPlayer()
        {
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Sounds");
            IoUtil.EnsureFolder(DataRoot);
            Instance = new GSoundPlayer();
        }

        public GSoundPlayer()
        {
            Player.MediaOpened += Player_MediaOpened;
            Initialize();
        }

        
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            SoundExtensions.Add(".wav");
            SoundExtensions.Add(".mp3");
            SoundExtensions.Add(".flac");
            SoundExtensions.Add(".m4a");
            SoundExtensions.Add(".wma");
            SoundExtensions.Add(".ac3");
            SoundExtensions.Add(".oga");
            SoundExtensions.Add(".ogg");
            SoundExtensions.Add(".opus");

            LoadSounds();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        private void AddSound(Sound sound)
        {
            Sounds.Add(sound);
        }


        /// <summary>
        /// Load files from the App/Sounds folder and add valid sound files to the list
        /// </summary>
        private void LoadSounds()
        {
            lock(SoundsSerializationLocker)
            {
                try
                {
                    // Eudora.Net supports loading sounds on the fly; no need to
                    // restart the app, just reload the sounds to catch new files.
                    Sounds.Clear();

                    // Begin by adding the system sounds
                    Sounds.Add(new Sound("System: Beep", string.Empty, true));
                    Sounds.Add(new Sound("System: Asterisk", string.Empty, true));
                    Sounds.Add(new Sound("System: Question", string.Empty, true));
                    Sounds.Add(new Sound("System: Exclamation", string.Empty, true));
                    Sounds.Add(new Sound("System: Hand", string.Empty, true));

                    // Now scan for audio files in the \Sounds folder
                    DirectoryInfo di = new DirectoryInfo(DataRoot);
                    var files = di.GetFiles();
                    if(files == null)
                    {
                        return;
                    }

                    foreach(FileInfo file in files)
                    {
                        if(IsSoundFile(file.FullName))
                        {
                            AddSound(new Sound(file.Name, file.FullName));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        #endregion Construction & Initialization
        ////////////////////////////////////////////////////////////////////////
        


        ////////////////////////////////////////////////////////////////////////
        #region Interface
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When loading is complete, the event will fire and begin playback as a result
        /// </summary>
        /// <param name="sound"></param>
        public void Play(Sound sound)
        {
            if(sound.IsSystemSound)
            {
                PlaySystemSound(sound);
                return;
            }
            Player.Open(new Uri(sound.Path));
        }

        public void Play(string soundName)
        {
            var sound = Sounds.Where(i => i.Name.Equals(soundName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (sound is not null) Play(sound);
        }

        /// <summary>
        /// Allows the user to add new sounds without having to restart the app
        /// </summary>
        public void ReloadSounds()
        {
            LoadSounds();
        }

        ////////////////////////////////////////////////////////////////////////
        #endregion Interface
        ////////////////////////////////////////////////////////////////////////
    }


}
