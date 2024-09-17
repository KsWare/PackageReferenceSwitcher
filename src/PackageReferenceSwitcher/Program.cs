using System.Reflection;
using System.Xml.Linq;
using EnvDTE;
using JetBrains.Annotations;
using KsWare.VsFileEditor;
using KsWare.VsFileEditor.Dom;

namespace KsWare.PacketReferenceSwitcher;

internal class Program {

	private static CommandLineArguments Args { get; } = new();
	private static SlnFile? Solution { get; set; }


	static void Main(string[] args) {
		for (var i = 0; i < args.Length; i++) {
			var normArg = args[i].ToLower().Replace('/', '-').Replace("--", "-").Replace("-?", "-help");
			switch (normArg) {
				case "help" : HelpAndExit(); break;
				case "init" : Args.Command = Commands.Init; break;
				case "remove" : Args.Command = Commands.Remove; break;
				case "-help" : HelpAndExit(); break;
				default:
					if (File.Exists(args[i]) && Path.GetExtension(args[i])==".sln") Args.SolutionFile = args[i];
					else if (File.Exists(args[i]) && ProjUtils.ProjectExtensions.Contains(Path.GetExtension(args[i]))) Args.ProjectFile = args[i];
					else Error($"ERROR Unknown parameter at index {i}: '{args[i]}'");
					break;
			}
		}

		switch (Args.Command) {
			case Commands.Init: Init(); break;
			case Commands.Remove: Remove(); break;
		}
	}

	private static void Init() {
		var projects = SlnUtils.GetProjectFiles(Args.SolutionFile);
		foreach (var project in projects) {
			AddConditionsForProject(project);
		}
	}

	private static void Remove() {
//		Error("Remove command ist not yet implemented!");
		var projects = SlnUtils.GetProjectFiles(Args.SolutionFile);
		foreach (var project in projects) {
			RemoveChangesForProject(project);
		}
	}

	public static void AddConditionsForProject(string csprojPath) {
		Console.WriteLine($"Processing '{Path.GetFileName(csprojPath)}'");
		var csproj = ProjFile.Load(csprojPath,null);
		var packageReferences = csproj.PackageReferences;
		
		var hasChanges = false;
		foreach (var packageReference in packageReferences) {
			var packageName = packageReference.Include;
			if (string.IsNullOrEmpty(packageName)) continue;
			var projectReference = csproj.FindProjectReferenceForPackage(packageName);
			if (projectReference != null) {
				projectReference.Condition = "'$(Configuration)' == 'Debug'";
				packageReference.Condition = "'$(Configuration)' == 'Release'";
				hasChanges = true;
				continue;
			}
			{ // look in solution
				var subProj = FindPackageProjectInSolution(packageName);
				if (subProj != null) {
					var relPath = "";
					var pr = new XElement("ProjectReference",
						new XAttribute("Include", relPath),
						new XAttribute("Condition", "'$(Configuration)' == 'Debug'"));
					packageReference.Element.Parent!.Add(pr);
					packageReference.Condition = "'$(Configuration)' == 'Release'";
					hasChanges = true;
					continue;
				}
			}
			{ // look in folder
				// TODO
			}
		}

		if (hasChanges) {
			csproj.Save();
			Console.WriteLine("  All conditions has been initialized");
		}
		else {
			Console.WriteLine("  No changes where necessary.");
		}
	}

	private static ProjFile? FindPackageProjectInSolution(string packageName) {
		if (Args.SolutionFile == null) return null;
		Solution ??= SlnFile.Load(Args.SolutionFile);
		return Solution.FindProjectForPackage(packageName);
	}

	private static void RemoveChangesForProject(string projPath) {
		Console.WriteLine($"Processing '{Path.GetFileName(projPath)}'");
		var proj = ProjFile.Load(projPath);
		var projectReferences = proj.ProjectReferences;
		var packageReferences = proj.PackageReferences;
		var hasChanges = false;
		foreach (var packageReference in packageReferences) {
			if(packageReference.Condition != "'$(Configuration)' == 'Release'") continue;
			packageReference.Condition = null;
			hasChanges = true;
		}
		foreach (var projectReference in projectReferences) {
			if(packageReferences.All(r => r.Include != projectReference.Name)) continue;
//			if(projectReference.Condition != "'$(Configuration)' == 'Debug'") continue;
//			projectReference.Condition = null;
			projectReference.Element.Remove();
			hasChanges = true;
		}

		if (hasChanges) {
			proj.Save();
			Console.WriteLine("  All conditions has been removed");
		}
		else {
			Console.WriteLine("  No changes where necessary.");
		}
	}

	[ContractAnnotation("=> halt")]
	private static void Error(string msg) {
		Console.Error.WriteLine(msg);
		Environment.Exit(1);
	}

	[ContractAnnotation("=> halt")]
	private static void HelpAndExit() {
		var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		var txt = $$"""
			KsWare PacketReferenceSwitcher v{{version}}
			Copyright © 2024 by KsWare. All rights reserved.
			¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
			Usage: PacketReferenceSwitcher <command> <switches> <file>
			
			<command>:
			  help              shows the help
			  init              initializes the specified solution
			  remove            removes all changes
			  
			<switches>:
			  -? --help         shows the help
			  
			<file>              sln file path
			""";
		Console.WriteLine(txt);
		Environment.Exit(0);
	}

}

public enum Commands {

	None,Init,

	Remove

}
public class CommandLineArguments {

	public Commands Command { get; set; }
	public string? SolutionFile { get; set; }
	public string? ProjectFile { get; set; }

}