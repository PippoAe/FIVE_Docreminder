namespace docreminder.Helper_Classes
{
    public class FileModel
    {
        public string FileName { get; set; }
        public byte[] FileStream { get; set; }
        public string FileType { get; set; }
 
        public FileModel(string filename, byte[] filestream, string filetype)
        {
            FileName = filename;
            FileStream = filestream;
            FileType = filetype;
        }

    }
}
