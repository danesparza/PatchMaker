using System;
using System.Collections.Generic;
using System.IO;
using PatchMaker.Utilities;

namespace PatchMaker
{
    public class SimpleFileCompare : IEqualityComparer<FileInfo>
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

            //  make sure last update times are identical
            if(f1.LastWriteTimeUtc != f2.LastWriteTimeUtc)
                return false;

            return true;
        }

        public int GetHashCode(FileInfo fi)
        {
            string s = String.Format("{0}{1}", fi.Name, fi.Length);
            return s.GetHashCode();
        }
    }
}
