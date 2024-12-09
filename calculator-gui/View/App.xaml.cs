using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Media;
using System.Media;

namespace calculator_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App MainApp => ((App) Application.Current);

        public List<Theme> themes = new List<Theme>();
        private Theme _currentTheme;
        public GraphingColours graphingColours;
        public ColourList colourList;

        public Sound clickSound;

        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                ResourceDictionary styles = new ResourceDictionary() { Source = new Uri("View/Themes/Styles.xaml", UriKind.Relative) };
                ResourceDictionary theme = new ResourceDictionary() { Source = new Uri(_currentTheme.Path, UriKind.Relative) };
                theme.MergedDictionaries.Add(styles);
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(theme);
                //Resources.Clear();
                //Resources.MergedDictionaries.Clear();
                //Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(_currentTheme.Path, UriKind.Relative) });
                graphingColours.UpdateColours();
            }
        }

        public int angleType; // 0=degrees, 1=radians
        public bool viewGraphBSP; // shows the lines of the quadtree
        public bool useAutoBSPDepth = false;
        public int maxBSPDepth;
        public bool performanceStatsEnabled;
        public bool soundsOn = false;

        public App()
        {
            InitializeComponent();

            graphingColours = new GraphingColours();

            themes.Add(new Theme() { Name = "Light", Path = "View/Themes/Light.xaml" });
            themes.Add(new Theme() { Name = "Dark", Path = "View/Themes/Dark.xaml" });
            themes.Add(new Theme() { Name = "High Contrast Light", Path = "View/Themes/HighContrastLight.xaml" });
            themes.Add(new Theme() { Name = "High Contrast Dark", Path = "View/Themes/HighContrastDark.xaml" });
            themes.Add(new Theme() { Name = "Colourful", Path = "View/Themes/Colourful.xaml" });
            themes.Add(new Theme() { Name = "Industrial", Path = "View/Themes/Factorio.xaml" });
            themes.Add(new Theme() { Name = "Space Age", Path = "View/Themes/Kerbal.xaml" });
            themes.Add(new Theme() { Name = "Programmer", Path = "View/Themes/Perry.xaml" });
            CurrentTheme = themes[0];

            clickSound = new Sound("View/Themes/Sounds/click.wav");
        }
    }

    public class Theme
    {
        public string Name;
        public string Path;
    }

    public class Sound
    {
        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                Uri uri = new Uri(_path, UriKind.Relative);
                Stream resourceStream = Application.GetResourceStream(uri).Stream;
                player = new SoundPlayer(resourceStream);
            }
        }
        private SoundPlayer player;

        public Sound(string newPath)
        {
            Path = newPath;
        }

        public void Play()
        {
            if (App.MainApp.soundsOn)
            {
                player.Play();
            }
        }

        public void Stop()
        {
            player.Stop();
        }
    }
}
