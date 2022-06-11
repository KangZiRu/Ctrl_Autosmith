using System;
using System.Windows;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace Ctrl_Auto_Smelts
{
	public class Ctrl_Auto_SmeltsSubModule : MBSubModuleBase
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			try
			{
				Harmony patcher = new Harmony("Reworked_SkillsSubModulePatcher");
				patcher.PatchAll();
			}
			catch (Exception exception)
			{
				Exception exception2 = exception;
				string str = exception2.Message;
				Exception innerException = exception2.InnerException;
				bool flag = innerException != null;
				string message;
				if (flag)
				{
					message = innerException.Message;
				}
				else
				{
					message = null;
				}
				MessageBox.Show("Reworked_SkillsSubModule Error patching:\n" + str + " \n\n" + message);
			}
		}
	}
}
