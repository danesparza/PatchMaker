using System;
using System.Collections.Generic;
using System.IO;
using PatchMaker.Utilities;

namespace PatchMaker
{
    class CompleteFileCompare : IEqualityComparer<FileInfo>
    {
        /// <summary>
        /// Files or wildcards to exclude
        /// </summary>
        public string[] ExcludeSpec { get; set; }

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            //  See http://stackoverflow.com/a/211042/19020
            //  for more information on this byte-by-byte comparison logic

            // make sure lengths are identical
            long length = f1.Length;
            if(length != f2.Length)
                return false;

            //  make sure filenames are identical
            if(f1.Name != f2.Name)
                return false;

            //  If the file is in the exlude list, don't bother comparing it
            if(FileHelper.FileInExcludeList(f1.Name, this.ExcludeSpec))
                return true;

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
    }
}
