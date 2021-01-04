using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft.Smelting;
using TaleWorlds.InputSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft;


namespace Ctrl_Auto_Smelts
{
	[HarmonyPatch(typeof(SmeltingVM), "SmeltSelectedItems", new Type[]
	{
		typeof(Hero)
	})]
	public class Patch1
	{
		private static bool Prefix(SmeltingVM __instance, Hero currentCraftingHero, ref SmeltingItemVM ____currentSelectedItem, ref ICraftingCampaignBehavior ____smithingBehavior)
		{
			if (!shouldRun) return true;

			SmeltingItemVM currentSmeltItemVM = ____currentSelectedItem;
			if (currentSmeltItemVM != null)
			{
				ItemObject smeltedItem = currentSmeltItemVM.EquipmentElement.Item;
				CraftingResourceItemVM charcoalItemVM = null;
				foreach (var material in currentSmeltItemVM.InputMaterials)
				{
					if (material.ResourceItem.ToString().Equals("charcoal"))
					{
						charcoalItemVM = material;
					}
				}

				if (charcoalItemVM == null)
				{
					WriteLog("Invalid smelting");
					return false;
				}

				bool isValid = currentSmeltItemVM != null && ____smithingBehavior != null;
				if (isValid)
				{
					bool isCtrlHeldDown = Input.IsKeyDown(InputKey.LeftControl);
					bool isShiftHeldDown = Input.IsKeyDown(InputKey.LeftShift);
					int maxFailSafe;

					if (isCtrlHeldDown)
					{
						maxFailSafe = 100;
					}
					else if (isShiftHeldDown)
					{
						maxFailSafe = 5;
					}
					else
					{
						return true;
					}

					shouldRun = false;

					ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
					int charcoalCount = itemRoster.GetItemNumber(charcoalItemVM.ResourceItem);
					int smeltItemCount = itemRoster.GetItemNumber(smeltedItem);
					int smeltAmount = Math.Min(Math.Min(smeltItemCount * charcoalItemVM.ResourceAmount, charcoalCount), maxFailSafe); // 4 swords, 10 charcoal = 4 smelted items. 10 swords, 2 charcoal = 2 smelted items.

					bool hasRequiredMaterials = currentSmeltItemVM.InputMaterials != null && currentSmeltItemVM.InputMaterials.Count > 0;
					if (hasRequiredMaterials)
					{
						while (____currentSelectedItem != null && currentSmeltItemVM != null && smeltAmount > 0)
						{
							__instance.SmeltSelectedItems(currentCraftingHero);
							smeltAmount--;
							bool hasNoMaterialsOrSmeltingIsDone = !(currentSmeltItemVM != __instance.CurrentSelectedItem || (currentSmeltItemVM.InputMaterials?.Count > 0));
							if (hasNoMaterialsOrSmeltingIsDone)
							{
								break;
							}
						}
					}

					shouldRun = true;
				}
			}

			return false;
		}

		private static bool shouldRun = true;

		private static void WriteLog(string logging)
		{
			InformationManager.DisplayMessage(new InformationMessage(logging));
		}
	}
}
