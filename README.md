# s3i - Batch Windows MSI installer 

This is a simple learning project I created to become familiar with .NET Core 3.0, its Windows/Linux binary portability and [FDE](https://docs.microsoft.com/en-us/dotnet/core/deploying/#framework-dependent-executables-fde) application deployment model. Also, it gives an example of using certain features like continuous integrations with [AppVeyor](https://appveyor.com), and continuous deployment to [GitHub Releases](https://help.github.com/en/github/administering-a-repository/about-releases) - all completely free for open source projects!

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

## Installation Prerequisites

- Windows 7/10 (with [Windows Installer](https://docs.microsoft.com/en-us/windows/win32/msi/overview-of-windows-installer))
- [.NET Core Runtime 3.0.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

Latest version of `s3i.msi` can be found on the project's [Releases tab](https://github.com/OlegBoulanov/s3i/releases)

# Functionality

## Use case
Having a fleet of (real, not virtual) Windows computers, we need to automate software installation and configuration, 
depending on individual or group settings. Software amy be produced by a separate CI/CD system and uploaded to version 
specific folders and/or AWS S3 buckets automatically. Upgarding a group of hosts to newer version would require changing 
a single line in group configuration file. s3i, ran manually or automatically, would compare already installed product 
semantic version and properties with requested one and take proper actions, like install, reinstall, or uninstall. 
A newer version would be installed over existing one in one pass, reinstall (to downgrade or change of properties) 
would require uninstall, followed by install.

s3i allows to install/upgrade/downgrade software packaged for Microsoft Installer, downloading configuration files and 
installers from remote storage (only public Web server, like GitHub Releases, or AWS S3 bucket supported recently)

## Setup Example

```
           command line to run on host       S3/http
/------\                                    /--------------\
| Host | -- s3i http://../products.ini--->  | products.ini |   host (group) configuration file
\------/                                    \--------------/
  /|\                                             | |
   |               S3/http                        | |  links to products to be downloaded and installed
   |              /-------\                       | |
   \------------  | *.msi |---\  <----------------/ |
  download        \-------/   |  <------------------/
 and install          \-------/  
                        /|\
                         |        /--------------\
                         \--------| CI/CD system | uploads *.msi(s) to version-specific subfolders
                                  \--------------/
```
s3i reads configuration files, specified in command line, downloads and caches product installers and properties, 
and performs required installations, upgrades, or downgrades by invoking Windows msiexec.exe with proper arguments.

### AWS S3 prerequisites

By default, s3i uses current user's `[default]` AWS profile. Profile name can be changed using `--profile` command line option. 
Profile credentials should allow read access to all necessary S3 buckets and prefixes.

### Product Installer Reqirements

Product Installer (.msi) is expected to be able to run in unattended mode, being configured with use of public properties, 
passed as msiexec command line arguments (`s3i_setup` project is a simple example of such product) 

### Configuration file format

Configuration file contains one or several product specifications:
- Product name, for example, `SomethingUseless`
- Product installer URL, like `https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi`
- Optional set of product public properties (key/value pairs) to be passed to unattended MSI installation

Here is an example of `products.ini` file:
```
; List of product name = installer URL
[$products$]
SomethingUseless = https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msi
EvenMoreUseless = https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi

; Sections specify optional product properties
[SomethingUseless]
ImportantProperty = just an example
NotSoImportant = but we pass it anyway, just for fun

[EvenMoreUseless]
HelloWorld = You welcome!
```
## Use

__Printing s3i help info__
```
C:\Users\current-user>s3i
s3i: msi package batch installer v1.0.12345
 Usage:
  s3i [<option> ...] <products> ...
 Options:
  -h, --help                        Print this help info [False]
  -p, --profile <profile-name>      AWS user profile name [default]
  -e, --envvar <var-name>           Environment variable name (default command line) [s3i_args]
  -s, --stage <path>                Path to staging folder [C:\Users\current-user\AppData\Local\Temp\s3i]
  -m, --msiexec <command>           MsiExec command [msiexec.exe]
  -a, --msiargs <args>              MsiExec extra args [/passive]
  -t, --timeout <timespan>          Installation timeout [00:03:00]
  -d, --dryrun                      Dry run [False]
  -v, --verbose                     Print full log info [False]
```

__Dry run (running with no actual installation)__
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose --dryrun
Products [2]:
  SomethingUseless: https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msii
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/develop/1.2.3-beta2+test/installer.msi
    ImportantProperty = just an example
    NotSoImportant = but we pass it anyway, just for fun
  EvenMoreUseless = https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\other.product\release\3.7.5\setup.msi
    HelloWorld = You welcome!
Install [2]:
...
(DryRun) Download ...
(DryRun) Install https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msi
(DryRun) Install https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi
Save C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/develop/1.2.3-beta2+test/installer.json
Save C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\other.product\release\3.7.5\setup.json
```
Similar results can be achieved by setting `msiexec` command to `echo msiexec`:
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose --msiexec "echo msiexec"
Products [2]:
...
Install [2]:
...
(Execute) Download ...
(Execute) Install https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msi
msiexec /i C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/develop/1.2.3-beta2+test/installer.msi /passive
...
```

__Installing products from configuration file on AWS S3__
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose
Products [2]:
  SomethingUseless: https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msii
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/develop/1.2.3-beta2+test/installer.msi
    ImportantProperty = just an example
    NotSoImportant = but we pass it anyway, just for fun
  EvenMoreUseless = https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\other.product\release\3.7.5\setup.msi
    HelloWorld = You welcome!
Install [2]:
...
(Execute) Download ...
(Execute) Install https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3-beta2+test/installer.msii
(Execute) Install https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi
Save C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/develop/1.2.3-beta2+test/installer.json
Save C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\other.product\release\3.7.5\setup.json
```

__Upgrading one product__

After changing `products.ini` file: ~~develop/1.2.3-beta2+test~~ _release/1.2.4_, run the same s3i command again:
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose
Products [2]:
  SomethingUseless: https://deployment.s3.amazonaws.com/useless.product/release/1.2.4/installer.msi
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/release/1.2.4/installer.msi
    ImportantProperty = just an example
    NotSoImportant = but we pass it anyway, just for fun
  EvenMoreUseless = https://deployment.s3.amazonaws.com/other.product/release/3.7.5/setup.msi
      => C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\other.product\release\3.7.5\setup.msi
    HelloWorld = You welcome!
Install [1]:
...
(Execute) Download ...
(Execute) Install https://deployment.s3.amazonaws.com/useless.product/release/1.2.4/installer.msi
Save C:\Users\current-user\AppData\Local\Temp\s3i\deployment.s3.amazonaws.com\useless.product/release/1/2/4/installer.json
```

__Downgrading or change of product properties__

Can be done the same way, as upgrading, but the version in the URL should be earlier than already installed, 
and the newer version of the product will be uninstalled first, and then the earlier version will be installed back.

__Uninstalling product:__

To uninstall a product, delete (or comment out with semicolon) product `name = URL` entry 
from `[$products$]` section of the config file, and run s3i again.

## Simple automation

### s3i Windows Service

`s3i service` runs s3i with service arguments passed to the program at Windows startup.

`CMDLINEARGS` s3i.msi installer property allows to set s3i command line parameters for the service at service installation time:
`C:\>msiexec /i s3i.msi CMDLINEARGS="https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose"`

### s3i_args Environment variable

`set s3i_args= https://install.company.s3.amazonaws.com/Test/Group/products.ini --verbose`

Running s3i with no arguments will now produce the same result as in examples above
