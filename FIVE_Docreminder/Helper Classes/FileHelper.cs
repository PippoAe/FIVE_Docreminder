using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace docreminder
{
    //This Class handles Tasks wich need access to the file-system. (Loading File, Directory-Crawling etc.)
    static class FileHelper
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Adds the current executing directory to a given Filename.
        public static string GetFullFileDir(string filename)
        {
            string yourpath = Environment.CurrentDirectory + @"\" + filename;
            return yourpath;
        }

        public static string GetFullFileDirAppdata(string filename)
        {
            string yourpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Kendox\MS_Uploader\" + filename;
            return yourpath;
        }

        //Returns a text-based file as string with removed special characters (such as \n \r \t)
        public static string GetFileAsString(string filename)
        {
            

            //Add Appdir if path is relative.
            string sFilePath = "";
            if (!filename.Contains("C:"))
                sFilePath = GetFullFileDir(filename);
            else
            {
                sFilePath = filename;
            }

            string contents = "";
            try
            {
                contents = Regex.Replace(File.ReadAllText(sFilePath), @"[\r\n\t ]+", " ");
            }
            catch (Exception e)
            {
                log4.Error("Couldn't find File '" + filename + "' in application directory.\n\n" + e.ToString());
            }
            return contents;
        }

        //Gets text between two words.
        public static String GetTextBetween(String source, String leftWord, String rightWord)
        {
            MatchCollection matches = Regex.Matches(source, "(?<=\\"+leftWord + ")(.*?)(?=\\" + rightWord + ")");

            //if more than one match, get second.
            string ret = matches[0].ToString();
            if (matches.Count > 1)
            {
                ret = matches[1].ToString();
            }

            return ret;
        }

        //Replaced Text Between
        public static MatchCollection GetMatchesBetween(String source, String leftWord, String rightWord)
        {
            MatchCollection matches = Regex.Matches(source, "(?<=" + leftWord + ")(.*?)(?=" + rightWord + ")");
            return matches;
        }

        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            try
            {
                using (TextReader reader = new StringReader(objectData))
                {
                    result = serializer.Deserialize(reader);
                }
            }
            catch
            {
                result = "";
            }

            return result;
        }


        public static bool IsValidMail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string[] EncryptString(string sPlaintext)
        {
            // Data to protect. Convert a string to a byte[] using Encoding.UTF8.GetBytes().
            byte[] plaintext = System.Text.Encoding.UTF8.GetBytes(sPlaintext);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] entropy = new byte[20];
            
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            byte[] ciphertext = ProtectedData.Protect(plaintext, entropy,
                DataProtectionScope.CurrentUser);

            string sCiphertext = System.Convert.ToBase64String(ciphertext);
            string sEntropy = System.Convert.ToBase64String(entropy);

            return new string[] { sCiphertext, sEntropy };
        }

        public static string DecryptString(string sCyphertext,string sEntropy)
        {
            byte[] bCyphertext = System.Convert.FromBase64String(sCyphertext);
            byte[] bEntropy = System.Convert.FromBase64String(sEntropy);

            byte[] plaintext = ProtectedData.Unprotect(bCyphertext, bEntropy,
    DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plaintext);

        }

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        public static string getMimeFromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found");

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                if (fs.Length >= 256)
                    fs.Read(buffer, 0, 256);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }
            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                return "unknown/unknown";
            }
        }

        public static string SerializeObjectUTF8<T>(this T toSerialize)
        {

            XmlSerializer serializer = new XmlSerializer(toSerialize.GetType());

            // create a MemoryStream here, we are just working
            // exclusively in memory
            System.IO.Stream stream = new System.IO.MemoryStream();

            // The XmlTextWriter takes a stream and encoding
            // as one of its constructors
            System.Xml.XmlTextWriter xtWriter = new System.Xml.XmlTextWriter(stream, Encoding.UTF8);

            serializer.Serialize(xtWriter, toSerialize);

            xtWriter.Flush();

            // go back to the beginning of the Stream to read its contents
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            // read back the contents of the stream and supply the encoding
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8);

            string result = reader.ReadToEnd();

            return result;
        }



        public static Stream Compress(this IEnumerable<docreminder.Helper_Classes.FileModel> files)
        {
            if (files.Any())
            {
                var ms = new MemoryStream();
                var archive = new ZipArchive(ms, ZipArchiveMode.Create, false);
                foreach (var file in files)
                {
                    var entry = archive.add(file);
                }
                ms.Position = 0;
                return ms;
            }
            return null;
        }

        private static ZipArchiveEntry add(this ZipArchive archive, docreminder.Helper_Classes.FileModel file)
        {
            var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);
            using (var stream = entry.Open())
            {
                stream.Write(file.FileStream, 0, file.FileStream.Length);
                stream.Position = 0;
                stream.Close();
            }
            return entry;
        }

        public static byte[] ReadToEnd(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string GetUniqueFilePath(string filepath)
        {
            if (File.Exists(filepath))
            {
                string folder = Path.GetDirectoryName(filepath);
                string filename = Path.GetFileNameWithoutExtension(filepath);
                string extension = Path.GetExtension(filepath);
                int number = 1;

                Match regex = Regex.Match(filepath, @"(.+) \((\d+)\)\.\w+");

                if (regex.Success)
                {
                    filename = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }

                do
                {
                    number++;
                    filepath = Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
                }
                while (File.Exists(filepath));
            }

            return filepath;
        }

        public static string CleanUpFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }



        //public static byte[] CreateZipFile(byte[][] documents)
        //{
        //    using (var compressedFileStream = new MemoryStream())
        //    {
        //        //Create an archive and store the stream in memory.
        //        using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false))
        //        {
        //            foreach (var document in documents)
        //            {
        //                //Create a zip entry for each attachment
        //                var zipEntry = zipArchive.CreateEntry(caseAttachmentModel.Name);

        //                //Get the stream of the attachment
        //                using (var originalFileStream = new MemoryStream(caseAttachmentModel.Body))
        //                {
        //                    using (var zipEntryStream = zipEntry.Open())
        //                    {
        //                        //Copy the attachment stream to the zip entry stream
        //                        originalFileStream.CopyTo(zipEntryStream);
        //                    }
        //                }
        //            }

        //        }

        //        return compressedFileStream.ToArray();
        //    }
        //}


        //public static string ToInvariantString(this object obj)
        //{
        //    return obj is IConvertible ? ((IConvertible)obj).ToString(Cultu)
        //        : obj is IFormattable ? ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture)
        //        : obj.ToString();
        //}
    }
}
