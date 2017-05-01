#addin "Cake.FileHelpers"

/*
    This should be a simple file.  It is not because the new .Net Core
    tooling does not support .Net 3.5
 */

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = string.Format("0.7.{0}.{1}"
	, (int)((DateTime.UtcNow - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays)
	, (int)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalSeconds / 2));

private string[] MergeCompileLines(string[] file, string[] newCompiles)
{
  var first = int.MaxValue;
  var last = int.MinValue;
  
  for (var i = 0; i < file.Length; i++)
  {
    if (file[i].Trim().StartsWith("<Compile Include="))
    {
      first = Math.Min(first, i);
      last = Math.Max(last, i);
    }
  }
  
  return file.Take(first)
    .Concat(newCompiles)
    .Concat(file.Skip(last + 1))
    .ToArray();
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  CleanDirectory(Directory("./src/BracketPipe/bin/" + configuration + "/"));
  CleanDirectory(Directory("./publish/BracketPipe/lib/"));
  CleanDirectory(Directory("./artifacts/"));
});

Task("Patch-Version")
  .IsDependentOn("Clean")
  .Does(() =>
{
  CreateAssemblyInfo("./src/BracketPipe/AssemblyInfo.Version.cs"
  , new AssemblyInfoSettings {
      Version = version,
      FileVersion = version
    }
  );
});

Task("Patch-Project-Files")
  .IsDependentOn("Patch-Version")
  .Does(() =>
{
  var compileLines = FileReadLines("./src/BracketPipe/BracketPipe.csproj")
    .Where(l => l.Trim().StartsWith("<Compile Include="))
    .ToArray();
  
  var newLines = MergeCompileLines(FileReadLines("./src/BracketPipe/BracketPipe.Net35.csproj"), compileLines);
  FileWriteLines("./src/BracketPipe/BracketPipe.Net35.csproj", newLines);
});

Task("Restore-NuGet-Packages")
  .IsDependentOn("Patch-Project-Files")
  .Does(() =>
{
  if (FileExists("./src/BracketPipe/project.json"))
    MoveFile("./src/BracketPipe/project.json", "./src/BracketPipe/__project.__json");
  if (FileExists("./src/BracketPipe/project.lock.json"))
    MoveFile("./src/BracketPipe/project.lock.json", "./src/BracketPipe/__project.lock.__json");
  
  NuGetRestore("./src/BracketPipe/BracketPipe.Net35.csproj");
  DotNetCoreRestore("./src/BracketPipe/BracketPipe.NetCore.csproj");
}).OnError(ex => 
{
  if (FileExists("./src/BracketPipe/__project.__json"))
    MoveFile("./src/BracketPipe/__project.__json", "./src/BracketPipe/project.json");
  if (FileExists("./src/BracketPipe/__project.lock.__json"))
    MoveFile("./src/BracketPipe/__project.lock.__json", "./src/BracketPipe/project.lock.json");
});

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
{
  DotNetBuild("./src/BracketPipe/BracketPipe.Net35.csproj", settings =>
    settings.SetConfiguration(configuration));
  
  DotNetCoreBuild("./src/BracketPipe/BracketPipe.NetCore.csproj", new DotNetCoreBuildSettings
  {
     Configuration = configuration
  });
  
  CopyFiles("./src/BracketPipe/bin/" + configuration + "/**/BracketPipe.dll", "./publish/BracketPipe/lib/", true);
  var files = GetFiles("./publish/BracketPipe/lib/**/*")
    .Where(f => !f.ToString().EndsWith("BracketPipe.dll", StringComparison.OrdinalIgnoreCase));
  foreach (var file in files)
    DeleteFile(file);
}).Finally(() => 
{
  if (FileExists("./src/BracketPipe/__project.__json"))
    MoveFile("./src/BracketPipe/__project.__json", "./src/BracketPipe/project.json");
  if (FileExists("./src/BracketPipe/__project.lock.__json"))
    MoveFile("./src/BracketPipe/__project.lock.__json", "./src/BracketPipe/project.lock.json");
});

Task("NuGet-Pack")
  .IsDependentOn("Build")
  .Does(() =>
{
  var nuGetPackSettings = new NuGetPackSettings {
    Version = version,
    OutputDirectory = "./artifacts/"
  };
  NuGetPack("./publish/BracketPipe/BracketPipe.nuspec", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet-Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
