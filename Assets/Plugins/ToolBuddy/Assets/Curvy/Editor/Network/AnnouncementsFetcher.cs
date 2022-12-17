// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

//#define CURVY_SHOW_ALL_ANNOUNCEMENTS

using System;
using System.Linq;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace FluffyUnderware.CurvyEditor.Network
{
    /// <summary>
    /// Fetches announcements from server and display them if not previously displayed
    /// </summary>
    [InitializeOnLoad]
    class AnnouncementsFetcher
    {
        [Serializable]
        class Announcement
        {
#pragma warning disable 0649
            public string Id;
            public string Title;
            public string Content;
#pragma warning restore 0649
        }

        private UnityWebRequest WebRequest { get; set; }

        static AnnouncementsFetcher()
        {
            if (CurvyProject.Instance.EnableAnnouncements == false)
                return;

            const string preferenceName = "LastFetchedAnnouncementDate";
            int lastFetchedAnnouncementDate = CurvyProject.Instance.GetEditorPrefs(preferenceName, 17522856);// is the number of hours in the DateTime equivalent to the 1th of January 2000
            int utcNowHours = (int)(DateTime.UtcNow.Ticks / (10000L * 1000L * 3600L));
            int deltaHours = utcNowHours - lastFetchedAnnouncementDate;
#if CURVY_SHOW_ALL_ANNOUNCEMENTS == false
            if (deltaHours > 24)
#endif
            {
                new AnnouncementsFetcher().Fetch();
                CurvyProject.Instance.SetEditorPrefs(preferenceName, utcNowHours);
            }
#if CURVY_DEBUG
        else
            Debug.Log("Ignored news fetching: " + deltaHours);
#endif
        }

        private void Fetch()
        {
            string url = "https://announcements.curvyeditor.com/?version=" + CurvySpline.VERSION;

#if CURVY_DEBUG
        Debug.Log(url);
#endif

            WebRequest = UnityWebRequest.Get(url);
            WebRequest.SendWebRequest();
            EditorApplication.update += CheckWebRequest;
        }

        void CheckWebRequest()
        {
            if (WebRequest.isDone)
            {
                EditorApplication.update -= CheckWebRequest;
#if UNITY_2020_2_OR_NEWER
                if (WebRequest.result != UnityWebRequest.Result.ConnectionError
                    && WebRequest.result != UnityWebRequest.Result.ProtocolError)
#elif UNITY_2017_1_OR_NEWER
                if (WebRequest.isNetworkError == false 
                    && WebRequest.isHttpError == false)
#else
                if (WebRequest.isError == false)
#endif
                {
                    string downloadHandlerText = WebRequest.downloadHandler.text;
                    WebRequest.Dispose();
#if CURVY_DEBUG
                Debug.Log("Received: " + downloadHandlerText);
#endif
                    if (String.IsNullOrEmpty(downloadHandlerText) == false)
                        ProcessAnnouncements(downloadHandlerText);
                }
                else
                {
                    WebRequest.Dispose();
#if CURVY_DEBUG
                Debug.LogError("Error: " + WebRequest.error);
#endif
                }
            }
        }

        private static void ProcessAnnouncements(string responseText)
        {
            const string preferenceName = "ProcessedAnnouncements";
            try
            {
                SerializableArray<Announcement> announcements = JsonUtility.FromJson<SerializableArray<Announcement>>(responseText);
                string[] shownAnnouncements = CurvyProject.Instance.GetEditorPrefs(preferenceName);
                var reversedAnnouncements = announcements.Array.Reverse();//Reversed so that the first announcement's window is shown first
                int newsIndex = 0;
                foreach (Announcement announcement in reversedAnnouncements)
                {
#if CURVY_SHOW_ALL_ANNOUNCEMENTS == false
                    if (shownAnnouncements.Contains(announcement.Id) == false)
#endif
                    {
                        AnnouncementWindow.Open(announcement.Title, announcement.Content, new Vector2(newsIndex * 20, newsIndex * 20));
                        DTLog.Log(String.Format("[Curvy] Announcement: {0}: {1}", announcement.Title, announcement.Content));
                        newsIndex++;
                        CurvyProject.Instance.SetEditorPrefs(preferenceName, shownAnnouncements.Add(announcement.Id));
                    }
#if CURVY_DEBUG
                else
                    Debug.Log("Already shown announcement " + announcement.Id);

#endif
                }
            }

#if CURVY_DEBUG
        catch (ArgumentException e)// exception can be thrown by JsonUtility.FromJson
        {
            Debug.LogException(e);
        }
#else
            catch (ArgumentException)// exception can be thrown by JsonUtility.FromJson
            {
            }
#endif

        }
    }
}