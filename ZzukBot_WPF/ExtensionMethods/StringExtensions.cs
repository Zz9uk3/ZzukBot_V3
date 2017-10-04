using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Threading;
using ZzukBot.Settings;
using ZzukBot.UI;
using ZzukBot.WPF;

namespace ZzukBot.ExtensionMethods
{
    /// <summary>
    ///     StringExtensions
    /// </summary>
    public static class StringExtensions
    {
        internal static string BToString(this byte[] value)
        {
            return Encoding.Unicode.GetString(value);
        }
        internal static byte[] ToByte(this string value)
        {
            return Encoding.Unicode.GetBytes(value);
        }

        /// <summary>
        ///     Extension method to write a  datetime.now + value + newline to the bots log folder (file: ChatLog.txt => Content
        ///     will be written to ZzukBot\Logs\ChatLog.txt
        /// </summary>
        /// <param name="value">this</param>
        /// <param name="file"></param>
        /// <param name="showInDebug"></param>
        /// <param name="showDate"></param>
        public static void Log(this string value, string file, bool showDate = true)
        {
            var input = (showDate ? "[" + DateTime.Now + "] " : "") + value + Environment.NewLine;
            if (!Directory.Exists(Paths.Logs))
                Directory.CreateDirectory(Paths.Logs);
            File.AppendAllText(Paths.Logs + "\\" + file, input);
            var visualLog = $"[{file}] {"[" + DateTime.Now.ToString("t") + "] " + value}".Trim();
            new Action(() => SharedViewModel.Instance.DebugLog.Add(visualLog)).BeginDispatch(DispatcherPriority.Normal);

        }

        internal static void ClearLog(this string value)
        {
            var logFile = Paths.Root + "\\Logs\\" + value;
            if (!File.Exists(logFile)) return;
            File.Delete(logFile);
        }

        internal static SecureString ToSecure(this string value)
        {
            var secure = new SecureString();
            foreach (var c in value)
                secure.AppendChar(c);
            return secure;
        }

        internal static void FileCreate(this string value, byte[] parArr)
        {
            if (File.Exists(value))
            {
                var bytes = File.ReadAllBytes(value);
                if (bytes.SequenceEqual(parArr)) return;
                File.WriteAllBytes(value, parArr);
            }
            else
            {
                File.WriteAllBytes(value, parArr);
            }
        }

        internal static bool FileEqualTo(this string value, byte[] parArr)
        {
            if (File.Exists(value))
            {
                var bytes = File.ReadAllBytes(value);
                if (bytes.SequenceEqual(parArr)) return true;
            }
            return false;
        }

        internal static void CreateFolderStructure(this string value)
        {
            if (!Directory.Exists(value))
            {
                $"Directory {value} doesnt exist yet. Creating it".Log(LogFiles.PreInjectLog);

                Directory.CreateDirectory(value);
            }
        }
    }
}