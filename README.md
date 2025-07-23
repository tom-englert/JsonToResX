
# JsonToResX
[![Build status](https://ci.appveyor.com/api/projects/status/TODO/branch/main?svg=true)](https://ci.appveyor.com/project/tom-englert/jsontoresx/branch/main)
[![NuGet Status](https://img.shields.io/nuget/v/TomsToolbox.JsonToResX.svg)](https://www.nuget.org/packages/TomsToolbox.JsonToResX/)

A DotNet command line tool to convert .json resrouces files used in angular to .resx files and vice versa.

## Intention of this tool
In Angular applications, it is common to use .json files for localization. However, in .NET applications, the standard is to use .resx files.
This tool allows you to convert between these two formats easily, so you can work with localization in both environments without hassle.

For example, you can use it to convert your Angular localization files into .resx files to be able to use comfortable localization tools 
like [ResXResourceManager](https://github.com/dotnet/ResXResourceManager), that support automatic translations and many checks and verifications, and then convert the .resx files back to .json files for use in your Angular application.

## Conventions
In .json files, the keys are commonly delimited by dots, e.g. `app.title`, `app.subtitle`, while in .resx files the keys are usually delimited by underscores, e.g. `app_title`, `app_subtitle`.
This tool will convert the keys accordingly, so you work in both environments seamlessly.

The Angular translations loader replaces placeholders like `{{name}}` with the actual values at runtime. 
However, in .resx files, [ResXResourceManager](https://github.com/dotnet/ResXResourceManager) expects named placeholders in the format `${name}`.
This tool will convert the placeholders accordingly, so you can take advantage of the placeholder validation features provided by [ResXResourceManager](https://github.com/dotnet/ResXResourceManager).

## Sample
### Input (Test.json)
```json
{
  "app.title": "My Application",
  "app.subtitle": "Welcome to my application",
  "app.greeting": "Hello {{name}}"
}
```
### Output (Test.resx)
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  ...
  <data name="app_title" xml:space="preserve">
    <value>My Application</value>
  </data>
  <data name="app_subtitle" xml:space="preserve">
    <value>Welcome to my application</value>
  </data>
  <data name="app_greeting" xml:space="preserve">
    <value>Hello ${name}</value>
  </data>
</root>
```

## Installation
`dotnet tool install TomsToolbox.JsonToResX -g`

## Usage
```
Usage: JsonToResX [--input <String>] [--output <String>]

JsonToResX

Options:
  --input <String>     Input file, either a .json file or a .resx file
  --output <String>    Output file, either a .resx file or a .json file
  -h, --help           Show help message
  --version            Show version
```
## Example

```
dotnet tool install TomsToolbox.JsonToResX -g
JsonToResX --input Resources.json --output Resources.resx
```