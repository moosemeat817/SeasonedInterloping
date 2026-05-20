using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.AddressableAssets;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Scenes;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Il2CppCollection = Il2CppSystem.Collections.Generic;

namespace SeasonedInterloping;

[HarmonyPatch]
internal class Patches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.Enable), [typeof(bool)])]
	private static void Postfix(Panel_SelectExperience __instance)
	{
		foreach (var item in __instance.m_MenuItems)
		{
			SandboxConfig sandboxConfig = item.m_SandboxConfig;
			if (sandboxConfig.m_XPMode.m_ModeType == ExperienceModeType.Interloper)
			{
				AddMoreRegionOptions(sandboxConfig);
			}
		}
	}

	private static void AddMoreRegionOptions(SandboxConfig sandboxConfig)
	{
		RegionSpecification[] allRegions = FindRegions();
		foreach (string regionName in (ReadOnlySpan<string>)["AirfieldRegion", "CanneryRegion", "MiningRegion", "MountainPassRegion"])
		{
			EnsureRegionAddedToAvailableStartOptions(sandboxConfig, regionName, allRegions);
		}
	}

	private static void EnsureRegionAddedToAvailableStartOptions(SandboxConfig sandboxConfig, string regionName, RegionSpecification[] allRegions)
	{
		Il2CppReferenceArray<RegionSpecification> availableStartRegions = sandboxConfig.m_AvailableStartRegions;
		bool alreadyInserted = false;
		foreach (RegionSpecification region in availableStartRegions)
		{
			if (region.name == regionName)
			{
				alreadyInserted = true;
				break;
			}
		}
		if (!alreadyInserted)
		{
			foreach (RegionSpecification region in allRegions)
			{
				if (region.name == regionName)
				{
					RegionSpecification[] newArray = new RegionSpecification[availableStartRegions.Length + 1];
					availableStartRegions.CopyTo(newArray, 0);
					newArray[availableStartRegions.Length] = region;
					sandboxConfig.m_AvailableStartRegions = newArray;
				}
			}
		}
	}

	private static Il2CppCollection.List<IResourceLocation> FindRegionLocations()
	{
		return AssetHelper.FindAllAssetsLocations<RegionSpecification>().Cast<Il2CppCollection.List<IResourceLocation>>();
	}

	private static RegionSpecification[] FindRegions()
	{
		Il2CppCollection.List<IResourceLocation> regionList = FindRegionLocations();
		RegionSpecification[] regionArray = new RegionSpecification[regionList.Count];
		for (int i = regionList.Count - 1; i >= 0; i--)
		{
			RegionSpecification region = Addressables.ResourceManager.ProvideResource<RegionSpecification>(regionList[i]).WaitForCompletion();
			regionArray[i] = region;
		}
		return regionArray;
	}
}