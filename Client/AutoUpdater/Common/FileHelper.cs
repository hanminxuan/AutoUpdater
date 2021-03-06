﻿//using AutoUpdater.Model;
using MyRobotModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoUpdater.Common
{
    public class FileHelper
    {
        List<FileInformation> FileList = new List<FileInformation>();
        //public static List<FileByteContent> FileByteContentsList = new List<FileByteContent>();//文件内容

        /// <summary>
        /// 无线递归目录下所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public List<FileInformation> GetAllFiles(DirectoryInfo dir)
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                if (System.IO.Path.GetFileName(fi.FullName) == Process.GetCurrentProcess().ProcessName + ".exe")
                {
                    continue;
                }

                Guid fileId = Guid.NewGuid();
                FileList.Add(new FileInformation
                {
                    FileId = fileId,
                    FileName = fi.Name,
                    FilePath = fi.FullName,
                    BaseFilePath = fi.FullName.Substring(fi.FullName.IndexOf("UpdateTempFile") + "UpdateTempFile".Length).Replace("\\\\", "\\"),
                    FileMd5 = GetFileMd5(fi.FullName),
                    文件总长度=fi.Length
                });
                //FileByteContentsList.Add(new FileByteContent
                //{
                //    FileId = fileId,
                //    FileBytes = GetFileBytes(fi.FullName),
                //    FileName = fi.Name,
                //    FilePath = fi.FullName,
                //    BaseFilePath = fi.FullName.Substring(fi.FullName.IndexOf("UpdateTempFile") + "UpdateTempFile".Length).Replace("\\\\","\\"),
                //    FileMd5 = GetFileMd5(fi.FullName)
                //});
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d);
            }
            return FileList;
        }

        /// <summary>
        /// 获取文件md5
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMd5(string filePath)
        {

            var fi = new FileInfo(filePath);
            fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly & ~FileAttributes.Hidden;//去掉只读和隐藏属性

            FileStream file = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将文件转换成byte[]
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] GetFileBytes(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);

                return buffur;
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", ex.ToString(), Guid.NewGuid().ToString());
                //MessageBoxHelper.ShowPrompt(ex.Message);
                return null;
            }
            finally
            {
                if (fs != null)
                {

                    //关闭资源
                    fs.Close();
                }
            }
        }

        //public byte[] Get按大小分段读取(string filePath)
        //{
        //    FileStream fs;
        //    //打开文件  
        //    try
        //    {
        //        fs = new FileStream(filePath, FileMode.Open);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    //尚未读取的文件内容长度  
        //    long left = fs.Length;
        //    //存储读取结果  
        //    byte[] bytes = new byte[20*1024*1024];//10MB每秒
        //    //每次读取长度  
        //    int maxLength = bytes.Length;
        //    //读取位置  
        //    int start = 0;
        //    //实际返回结果长度  
        //    int num = 0;
        //    //当文件未读取长度大于0时，不断进行读取  
        //    while (left > 0)
        //    {
        //        fs.Position = start;
        //        num = 0;
        //        if (left < maxLength)
        //            num = fs.Read(bytes, 0, Convert.ToInt32(left));
        //        else
        //            num = fs.Read(bytes, 0, maxLength);
        //        if (num == 0)
        //            break;
        //        start += num;
        //        left -= num;
        //    }
        //    fs.Close();
        //}

        /// <summary>
        /// 根据bytes生成文件
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="savePath"></param>
        public static void BytesToFile(byte[] buff, string savePath)
        {
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
            }

            FileStream fs = new FileStream(savePath, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            bw.Close();
            fs.Close();
        }

        /// <summary>
        /// 根据图片完整路径获取Image对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static System.Drawing.Image GetImageObj(string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);

            fs.Close();

            return result;

        }

        /// <summary>
        /// 强制结束某一个进程
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="exitCode"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern long TerminateProcess(int handle, int exitCode);

        public static void KillProcess(string processName)
        {
            try
            {
                Process[] myProcess = Process.GetProcessesByName(processName.Substring(0, processName.Length - 4));
                foreach (var item in myProcess)
                {
                    TerminateProcess(int.Parse(item.Handle.ToString()), 0);
                }
            }
            catch { }
        }

        public static string GetFileSizedisplay(double _fileSize)
        {
            try
            {
                if (_fileSize < 1024)
                {
                    //b 字节
                    return $"{_fileSize}字节";
                }
                else if (_fileSize >= 1024 && _fileSize < 1048576)
                {
                    //kb
                    return $"{(_fileSize / 1024).ToString("#0")}KB";
                }
                else if (_fileSize >= 1024 * 1024 && _fileSize < 1073741824)
                {
                    //mb
                    return $"{(_fileSize / 1024 / 1024).ToString("#0.0")}MB";
                }
                else if (_fileSize >= 1024 && _fileSize < 1099511627776)
                {
                    //G
                    return $"{(_fileSize / 1024 / 1024 / 1024).ToString("#0.0")}G";
                }
                else if (_fileSize >= 1099511627776)
                {
                    //T
                    return $"{(_fileSize / 1024 / 1024 / 1024 / 1024).ToString("0.0")}T";
                }
                else
                {
                    return "--";
                }
            }
            catch(Exception ex)
            {
                return "--";
            }
        }
    }
}
