using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SetInfoWallpaper
{
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            None,
            Tiled,
            Centered,
            Stretched
        }

        public static void SetWallpaper(string wallpaperPath, Style style = Style.None)
        {
            if (style != Style.None)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                switch (style)
                {
                    case Style.Stretched:
                        key.SetValue(@"WallpaperStyle", 2.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                    case Style.Centered:
                        key.SetValue(@"WallpaperStyle", 1.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;

                    case Style.Tiled:
                        key.SetValue(@"WallpaperStyle", 1.ToString());
                        key.SetValue(@"TileWallpaper", 1.ToString());
                        break;

                    default:
                        break;
                }
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                wallpaperPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
