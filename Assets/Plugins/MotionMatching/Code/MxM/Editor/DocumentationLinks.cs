using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MxM
{
    public static class MxMDocumentationLinks
    {
        public static string AssetStore = "https://assetstore.unity.com/packages/tools/animation/motion-matching-for-unity-145624";
        public static string Discord = "https://discord.gg/CFyhERt";
        public static string IssueTracker = "https://github.com/Avindr/MxM-IssueTracking";
        public static string QuickStart = "https://docs.google.com/document/d/1EWlYWT4mFi9d895J6P7GBOeDQXM-HK5wjqgY3w9hu3w/edit?usp=sharing";
        public static string UserManual = "https://docs.google.com/document/d/1zBdEQh8nJyOkWnKS5fAwchaE7u0kEvl4fSxlR7S2Wfk/edit?usp=sharing";
        public static string AnimAuth = "https://docs.google.com/document/d/116VnMJEkSyHuVOESaIhdd4KAvUJ6OWFDWhrKZnbrPDc/edit?usp=sharing";
        public static string FAQ = "https://docs.google.com/document/d/1-aLFObNXfIibj8Cz4y12QVrdZsJkIDKvBp0ygNfZjvg/edit?usp=sharing";
        public static string YT_Tutorial = "https://www.youtube.com/playlist?list=PLV3KWwaS27WJqBeZCOVp1h5VKTshdrQrk";
        public static string YT_Tips = "https://www.youtube.com/playlist?list=PLV3KWwaS27WJgoK9UXDjrWRoqowJ7ZiUd";
        public static string YT_Walkthrough = "https://www.youtube.com/playlist?list=PLV3KWwaS27WJ5qLcpuVmeyM4fOJnXOQno";

        [MenuItem("Tools/MxM/Support/AssetStore")]
        public static void OpenAssetStorePage() { Application.OpenURL(AssetStore); }

        [MenuItem("Tools/MxM/Support/Discord")]
        public static void OpenDiscord() { Application.OpenURL(Discord); }

        [MenuItem("Tools/MxM/Support/Issue Tracker (Bug Reports)")]
        public static void OpenIssueTracker() { Application.OpenURL(IssueTracker); }

        [MenuItem("Tools/MxM/Documentation/Quick Start Guide")]
        public static void OpenQuickStartGuide() { Application.OpenURL(QuickStart); }

        [MenuItem("Tools/MxM/Documentation/User Manual")]
        public static void OpenUserManual() { Application.OpenURL(UserManual); }

        [MenuItem("Tools/MxM/Documentation/Animation Guidelines")]
        public static void OpenAnimationGuidelines() { Application.OpenURL(AnimAuth); }

        [MenuItem("Tools/MxM/Documentation/FAQ")]
        public static void OpenFAQ() { Application.OpenURL(FAQ); }

        [MenuItem("Tools/MxM/Youtube/General Tutorials")]
        public static void OpenGeneralYTTutorials() { Application.OpenURL(YT_Tutorial); }

        [MenuItem("Tools/MxM/Youtube/QuickTips")]
        public static void OpenGeneralYTTips() { Application.OpenURL(YT_Tips); }

        [MenuItem("Tools/MxM/Youtube/Gameplay Walkthrough (WIP)")]
        public static void OpenGeneralYTWalkthrough() { Application.OpenURL(YT_Walkthrough); }

    }
}