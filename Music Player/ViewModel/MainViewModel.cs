using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Data;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Threading;
using System;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Music_Player.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    /// 
    public class MainViewModel : ViewModelBase 
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        // Model
        private AudioPlayer musicPlayerModel;
        private LibraryManager libraryManagerModel;

        //ViewModel
        private string nowPlayingTrack = "Play a song";
        private string nowPlayingArtist = "";
        private int nowPlayingLenght = 0;
        private bool isPlaying = false;
        private DataView results = new DataView();
        private DataTable songs = new DataTable();
        private int volume = 75;
        private int percentagePlayed = 0;
        private int timeEllapsed = 0;
        private DispatcherTimer timer;
        private RelayCommand<int> playCommand;
        private RelayCommand addCommand;
        private RelayCommand nextCommand;
        private RelayCommand prevCommand;
        public MainViewModel()
        {
            Messenger.Default.Register<string>(this, OnStringMessageReceived);

            musicPlayerModel = new AudioPlayer();
            libraryManagerModel = new LibraryManager();

            songs = libraryManagerModel.GetSongs();
            DataTable songsDataView = new DataTable();
            songsDataView.Columns.Add("Title",typeof(string));
            songsDataView.Columns.Add("Artist", typeof(string));
            songsDataView.Columns.Add("Album", typeof(string));
            songsDataView.Columns.Add("Genre", typeof(string));
            songsDataView.Columns.Add("Time", typeof(string));
            foreach(DataRow row in songs.Rows)
            {
                int t = Int16.Parse(row["Time"].ToString());
                string tt = t % 60 < 10 ? t / 60 + ":0" + t % 60 : t / 60 + ":" + t % 60;
                songsDataView.Rows.Add(row["Title"].ToString(), row["Artist"].ToString(), row["Album"].ToString(), row["Genre"].ToString(),tt);
            }
            Results = songsDataView.AsDataView();
            timer = new DispatcherTimer();
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void dispatcherTimer_Tick(object sender, System.EventArgs e)
        {
            if (TimeEllapsed >= nowPlayingLenght)
            {
                timer.Stop();
                return;
            }
            TimeEllapsed += 1;
        }
        private void OnStringMessageReceived(string msg)
        {
            if (msg.Equals("ReloadLibrary"))
            {
                songs = libraryManagerModel.GetSongs();
                DataTable songsDataView = new DataTable();
                songsDataView.Columns.Add("Title", typeof(string));
                songsDataView.Columns.Add("Artist", typeof(string));
                songsDataView.Columns.Add("Album", typeof(string));
                songsDataView.Columns.Add("Genre", typeof(string));
                songsDataView.Columns.Add("Time", typeof(string));
                foreach (DataRow row in songs.Rows)
                {
                    int t = Int16.Parse(row["Time"].ToString());
                    string tt = t % 60 < 10 ? t / 60 + ":0" + t % 60 : t / 60 + ":" + t % 60;
                    songsDataView.Rows.Add(row["Title"].ToString(), row["Artist"].ToString(), row["Album"].ToString(), row["Genre"].ToString(), tt);
                }
                Results = songsDataView.AsDataView();
            }else if (msg.Equals("ReloadTrack"))
            {
                TimeEllapsed = 0;
                timer.Start();
                IsPlaying = true;
                NowPlayingArtist = musicPlayerModel.Artist;
                NowPlayingTrack = musicPlayerModel.Track + " - " + musicPlayerModel.Album;
                nowPlayingLenght = musicPlayerModel.GetTrackLength();
            }
        }

        private void UpdateQueue(int selectedIndex)
        {
            musicPlayerModel.SetQueue(songs,selectedIndex);
        }
        private void ScanFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryScanner ds = DirectoryScanner.Instance;
                libraryManagerModel.AddSongs(ds.ScanRecursive(dialog.SelectedPath,null));
            }
        }
        private void NextSong()
        {
            musicPlayerModel.Next();
        }
        private void PrevSong()
        {
            musicPlayerModel.Prev();
        }
        public RelayCommand<int> PlayCommand
        {
            get
            {
                if (playCommand == null)
                {
                    playCommand = new RelayCommand<int>(selectedIndex => UpdateQueue(selectedIndex));
                }

                return playCommand;
            }
        }
        public RelayCommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new RelayCommand(ScanFolder);
                }

                return addCommand;
            }
        }
        public RelayCommand NextCommand
        {
            get
            {
                if (nextCommand == null)
                {
                    nextCommand = new RelayCommand(NextSong);
                }

                return nextCommand;
            }
        }
        public RelayCommand PrevCommand
        {
            get
            {
                if (prevCommand == null)
                {
                    prevCommand = new RelayCommand(PrevSong);
                }

                return prevCommand;
            }
        }
        public string NowPlayingTrack
        {
            get 
            {
                return nowPlayingTrack.Equals("")?"Unknown Track":nowPlayingTrack;
            }
            set 
            {
                if (nowPlayingTrack == value)
                    return;
                nowPlayingTrack = value;
                RaisePropertyChanged("NowPlayingTrack");
            }
        }
        public string NowPlayingArtist
        {
            get
            {
                return nowPlayingArtist.Equals("") ? "Unknown Artist" : nowPlayingArtist; ;
            }
            set
            {
                if (nowPlayingArtist == value)
                    return;
                nowPlayingArtist = value;
                RaisePropertyChanged("NowPlayingArtist");
            }
        }
        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
            set
            {
                if (isPlaying == value)
                    return;
                isPlaying = value;
                RaisePropertyChanged("IsPlaying");
                if (isPlaying)
                {
                    timer.Start();
                    musicPlayerModel.Play();
                }
                else
                {
                    timer.Stop();
                    musicPlayerModel.Pause();
                }
            }
        }
        public DataView Results
        {
            get
            {
                return results;
            }
            set
            {
                if (results == value)
                    return;
                results = value;
                RaisePropertyChanged("Results");
            }
        }
        public int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (volume == value)
                    return;
                volume = value;
                musicPlayerModel.ChangeVolume(volume);
                RaisePropertyChanged("Volume");
            }
        }
        public int TimeEllapsed
        {
            get
            {
                return timeEllapsed;
            }
            set
            {
                if (timeEllapsed == value)
                    return;
                timeEllapsed = value;
                PercentagePlayed = (int)((double)(timeEllapsed) / (double)(nowPlayingLenght) * 1000);
                RaisePropertyChanged("TimeEllapsed");
            }
        }
        public int PercentagePlayed
        {
            get
            {
                return percentagePlayed;
            }
            set
            {
                if (percentagePlayed == value)
                    return;
                RaisePropertyChanging("PercentagePlayed");
                int oldvalue = percentagePlayed;
                percentagePlayed = value;
                RaisePropertyChanged("PercentagePlayed");
                if ((int)((double)TimeEllapsed / (double)nowPlayingLenght * 1000) != percentagePlayed)
                {
                    TimeEllapsed = (int)((double)percentagePlayed/1000 * nowPlayingLenght);
                    musicPlayerModel.Seek(TimeEllapsed);
                }
            }
        }
    }
}