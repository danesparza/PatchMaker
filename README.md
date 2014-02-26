PatchMaker
==========

Console based directory compare and patch maker

Command line example:

    Patchmaker.exe --source "C:\Deployment\FormBuilder" --target "\\SOMESERVER\SomeSharename\SomeRemoteDirectory" --output "C:\Patchwork" --exclude *.config *.pdb
    
This will
* Compare files in `C:\Deployment\FormBuilder` and `\\SOMESERVER\SomeSharename\SomeRemoteDirectory`
* If different / new files are found, the files in `C:\Deployment\FormBuilder` will be copied to the `"C:\Patchwork"` directory
* Files matching the wildcard specs `*.config` and `*.pdb` will be ignored
