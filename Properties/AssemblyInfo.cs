using MelonLoader;
using System.Reflection;

[assembly: AssemblyTitle(BuildInfo.ModName)]
[assembly: AssemblyCopyright($"Created by Fuar")]

[assembly: AssemblyVersion(BuildInfo.ModVersion)]
[assembly: AssemblyFileVersion(BuildInfo.ModVersion)]
[assembly: MelonInfo(typeof(FrozenFood.Implementation), BuildInfo.ModName, BuildInfo.ModVersion, BuildInfo.ModAuthor)]

//This tells MelonLoader that the mod is only for The Long Dark.
[assembly: MelonGame("Hinterland", "TheLongDark")]

internal static class BuildInfo
{
	internal const string ModName = "Frozen Food";
	internal const string ModAuthor = "Fuar";
	internal const string ModVersion = "1.5.0";
}