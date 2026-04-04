<Query Kind="Program">
  <NuGetReference>FileBasedApp.Toolkit</NuGetReference>
  <NuGetReference>FileBasedApp.Toolkit.CSharp</NuGetReference>
  <Namespace>FileBasedApp.Toolkit</Namespace>
  <Namespace>FileBasedApp.Toolkit.CommandCli</Namespace>
  <Namespace>FileBasedApp.Toolkit.CSharp</Namespace>
  <Namespace>FileBasedApp.Toolkit.CSharp.Extensions</Namespace>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Roslynator</Namespace>
  <Namespace>Roslynator.CSharp</Namespace>
  <Namespace>Roslynator.CSharp.Syntax</Namespace>
  <Namespace>Roslynator.Text</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TruePath</Namespace>
</Query>

async Task Main()
{
	// The root of the github repository
	var root = AbsolutePath.Create(Util.CurrentScriptPath) / ".." / "..";
	var autoSpectrePath = root / "Src" / "AutoSpectre" / "AutoSpectre.csproj";
	
	var project = await CsharpProjectAnalysis.Init.Load(autoSpectrePath);
	var list = project.Compilation.GetNamedTypeSymbolForCurrentAssembly()
		.Where(x => x.Name.EndsWith("Attribute") || x.Name == "AutoSpectreForm")				
		.ToList();
		
	var builder = new StringBuilder();
	foreach (var element in list)
	{
		builder.AppendLine($"public class {element.Name}Names")
		.AppendLine("{");
		
		builder.AppendLine($"""public const string AttributeName = "{element.Name}";""");
		
		foreach (var p in element.GetMembers().OfType<IPropertySymbol>())
		{
			builder.AppendLine($"""public const string {p.Name} = "{p.Name}";""");			
		}
		
		builder.AppendLine("}");
		
	}
	
	builder.Dump("A dump that can be pasted into code");
		
	
}

// You can define other methods, fields, classes and namespaces here
