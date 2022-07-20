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
using static NaverMapApp.Logic.NaverMapApi.StaticMap;

namespace NaverMapApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IniFile? iniFile;
        private static ImgFile? imgFile;
        private static StaticMap? staticMap;
        private static Geocode? geocode;

        public MainWindow()
        {
            InitializeComponent();

            btn_StaticMapRequest.Click += Btn_StaticMapRequest_Click;

            btn_Geocode.Click += Btn_Geocode_Click;

            iniFile = new IniFile();
            imgFile = new();

            //MapApi = new NaverMapApi();
            staticMap = new();
            staticMap.SetKey(
                iniFile.Read("StaticMapApi", "Client_ID"),
                iniFile.Read("StaticMapApi", "Client_Secret")
                  );

            geocode = new();
            geocode.SetKey(
                iniFile.Read("StaticMapApi", "Client_ID"),
                iniFile.Read("StaticMapApi", "Client_Secret")
                  );

            
        }

        private void Btn_Geocode_Click(object sender, RoutedEventArgs e)
        {
            geocode.query = TextBox_Address.Text;//"종로구 청와대로 1";

            bool check = geocode.SetUrl(out string msg);
            if (!check)
            {
                
            }
            else
            {
                if(geocode.Request())
                {
                    staticMap.Center = new StaticMap.CENTER(geocode.rootObjects.addresses[0].x, geocode.rootObjects.addresses[0].y);
                    staticMap.Size = new StaticMap.SIZE("800", "600");
                    staticMap.Level = 16;


                    Marker marker = new Marker();
                    marker.type = Marker.TYPE.t;
                    marker.size = Marker.SIZE.mid;
                    marker.color = Marker.COLOR.Blue;
                    marker.pos = geocode.rootObjects.addresses[0].x +" " + geocode.rootObjects.addresses[0].y;
                    marker.label = geocode.rootObjects.addresses[0].jibunAddress;
                    staticMap.AddMarker(marker);

                    check = staticMap.SetUrl(out string msg2);
                    if (!check)
                    {
                        MessageBox.Show(msg2);
                    }
                    else
                    {
                        if (staticMap.Request())
                        {
                            string FileName = "geocode";
                            staticMap.ResponseToFile(FileName);
                            string Path = System.Reflection.Assembly.GetExecutingAssembly().Location;

                            if (staticMap.Format == StaticMap.FORMAT.jpg)
                            {
                                FileName = FileName + ".jpg";
                            }
                            else
                            {
                                FileName = FileName + ".png";
                            }

                            Path = System.IO.Path.GetDirectoryName(Path) + "\\" + FileName;


                            Map_image.Source = imgFile.LoadImage(Path);
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        private void Btn_StaticMapRequest_Click(object sender, RoutedEventArgs e)
        {
            //staticMap.Center = new StaticMap.CENTER("127.1054221", "37.3591614");
            //staticMap.Size = new StaticMap.SIZE("800", "600");
            //staticMap.Level = 10;

            //staticMap.Static_Map_Sample_1();
            //staticMap.Static_Map_Sample_2();
            staticMap.Static_Map_Sample_1();
            bool check = staticMap.SetUrl(out string msg);

            if (!check)
            {
                MessageBox.Show(msg);
            }
            else
            {
                if (staticMap.Request())
                {
                    string FileName = "test";
                    staticMap.ResponseToFile(FileName);
                    string Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    
                    if (staticMap.Format == StaticMap.FORMAT.jpg)
                    {
                        FileName = FileName + ".jpg";
                    }
                    else
                    {
                        FileName = FileName + ".png";
                    }

                    Path = System.IO.Path.GetDirectoryName(Path) + "\\" + FileName;


                    Map_image.Source = imgFile.LoadImage(Path);
                }
                else
                {

                }
            }
        }
    }
}
