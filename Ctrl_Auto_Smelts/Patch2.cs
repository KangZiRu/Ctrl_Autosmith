using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft.Refinement;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;


namespace Ctrl_Auto_Smelts
{
	[HarmonyPatch(typeof(RefinementVM), "ExecuteSelectedRefinement", new Type[]
	{
		typeof(Hero)
	})]
	public class Patch2
	{
		private static bool Prefix(RefinementVM __instance, Hero currentCraftingHero, ref RefinementActionItemVM ____currentSelectedAction)
		{
			if (!shouldRun) return true;

			bool isValid = ____currentSelectedAction != null;
			if (isValid)
			{
				bool isCtrlHeldDown = Input.IsKeyDown(InputKey.LeftControl);
				bool isShiftHeldDown = Input.IsKeyDown(InputKey.LeftShift);

				int maxRefine;

				if (isCtrlHeldDown)
				{
					maxRefine = 100;
				}
				else if (isShiftHeldDown)
				{
					maxRefine = 5;
				}
				else
				{
					return true;
				}

				shouldRun = false;

				var smithingBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();

				var refineFormula = ____currentSelectedAction.RefineFormula;
				int energyCostForRefining = Campaign.Current.Models.SmithingModel.GetEnergyCostForRefining(ref refineFormula, currentCraftingHero);
				int currentStamina = smithingBehavior.GetHeroCraftingStamina(currentCraftingHero);
				int maxRefineAmountByStamina = energyCostForRefining > 0 ? currentStamina / energyCostForRefining : maxRefine;

				int staminaRemaining = currentStamina - (maxRefineAmountByStamina * (energyCostForRefining - 1));
				if (staminaRemaining <= 10)
				{
					maxRefineAmountByStamina--;
					if (staminaRemaining >= energyCostForRefining && staminaRemaining < 10)
					{
						maxRefineAmountByStamina--;
					}
				}

				maxRefine = Math.Min(maxRefine, maxRefineAmountByStamina);

				WriteLog($"Refine amount: {maxRefine}");

				while (maxRefine-- > 0 && ____currentSelectedAction != null)
				{
					__instance.ExecuteSelectedRefinement(currentCraftingHero);
				}

				shouldRun = true;
			}

			return false;
		}

		private static bool shouldRun = true;

		private static void WriteLog(string logging)
		{
			InformationManager.DisplayMessage(new InformationMessage(logging));
			// File.AppendAllText(@"c:\Prajna\bannerlord.txt", logging + Environment.NewLine);
		}
	}
}
