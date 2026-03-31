For Japanese users, see readme.ja.md

[日本語版は readme.ja.md](readme.ja.md) をご参照ください。

-----------

# WinZoPNG

WinZoPNG – a Windows GUI wrapper around ZopfliPNG that enables concurrent file‑level processing of multiple PNGs.

This product inspired by the WOptiPNG: https://github.com/tp7/WOptiPNG/


# Limitations

 * Overwrite original files without prompting or backup. Backup them if needed.
 * Only for PNG files. Other types are not supported.<br>Convert to `.png` before optimizing.
 * APNG/MNG is also unsupported because zopflipng does not support them.
 * ETA is inaccurate. Rough calculation by start time and file size in progress.
   Sleep/hibernation interruptions and dynamic changes in the number of running threads are not included in the ETA calculation.


## Requirement
.Net Framework 8.0 Runtime required.

| -    | Minimum | Recommended |
| :--: | ----- | ----- |
| OS   | Windows 10 or later x64 (*1) |
| CPU  | Dual Cores | 4 or more cores/threads |
| Mem  | 4GB (*2) | 8GB or more |
| Screen | SXGA (1280x1024) | larger than the minimum |

Get runtime from here
: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

 * (*1) Version that supported by Microsoft.
 * (*2) Required memory depends on graphic file you want to optimize.


## How to install

Extract the zip to a folder that is exclusive to this app.
Create a shortcut if necessary.


## How to uninstall

Before removing, launch this app and click "About" button.
You can find the configuration folder.

Just remove installed files/folder.
Remove `C:\Users\%USERNAME%\AppData\Local\WinZoPNG` folder if you want to remove configuration file too.


## How to use

 1. Add files by drag and drop file(s) or directory into list, or click "Add Files"/"Add Dir" button.
 2. Check / set configurations, and click "Execute" button.
    Configurations are described below.
 3. When executing:
     * files cannot be added or removed from the list while processing.
     * Progress shown at status bar.
 4. When finish all files, result shown at status bar.


### Progress status

example) 
19:09:41; Elapse 0:00:46; 36.45% (22/60) done; ETA 19:11:01; 8 files are Running; Remain 30 file(s); Total Reduced approx.2.17MiB (2,270,544Bytes 6.95%)

Each items mean from left:

 * Current Time (HH:MM:SS)
 * Elapse Time (HH:MM:SS)
 * Progress rate calculated by file size
 * Finished file count / Total file count
 * ETA: Estimated Time of Arrival (if the ETA falls on the next day, the date MM/DD is also shown)
 * Current Running File count
 * Remain file count
 * Total reduced data size (with percentage)

Additionally, double-click on status bar, copy progress text to clipboard.

ETA is a very rough estimate. calculated by sum of original files total size, and elapsed time.
It is not affected by system sleep or changes in the number of threads.


## Configurations


### Threads

How many parallel processes to run.
On many cores/threads system, It is recommended to set the thread count slightly lower than the maximum available cores for stability.
eg) at 8c16t system, use 12-14 threads.

Over‑committing (using more threads than CPU cores) is not allowed because it provides no benefit.

Dynamically changeable while running, but decrease only at finish a file.


### Priority

Process priority for optimize process (zopflipng.exe)
Recommended: Idle


### Optimization levels

Default:
  no additional options, good balance for processing time and compression rate.

Better
: `--filters=0me` usually yields better compression than default, but takes more time.

Strong
: `--filters=01234me` usually yields better compression than default, but takes more time than the `Better` level.

Insane
: `--filters=01234mepb` Strongly discouraged.
This option incurs extremely long processing times and should only be used for exceptional cases.
(e.g., critical banner images that must be as small as possible)

WinZoPNG always runs zopflipng with `-m` options.


### Keep Timestamp

Keep timestamp (Create and Last-Modify) of original file after optimize.

**Access time is not preserved.**


## License

MIT license.
see license.txt for details.

You can download source and binary from below:

Original ZopfliPNG made by Google, Apache-2.0 license
: https://github.com/google/zopfli

ZopfliPNG-bin made by imagemin group, MIT License
: https://github.com/imagemin/zopflipng-bin

WinZoPNG made by CatStorming, MIT License
: https://github.com/CatStorming/WinZoPNG


## Develop env

 * OS: Windows 10 Pro x64 Japanese
 * IDE: Microsoft Visual Studio Community 2022 (64‑bit)
 * CPU: AMD Ryzen 7 3700X


## Miscellaneous

I built this to learn programming, such as multi-threading, windows forms, ListView virtual mode, lambda, etc.
The code is intentionally simple and may not follow best practices,
but build is easy, no external libraries needed except .Net Framework.

Double click status bar to copy status text to clipboard.

(TM)/(R) marks are omitted in app and documents.


## TODO

No fixed timeline.

 * cancel with killing zopflipng.exe
 * display the output of zopflipng.exe
 * multilingual support
 * command line support
 * Refactoring codes, MVC separation


## Change Log

v1.0.1 - minor update
 * various minor changes such as typo, small bugfix

v1.0.0 - first release

[EOF]
