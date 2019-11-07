# QTI Package Converter: Convert QTI2.x to QTI3.0

This package converter does a best effort to convert 2.x packages to 3.0.

This command line tool was originally created to modify qti2.x packages.
Mainly for features that are not possible in an authoring system.

This tool is still a good start to modify 2.x packages. To do so, add new folder
under implementations and create your own converter.

## Prerequisites

To run you neet the dotnet core 3.0 runtime and to build the sdk. Both can be found <a href="https://dotnet.microsoft.com/download/dotnet-core/3.0">here</a>.


## Usage

```sh
dotnet restore
dotnet run QtiPackageConverter package.zip 30
```

package.zip can be a relative or absolute path, but can also be a folder with multiple packages.

It handles packages with an imsmanifest but also handles zip files with item.xml files only.

## known issues.

rubikBlocks and itemControl elements are removed during the conversion.
Without adding the 'use' attribute rubiksBlocks do not validate.
Because we don't use them; they are removed, but feel free to change the code in ConvertItem.cs
to do a proper 3.0 conversion.

$# TODO

XSD validation of the package at the end of the conversion to verify the result.
