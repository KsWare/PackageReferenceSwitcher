# PackageReferenceSwitcher

CLI for switching between PackageReference and ProjectReference in project files.

Unlike [other solutions](#Other-solutions), we use a permanent configuration.  
Switching is done by selecting Debug/Release configuration.

PackageReferenceSwitcher prepares the solution and the projects for this.

## Usage

- Add the package projects to the solution (search in folders will be added later)  
- call `PackageReferenceSwitcher init "Path-To-Your-Solution.sln"`

uninstall:
- call `PackageReferenceSwitcher remove "Path-To-Your-Solution.sln"`

## Additional

Add as external tool and use `init "$(SolutionDir)$(SolutionFileName)"` as arguments.

To keep the packet versions up-to-date, use our tool 'UpdatePackageReferences'

## Example

before:
```xml
  <ItemGroup>
    <PackageReference Include="KsWare.VsFileEditor" Version ="0.1.0" />
  </ItemGroup>
 ```
 after init:
```xml
  <ItemGroup>
    <PackageReference Include="KsWare.VsFileEditor" Version="0.1.0" Condition="'$(Configuration)' == 'Release'" />
    <ProjectReference Include="..\..\..\VsFileEditor\src\VsFileEditor\VsFileEditor.csproj" Condition="'$(Configuration)' == 'Debug'" />
  </ItemGroup>
```
## Other solutions
choose the one that suits you best
- NuGetReferenceSwitcher (RicoSuter) (Last Update 2015) [marketplace VS 2019](https://marketplace.visualstudio.com/items?itemName=RicoSuter.NuGetReferenceSwitcherforVisualStudio2019), [GitHub](https://github.com/RicoSuter/NuGetReferenceSwitcher)
- NuGetReferenceSwitcher Fork (Last Update 2024) [GitHub](https://github.com/Schnazey/NuGetReferenceSwitcher)
- NuGetSwitcher (Last Update 2022) [marketplace VS 2022](https://marketplace.visualstudio.com/items?itemName=github0UserName.DCB9FB28-5610-4A94-9471-4BF2D0556BC5), [GitHub](https://github.com/0UserName/NuGetSwitcher)