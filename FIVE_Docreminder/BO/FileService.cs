using docreminder.InfoShareService;
using System.IO;

namespace docreminder.BO
{

    /// <summary>
    /// Defines methods for uploading and downloading files.
    /// </summary>
    public class FileService
    {
        FileClient FileClient;

        public FileService(FileClient fileClient)
        {
            this.FileClient = fileClient;
        }

        public FileService()
        {
            this.FileClient = new FileClient();
        }

        /// <summary>
        /// Uploads file bytes to the info share server all at once.
        ///
        /// Reads the content of the file into a byte array all at once.
        /// Calls the uploadFileBytes method on an instance of the InfoShareService.FileClient
        /// class and passes the connection id and the byte array as arguments. 
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="fileName">the file name</param>
        /// <returns>a unique file id</returns>
        public string UploadFileBytes(string connectionID, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                bytes = System.IO.File.ReadAllBytes(fileName);

                // last argument: the zero-based byte offset
                string fileID = this.FileClient.UploadFileBytes(connectionID, null, bytes, 0); // returns a unique fileID
                return fileID;
            }
        }

        public string UploadFileBytesOnly(string connectionID, byte[] fileBytes)
        {
            string fileID = this.FileClient.UploadFileBytes(connectionID, null, fileBytes, 0); // returns a unique fileID
            return fileID;
        }

        /// <summary>
        /// Uploads file bytes to the info share server in chunks.
        /// 
        /// Reads the content of the file into the buffer byte array step by step whereby
        /// the chunk size argument determines the size of the buffer and copies
        /// the buffer content into a second byte array. At every step it calls the 
        /// UploadFileBytes method on an instance of the InfoShareService.FileClient class 
        /// and passes the connection id and the second byte array as arguments. 
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="fileName">the file name</param>
        /// <param name="chunkSize">the chunk size</param>
        /// <returns>a unique file id</returns>
        public string UploadFileBytesInChunks(string connectionID, string fileName, int chunkSize)
        {

            string fileID = null;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[chunkSize];
                int offset = 0;
                int readBytes = 0;

                // Each reading from the file stream overwrites the buffer content unless 
                // less bytes are read than the constant buffer size (= chunk size). In this case, 
                // we want only the newly read bytes from the buffer. Thus, in each loop pass we 
                // write only the newly read bytes from the buffer into a new instance of the 
                // MemoryStream class and writes the stream contents into a second byte array.
                while ((readBytes = fs.Read(buffer, 0, chunkSize)) > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, readBytes); // buffer, 0 = start offset in the buffer, readBytes = the number of bytes to write
                        byte[] bytes = ms.ToArray();

                        // In the first loop iteration fileID is null  
                        fileID = this.FileClient.UploadFileBytes(connectionID, fileID, bytes, offset); // returns a unique fileID
                        offset = offset + readBytes; // calculates offset for next loop pass
                    }
                }
            }

            return fileID;

        }

        /// <summary>
        /// Downloads a file in bytes from the info share server and creates a file 
        /// with the specified file path.
        ///
        /// Calls the DownloadFileBytes method on an instance of the InfoShareService.FileClient class
        /// and passes the connection id, and the document id as arguments. This method 
        /// returns a byte array. Creates a directory, if the directory does not exist. Creates a file
        /// stream and a file, and writes the byte array to the file stream. 
        /// The file is created with the specified file path. 
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentID">the document id</param>
        /// <param name="filePath">the file path. The path consists of the path to the target directory and the file name.</param>
        public void DownloadFileBytes(string connectionID, string documentID, string filePath)
        {
            byte[] bytes = this.FileClient.DownloadFileBytes(connectionID, documentID, null, null, null, null,
                    -1 /* start byte for reading */, -1 /* Number of bytes to read */, false);

            // Gets the directory name without the file name
            string directoryName = Path.GetDirectoryName(filePath);

            // Any and all directories specified in directoryName are created, unless they already exist
            DirectoryInfo dirInfo = Directory.CreateDirectory(directoryName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }

        public byte[] DownloadFileBytesOnly(string connectionID, string documentID)
        {
            byte[] bytes = this.FileClient.DownloadFileBytes(connectionID, documentID, null, null, null, null,
        -1 /* start byte for reading */, -1 /* Number of bytes to read */, false);

            return bytes;
        }

        /// <summary>
        /// Downloads a file as a stream from the info share server to a file with the
        /// specified file path.
        ///
        /// Calls the DownloadFile method on an instance of the InfoShareService.FileClient class
        /// and passes the connection id, and the document id as arguments. This method 
        /// returns a stream. Creates a directory, if the directory does not exist. Creates a file
        /// stream and a file, reads the bytes from the stream and writes them to the file stream. 
        /// The file is created with the specified file path. 
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentID">the document id</param>
        /// <param name="filePath">the file path. The path must consists of the path to the target directory and the file name.</param>
        public void DownloadFile(string connectionID, string documentID, string filePath)
        {

            using (Stream stream = FileClient.DownloadFile(connectionID, documentID, null, null, null, null, false))
            {
                // Gets the directory name without the file name
                string directoryName = Path.GetDirectoryName(filePath);

                // Any and all directories specified in directoryName are created, unless they already exist
                DirectoryInfo dirInfo = Directory.CreateDirectory(directoryName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

    }

}
