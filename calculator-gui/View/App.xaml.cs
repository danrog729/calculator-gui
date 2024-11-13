using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics.Eventing.Reader;

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
            }
        }

        public int angleType; // 0=degrees, 1=radians
        public bool viewGraphBSP; // shows the lines of the quadtree
        public bool useAutoBSPDepth = false;
        public int maxBSPDepth;

        public App()
        {
            InitializeComponent();
            themes.Add(new Theme() { Name = "Light", Path = "View/Themes/Light.xaml" });
            themes.Add(new Theme() { Name = "Dark", Path = "View/Themes/Dark.xaml" });
            themes.Add(new Theme() { Name = "High Contrast Light", Path = "View/Themes/HighContrastLight.xaml" });
            themes.Add(new Theme() { Name = "High Contrast Dark", Path = "View/Themes/HighContrastDark.xaml" });
            themes.Add(new Theme() { Name = "Colourful", Path = "View/Themes/Colourful.xaml" });
            themes.Add(new Theme() { Name = "Industrial", Path = "View/Themes/Factorio.xaml" });
            themes.Add(new Theme() { Name = "Space Age", Path = "View/Themes/Kerbal.xaml" });
            themes.Add(new Theme() { Name = "Programmer", Path = "View/Themes/Perry.xaml" });
            CurrentTheme = themes[0];
        }
    }

    public class Theme
    {
        public string Name;
        public string Path;
    }
}
