﻿using System;
using MagicLeap.SetupTool.Editor.Interfaces;
using MagicLeap.SetupTool.Editor.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MagicLeap.SetupTool.Editor.Setup
{
	public class SetScriptingBackendStep: ISetupStep
	{
		//Localization
		private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";
		private const string CONDITION_MET_LABEL = "Done";
		private const string SET_SCRIPTING_BACKEND_LABEL = "Set IL2CPP scripting backend";
		
		private bool _correctScriptingBackend;
		public bool CanExecute => EnableGUI();
		/// <inheritdoc />
		public Action OnExecuteFinished { get; set; }
		public bool Block => false;
		
		/// <inheritdoc />
		public bool Required => true;
		
		/// <inheritdoc />
		public bool Busy { get; private set; }
		/// <inheritdoc />
		public bool IsComplete => _correctScriptingBackend;
		/// <inheritdoc />
		public void Refresh()
		{
#if UNITY_2023_1_OR_NEWER
			_correctScriptingBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP;
#else
			_correctScriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
#endif

		}

		private bool EnableGUI()
		{
			var correctBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && XRPackageUtility.IsMagicLeapSDKInstalled;
			return correctBuildTarget;
		}
		/// <inheritdoc />
		public bool Draw()
		{
			GUI.enabled = EnableGUI();
			if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_SCRIPTING_BACKEND_LABEL, _correctScriptingBackend, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Busy = true;
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute() {
			if (IsComplete)
			{
#if ML_SETUP_DEBUG
				Debug.Log($"Not executing step: {this.GetType().Name}.IsComplete: {IsComplete} || Busy: {Busy}");
#endif
				Busy = false;
				return;
			}
#if UNITY_2023_1_OR_NEWER
			PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
			_correctScriptingBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP;
#else
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
			_correctScriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
#endif

			Busy = false;
			OnExecuteFinished?.Invoke();
#if ML_SETUP_DEBUG
			Debug.Log($"{this.GetType().Name} finished.");
#endif
		}

				
		/// <inheritdoc cref="ISetupStep.ToString"/>
		public override string ToString()
		{
			var info =$"Step: {this.GetType().Name}, CanExecute: {CanExecute}, Busy: {Busy}, IsComplete: {IsComplete}";
            
			if (!EnableGUI())
			{
				var correctBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
				var hasSdkInstalled =  XRPackageUtility.IsMagicLeapSDKInstalled;
				info += "\nDisabling GUI: ";
				if (!correctBuildTarget)
				{
					info += "[not the correct build target], ";
				}
				if (!hasSdkInstalled)
				{
					info += "[Package is not installed]";
				}
			}
			
#if UNITY_2023_1_OR_NEWER
			info += $"\nMore Info: CorrectScriptingBackend: {_correctScriptingBackend} | {PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}";

#else
			info += $"\nMore Info: CorrectScriptingBackend: {_correctScriptingBackend} | {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}";
#endif


			return info;
		}
	}
}
