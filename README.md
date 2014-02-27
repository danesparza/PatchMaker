PatchMaker
==========

Console based directory compare and patch maker.  Compares 2 directories and copies the differences to a 3rd 'patch' directory structure.

#### Command line example

    Patchmaker.exe --source "C:\Deployment\FormBuilder" --target "\\SOMESERVER\SomeSharename\SomeRemoteDirectory" --output "C:\Patchwork" --exclude *.config *.pdb
    
This will
* Compare files in `C:\Deployment\FormBuilder` and `\\SOMESERVER\SomeSharename\SomeRemoteDirectory`
* If different / new files are found, the files in `C:\Deployment\FormBuilder` will be copied to the `"C:\Patchwork"` directory
* The relative directory path will be preserved in the new patch directory
* By default, patches are archived by date automatically.  In other words, the patch for February 27th will live under `C:\Patchwork\2014-02-27\`
* Files matching the wildcard specs `*.config` and `*.pdb` will be ignored

#### Command line switches
      -s, --source                Required. The source directory
      -t, --target                Required. The target directory
      -o, --output                Required. The patch base directory
      -d, --create-date-folder    (Default: True) Create a folder under the patch
                                  directory with the date of the patch
      -b, --compare-bytes         (Default: True) Do a byte-level comparison
      -x, --exclude               Files or wildcards to exclude from patch
      -?, --help                  Show this help screen

