using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Web.Configuration;

namespace Bitrix.Modules
{
    /// <summary>
    /// Summary description for buckup
    /// </summary>
    public static class BuckupMeneger
    {
        public static Process rar = new Process();
        public static FileInfo[] GetFiles(BuckupOptions options)
        {
            DirectoryInfo dir = new DirectoryInfo(options.BuckupFolder);
            //if (dir.Parent != null)
            //    dir = dir.Parent;
            return dir.GetFiles("*.rar", SearchOption.TopDirectoryOnly);
        }

        public static void Buckup(BuckupOptions options)
        {
            DoDBBuckup(options);

            DoRAR(options);
        }

        private static void DoRAR(BuckupOptions options)
        {

            rar.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            rar.StartInfo.FileName = string.Format("{0}bin\\rar.exe", options.BuckupFolder);

            rar.StartInfo.Arguments = options.GetCommandLine();

            if (!string.IsNullOrEmpty(rar.StartInfo.Arguments))
            {
                rar.Start();
                if(options.StepDuration > 0 || options.Sleep > 0)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), options);

                Thread.Sleep(500);
                rar.WaitForExit();

                if (options.IncludeDatabase)
                    File.Delete(options.DBBakFile);
            }
            else
                return;
        }

        public static void DoWork(object data)
        {
            try
            {
                BuckupOptions options = data as BuckupOptions;
                lock (rar)
                {
                    while (!rar.HasExited)
                    {
                        try
                        {
                            Thread.Sleep(options.StepDuration);
                            foreach (ProcessThread thread in rar.Threads)
                                Thread_Suspend(Thread_GetHandle(thread.Id));

                            Thread.Sleep(options.Sleep);
                            foreach (ProcessThread thread in rar.Threads)
                                Thread_Resume(Thread_GetHandle(thread.Id));
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        public static void Abort(Process rar)
        {
            if (rar != null && !rar.HasExited)
                rar.Kill();
        }

        private static void DoDBBuckup(BuckupOptions options)
        {
            if (options.IncludeDatabase)
            {
                SqlCommand cmd = new SqlCommand(options.GetBackupSQLCommand(), new SqlConnection(options.ConnectionString));
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            return;
        }

        public static object GetDBSize(BuckupOptions options)
        {
            SqlCommand cmd = new SqlCommand(options.GetDBSizeSQLCommand(), new SqlConnection(options.ConnectionString));
            cmd.Connection.Open();
            object size = cmd.ExecuteScalar();
            cmd.Connection.Close();
            return size;
        }

        #region Thread_Suspend
        [DllImport("Kernel32.dll")]
        static extern Int32 SuspendThread(IntPtr hThread);
        public static int Thread_Suspend(IntPtr ThreadHandle)
        {
            return SuspendThread(ThreadHandle);
        }
        #endregion

        #region Thread_Resume
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        public static int Thread_Resume(IntPtr ThreadHandle)
        {
            return ResumeThread(ThreadHandle);
        }
        #endregion

        #region Thread_GetHandle
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        public static IntPtr Thread_GetHandle(int ThreadID)
        {
            return OpenThread(ThreadAccess.SUSPEND_RESUME, false, ThreadID);
        }

        public static IntPtr Thread_GetHandle(int ThreadID, ThreadAccess DesiredAccess)
        {
            return OpenThread(DesiredAccess, false, ThreadID);
        }
        #endregion

        #region Handle_Close
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        public static bool Handle_Close(IntPtr OpenedHandle)
        {
            return CloseHandle(OpenedHandle);
        }
        #endregion
    }

    public class BuckupOptions
    {
        public string BuckupFolder { get; set; }
        public string DestinationFolder { get; set; }
        public string PublicPartFolder { get; set; }
        public string RarFile { get; set; }
        public string DBBakFile { get; set; }

        public bool IncludeCorePart { get; set; }
        public bool IncludePublicPart { get; set; }
        public bool IncludeDatabase { get; set; }
        public bool DisableCompresing { get; set; }
        public bool CheckPackage { get; set; }
        public bool ExcludeStandardTypesCompressing { get; set; }

        public long ExcludeFileSize { get; set; }
        public string[] ExcludeFiles { get; set; }

        public int StepDuration { get; set; }
        public int Sleep { get; set; }

        public string ConnectionString { get; set; }

        public BuckupOptions()
        {
        }

        public string GetCommandLine()
        {
            DirectoryInfo dir = new DirectoryInfo(this.BuckupFolder);

            string buckupFileName = string.Format("{0}_{1}.{2}.{3}_{4}.{5}.{6}.rar", dir.Name, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            this.RarFile = string.Format("{0}{1}", DestinationFolder, buckupFileName);

            //-m<n>    Установить метод сжатия 
            //-ms    Указать типы файлов для архивирования без сжатия 
            //-rr[N]    Добавить информацию для восстановления 
            //-sfx[имя]    Создать самораспаковывающийся архив 
            //-t    Протестировать файлы после архивирования
            //7z, ace, arj, bz2, cab, gz, jpeg, jpg, lha, lzh, mp3, rar, taz, tgz, z, zip
            //m<0..5>       Метод сжатия (0-без сжатия...3-обычный...5-максимальный)
            //D:\bitrixnet3\bin\Rar.exe a -m0 -x*.dll -x*.aspx D:\bitrixnet3\bitrixnet3_2012.4.27_0.44.3.rar  D:\bitrixnet3
            //rar a -r  -x.svn -x*/.svn -x*/.svn/* -x*/anotherSubFolder -x*/anotherSubFolder/* myarchive

            string commandLine = string.Empty;
            string command = "a";
            string key = " -o+ -ep1 ";
            string buckup = this.RarFile;
            string folderOrfileOrFileList = string.Empty;
            string excludeFileList = Path.GetTempFileName();
            string filesListText = string.Empty;
            //key += string.Format(" -sfxdefault.sfx -z{0}sfx.txt", this.BuckupFolder);

            if (this.DisableCompresing)
                key += " -m0 ";
            else if (this.ExcludeStandardTypesCompressing)
                key += " -ms*.jpg;*.jpeg;*.mp3;*.7z;*.bz2;*.cab;*.gz;*.lha;*.lzh;*.rar;*.taz;*.tgz;*.z;*.zip;*.ace;*.arj";

            if (this.CheckPackage)
                key += " -t";


            if (this.ExcludeFiles.Length > 0)
            {
                filesListText = String.Join("\r\n", this.ExcludeFiles)+"\r\n";
                File.AppendAllText(excludeFileList, filesListText);
            }

            if (this.ExcludeFileSize > 0)
            {

                IEnumerable<string> excludeResults = new DirectoryInfo(this.BuckupFolder)
                    .GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f => f.Length > this.ExcludeFileSize)
                    .Select(f => f.FullName);

                filesListText = String.Join("\r\n", excludeResults.ToArray<string>());
                File.AppendAllText(excludeFileList, filesListText);
            }

            key += string.Format(" -x@{0}", excludeFileList);

            folderOrfileOrFileList = Path.GetTempFileName();

            string[] dirs = Directory.GetDirectories(this.BuckupFolder);
            string[] results = Directory.GetFiles(this.BuckupFolder);

            filesListText = String.Join("\r\n", results.Union(dirs).ToArray());

            File.AppendAllText(folderOrfileOrFileList, filesListText);

            folderOrfileOrFileList = string.Format(" \"@{0}\"", folderOrfileOrFileList);

            commandLine = string.Format("{0} {1} {2} {3}", command, key, buckup, folderOrfileOrFileList);
            return commandLine;
        }

        public string GetDBSizeSQLCommand()
        {
            SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(this.ConnectionString);

            string query = string.Format(@"CREATE TABLE #DatabaseInfo (
                            name varchar(1000), 
                            db_size varchar(2550), 
                            owner varchar(200), 
                            dbid smallint,
                            created smalldatetime,
                            status varchar(2500), 
                            compatibility_level varchar(2500))

                            INSERT INTO #DatabaseInfo
                            exec SP_HELPDB

                            SELECT db_size as SizeMB
                            FROM #DatabaseInfo where name='{0}'

                            drop table #DatabaseInfo", connection.InitialCatalog);
            return query;

        }
        public string GetBackupSQLCommand()
        {
            SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(this.ConnectionString);
            string buckupFileName = string.Format(@"{0}{1}.bak", this.BuckupFolder, connection.InitialCatalog);

            string query = string.Format(@"BACKUP DATABASE {0}
            TO DISK = '{1}'
             WITH FORMAT,
                MEDIANAME = '{0}ServerBackups',
                NAME = 'Full Backup of {0}';", connection.InitialCatalog, buckupFileName);

            this.DBBakFile = buckupFileName;
            return query;
        }
    }

}