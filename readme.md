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

```sh
dotnet restore
dotnet run QtiPackageConverter package.zip 30
```

options: 
```sh
package.zip 30 or 22: converts 2.1 packages to 2.2 or 2.1
package.zip 21: converts 2.2 packages to 2.1
--validate: validates the package, takes longer than converting the item
--local: copies schemas in the package.zip and uses local schemas instead of ims schema's

package.zip validate21, validate22 or validate30 - validates 2.2 or 3.0 packages
```

package.zip can be a relative or absolute path, but can also be a folder with multiple packages.

It handles packages with an imsmanifest but also handles zip files with item.xml files only.

## known issues.

3.0
rubikBlocks and itemControl elements are removed during the conversion.
Without adding the 'use' attribute rubiksBlocks do not validate.
Because we don't use them; they are removed, but feel free to change the code in ConvertItem.cs
to do a proper 3.0 conversion.

