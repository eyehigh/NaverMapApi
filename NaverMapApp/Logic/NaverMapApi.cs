using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NaverMapApp.Logic
{
    internal class NaverMapApi
    {
        

        /// <summary>
        /// DataVersion API 연동 필요성
        /// StaticMap API 는 이미지 캐싱을 사용합니다.
        /// 클라이언트에서 동일 URL 요청 시 캐싱으로 인해, 
        /// 원본 서버의 배경 타일 업데이트가 반영 안 되는 문제가 발생 할 수 있습니다.
        /// 이를 해결하기 위한 방법으로 클라이언트측에서 StaticMap API 호출 시
        /// DataVersion API 에서 내려주는 버전 파라미터값을 붙여서 호출을 권장합니다.
        /// 배경 타일에 대한 최신성 이슈가 없을 경우 DataVersion API 와 연동할 필요가 없습니다.
        /// </summary>
        #region DataVersion API
        public class DataVersion
        {
            public string? Major_Verion;
            public string? Minor_Verion;
            public int interval;
        }
        #endregion DataVersion API
        public class StaticMap
        {
            public const string Url_ID_KEY = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster";
            public const string Url_HTTP_REFERER = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster-cors";

            private const string Header_Client_ID = "X-NCP-APIGW-API-KEY-ID";
            private string Client_ID = "";

            private const string Header_Client_Secret = "X-NCP-APIGW-API-KEY";
            private string Client_Secret = "";

            public bool SetKey(string p_Client_ID, string p_Client_Secret)
            {
                Client_ID = p_Client_ID;
                Client_Secret = p_Client_Secret;

                return true;
            }
            private WebResponse? response = null;

            #region Param
            public string? Url;
            /// <summary>
            /// 필수여부 : N,
            /// 좌표 체계를 나타냄,
            ///값을 생략할 경우 WGS84 경위도 좌표 체계(EPSG:4326)로 인식
            /// </summary>
            public CRS Crs;
            public enum CRS
            {

                //지원하는 좌표 체계:
                WGS84,  // EPSG:4326
                UTMK,   //  NHN:2048
                TM128,  //  NHN:128
                GRS80,  //  EPSG:4258
                Bessel,  //  EPSG:4162 
                KoreaEastBelt, // EPSG:2096
                KoreaCentralBelt, // EPSG:3857
                KoreaWestBelt, // EPSG:2098
                GoogleMaps, // EPSG:3857 or EPSG:900913
                Korea2000 // EPSG:5179
            }
            /// <summary>
            /// 필수여부 : Y or N
            /// 중심 좌표이며 ‘center=X 좌표,Y 좌표’ 형식으로 입력
            /// X 좌표, Y 좌표 순서는 좌표 체계 정의를 따름
            /// 예를 들어, WGS84 경위도 좌표 체계인 경우 입력 형식은<경도, 위도> 순서
            /// markers 파라미터 설정 시 생략 가능
            /// </summary>
            public CENTER? Center;
            public class CENTER
            {

                public string X;
                public string Y;
                public CENTER(string x, string y)
                {
                    X = x;
                    Y = y;
                }
            }
            /// <summary>
            /// 필수 여부 : Y or N,
            /// 줌 레벨이며 markers 파라미터 설정 시 생략 가능,
            /// 입력 범위: 0~20
            /// </summary>
            public int Level;

            /// <summary>
            /// 필수 여부 : Y
            /// 가로, 세로 이미지 크기이며 'w=크기&h=크기' 형식으로 입력,
            /// 최소 1~1024 픽셀 지원
            /// </summary>
            public SIZE? Size;
            public class SIZE
            {
                public string W;
                public string H;
                public SIZE(string w, string h)
                {
                    W = w;
                    H = h;
                }
            }
            /// <summary>
            /// 필수 여부 : N
            /// 지도 유형이며 입력 가능한 값은 다음과 같음
            ///- basic: 일반(기본값)
            ///- traffic: 교통 지도
            ///- satellite: 위성
            ///- satellite_base: 위성 배경
            ///- terrain: 지형도
            /// </summary>
            public MAPTYPE MapType;
            public enum MAPTYPE
            {
                basic, //일반(기본값)
                traffic, // 교통지도
                satellite, //위성
                satellite_base, //위성 배경
                terrain //지형도
            }
            /// <summary>
            /// 필수 여부 : N
            /// 반환 이미지 형식이며 입력 가능한 값은 다음과 같음
            /// - jpg 또는 jpeg(기본값) : 압축 품질 85%, 24 비트
            /// - png8: 8 비트
            /// - png: 24 비트
            /// </summary>
            public FORMAT Format;
            public enum FORMAT
            {
                jpg,   //  jpg 또는 jpeg(기본값): 압축 품질 85%, 24 비트
                png8, // 8비트
                png   // 24비트
            }
            /// <summary>
            /// 필수 여부 : N
            /// 고해상도 디스플레이 지원을 위한 옵션이며 입력 가능한 값은 다음과 같음
            /// - 1: 저해상도(기본값)
            /// - 2: 고해상도
            /// </summary>
            public int Scale;

            /// <summary>
            /// 필수 여부 : N
            ///  마커 종류별 스타일 및 위치 지정
            /// </summary>
            public List<Marker>? Markers;
            public class Marker
            {
                public TYPE type = TYPE.d; //마커유형
                                          //필수여부 : N, 기본값 : d
                                          // d(default), n(number), a(alphabet), t(tooltip)
                                          //설정 예) type:d 또는 type:n
                public enum TYPE
                {
                    d,
                    n,
                    a,
                    t,
                    e
                }
                public SIZE size = SIZE.mid; //마커 크기
                                            //필수여부 : N, 기본값 : mid
                                            //tiny, small, mid
                                            //tiny일 경우 label 표현은 생략됨
                                            //설정 예) size:tiny 또는 size:small
                public enum SIZE
                {
                    tiny,
                    small,
                    mid,
                }
                public COLOR color = COLOR.Default; //색상
                                                    //필수여부 : N, 기본값 : 없음
                                                    //생략하면 기본 색상값인 0x08da76으로 표시
                                                    //사전 정의 색상은 대표적으로 사용하는 색상을 쉽게 사용할 수 있도록 문자열로 정의한 값이며, 사전 정의 색상은 아래와 같음
                                                    //- Default: 0x08DA76   //- Blue: 0x029DFF  //- Orange: 0xFE8C52
                                                    //- Yellow: 0xFFBA01    //- Red: 0xFF6355    //- Brown: 0xA4885B
                                                    //- Green: 0x63AA41     //- Purple: 0xD182C8    //- Gray: 0x666666
                                                    //설정 예) color:green 또는 color:0x00FF00
                public string custom_color;
                public enum COLOR
                {
                    Default,
                    Blue,
                    Orange,
                    Yellow,
                    Red,
                    Brown,
                    Green,
                    Purple,
                    Gray,
                    Custom
                }
                public string? label;  //필수여부 : N, 기본값 : 없음
                                       // A-Z, 0-9                    
                                       // 생략하면 마커만 표시
                                       // 설정 예) label:A, label:9
                public string? pos; //마커가 표시될 위치이며 여러개 입력가능
                                    //필수여부 : Y, 기본값 : 없음
                                    // 설정 예) pos:127.15(공백)38.15,126.12(공백)37.523
                public string viewSizeRatio ="1.0"; //마커 유형(type)과 크기(size)별 기본 디자인 기반으로 마커의 크기 조절
                                                    // -소수점 1자리만 지원 // - 0.1보다 작으면 0.1, 2.0보다 크면 2.0으로 설정
                                                    // 필수여부 : N, 기본값 : 1.0
                                                    // 설정 예) viewSizeRatio:2.0

                //------------------ type:e ---------------------//
                public string? icon;    // URL 경로이며 png, svg 타입 지원
                                        //필수여부 : Y, 기본값 : 없음
                                        // 설정 예) icon:http://aaa/bbb.svg    icon:http://aaa/bbb.png
                public string? anchor = "bottom";  //지도 이미지에서 마커 이미지 위치의 오프셋값이며, 마커 표시 위치를 세부적으로 조정해야 할 때 설정
                                        //double 또는 text 타입으로 설정 가능
                                        //double 타입으로 입력할 경우 소수점 이하 두 자리(0.00~1.00)까지 표현 가능하며,
                                        //xOffset, yOffset 순서로 입력(예: 왼쪽 위는 0.0,0.0, 오른쪽 아래는 1.0,1.0)
                                        //text 타입으로 입력할 경우 입력 가능한 값은 다음과 같음
                                        //- top: 0.5, 0.0
                                        //- bottom: 0.5, 1.0(기본값)
                                        //- left: 0.0, 0.5
                                        //- right: 1.0, 0.5
                                        //- center: 0.5, 0.5
                                        //- topleft: 0.0, 0.0
                                        //- topright: 0.0, 1.0
                                        //- bottomleft: 1.0, 0.0
                                        //- bottomright: 1.0, 1.0
                                        //예를 들어, 핀 모양의 아이콘일 경우 보통 이미지의 가운데 맨 아래쪽을 위치로 표시하므로,
                                        //double형으로 입력할 때는 double:0.5,1.0으로 입력하고, text형으로 입력할 때는 text:bottom으로 입력
                                        // 설정 예) anchor:0.5,0.0 또는 anchor:top

            }

            /// <summary>
            /// 라벨 언어 설정이며 입력 가능한 값은 다음과 같음
            /// - ko: 한글(기본값)
            /// - en: 영어
            /// - ja: 일본어
            /// - zh: 중국어
            /// </summary>
            public LANG Lang;
            public enum LANG
            {
                ko,
                en,
                ja,
                zh
            }
            /// <summary>
            /// 필수 여부 : N
            /// public_transit 파라미터 전달시 대중교통 정보를 노출
            /// </summary>
            public bool Public_Transit;

            /// <summary>
            /// 필수 여부 : Y
            /// 서비스에서 사용할 dataversion 파라미터 전달 (CDN 캐시 무효화)
            /// </summary>
            public string? DataVersion;

            #endregion Param

            public StaticMap()
            {
                Url = null;
                Crs = CRS.WGS84;
                Center = null;
                Level = -1;
                Size = null;
                MapType = MAPTYPE.basic;
                Format = FORMAT.jpg;
                Scale = 1;
                Markers = new List<Marker>();
                Lang = LANG.ko;
                Public_Transit = false;
                DataVersion = null;
            }
            public bool SetUrl(out string msg)
            {
                int param_count = 0;
                StringBuilder sb = new();
                sb.Append(Url_ID_KEY);
                sb.Append('?');
                if (Crs != CRS.WGS84)
                {
                    sb.Append("crs=");
                    string? temp = null;
                    if (Crs == CRS.UTMK)
                        temp = "NHN:2048";
                    else if (Crs == CRS.TM128)
                        temp = "NHN:128";
                    else if (Crs == CRS.GRS80)
                        temp = "EPSG:4258";
                    else if (Crs == CRS.Bessel)
                        temp = "EPSG:4162";
                    else if (Crs == CRS.KoreaEastBelt)
                        temp = "EPSG:2096";
                    else if (Crs == CRS.KoreaCentralBelt)
                        temp = "EPSG:2097";
                    else if (Crs == CRS.KoreaWestBelt)
                        temp = "EPSG:2098";
                    else if (Crs == CRS.GoogleMaps)
                        temp = "EPSG:3857";
                    else if (Crs == CRS.Korea2000)
                        temp = "EPSG:5179";

                    sb.Append(temp);
                    param_count++;
                }
                if (Center != null)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("center=");
                    sb.Append(Center.X);
                    sb.Append(",");
                    sb.Append(Center.Y);
                    param_count++;

                }
                else if (Center == null && Markers.Count == 0)
                {
                    msg = "ERROR : Center 누락(Center 설정 또는 Marker 설정 필요)";
                    return false;
                }
                if (Level >= 0 && Level <= 20)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("level=");
                    sb.Append(Level);
                    param_count++;
                }
                else
                {
                    if (Markers.Count == 0)
                    {
                        msg = "ERROR : Level 누락";
                        return false;
                    }
                }
                if (Size != null)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("w=");
                    sb.Append(Size.W);
                    sb.Append('&');
                    sb.Append("h=");
                    sb.Append(Size.H);
                    param_count++;
                }
                else
                {
                    msg = "ERROR : Size(w, h) 누락";
                    return false;
                }
                if (MapType != MAPTYPE.basic)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("maptype=");
                    sb.Append(Enum.GetName(typeof(MAPTYPE), MapType));
                    param_count++;
                }
                if (Format != FORMAT.jpg)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("format=png");
                    sb.Append(Enum.GetName(typeof(FORMAT), Format));
                    param_count++;
                }
                if (Scale == 2)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("scale=");
                    sb.Append(Scale);
                    param_count++;
                }
                if (Markers.Count > 0)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append(GetMarkersString());
                    param_count++;
                }
                if (Lang != LANG.ko)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("lang=");
                    sb.Append(Enum.GetName(typeof(LANG), Lang));
                    param_count++;
                }
                if (Public_Transit == true)
                {
                    if (param_count > 0)
                        sb.Append('&');
                    sb.Append("public_transit");
                    param_count++;
                }
                if (DataVersion != null)
                {

                }
                Url = sb.ToString();
                msg = "OK";
                return true;
            }
            public string GetMarkersString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i=0; i< Markers.Count; i++)
                {
                    if(i !=0)
                        sb.Append("&");
                    

                    if (Markers[i].type != Marker.TYPE.e)
                    {
                        sb.Append("markers=");
                        sb.Append("type:");
                        sb.Append(Enum.GetName(typeof(Marker.TYPE), Markers[i].type));

                        sb.Append("|");
                        sb.Append("size:");
                        sb.Append(Enum.GetName(typeof(Marker.SIZE), Markers[i].size));

                        sb.Append("|");
                        sb.Append("color:");
                        if (Markers[i].color != Marker.COLOR.Custom)
                        {
                            sb.Append(Enum.GetName(typeof(Marker.COLOR), Markers[i].color));
                        }
                        else
                        {
                            sb.Append(Markers[i].custom_color);
                        }


                        if (Markers[i].label != null)
                        {
                            sb.Append("|");
                            sb.Append("label:");
                            sb.Append(Markers[i].label);
                        }

                        sb.Append("|");
                        sb.Append("pos:");
                        sb.Append(Markers[i].pos);

                        sb.Append("|");
                        sb.Append("viewSizeRatio:");
                        sb.Append(Markers[i].viewSizeRatio);
                    }
                    else
                    {
                        sb.Append("markers=");
                        sb.Append("type:");
                        sb.Append(Enum.GetName(typeof(Marker.TYPE), Markers[i].type));

                        sb.Append("|");
                        sb.Append("icon:");
                        sb.Append(Markers[i].icon);

                        sb.Append("|");
                        sb.Append("anchor:");
                        sb.Append(Markers[i].anchor);

                        sb.Append("|");
                        sb.Append("pos:");
                        sb.Append(Markers[i].pos);
                    }
                }
                return sb.ToString();
            }
            public void AddMarker(Marker marker)
            {
                Markers.Add(marker);
            }
            public bool Request()
            {
                if (Url == null)
                    return false;

                WebRequest request = WebRequest.Create(Url);
                request.Headers.Add(Header_Client_ID, Client_ID);
                request.Headers.Add(Header_Client_Secret, Client_Secret);

                response = request.GetResponse();
                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
                Debug.WriteLine(((HttpWebResponse)response).StatusCode);

                if(((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
            public bool ResponseToFile(string FileName)
            {
                if(response == null)
                {
                    return false;
                }
                string FileFullName = "";
                if (Format == FORMAT.jpg)
                {
                    FileFullName = FileName + ".jpg";
                }
                else
                {
                    FileFullName = FileName + ".png";
                }

                byte[]? downloadData = StreamToByte();

                if (downloadData == null)
                    return false;

                using (FileStream FS = new(FileFullName, FileMode.Create))
                {
                    FS.Write(downloadData, 0, downloadData.Length);
                }
                response.Close();

                return true;
            }
            public byte[]? StreamToByte()
            {
                if (response == null)
                    return null;

                Stream stream = response.GetResponseStream();

                // 총 사이즈
                int dataLength = (int)response.ContentLength;
                Debug.WriteLine("data Length : " + dataLength);

                byte[] buffer = new byte[1024];

                // 메모리로 다운로드 작업
                MemoryStream memoryStream = new MemoryStream();
                while (true)
                {
                    int byteRead = stream.Read(buffer, 0, buffer.Length);

                    if (byteRead == 0)
                        break;
                    else
                        memoryStream.Write(buffer, 0, byteRead);
                }

                return memoryStream.ToArray();
            }


            #region SAMPLE
            public void TestFunc()
            {
                // Create a request for the URL.
                WebRequest request = WebRequest.Create("https://naveropenapi.apigw.ntruss.com/map-static/v2/raster?w=300&h=300&center=127.1054221,37.3591614&level=16");
                request.Headers.Add(Header_Client_ID, Client_ID);
                request.Headers.Add(Header_Client_Secret, Client_Secret);
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;

                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                    using (FileStream lxFS = new FileStream("TEST.jpg", FileMode.Create))
                    {
                        lxFS.Write(lnByte, 0, lnByte.Length);
                    }
                }
                /*
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    Console.WriteLine(responseFromServer);
                }*/

                // Close the response.
                response.Close();
            }

            public void Static_Map_Sample_1()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
            }
            public void Static_Map_Sample_2()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 11;
                MapType = MAPTYPE.basic; //일반 지도 요청, 생략가능
            }
            public void Static_Map_Sample_3()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 11;
                MapType = MAPTYPE.traffic; // 교통 정보 지도 요청
            }
            public void Static_Map_Sample_4()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 11;
                MapType = MAPTYPE.satellite; //위성 지도 요청
            }
            public void Static_Map_Sample_5()
            {
                
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 11;
                MapType = MAPTYPE.satellite_base; //위성 배경 지도 요청
            }
            public void Static_Map_Sample_6()
            {

                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
                Format = FORMAT.jpg; //jpeg(jpg) 이미지 형식 요청
            }
            public void Static_Map_Sample_7()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
                Format = FORMAT.png8; //png8 이미지 형식 요청
            }
            public void Static_Map_Sample_8()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
                Format = FORMAT.png; //png 이미지 형식 요청
            }
            public void Static_Map_Sample_9()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
                Scale = 1; //저해상도용 이미지 요청.
                           //요청한 w, h 크기(픽셀 단위)의 이미지를 256 x 256 타일 기반으로 생성해서 반환합니다.
                           //이 값이 기본값이며, 생략할 수 있습니다.
            }
            public void Static_Map_Sample_10()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("127.1054221", "37.3591614");
                Level = 16;
                Scale = 2; //고해상도용 이미지 요청. 
                           //요청한 w, h 크기의 이미지를 512 x 512 타일 기반으로 생성해서 반환합니다. 
                           //즉, scale=1과 동일한 지도 서비스 지역이 반환되지만 각 크기에 포함되는 픽셀은 2배로 늘어납니다.
                           //예들 들어, 320 x 320이 요청되면 640 x 640 이미지를 반환합니다.
            }
            public void Static_Map_Sample_11()
            {
                Crs = CRS.WGS84; // EPSG:4326, 기본값 생략가능
                Size = new SIZE("375", "258");
                Center = new CENTER("126.96311062857343", "37.50843783043817");
                Level = 16;
                Lang = LANG.ko; //생략가능
                                //LANG.en;
                                //LANG.jp;
                                //LANG.zh;
            }
            public void Static_Map_Sample_12()
            {
                Size = new SIZE("300", "300");
                Center = new CENTER("126.96311062857343", "37.50843783043817");
                Level = 16;
                Public_Transit = true;
            }
            public void Static_Map_Sample_13()
            {
                Size = new SIZE("300", "300");
                Marker marker = new Marker();
                marker.type = Marker.TYPE.d;
                marker.size = Marker.SIZE.tiny;
                marker.pos = "127.1054221 37.3591614";
                AddMarker(marker);
            }
            public void Static_Map_Sample_14()
            {
                Size = new SIZE("300", "300");
                Marker marker = new Marker();
                marker.type = Marker.TYPE.d;
                marker.size = Marker.SIZE.mid;
                marker.pos = "126.9865479 37.5612557";
                AddMarker(marker);

                marker = new Marker();
                marker.type = Marker.TYPE.d;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Red;
                marker.pos = "126.9870479 37.5695075,126.9950680 37.5612557,126.9743160 37.5620754";
                AddMarker(marker);

                marker = new Marker();
                marker.type = Marker.TYPE.d;
                marker.size = Marker.SIZE.tiny;
                marker.color = Marker.COLOR.Green;
                marker.pos = "126.9810479 37.5695075,126.9950680 37.5672557,126.9843160 37.5570754";
                AddMarker(marker);
            }
            public void Static_Map_Sample_15()
            {
                Size = new SIZE("300", "300");
                Marker marker = new Marker();
                marker.type = Marker.TYPE.d;
                marker.size = Marker.SIZE.mid;
                marker.pos = "127.1054221 37.3591614";
                marker.viewSizeRatio = "2.0";
                AddMarker(marker);
            }
            public void Static_Map_Sample_16()
            {
                
                Size = new SIZE("300", "300");

                //markers=type:n|size:mid|pos:126.9865479%2037.5612557|label:1
                Marker marker = new Marker();
                marker.type = Marker.TYPE.n;
                marker.size = Marker.SIZE.mid;
                marker.pos = "126.9865479 37.5612557";
                marker.label = "1";
                AddMarker(marker);

                //markers = type:n | size:small | color:blue | pos:126.9870479 % 2037.5695075 | label:2
                marker = new Marker();
                marker.type = Marker.TYPE.n;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9870479 37.5695075";
                marker.label = "2";
                AddMarker(marker);

                //markers=type:n|size:small|color:blue|pos:126.9950680%2037.5612557|label:3
                marker = new Marker();
                marker.type = Marker.TYPE.n;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9950680 37.5612557";
                marker.label = "3";
                AddMarker(marker);

                //markers=type:n|size:small|color:blue|pos:126.9743160%2037.5620754|label:4
                marker = new Marker();
                marker.type = Marker.TYPE.n;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9743160 37.5620754";
                marker.label = "4";
                AddMarker(marker);
            }
            public void Static_Map_Sample_17()
            {

                Size = new SIZE("300", "300");

                //markers = type:a | size:mid | pos:126.9865479 % 2037.5612557 | label:a 
                Marker marker = new Marker();
                marker.type = Marker.TYPE.a;
                marker.size = Marker.SIZE.mid;
                marker.pos = "126.9865479 37.5612557";
                marker.label = "a";
                AddMarker(marker);

                //markers = type:a | size:small | color:blue | pos:126.9870479 % 2037.5695075 | label:b
                marker = new Marker();
                marker.type = Marker.TYPE.a;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9870479 37.5695075";
                marker.label = "b";
                AddMarker(marker);

                //markers = type:a | size:small | color:blue | pos:126.9950680 % 2037.5612557 | label:c
                marker = new Marker();
                marker.type = Marker.TYPE.a;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9950680 37.5612557";
                marker.label = "c";
                AddMarker(marker);

                //markers = type:a | size:small | color:blue | pos:126.9743160 % 2037.5620754 | label:d
                marker = new Marker();
                marker.type = Marker.TYPE.a;
                marker.size = Marker.SIZE.small;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.9743160 37.5620754";
                marker.label = "d";
                AddMarker(marker);
            }
            public void Static_Map_Sample_18()
            {
                //crs=EPSG:4326&scale=1&format=png&w=375&h=258
                Crs = CRS.WGS84;
                Scale = 1;
                Format = FORMAT.png;
                Size = new SIZE("375", "258");

                //markers=type:t|pos:126.9616187%2037.507435|label:%EB%8F%99%EC%9E%91%EA%B5%AC,%20%EC%84%9C%EC%B4%88%EA%B5%AC,%20%EA%B4%80%EC%95%85%EA%B5%AC
                Marker marker = new Marker();
                marker.type = Marker.TYPE.t;
                marker.pos = "126.9616187 37.507435";
                marker.label = "동작구, 서초구, 관악구";
                AddMarker(marker);

                //markers=type:t|color:blue|pos:126.96060539999999%2037.507685699999996|label:%EB%8F%99%EC%9E%91%EA%B5%AC,%20%EC%84%9C%EC%B4%88%EA%B5%AC,%20%EC%9A%A9%EC%82%B0%EA%B5%AC%20%EB%B0%A9%EB%A9%B4
                marker = new Marker();
                marker.type = Marker.TYPE.t;
                marker.color = Marker.COLOR.Blue;
                marker.pos = "126.96060539999999 37.507685699999996";
                marker.label = "동작구, 서초구, 용산구 방면";
                AddMarker(marker);

                //markers=type:t|color:0xEE3A3A|pos:126.9616377%2037.506708950000004|label:%EB%8F%99%EC%9E%91%EA%B5%AC,%20%EC%84%9C%EC%B4%88%EA%B5%AC,%20%EC%9A%A9%EC%82%B0%EA%B5%AC%20%EB%B0%A9%EB%A9%B4
                marker = new Marker();
                marker.type = Marker.TYPE.t;
                marker.color = Marker.COLOR.Custom;
                marker.custom_color = "0xEE3A3A";
                marker.pos = "126.9616377 37.506708950000004";
                marker.label = "동작구, 서초구, 용산구 방면";
                AddMarker(marker);
            }
            public void Static_Map_Sample_19()
            {
                //w=300&h=300&scale=2 
                Size = new SIZE("300", "300");
                Scale = 2;

                //markers=type:e|anchor:center|icon:https://aaa.bbb.com/icon/construction-medium@2x.png|pos:127.0597827%2037.5118871
                Marker marker = new Marker();
                marker.type = Marker.TYPE.e;
                marker.anchor = "center";
                marker.icon = "https://aaa.bbb.com/icon/construction-medium@2x.png";
                marker.pos = "127.0597827 37.5118871";
                AddMarker(marker);
            }
            public void Static_Map_Sample_20()
            {
                //w=300&h=300&scale=2 
                Size = new SIZE("300", "300");
                Scale = 2;

                //markers=type:e|anchor:center|icon:https://aaa.bbb.com/icon/construction-medium@2x.png|pos:127.0597827%2037.5118871
                Marker marker = new Marker();
                marker.type = Marker.TYPE.t;
                marker.color = Marker.COLOR.Custom;
                marker.custom_color = "0xEE3A3A";
                marker.pos = "14124108.6000623 4497680.4883394";
                AddMarker(marker);
            }
            #endregion SAMPLE
        }


        public class Geocode
        {
            public const string Url_ID_KEY = "https://naveropenapi.apigw.ntruss.com/map-geocode/v2/geocode";
            //public const string Url_HTTP_REFERER = "https://naveropenapi.apigw.ntruss.com/map-static/v2/raster-cors";

            private const string Header_Client_ID = "X-NCP-APIGW-API-KEY-ID";
            private string Client_ID = "";

            private const string Header_Client_Secret = "X-NCP-APIGW-API-KEY";
            private string Client_Secret = "";

            public string? Url;
            private WebResponse? response = null;

            public string? query; // 주소
                                  // 필수여부 : Y
            public string? coordinate; //검색 중심 좌표 // 필수여부 : Y
                                       // 'lon,lat' 형식으로 입력
            public string? filter; // 검색 결과 필터링 조건 // 필수여부 : N
                                   //'필터 타입@코드1;코드2;... ' 형식으로 입력
                                   // 제공하는 필터 타입은 다음과 같음:
                                   // HCODE: 행정동 코드
                                   // BCODE: 법정동 코드
                                   // 예) HCODE@4113554500;4113555000
            public int page = 1;  //페이지 번호 // 필수여부 : N
                                  // 기본값은 1
            public int count = 10; // 결과 목록 크기 // 필수여부 : N
                                   //입력 범위 : 1~100
                                   //기본값 : 10
            /*
            public class Root
            {
                [JsonInclude]
                public string status;
                [JsonInclude]
                public string meta;
                [JsonInclude]
                public List<Address> Addresses;
            }
            public class Address
            {
                [JsonInclude]
                public string roadAddress;
                [JsonInclude]
                public string englishAddress;
                [JsonInclude]
                public string x;
                [JsonInclude]
                public string y;
                [JsonInclude]
                public double distance;

                [JsonInclude]
                public List<AddressElement> AddressElements;
            }
            public class AddressElement
            {
                [JsonInclude]
                public string types;
                [JsonInclude]
                public string longName;
                [JsonInclude]
                public string shortName;
                [JsonInclude]
                public string code;
            }*/

                public bool SetKey(string p_Client_ID, string p_Client_Secret)
            {
                Client_ID = p_Client_ID;
                Client_Secret = p_Client_Secret;

                return true;
            }
            public bool SetUrl(out string msg)
            {
                msg = "";
                return false;
            }
            public bool Request()
            {
                //if (Url == null)
                //    return false;

                WebRequest request = WebRequest.Create(Url_ID_KEY + "?query=분당구 불정로 6");
                //request.
                request.Headers.Add(Header_Client_ID, Client_ID);
                request.Headers.Add(Header_Client_Secret, Client_Secret);

                response = request.GetResponse();
                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
                Debug.WriteLine(((HttpWebResponse)response).StatusCode);

                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                {
                    ResponseToJson();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
            public void ResponseToJson()
            {
                //if (response == null)
                //    return null;

                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    //string result = streamReader.ReadToEnd();
                    using(JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                    {
                        JObject json = (JObject)JToken.ReadFrom(jsonTextReader);

                        Debug.WriteLine(json.ToString());
                    }
                }
                    
                
               // Debug.WriteLine(result);
            }
        }


        public class CommonError
        {
            public int HttpStatusCode;
            public int ErrorCode;
            public string? ErrorMessage;
            public string? Description;



            public CommonError GetError(int HttpStatusCode, int ErrorCode)
            {
                CommonError error = new CommonError();

                return error;
            }
        }
    }
}
