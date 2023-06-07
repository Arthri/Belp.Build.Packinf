# Belp.Build.Packinf
An MSBuild extension that makes it easier to create and maintain NuGet packages.

## Installation

### Requirements
- A project written in SDK-style. This includes any project for .NET Core(or newer) or .NET 5(or newer).

### Install using an Editor
1. Locate the project file(for example, `Project.csproj`, `Project.fsproj`).
1. Open the project file in an editor.
1. Locate the an `<ItemGroup>`.
1. Add a new `PackageReference` element to the item group with the `Include` attribute set to `Belp.Build.Packinf` and the `Version` attribute set to `0.0.1`. For example, `<PackageReference Include="Belp.Build.Packinf" Version="0.0.1" />`.
1. Run `dotnet restore`.

#### Uninstallation
1. Locate the project file(for example, `Project.csproj`, `Project.fsproj`).
1. Open the project file in an editor.
1. Locate the `PackageReference` element with an `Include` attribute set to `Belp.Build.Packinf`.
1. Delete the element.

### Install using the .NET CLI
1. Open a terminal.
1. Navigate to the containing directory of the project file.
1. Run the command `dotnet add package Belp.Build.Packinf`.

#### Uninstallation
1. Open a terminal.
1. Navigate to the containing directory of the project file.
1. Run the command `dotnet remove package Belp.Build.Packinf`.

### Install using Visual Studio Package Manager
1. Open Visual Studio.
1. Right click the project in the Solution Explorer.
1. Click on "Manage NuGet Packages".
1. Go to the "Browse" tab.
1. Search for `Belp.Build.Packinf`.
1. Install.

#### Uninstallation
1. Open Visual Studio.
1. Right click the project in the Solution Explorer.
1. Click on "Manage NuGet Packages".
1. Go to the "Installed" tab.
1. Click on `Belp.Build.Packinf`.
1. Click on "Uninstall".

## Usage

### `_Package` files
Files that are named `_Package` will be automatically renamed to `$(PackageId)` when packed. For example, if `$(PackageId)` is `Belp.Build.XYZ` and a file is named `_Package.txt`, it will be packed as `Belp.Build.XYZ.txt`. This feature makes it easier for forks to edit and maintain files that are required to be named in that manner.

### `Assets/` folder
By default, all files inside `Assets/` will be packed. This behavior can be disabled by setting `$(EnableDefaultPackItems)` to false.

### README file
Files that are named `README`(case-insensitive) and are placed beside the project will be packed as the package's README file. If no READMEs are present beside the project, and SourceLink is installed in the project, the file named `README.md` at the repository's root(if present) will be used instead. Only one README should be defined.

### Autoasmver
Autoasmver is enabled by default. It will automatically set the assembly version of compiled assemblies to `Major.0.0.0`. Set `$(DisableAutoasmver)` to `true` to disable Autoasmver.

### Autocopyright
Autocopyright is enabled by default. Autocopyright makes it easier to define the copyright. For example, the configuration below will result in the copyright notice `Copyright (C) 2023 Arthri.\nAll Rights Reserved`
```xml
  <ItemGroup>
    <CopyrightOwner Include="Arthri" Years="2023" />
  </ItemGroup>
```

## Development

### Prerequisites
- Install the .NET 7.0 SDK version 7.0.100 or newer.

### Building with Visual Studio
1. Open `Belp.SDK.Packinf.sln`.
1. Open the Solution Explorer.
1. Right click on the project `Belp.SDK.Packinf` in the Solution Explorer.
1. Click on `Pack`.

### Building with .NET CLI
1. Open a terminal in the repository root.
1. Run `dotnet pack`

### Output
By default, the output is located in `src/Belp.SDK.Packinf/Belp.SDK.Packinf/bin/Release/`.
