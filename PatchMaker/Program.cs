﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatchMaker.Utilities;

namespace PatchMaker
{
    class Program
    {
        /// <summary>
        /// Directory comparison and patch creation tool.  See 
        /// http://msdn.microsoft.com/en-us/library/bb546137.aspx for more info
        /// on some implementation details
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //  Parse commandline options
            CommandLineOptions options = new CommandLineOptions();
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

                //  If we should create a date folder, make that part of the base patch directory
                string basePatchDirectory = options.PatchBaseDirectory;
                if(options.CreateDateFolder)
                {
                    basePatchDirectory = Path.Combine(options.PatchBaseDirectory, DateTime.Now.ToString("yyyy-MM-dd"));
                }
                
                //  Create the output directory, if it doesn't already exist:
                if(!Directory.Exists(basePatchDirectory))
                {
                    Directory.CreateDirectory(basePatchDirectory);
                }

                //  Read in the source and target directory file information:
                DirectoryInfo sourceDirectory = new DirectoryInfo(options.SourceDirectory);
                DirectoryInfo targetDirectory = new DirectoryInfo(options.TargetDirectory);
                IEnumerable<FileInfo> allSourceFiles = new List<FileInfo>();
                IEnumerable<FileInfo> allTargetFiles = new List<FileInfo>();

                var sfTask = Task.Factory.StartNew(() =>
                {
                    string output = string.Empty;
                    output += string.Format("Scanning source directory ...");
                    allSourceFiles = sourceDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                    output += string.Format("{0} files found.\n", allSourceFiles.Count());
                    Console.Write(output);
                });

                var tfTask = Task.Factory.StartNew(() =>
                {
                    string output = string.Empty;
                    output += string.Format("Scanning target directory ...");
                    allTargetFiles = targetDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                    output += string.Format("{0} files found.\n", allTargetFiles.Count());
                    Console.Write(output);
                });
                
                //  Determine which file comparison to use:
                IEqualityComparer<FileInfo> fileComparison;
                if(options.CompareBytes)
                    fileComparison = new CompleteFileCompare() { ExcludeSpec = options.ExcludeSpec };
                else
                    fileComparison = new SimpleFileCompare() { ExcludeSpec = options.ExcludeSpec };

                try
                {
                    //  Wait for our directory scan to complete
                    Task.WaitAll(new Task[] { sfTask, tfTask });
                }
                catch(AggregateException e)
                {
                    for(int j = 0; j < e.InnerExceptions.Count; j++)
                    {
                        Console.WriteLine("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
                    }
                }

                //  See if we can determine if the directories are the same...
                if((allSourceFiles.Count() == allTargetFiles.Count()) && allSourceFiles.SequenceEqual(allTargetFiles, fileComparison))
                {
                    Console.WriteLine("The two folders are the identical");
                    return;
                }
                
                //  If they're not the same, see what files exist
                var filesToPatch = allSourceFiles.Except(allTargetFiles, fileComparison);
                /*
                var filesToPatch = (from file in allSourceFiles
                                      select file).AsParallel().Except(allTargetFiles, fileComparison);
                */

                Console.WriteLine("The following files are different and will be added to the patch:");
                foreach(var fi in filesToPatch)
                {
                    //  If the file is in the exlude list, don't bother comparing it
                    if(!FileHelper.FileInExcludeList(fi.Name, options.ExcludeSpec))
                    {
                        //  Copy our files to our patch base directory and print out each one
                        Console.WriteLine(FileHelper.CopyToPatchDirectory(basePatchDirectory, options.SourceDirectory, fi));
                    }
                }
            }
        }

        
    }

    
}
