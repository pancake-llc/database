using System;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public class DBEditorUtil
    {
        public static void ExitIf(bool condition, string message, params object[] parameters)
        {
            if (!condition) return;
            Exit(message, parameters);
        }

        public static void Exit(string message, params object[] parameters)
        {
            EditorUtility.DisplayDialog("Error", Format(message, parameters), "ok");
            ExitGui();
        }

        public static void ExitGui()
        {
            ResetHotControl();
            GUIUtility.ExitGUI();
        }

        public static void ResetHotControl()
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }

        public static string Format(string message, params object[] args)
        {
            if (args == null || args.Length == 0) return message;
            try
            {
                var indexOf = message.IndexOf('$');
                if (indexOf == -1) return message;

                for (var i = 0; i < 100 && indexOf >= 0; i++)
                {
                    var toReplace = "{" + i + "}";
                    message = message.Substring(0, indexOf) + toReplace + message.Substring(indexOf + 1);
                    indexOf += toReplace.Length;
                    if (indexOf >= message.Length) indexOf = -1;
                    else indexOf = message.IndexOf('$', indexOf);
                }

                return string.Format(message, args);
            }
            catch (Exception)
            {
                return message;
            }
        }
    }
}