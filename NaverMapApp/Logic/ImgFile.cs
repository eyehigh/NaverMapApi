using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NaverMapApp.Logic
{
    internal class ImgFile
    {
        public BitmapImage LoadImage(string Path)
        {
            //Image myImage = new Image();
            //myImage.Width = 200;

            BitmapImage myBitmapImage = new BitmapImage();

            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Path);

            //myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();

            return myBitmapImage;
        }
    }
}
