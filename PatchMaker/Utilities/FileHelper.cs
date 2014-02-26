using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PatchMaker.Utilities
{
    public static class FileHelper
    {
        #region Exclude list helpers

        /// <summary>
        /// Returns true or false based on whether the passed filename
        /// should be excluded from the results
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="excludePatterns"></param>
        /// <returns></returns>
        public static bool FileInExcludeList(string filename, string[] excludePatterns)
        {
            List<string> matches = new List<string>();
            bool retval = false;

            foreach(string pattern in excludePatterns)
            {
                Regex regex = FindFilesPatternToRegex.Convert(pattern);

                if(regex.IsMatch(filename))
                {
                    retval = true;
                    break;
                }
            }

            return retval;
        }

        internal static class FindFilesPatternToRegex
        {
            private static Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
            private static Regex IlegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
            private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
            private static string NonDotCharacters = @"[^.]*";
            public static Regex Convert(string pattern)
            {
                if(pattern == null)
                {
                    throw new ArgumentNullException();
                }
                pattern = pattern.Trim();
                if(pattern.Length == 0)
                {
                    throw new ArgumentException("Pattern is empty.");
                }
                if(IlegalCharactersRegex.IsMatch(pattern))
                {
                    throw new ArgumentException("Patterns contains ilegal characters.");
                }
                bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
                bool matchExact = false;
                if(HasQuestionMarkRegEx.IsMatch(pattern))
                {
                    matchExact = true;
                }
                else if(hasExtension)
                {
                    matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
                }
                string regexString = Regex.Escape(pattern);
                regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
                regexString = Regex.Replace(regexString, @"\\\?", ".");
                if(!matchExact && hasExtension)
                {
                    regexString += NonDotCharacters;
                }
                regexString += "$";
                Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return regex;
            }
        } 

        #endregion

        /// <summary>
        /// Copies the file from the source path to the base patch directory,
        /// preserving its location in the directory structure
        /// </summary>
        /// <param name="basePatchDirectory"></param>
        /// <param name="sourceBaseDirectory"></param>
        /// <param name="sourceFileToCopy"></param>
        /// <returns></returns>
        public static string CopyToPatchDirectory(string basePatchDirectory, string sourceBaseDirectory, FileInfo sourceFileToCopy)
        {
            //  First, determine the relative source path
            string relativePath = Path.GetDirectoryName(sourceFileToCopy.FullName).Substring(sourceBaseDirectory.Length);
            
            //  Make sure the relative path doesn't start with a directory seperator
            if(relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativePath = relativePath.Substring(1);
            }

            //  Create this path under the basePatchDirectory
            Directory.CreateDirectory(Path.Combine(basePatchDirectory, relativePath));

            //  Copy the file
            string newFilePath = Path.Combine(basePatchDirectory, relativePath, sourceFileToCopy.Name);
            File.Copy(sourceFileToCopy.FullName, newFilePath, true);

            return newFilePath;
        }
    }
}
