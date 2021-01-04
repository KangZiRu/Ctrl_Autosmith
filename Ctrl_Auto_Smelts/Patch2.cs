using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
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

				while (maxFailSafe-- > 0 && ____currentSelectedAction != null)
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
