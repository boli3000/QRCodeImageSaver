using DotLiquid.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using ZXing;
using ZXing.Client.Result;
using ZXing.Common;
using ZXing.PDF417.Internal;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace QRImageSaver
{
    class Program
    {
        static BarcodeReader barcodeReader;
        static string ConfigKey_MAIN_FOLDER = "MAIN_FOLDER";
        static string ConfigKey_TEST_FOLDER = "TEST_FOLDER";

        static string SrcPath = String.Empty;
        static string TestPath = String.Empty;

        static string[] images;


        static bool initBarcodeReader()
        {

            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new DecodingOptions { TryHarder = true }
            };
            if (barcodeReader == null)
            {
                Console.WriteLine("Barcode Reader init fail");
                return true;
            }
            else
            {
                Console.WriteLine("Barcode Reader ready");
                return false;
            }


        }
        static void Main(string[] args)
        {
            if (initBarcodeReader())
                return;
            if (readConfig())
                return;
            else
                Console.WriteLine("Config Ok");

            Console.WriteLine("Image source: " + SrcPath);
            Console.WriteLine("Test folder: " + TestPath);

            if(getImages(SrcPath))
                return;
            foreach (string imagefile in images) 
            {
                Console.WriteLine("----------------------");
                string QRCode = String.Empty;
                var bitmap = (Bitmap)Bitmap.FromFile(imagefile);
                bitmap.Tag = Path.GetFileName(imagefile);

                QRCode = Decode(bitmap);
                if(QRCode != String.Empty)
                {
                    HandleFile(imagefile, QRCode, bitmap);
                }
                Console.WriteLine("----------------------");
            }
            Console.Write("Press <Enter> to exit... ");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }
        static string Decode(Bitmap bitmap)
        {
            Console.WriteLine("Decode Image: " + bitmap.Tag);
            var result = barcodeReader.Decode(bitmap);
            if (result == null)
            {
                Console.WriteLine("No barcode recognized");
                return String.Empty;
            }
            else
            {
                Console.WriteLine("QR Code found " + result.Text);
            }
            return result.Text;
            
        }
        static bool readConfig()
        {
            Console.WriteLine("Read Config file..");
            string line;
            StreamReader file = new StreamReader("QRImageSaverConfig.config");
            bool ret = true;

            if (file == null)
            {
                Console.WriteLine(" Config file not found");
                ret = true;
            }
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains(ConfigKey_MAIN_FOLDER))
                {
                    String[] pathes = line.Split("\t");
                    SrcPath = pathes[1];
                    SrcPath = SrcPath.Trim();
                    ret = false;
                }
                if (line.Contains(ConfigKey_TEST_FOLDER))
                {
                    String[] pathes = line.Split("\t");
                    TestPath = pathes[1];
                    TestPath = TestPath.Trim();
                    ret = false;
                }
            }
            if (SrcPath == String.Empty || TestPath == String.Empty)
            {
                Console.WriteLine("Error get main folder from config file");
                ret = true;
            }
            else
            {
                if (!Directory.Exists(SrcPath))
                {
                    Console.WriteLine(SrcPath + " Error path not found");
                    ret = true;
                }
                if (!Directory.Exists(TestPath))
                {
                    Console.WriteLine(TestPath + " Error path not found");
                    ret = true;
                }
            }
            file.Close();
            return ret;

        }
        static bool getImages(string FolderPath) 
        {
            bool ret = true;
            images = Directory.GetFiles(FolderPath,"*.jpg");
            if (images.Length == 0)
            {
                Console.WriteLine("No Images Found");
                ret = true;
            }
            else
                ret = false;
            return ret;
        }
        static bool HandleFile(string imagefilepath,string QRCode, Bitmap bitmap)
        {
            string Test_Type = QRCode.Substring(QRCode.Length - 2);
            string Test_Folder = QRCode.Remove(QRCode.Length - 3);
            Console.WriteLine("Test body: "+Test_Folder + " Test Side: "+ Test_Type);

            string Cmplt_Test_Folder = TestPath + Test_Folder + "\\";
            if (!Directory.Exists(Cmplt_Test_Folder))
            {
                Directory.CreateDirectory(Cmplt_Test_Folder);
            }

            string Cmplt_Test_Folder_Type = Cmplt_Test_Folder + Test_Type + "\\";

            if (!Directory.Exists(Cmplt_Test_Folder_Type))
            {
                Directory.CreateDirectory(Cmplt_Test_Folder_Type);
            }

            string NewfileName = File.GetCreationTime(imagefilepath).ToString("yyyy-MM-dd")+".jpg";
            
            string NewFileSrc = Cmplt_Test_Folder_Type + NewfileName;
            
            if (!File.Exists(NewFileSrc))
            {
                Console.WriteLine("Copy Image to new file: " + NewFileSrc);
                File.Copy(imagefilepath, NewFileSrc);
            }
            else
                Console.WriteLine("!!!Error " + NewFileSrc+ " allready exists");

            bool ret = true;
            return ret;
        }
        public static DateTime GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                PropertyItem propItem = null;
                try
                {
                    propItem = myImage.GetPropertyItem(36867);
                }
                catch { }
                if (propItem != null)
                {
                    string dateTaken = "x";
                    return DateTime.Parse(dateTaken);
                }
                else
                    return new FileInfo(path).LastWriteTime;
            }
        }
    }
}
