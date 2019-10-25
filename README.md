# s3i - MSI Package Batch Installer 

This is a simple learning project I created to become familiar with .NET Core 3.0, 
its Windows/Linux binary portability and 
[FDE](https://docs.microsoft.com/en-us/dotnet/core/deploying/#framework-dependent-executables-fde) 
application deployment model. 

Also, it gives an example of using certain features 
like continuous integrations with [AppVeyor](https://appveyor.com), 
and continuous deployment to Windows computers from [GitHub Releases](https://help.github.com/en/github/administering-a-repository/about-releases) - 
all completely free for open source projects! For private projects, a secure storage, 
like AWS S3 can be used for storing configuration and/or software installers.

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

## Installation Prerequisites

- Windows 7/10 (with [Windows Installer](https://docs.microsoft.com/en-us/windows/win32/msi/overview-of-windows-installer))
   - or Linux (I develop and test on Ubuntu 18.04) - but only  dry run and simulation would work, since there is no `msiexec` for Linux
- [.NET Core Runtime 3.0.0](https://dotnet.microsoft.com/download/dotnet-core/3.0), which runs on Windows, Linux, or OSX

Latest version of `s3i.msi` can be found on the project's [Releases tab](https://github.com/OlegBoulanov/s3i/releases/latest)

## Self Test

Download and install `s3i.msi` from the link above first. Then, run the follwing command in Windows (or Linux) Command Line window:
```
C:\Users\olegb\> s3i https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/Examples/Config.ini --verbose --dryrun
```

Output should be similar to that:
```
Products [1]:
  UselessProduct: https://github.com/OlegBoulanov/s3i/releases/download/v1.0.265/wixExample.msi => C:\Users\olegb\AppData\Local\Temp\s3i\github.com\OlegBoulanov\s3i\releases\download\v1.0.265\wixExample.msi
    UselessProperty = doing nothing at all
Uninstall [1]:
  https://github.com/OlegBoulanov/s3i/releases/download/v1.0.265/wixExample.msi
Install [1]:
  https://github.com/OlegBoulanov/s3i/releases/download/v1.0.265/wixExample.msi
(DryRun) Uninstall C:\Users\olegb\AppData\Local\Temp\s3i\github.com\OlegBoulanov\s3i\releases\download\v1.0.265\wixExample.msi
(DryRun) Download 1 product:
  https://github.com/OlegBoulanov/s3i/releases/download/v1.0.265/wixExample.msi
(DryRun) Install C:\Users\olegb\AppData\Local\Temp\s3i\github.com\OlegBoulanov\s3i\releases\download\v1.0.265\wixExample.msi
(DryRun) Save C:\Users\olegb\AppData\Local\Temp\s3i\github.com\OlegBoulanov\s3i\releases\download\v1.0.265\wixExample.json
Elapsed: 00:00:00.8382180
```
Also, one file is created in your %TEMP% directory:
```
C:\Users\olegb\AppData\Local\Temp\s3i\github.com\OlegBoulanov\s3i\releases\download\v1.0.265\wixExample.json
```
Linux directory will be different, but still user-specific.
## Functionality and more detailed examples

### Operational model



Detailed desription can be found in [wiki](https://github.com/OlegBoulanov/s3i/wiki)
