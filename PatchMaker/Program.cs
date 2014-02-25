using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            { 
                //  Look at the source directory.  If it doesn't exist, complain
                if(!Directory.Exists(options.SourceDirectory))
                {
                    Console.WriteLine("Source directory {0} doesn't exist.  Please select a directory that already exists", options.SourceDirectory);
                    return;
                }

                //  Look at the target directory.  If it doesn't exist, complain
                if(!Directory.Exists(options.TargetDirectory))
                {
                    Console.WriteLine("Target directory {0} doesn't exist.  Please select a directory that already exists", options.TargetDirectory);
                    return;
                }

                //  Create the output directory, if it doesn't already exist:
                if(!Directory.Exists(options.PatchBaseDirectory))
                {
                    Directory.CreateDirectory(options.PatchBaseDirectory);
                }

                //  Read in the source and target directory file information:
                DirectoryInfo sourceDirectory = new DirectoryInfo(options.SourceDirectory);
                DirectoryInfo targetDirectory = new DirectoryInfo(options.TargetDirectory);

                IEnumerable<FileInfo> allSourceFiles = sourceDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                IEnumerable<FileInfo> allTargetFiles = targetDirectory.GetFiles("*.*", SearchOption.AllDirectories);

                //A custom file comparer defined below
                FileCompare myFileCompare = new FileCompare();

                //  See http://msdn.microsoft.com/en-us/library/bb546137.aspx
                // This query determines whether the two folders contain 
                // identical file lists, based on the custom file comparer 
                // that is defined in the FileCompare class. 
                // The query executes immediately because it returns a bool. 
                bool areIdentical = allSourceFiles.SequenceEqual(allTargetFiles, myFileCompare);

                if(areIdentical == true)
                {
                    Console.WriteLine("the two folders are the same");
                }
                else
                {
                    Console.WriteLine("The two folders are not the same");
                }
            }
        }
    }

    class FileCompare : IEqualityComparer<FileInfo>
    {

        #region IEqualityComparer<FileInfo> Members

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            //  See http://stackoverflow.com/a/211042/19020
            // make sure lengths are identical
            long length = f1.Length;
            if(length != f2.Length)
                return false;

            byte[] buf1 = new byte[4096];
            byte[] buf2 = new byte[4096];

            // open both for reading
            using(FileStream stream1 = File.OpenRead(f1.FullName))
            using(FileStream stream2 = File.OpenRead(f2.FullName))
            {
                // compare content for equality
                int b1, b2;
                while(length > 0)
                {
                    // figure out how much to read
                    int toRead = buf1.Length;
                    if(toRead > length)
                        toRead = (int)length;
                    length -= toRead;

                    // read a chunk from each and compare
                    b1 = stream1.Read(buf1, 0, toRead);
                    b2 = stream2.Read(buf2, 0, toRead);

                    for(int i = 0; i < toRead; ++i)
                        if(buf1[i] != buf2[i])
                            return false;
                }
            }

            return true;
        }

        public int GetHashCode(FileInfo fi)
        {
            string s = String.Format("{0}{1}", fi.Name, fi.Length);
            return s.GetHashCode();
        }

        #endregion
    }
}
