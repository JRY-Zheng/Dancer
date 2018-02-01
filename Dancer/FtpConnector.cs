using System;
 using System.Collections.Generic;
 using System.Text;
 using System.IO;
 using System.Net;
 using System.Windows.Forms;
 using System.Globalization;

 namespace Dancer
 {
    class FtpConnector
    {
        private FtpWeb ftpWeb;
        private string root_path = "";
        public void init(string FtpServerIP, string FtpUserID, string FtpPassword, string _root_path = "")
        {
            root_path = _root_path;
            ftpWeb = new FtpWeb(FtpServerIP, root_path, FtpUserID, FtpPassword);
        }
        public void workList(string music_list)
        {
            if (ftpWeb.CurrentDirectory() != root_path + "/" + music_list)
            {
                ftpWeb.GotoDirectory(root_path, false);
                if (!ftpWeb.DirectoryExist(music_list)) ftpWeb.MakeDir(music_list);
                ftpWeb.GotoDirectory(music_list, false);
            }
        }
        public void uploadFile(string file_path, string music_list)
        {
            workList(music_list);
            ftpWeb.Upload(file_path);
        }
        public void downloadFile(string file_path, string music_name, string music_list)
        {
            workList(music_list);
            ftpWeb.Download(file_path, music_name, false);
        }
    }
 }