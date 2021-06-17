# QTI Package Converter

This package converter does a best effort to convert 2.1 packages to 3.0 or 2.2.
It does a best effort to convert packages but is only tested with a few packages that have
the same folder structure. Packages with other folder structures or have extension schemas might fail.

This command line tool was originally created to modify qti2.x packages.
Mainly for features that are not possible in an authoring system.

This tool is still a good start to modify 2.x packages. To do so, add new folder
under implementations and create your own converter.

## Prerequisites

To run you need the dotnet core 3.1 runtime and to build the sdk. Both can be found <a href="https://dotnet.microsoft.com/download/dotnet-core/3.1">here</a>.

## Usage

The following commands will convert a qti 2.1 package to qti 3.0:

```sh
dotnet restore
dotnet run QtiPackageConverter package.zip 21_30 --validate
```

### conversion types: 

* 22_30 : converts 2.2 to 3.0
* 21_30 : converts 2.1 to 3.0
* 21_22 : converts 2.1 to 2.2
* 22_21 : converts 2.2 to 2.1
* validate30 : validates a 3.0 package
* validate22 : validates a 2.2 package
* validate21 : validates a 2.1 package

### other options:

* -- local: copies schemas in the package.zip and uses local schemas instead of ims-schema's
* -- validate: validates the package after conversion

package.zip can be a relative or absolute path, but can also be a folder with multiple packages.

It handles packages with an imsmanifest but also handles zip files with item.xml files only.

## known issues.

3.0
rubikBlocks and itemControl elements are removed during the conversion.
Without adding the 'use' attribute rubiksBlocks do not validate.
Because we don't use them; they are removed, but feel free to change the code in ConvertItem.cs
to do a proper 3.0 conversion.

