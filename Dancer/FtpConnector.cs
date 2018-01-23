using System;
 using System.Collections.Generic;
 using System.Text;
 using System.IO;
 using System.Net;
 using System.Windows.Forms;
 using System.Globalization;

 namespace Dancer
 {
    static class FtpConnector
    {
        private static FtpWeb ftpWeb;
        public static void init(string FtpServerIP, string FtpUserID, string FtpPassword)
        {
            ftpWeb = new FtpWeb(FtpServerIP, "", FtpUserID, FtpPassword);
        }
        public static void workList(string music_list)
        {
            if (ftpWeb.CurrentDirectory() != music_list)
            {
                ftpWeb.GotoDirectory("", false);
                if (!ftpWeb.DirectoryExist(music_list)) ftpWeb.MakeDir(music_list);
                ftpWeb.GotoDirectory(music_list, false);
            }
        }
        public static void uploadFile(string file_path, string music_list)
        {
            workList(music_list);
            ftpWeb.Upload(file_path);
        }
        public static void downloadFile(string file_path, string music_name, string music_list)
        {
            workList(music_list);
            ftpWeb.Download(file_path, music_name, false);
        }
    }
 }