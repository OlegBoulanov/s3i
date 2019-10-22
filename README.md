# s3i - Batch Windows MSI installer 

This is a simple learning project I created to become familiar with .NET Core 3.0, its Windows/Linux binary portability and [FDE](https://docs.microsoft.com/en-us/dotnet/core/deploying/#framework-dependent-executables-fde) deployment model. Also, it includes certain useful features like automating builds using [AppVeyor](https://appveyor.com) and deployment to GitHub Releases

## Build Status

  master:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/master?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/master)
  develop:
[![Build status](https://ci.appveyor.com/api/projects/status/s5poqaqr1xn2e5ml/branch/develop?svg=true)](https://ci.appveyor.com/project/OlegBoulanov/s3i/branch/develop)

## Installation requirements

- Windows 7/10
- [.NET Core Runtime 3.0.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

Latest version of `s3i.msi` can be found on the project's [Releases tab](https://github.com/OlegBoulanov/s3i/releases)

# Functionality

## Use case
Having a fleet of (real, not virtual) Windows computers, we need to automate software installation and configuration, depending on individual or group settings. Software amy be produced by a separate CI/CD system and uploaded to version specific folders and/or AWS S3 buckets automatically. Upgarding a group of hosts to newer version would require changing a single line in group configuration file. s3i, ran manually or automatically, would compare already installed version with requested one and take proper actions.

s3i allows to install/upgrade/downgrade software packaged for Microsoft Installer, downloading configuration files and installers from remote storage (http servers, like github releases, or AWS S3 buckets)

## Setup

```
           command line to run on host       S3/http
/------\                                  /--------------\
| Host | -- s3i http://../products.ini->  | products.ini |   host (group) configuration file
\------/                                  \--------------/
  /|\                                           | |
   |               S3/http                      | |  links to products to be downloaded and installed
   |              /-------\                     | |
   \------------  | *.msi |-\  <----------------/ |
  download        \-------/ |  <------------------/
 and install        \------/  
                      /|\
                       |        /--------------\
                       \--------| CI/CD system | uploads *.msi(s) to version-specific subfolders
                                \--------------/
```
s3i reads configuration files, specified in command line, downloads and caches product installers and properties, and performs required installations, upgrades, or downgrades by invoking Windows msiexec.exe with proper arguments.

### AWS S3 prerequisites

By default, s3i uses current user's `[default]` AWS profile. Profile name can be changed using `--profile` command line option. Profile credentials should allow read access to all necessary S3 buckets and prefixes.

### Configuration file format

Configuration file contains one or several product specifications:
- Product name, for example, `SomethingUseless`
- Product installer URL, like `https://deployment.s3.amazonaws.com/useless.product/develop/1.2.3.4-beta2+test/installer.msi`
- Set of properties (key/value pairs) to be passed to unattended MSI installation

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

__Printing s3i help info:__
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

__Installing products from configuration file on AWS S3:__
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

__Upgrading one product:__

After changing `products.ini` file: ~~develop/1.2.3-beta2+test~~ _release/1.2.4_, run the same s3i command again:
```
C:\Users\current-user>s3i https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose
Products [2]:
  SomethingUseless: https://deployment.s3.amazonaws.com/useless.product/release/1.2.4/installer.msii
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

__Downgrading:__

Can be done the same way, as upgrading, but the version in the URL should be earlier than already installed, and the product will be unbinstalled first, and the earlier version installed back.

__Removing product:__

To remove, delete (or comment out with semicolon) product `name = URL` from `[$products$]` section of the config file, and run s3i.

## Simple automation

### s3i Windows Service

`s3i service` runs s3i with service arguments passed to the program at Windows startup.

CMDLINEARGS s3i installer property allows to set s3i command line parameters for the service at service installation time:
`C:\>msiexec /i s3i.msi CMDLINEARGS="https://install.company.com.s3.amazonaws.com/Test/Group/products.ini --verbose"`

### s3i_args Environment variable

`set s3i_args= https://install.company.s3.amazonaws.com/Test/Group/products.ini --verbose`

Running s3i with no arguments will now produce the same result as in examples above
