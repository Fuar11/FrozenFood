using FrozenFood.Utils;
using MelonLoader;

namespace FrozenFood;
internal sealed class Implementation : MelonMod
{
    internal static SaveDataManager sdm = new SaveDataManager();
    public override void OnInitializeMelon()
	{
		MelonLogger.Msg("Frozen Foods online!");
	}
}
