using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertDialog4Unity
{
    public class Callback : AndroidJavaProxy
    {
        public System.Action<int> OnClickEvent;

        public Callback() : base("com.example.alert.Callback") { }

        public void OnClick(int whichButton)
        {
            OnClickEvent?.Invoke(whichButton);
        }
    }

    public static void Show(string desc, string positiveButtonName, string negativeButtonName, string neutralButtonName, System.Action<int> onClickEvent)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            Callback callback = new Callback();
            callback.OnClickEvent = onClickEvent;
            AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.example.alert.AlertDialog4Unity");
            androidJavaClass.CallStatic("Show", activity, desc, positiveButtonName, negativeButtonName, neutralButtonName, callback);
        }
#else
        onClickEvent?.Invoke(-1);
#endif
    }
}