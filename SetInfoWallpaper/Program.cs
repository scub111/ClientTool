using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SetInfoWallpaper
{
    class Program
    {
        static void SaveLog(string text)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter stream = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + "Log.txt", true);

                //Write a line of text
                stream.WriteLine(string.Format("{0} - {1}", DateTime.Now, text));

                //Close the file
                stream.Close();
            }
            catch
            {
            }
        }

        static string GetLocalIPAddress()
        {
            string LocalIPAddress = "none";
            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            string ip;
            Regex ipReg = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            for (int i = 0; i < addr.Length; i++)
            {
                ip = addr[i].ToString();
                if (ipReg.IsMatch(ip))
                {
                    LocalIPAddress = ip;
                    break;
                }
            }
            return LocalIPAddress;
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        static string LoadWallpaparConfig(string path)
        {
            try
            {
                //Pass the filepath and filename to the StreamReader Constructor
                StreamReader stream = new StreamReader(path);

                //Read a line of text
                string result = stream.ReadLine();

                //Close the file
                stream.Close();

                return result;
            }
            catch
            {
                return "";
            }
        }

        static void CreateImage(string windowsVersion, out Bitmap bitmap,  out string extention)
        {
            bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            if (windowsVersion.Contains("Windows XP"))
                extention = ".bmp";
            else
                extention = ".jpg";
        }

        static void SaveImage(Bitmap bitmap, string pathDestination)
        {
            if (File.Exists(pathDestination))
                File.Delete(pathDestination);


            string extention = Path.GetExtension(pathDestination);


            if (extention != ".bmp")
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                Encoder myEncoder =  Encoder.Quality;
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bitmap.Save(pathDestination, jpgEncoder, myEncoderParameters);
            }
            else
            {
                bitmap.Save(pathDestination);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        static void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        static void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (brush == null)
                throw new ArgumentNullException("brush");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        static void SetInfoWallpaper()
        {
            try
            {
                WindowsIdentity wi = WindowsIdentity.GetCurrent();
                string localIPAddress = GetLocalIPAddress();
                SaveLog(string.Format("Started as {0} ip={1}.", wi.Name, localIPAddress));

                string localDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                string windowsVersion = LoadWallpaparConfig("SetInfoWallpaper.ini");
                if (string.IsNullOrEmpty(windowsVersion))
                    windowsVersion = "Windows 7";

                const string folderTemp = "temp";
                if (!Directory.Exists(string.Format(@"{0}\{1}", localDir, folderTemp)))
                    Directory.CreateDirectory(string.Format(@"{0}\{1}", localDir, folderTemp));

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                string path0 = (string)key.GetValue("Wallpaper");

                bool recreate = false;

                string fileName = Path.GetFileNameWithoutExtension(path0);

                if (fileName == "EditedWallpaper")
                {
                    recreate = true;
                    fileName = "OriginalWallpaper";
                }

                string extention = Path.GetExtension(path0);

                Bitmap btmWallpaper;
                if (!recreate)
                {
                    if (!string.IsNullOrEmpty(path0))
                    {
                        try
                        {
                            btmWallpaper = new Bitmap(path0);
                        }
                        catch
                        {
                            CreateImage(windowsVersion, out btmWallpaper, out extention);
                        }
                    }
                    else
                    {
                        CreateImage(windowsVersion, out btmWallpaper, out extention);
                    }

                    SaveImage(btmWallpaper, string.Format(@"{0}\{1}\OriginalWallpaper{2}", localDir, folderTemp, extention));
                }
                else
                {
                    try
                    {
                        btmWallpaper = new Bitmap(string.Format(@"{0}\{1}\{2}{3}", localDir, folderTemp, fileName, extention));
                    }
                    catch
                    {
                        CreateImage(windowsVersion, out btmWallpaper, out extention);
                        SaveImage(btmWallpaper, string.Format(@"{0}\{1}\OriginalWallpaper{2}", localDir, folderTemp, extention));
                    }
                }

                Graphics g = Graphics.FromImage(btmWallpaper);

                double xOffsetImage = 0;
                double yOffsetImage = 0;

                double screenRatio = Screen.PrimaryScreen.Bounds.Width / (double)Screen.PrimaryScreen.Bounds.Height;
                double imageRatio = btmWallpaper.Width / (double)btmWallpaper.Height;
                double diffRatio = screenRatio - imageRatio;

                double k = 1.0;

                if (diffRatio >= 0)
                {
                    yOffsetImage = btmWallpaper.Height - (btmWallpaper.Width / (double)Screen.PrimaryScreen.Bounds.Width) * (double)Screen.PrimaryScreen.Bounds.Height;
                    k = (btmWallpaper.Height - yOffsetImage) / (double)Screen.PrimaryScreen.Bounds.Height;
                }
                else
                {
                    xOffsetImage = btmWallpaper.Width - (btmWallpaper.Height / (double)Screen.PrimaryScreen.Bounds.Height) * (double)Screen.PrimaryScreen.Bounds.Width;
                    k = (btmWallpaper.Width - xOffsetImage) / (double)Screen.PrimaryScreen.Bounds.Width;
                }

                Pen pen = new Pen(Color.Gray, 2 * (float)k);

                int x0 = (int)(xOffsetImage / 2.0);
                int y0 = (int)(yOffsetImage / 2.0);
                int w0 = btmWallpaper.Width - (int)xOffsetImage;
                int h0 = btmWallpaper.Height - (int)yOffsetImage;

                float fS = 1;
                if (windowsVersion.Contains("Windows 10") || windowsVersion.Contains("Windows XP"))
                {
                    fS = (15 * (float)k);
                }
                else if (windowsVersion.Contains("Windows 7"))
                {
                    if (extention == ".bmp")
                        fS = (5 * (float)k);
                    else
                        fS = (15 * (float)k);
                }

                Font font = new Font("Verdana", fS);

                int x0b = (int)(330 * k);

                int x1 = (int)(320 * k);
                int x2 = (int)(230 * k);

                int y1 = (int)(117 * k);
                int y2 = (int)(94 * k);
                int y3 = (int)(71 * k);

                Brush rectBrush = new SolidBrush(Color.FromArgb(180, Color.Gray));
                Brush fontBrush = Brushes.White;

                //g.DrawRectangle(pen, x0, y0, w0, h0);

                //DrawRoundedRectangle(g, pen, new Rectangle(x0 + w0 - x0b, y0 + h0 - (int)(122 * k), x0b, (int)(80 * k)), (int)(10 * k));
                FillRoundedRectangle(g, rectBrush, new Rectangle(x0 + w0 - x0b, y0 + h0 - (int)(122 * k), x0b, (int)(80 * k)), (int)(10 * k));
                //g.FillRectangle(rectBrush, x0 + w0 - x0b, y0 + h0 - (int)(122 * k), x0b, (int)(80 * k));
                //g.DrawRectangle(pen, x0 + w0 - x0b, y0 + h0 - (int)(122 * k), x0b, (int)(80 * k));

                //Ваш IP.
                g.DrawString("Ваш IP:", font, fontBrush, new PointF(x0 + w0 - x1, y0 + h0 - y1));
                g.DrawString(string.Format("{0}", localIPAddress), font, fontBrush, new PointF(x0 + w0 - x2, y0 + h0 - y1));

                //Компьютер.
                g.DrawString("Комп.:", font, fontBrush, new PointF(x0 + w0 - x1, y0 + h0 - y2));
                g.DrawString(string.Format("{0}", Environment.MachineName), font, fontBrush, new PointF(x0 + w0 - x2, y0 + h0 - y2));

                //Пользователь.
                g.DrawString("Польз.:", font, fontBrush, new PointF(x0 + w0 - x1, y0 + h0 - y3));
                g.DrawString(string.Format("{0}", wi.Name), font, fontBrush, new PointF(x0 + w0 - x2, y0 + h0 - y3));

                string pathDestination3 = string.Format(@"{0}\{1}\{2}{3}", localDir, folderTemp, "EditedWallpaper", extention);

                SaveImage(btmWallpaper, pathDestination3);

                Wallpaper.SetWallpaper(pathDestination3);

                SaveLog(string.Format("Success load wallpaper {0} (Image={1}:{2}, Screen={3}:{4}, k={5:0.##}, fS={6:0.##}).",
                    pathDestination3,
                    btmWallpaper.Width, btmWallpaper.Height,
                    Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
                    k, fS));
            }
            catch (Exception ex)
            {
                /*
                Console.WriteLine(string.Format("{0} -> {1}", ex.Source, ex.Message));
                Console.WriteLine("Type anything to exit.");
                Console.Read();
                */
                SaveLog(string.Format("{0} -> {1}", ex.Source, ex.Message));
            }
        }

        static void Main(string[] args)
        {
            DateTime t0 = DateTime.Now;
            Console.WriteLine("Start...");
            SetInfoWallpaper();
            TimeSpan diff = DateTime.Now - t0;
            Console.WriteLine(string.Format("Changing wallpaper took {0} s", diff.TotalSeconds));
            Thread.Sleep(1000);
            //Console.Read();
        }
    }
}
