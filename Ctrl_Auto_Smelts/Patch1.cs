using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft.Smelting;
using TaleWorlds.InputSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Party;

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
					int smeltAmountByMaterial = Math.Min(Math.Min(smeltItemCount * charcoalItemVM.ResourceAmount, charcoalCount), maxFailSafe); // 4 swords, 10 charcoal = 4 smelted items. 10 swords, 2 charcoal = 2 smelted items.

					int energyCostForSmelting = Campaign.Current.Models.SmithingModel.GetEnergyCostForSmelting(smeltedItem, currentCraftingHero);
					int currentStamina = ____smithingBehavior.GetHeroCraftingStamina(currentCraftingHero);
					int maxSmeltAmountByStamina = energyCostForSmelting > 0 ? currentStamina / energyCostForSmelting : maxFailSafe;

					int staminaRemaining = currentStamina - (maxSmeltAmountByStamina * (energyCostForSmelting - 1));
					if (staminaRemaining <= 10)
					{
						maxSmeltAmountByStamina--;
						if (staminaRemaining >= energyCostForSmelting && staminaRemaining < 10)
						{
							maxSmeltAmountByStamina--;
						}
					}

					int smeltAmount = Math.Min(smeltAmountByMaterial, maxSmeltAmountByStamina);

					WriteLog($"Smelt amount: {smeltAmount}");

					bool hasRequiredMaterials = currentSmeltItemVM.InputMaterials != null && currentSmeltItemVM.InputMaterials.Count > 0;
					
					if (hasRequiredMaterials)
					{
						while (____currentSelectedItem != null && currentSmeltItemVM != null && smeltAmount > 0)
						{
							__instance.TrySmeltingSelectedItems(currentCraftingHero);
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
