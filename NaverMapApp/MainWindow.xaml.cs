using NaverMapApp.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static NaverMapApp.Logic.NaverMapApi;

namespace NaverMapApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IniFile? iniFile;
        private static StaticMap? staticMap;

        public MainWindow()
        {
            InitializeComponent();

            btn_StaticMapRequest.Click += Btn_StaticMapRequest_Click;

            iniFile = new IniFile();

            //MapApi = new NaverMapApi();
            staticMap = new();
            staticMap.SetKey(
                iniFile.Read("StaticMapApi", "Client_ID"),
                iniFile.Read("StaticMapApi", "Client_Secret")
                  );
            
            
        }

        private void Btn_StaticMapRequest_Click(object sender, RoutedEventArgs e)
        {
            staticMap.Center = new StaticMap.CENTER("127.1054221", "37.3591614");
            staticMap.Size = new StaticMap.SIZE("800", "600");
            staticMap.Level = 10;
            bool check = staticMap.SetUrl(out string msg);

            if (!check)
            {
                MessageBox.Show(msg);
            }
            else
            {
                if (staticMap.Request())
                {
                    staticMap.ResponseToFile("test");
                }
                else
                {

                }
            }
        }
    }
}
