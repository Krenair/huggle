﻿//This is a source code or part of Huggle project
//
//This file contains code for core

/// <DOCUMENTATION>
/// There is no documentation for this
/// </DOCUMENTATION>

//Copyright (C) 2011-2012 Huggle team
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace huggle3
{
    public static class Languages
    {
        public static string Get(string id)
        {
            try
            {
                if (id.Contains("["))
                {
                    id = id.Replace("]", "");
                    id = id.Replace("[", "");
                }
                // return string
                if (Config.Messages.ContainsKey(Config.DefaultLanguage) != true && Config.Messages.ContainsKey(Config.Language) != true)
                {
                    return "<invalid language:" + id + ">";
                }
                if (Config.Messages[Config.Language].ContainsKey(id) == false)
                { // if there is no such a language it returns the english one
                    if (Config.Messages.ContainsKey(Config.DefaultLanguage) != true)
                    {
                        return "<not present in dict>" + id;
                    }
                    if (Config.Messages[Config.DefaultLanguage].ContainsKey(id))
                    {
                        return Config.Messages[Config.DefaultLanguage][id];
                    }
                    else
                    {
                        return "<invalid string:" + id + ">";
                    }
                }
                else
                {
                    // got it
                    if (Config.Messages[Config.Language].ContainsKey(id))
                    {
                        return Config.Messages[Config.Language][id];
                    }
                    return "<invalid string:" + id + ">";
                }
            }
            catch (Exception A)
            {
                if (Config.devs)
                {
                    // This is ignored on production build
                    Core.ExceptionHandler(A);
                }
            }
            return "<invalid> " + id;
        }
    }
    public static class Core
    {
        public static class Threading
        {
            private static int ThreadLast = 0;
            private static int ThreadCount = 0;
            private static List<ThreadS> ThreadList = new List<ThreadS>();
            private class ThreadS
            {
                public string Decription;
                public System.Threading.Thread handle;
                public bool Active = false;
            }

            /// <summary>
            /// This is only called when exiting
            /// </summary>
            /// <returns></returns>
            public static bool DestroyCore()
            {
                // All threads are aborted (usually when application die)
                int curr = 0;
                while (curr < Core.MThread)
                {
                    KillThread(curr);
                    curr++;
                }
                return true;
            }

            /// <summary>
            /// Kill thread id
            /// </summary>
            /// <param name="N"></param>
            /// <returns></returns>
            public static bool KillThread(int N)
            {
                // request thread to be aborted
                if (ThreadList[N].Active == false)
                {
                    return false;
                }
                if (ThreadList[N].handle.ThreadState == System.Threading.ThreadState.Aborted)
                {
                    // it's already down
                    ThreadList[N].Active = false;
                    ThreadList[N].Decription = "";
                    ThreadCount = ThreadCount - 1;
                    return false;
                }
                ThreadList[N].handle.Abort();
                ThreadList[N].Active = false;
                ThreadList[N].Decription = "";
                ThreadCount = ThreadCount - 1;
                return true;
            }

            /// <summary>
            /// Insted of kill thread, safe method to release handle of thread which is about to be aborted, should be called as last action of thread
            /// </summary>
            /// <param name="Thread"></param>
            public static void ReleaseHandle(int Thread)
            {
                if (ThreadList[Thread].Active == true)
                {
                    ThreadList[Thread].handle = null;
                    ThreadList[Thread].Active = false;
                    ThreadCount = ThreadCount - 1;
                }
            }

            /// <summary>
            /// Create a thread with name
            /// </summary>
            /// <param name="ThreadStart">Callback function</param>
            /// <param name="name"></param>
            /// <returns></returns>
            public static int CreateThread(System.Threading.ThreadStart ThreadStart, string name)
            {
                try
                {
                    ThreadLast++;
                    int ThreadID = ThreadLast;
                        while (ThreadList[ThreadID].Active != false)
                        {
                            if (ThreadID > Core.MThread || ThreadID > ThreadList.Count)
                            {
                                ThreadID = 0;
                            }
                            else
                            {
                                ThreadID++;
                            }
                        }
                    ThreadLast = ThreadID;
                    ThreadList[ThreadID].Active = true;
                    ThreadList[ThreadID].Decription = name;
                    ThreadList[ThreadID].handle = new System.Threading.Thread(ThreadStart);
                    ThreadList[ThreadID].handle.Name = name;
                    ThreadCount = ThreadCount  + 1;
                    return ThreadID;
                }
                catch (Exception A)
                {
                    Core.ExceptionHandler(A);
                    return -1;
                }
            }

            /// <summary>
            /// Create thread with no name
            /// </summary>
            /// <param name="ThreadStart">Callback function</param>
            /// <returns></returns>
            public static int CreateThread(System.Threading.ThreadStart ThreadStart)
            {
                try
                {
                    ThreadLast++;
                    int ThreadID = ThreadLast;
                    while (ThreadList[ThreadID].Active != false)
                    {
                        if (ThreadID > Core.MThread)
                        {
                            ThreadID = 0;
                        }
                        else
                        {
                            ThreadID++;
                        }
                    }
                    ThreadLast = ThreadID;
                    ThreadList[ThreadID].Active = true;
                    ThreadList[ThreadID].Decription= "Huggle";
                    ThreadList[ThreadID].handle = new System.Threading.Thread(ThreadStart);
                    return ThreadID;
                }
                catch (Exception A)
                {
                    return -1;
                }
            }

            /// <summary>
            /// Thread manager core
            /// </summary>
            public static void ManagerThread()
            {
                System.Threading.Thread.Sleep(2000);
            }

            /// <summary>
            /// Only used for initialisation of the huggle
            /// </summary>
            public static void CreateList()
            {
                ThreadList.Clear();
                int curr = 0;
                while (curr < Core.MThread)
                {
                    curr++;
                    ThreadList.Add(new ThreadS());
                }
            }

            /// <summary>
            /// Thread count
            /// </summary>
            public static int ThCount
            {
                get { return ThreadCount; }
            }

            /// <summary>
            /// Return windows / linux handle of thread
            /// </summary>
            /// <param name="N"></param>
            /// <returns></returns>
            public static System.Threading.Thread GetHandle(int N)
            {
                return ThreadList[N].handle;
            }

            /// <summary>
            /// Start
            /// </summary>
            /// <param name="ID"></param>
            public static void Execute(int ID)
            {
                if (ThreadList[ID].Active == true)
                {
                    Core.WriteLog("Starting thread: " + ThreadList[ID].Decription);
                    ThreadList[ID].handle.Start();
                    return;
                }
                Core.WriteLog("Unable to start the thread, non existend handle");
            }
        }

        /// <summary>
        /// Container for system log which is in memory
        /// </summary>
        public static List<string> SystemLog = new List<string>();
        /// <summary>
        /// Container for history
        /// </summary>
        private static string _history = "";

        /// <summary>
        /// For exception handler
        /// </summary>
        private static Exception core_er;

        /// <summary>
        /// Time when system was loaded
        /// </summary>
        private static DateTime Uptime;

        /// <summary>
        /// Custom reverts
        /// </summary>
        public static Dictionary<Page, string> CustomReverts = new Dictionary<Page,string>();
        /// <summary>
        /// Current queue
        /// </summary>
        public static Queue Current_Queue;
        /// <summary>
        /// Return true in case of fatal error when core is stopped
        /// </summary>
        public static bool Interrupted = false;
        /// <summary>
        /// Edit token
        /// </summary>
        public static string EditToken;
        public static bool HidingEdit = false;
        public static System.DateTime LastRCTime = new System.DateTime();
        public static Edit NullEdit = new Edit();
        public static string Patrol_Token;
        public static System.Threading.Thread MainThread;
        public static string[] months;

        public static System.Diagnostics.Process Process;

        public const int MThread=600; // Maximum number of threads in core

        /// <summary>
        /// Should contain list of all static arrays of objects accessible everywhere
        /// </summary>
        public struct All
        {
            public static List<Space> spaces = new List<Space>();
        }

        /// <summary>
        /// Threads which are not managed by core
        /// </summary>
        public struct SpecialThreads
        {
            public static System.Threading.Thread RecoveryThread;
            public static System.Threading.Thread ThreadManager;
        }

        /// <summary>
        /// Represent a block
        /// </summary>
        public class Block
        {
            public System.DateTime Block_Date;
            public string Block_Comment;
            public string Block_Duration;
            public string Block_Action;
            public user Block_Sysop;
            public user Block_User;
        }

        /// <summary>
        /// Used for cache
        /// </summary>
        public class CacheData
        {
            public Edit Edit;
            public string Text;
        }

        /// <summary>
        /// History item
        /// </summary>
        public class HistoryItem
        {
            public string text;
            public string url;
            public Edit Edit;
            public HistoryItem(string _url)
            {
                this.url = _url;
            }

            public HistoryItem(Edit _edit)
            {
                this.Edit = _edit;
            }
        }

        /// <summary>
        /// Handle for page move
        /// </summary>
        public class PageMove
        {
            public System.DateTime Date;
        }

        /// <summary>
        /// Represent a protection of page
        /// </summary>
        public class Protection
        {
            public bool Cascading;
            public bool Pending;
            public string MoveLevel;
            public string CreateLevel;
            public string Action;
            public string Summary;
            public string EditLevel;
            public System.DateTime Date;
            public user Sysop;
        }

        /// <summary>
        /// Command
        /// </summary>
        public class Command
        {
            public Edit Edit;
            public string Description;
            public user User;
            public Page Page;
        }

        /// <summary>
        /// Type
        /// </summary>
        enum CommandType
        {
            Revert,
            Report,
            Warning,
            Edit,
            Ignore,
        }

        /// <summary>
        /// Contains data of every edit to pages
        /// </summary>
        public class EditData
        {
            public Edit Edit;
            public Page Page;
            public string CaptchaWord; // deprecated
            public string CaptchaId;
            public string Text;
            public string Summary;
            public string Section;
            public string Token;
            public string EditTime;
            public bool Error;
            public bool Creating;
            public bool Minor;
            public bool CannotUndo;
            public bool Watch;
            public bool AutoSummary;
        }

        /// <summary>
        /// Upload info
        /// </summary>
        public class Upload
        {
            public user User;
            public Page File;
        }

        /// <summary>
        /// Warning
        /// </summary>
        public class Warning
        {
            public System.DateTime Date;
            public user User;
            
        }

        /// <summary>
        /// Return path of local user config
        /// </summary>
        /// <returns></returns>
        public static string LocalPath()
        {
            return Application.LocalUserAppDataPath + "huggle3" + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Called only for huggle shutdown, used after cleaning up the stuff
        /// </summary>
        public static void ShutdownSystem()
        {
            Core.Threading.DestroyCore();
            if (SpecialThreads.RecoveryThread != null)
            {
                if (SpecialThreads.RecoveryThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    SpecialThreads.RecoveryThread.Abort();
                }
            }
            Application.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool DebugLog(string text)
        {
            Debug.WriteLine(text);
            WriteLog("DEBUG: " + text);
            return true;
        }

        /// <summary>
        /// Debugging tool
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool History(string text)
        {
            if (_history.Length - Config.HistoryTrim > Config.HistoryLenght)
            {
                _history = _history.Substring(Config.HistoryTrim);
                _history = "{trimmed} " + _history;
            }
            _history = _history + " -> " + text;
            return false;
        }

        /// <summary>
        /// Callback function
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="PostdData"></param>
        public static void callback(System.Threading.SendOrPostCallback Target, Object PostdData)
        {
            Core.History("callback()");
        }

        /// <summary>
        /// This function look up a string
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="from1"></param>
        /// <param name="from2"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        public static string FindString(string Source, string from1, string from2, string To)
        {
            //
            Core.History("FindString(string, string, string, string)");
            if (Source == null)
            {    return null; }
            
            if (Source.Contains(from1) != true)
            {
                return null;
            }

            Source = Source.Substring(Source.IndexOf(from1) + from1.Length);
            if (Source.Contains(from2) != true)
            {
                return null;
            }
            Source = Source.Substring(Source.IndexOf(from2) + from2.Length);
            if (Source.Contains(To))
            {
                return Source.Substring(0, Source.IndexOf(To));
            }
            return "";
        }

        /// <summary>
        /// This function look up a string in string between other strings
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="from1"></param>
        /// <param name="from2"></param>
        /// <param name="from3"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        public static string FindString(string Source, string from1, string from2, string from3, string To)
        {
            // same one
            Core.History("FindString(string, string, string, string, string)");
            string temp_v = Source;
            Source = null;
            if (temp_v.Contains(from1) != true)
            {
                return null;
            }
            temp_v = temp_v.Substring(temp_v.IndexOf(from1) + from1.Length);
            if (temp_v.Contains(from2) != true)
            {
                return null;
            }
            temp_v = temp_v.Substring(temp_v.IndexOf(from2) + from2.Length);
            if (temp_v.Contains(from3) != true)
            {
                return null;
            }
            if (temp_v.Contains(To))
            {
                return temp_v.Substring(0, Source.IndexOf(To));
            }
            return "";
        }

        public static bool RegisterPlugin(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    System.Reflection.Assembly library = System.Reflection.Assembly.LoadFrom(path);
                    if (library == null)
                    {
                        Core.DebugLog("Unable to load " + path + " because the file can't be read");
                        return false;
                    }
                    Type[] types = library.GetTypes();
                    Type type = library.GetType("huggle3.RestrictedPlugin");
                    Type pluginInfo = null;
                    foreach (Type curr in types)
                    {
                        if (curr.IsAssignableFrom(type))
                        {
                            pluginInfo = curr;
                            break;
                        }
                    }
                    if (pluginInfo == null)
                    {
                        Core.DebugLog("Unable to load " + path + " because the library contains no module");
                        return false;
                    }
                    Plugin _plugin = (Plugin)Activator.CreateInstance(pluginInfo);
                    lock (Plugin.ExtensionList)
                    {
                        if (Plugin.ExtensionList.Contains(_plugin))
                        {
                            Core.DebugLog("Unable to load extension because the handle is already known to core");
                            return false;
                        }
                        bool problem = false;
                        foreach (Plugin x in Plugin.ExtensionList)
                        {
                            if (x.Name == _plugin.Name)
                            {
                                Core.WriteLog("Unable to load the extension, because the extension with same name is already loaded");
                                _plugin.status = Plugin.Status.Terminated;
                                problem = true;
                                break;
                            }
                        }
                        if (problem)
                        {
                            if (Plugin.ExtensionList.Contains(_plugin))
                            {
                                Plugin.ExtensionList.Remove(_plugin);
                            }
                        }
                        Core.WriteLog("Everything is fine, registering " + _plugin.Name);
                        Plugin.ExtensionList.Add(_plugin);
                    }
                    if (_plugin.Hook_RegisterSelf())
                    {
                        _plugin.Load();
                        _plugin.status = Plugin.Status.Active;
                        Core.WriteLog("Finished loading of module " + _plugin.Name);
                        return true;
                    }
                    else
                    {
                        Core.WriteLog("SYSTEM: failed to run OnRegister for " + _plugin.Name);
                    }
                    return false;
                }
            }
            catch (Exception fail)
            {
                Core.ExceptionHandler(fail);
            }
            return false;
        }

        /// <summary>
        /// This function look up a string in string between other strings
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="from"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        public static string FindString(string Source, string from, string To)
        {
            Core.History("FindString(Source, string, To)");
            if (Source == null)
            {
                return "";
            }
            if (Source.Contains(from))
            {
                Source = Source.Substring(Source.IndexOf(from) + from.Length);
                if (Source.Contains(To) == false)
                {
                    return "";
                }
                return Source.Substring(0, Source.IndexOf(To));
            }
            return "";
        }

        /// <returns>Target build</returns>
        public static string TargetBuild()
        {
            Core.History("TargetBuild()");
            switch (Config._Platform)
            { 
                case Config.platform.windows32:
                    return "Windows x86";
                case Config.platform.linux32:
                    return "Linux x86";
                case Config.platform.macos32:
                    return "MacOS x86";
                case Config.platform.linux64:
                    return "Linux x64";
                case Config.platform.windows64:
                    return "Windows x64";
            }
            return "<unknown build>";
        }

        /// <summary>
        /// Format a page to html
        /// </summary>
        /// <param name="Page"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string FormatHTML(Page Page, string Text)
        {
            History("Core.FormatHTML (" + Page + " )");
            try
            {
                string return_value = "";
                if (Text.Contains("<!-- start content -->") && Text.Contains("<!-- end content -->") && Text != "")
                {
                    return_value = Text.Substring(Text.IndexOf("<!-- start content -->"));
                    return_value = return_value.Substring(0, return_value.IndexOf("<!-- end content -->"));
                }
                else if (Text.Contains("<!-- content -->") && Text.Contains("<!-- mw_content -->"))
                {
                    return_value = Text.Substring(Text.IndexOf("<!-- content -->"));
                    return_value = return_value.Substring(0, return_value.IndexOf("<!-- mw_content -->"));
                }
                else if (Text.Contains("</h1>") && Text.Contains("<div class=\"printfooter\">"))
                {
                    return_value = Text.Substring(Text.IndexOf("</h1>"));
                    return_value = return_value.Substring(0, return_value.IndexOf("<div class=\"printfooter\">"));
                }

                if (Text.Contains("<script>") && Text.Contains("</script>"))
                {

                }
                return_value = "<h1>" + Page.Name + "</h1>" + return_value;

                return return_value;
            }
            catch (Exception B)
            { Core.ExceptionHandler(B); }
            return "";
        }

        /// <summary>
        /// Convert to wiki page
        /// </summary>
        /// <param name="text_html"></param>
        /// <returns></returns>
        public static string HtmltoWikitext(string text_html)
        {
            Core.History("HtmltoWikitext(text)");
            if (text_html.EndsWith(")") && text_html.StartsWith("("))
            {
                text_html = text_html.Substring(1, text_html.Length - 2);
                while (text_html.Contains("</a>") && text_html.Contains("<a href="))
                {
                    string target = System.Web.HttpUtility.HtmlDecode(Core.FindString(text_html, "<a href=", "title=\"", "\""));
                    string text = System.Web.HttpUtility.HtmlDecode(Core.FindString(text_html, "<a href=>", ">", "</a>"));

                    if (text == target)
                    {
                        text_html = text_html.Substring(0, text_html.IndexOf("<a href=")) + "[[" + text + "]]" + Core.FindString(text_html, "</a>");
                    }
                    else
                    {
                        text_html = text_html.Substring(0, text_html.IndexOf("<a href=")) + "[[" + target + "|" +text + "]]" + Core.FindString(text_html, "</a>");
                    }
                }
            }
            return CleanupHTML(text_html);
        }

        /// <summary>
        /// Check if page is mediawiki page
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static bool IsMW(string Content)
        {
            if (Content == null)
            {
                return false;
            }
            return System.Text.RegularExpressions.Regex.Match(Content, "<body class=.mediawiki").Success;
        }

        /// <summary>
        /// Strip html
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CleanupHTML(string data)
        {
            string return_value = data;
            if (data == null)
            {
                return data;
            }
            while (return_value.Contains("<") && return_value.Contains(">"))
            {
                return_value = return_value.Substring(0, return_value.IndexOf("<")) + return_value.Substring(return_value.IndexOf(">" + 1));
            }
            return return_value;
        }

        /// <summary>
        /// This function look up a string in string between other strings
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="From"></param>
        /// <returns></returns>
        public static string FindString(string Source, string From)
        {
            Core.History("Core.FindString( string, string)");
            if (From == null)
            {
                return "";
            }
            if (Source == null)
            {
                return "";
            }
            if (Source.Contains(From))
            {
                return Source.Substring(Source.IndexOf(From) + From.Length);
            }

            return "";
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="Name">User name</param>
        /// <returns></returns>
        public static user GetUser(string Name)
        {
            Core.History("GetUser()");
            return new user(Name);
        }

        /// <summary>
        /// Get month
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string Get_Month_Name(int i)
        {
            Core.History("Get_Month_Name(int)");
            if (i < 1 || i > 12)
            {
                return "";
            }else
            {
                return months[i];
            }
        }

        /// <summary>
        /// Parameter
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="param">String</param>
        /// <returns></returns>
        public static string Get_Parameter(string source, string param)
        {
            Core.History("GetParameter()");
            string return_value = "";
            try
            {
                if (source == null)
                {
                    return null;
                }
                if (source.Contains(param + "=\"") == false)
                {
                    return "";
                }
                return_value = source.Substring(source.IndexOf(param + "=\"") + (param + "=\"").Length);
                if (return_value.Contains("\""))
                {
                    return "";
                }
                return_value = return_value.Substring(0, return_value.IndexOf("\""));
                return System.Web.HttpUtility.HtmlDecode(return_value);
            }
            catch (Exception A)
            {
                Core.ExceptionHandler(A);
                return null;
            }
        }

        /// <summary>
        //// this function initialise config
        //// reset values
        //// those values will be default in case that not present in configs
        //// do not change unless you want to change default presets
        /// </summary>
        /// <returns></returns>
        public static bool InitConfig()
        {   
            Core.History("Core.InitConfig()");
            Config.Whitelist.Clear();
            Config.Approval = false;
            Config.AutoAdvance = false;
            Config.AutoReport = false;
            Config.AutoWarn = false;
            Config.AutoWhitelist = true;
            Config.Block = false;
            Config.BlockMessage = "";
            Config.BlockMessageDefault = false;
            Config.BlockReason = "Vandalism";
            Config.BlockSummary = "Blocked";
            Config.BlockTime = "0";
            Config.BlockTimeAnon = "0";
            Config.ConfigChanged = false;
            Config.ConfigSummary = "";
            Config.ConfirmIgnored = false;
            Config.ConfirmMultiple = false;
            Config.ConfirmPage = false;
            Config.ConfirmRange = false;
            Config.ConfirmSame = false;
            Config.ConfirmSelfRevert = true;
            Config.ConfirmWarned = false;
            Config.DefaultSummary = "";
            Config.Delete = false;
            Config.Email = false;
            Config.Enabled = false;
            Config.EnabledForAll = false;
            Config.ExtendReports = false;
            Config.FeedbackLocation = "WP:Huggle/Feedback";
            Config.MonthHeadings = true;
            Config.Password = "";
            Config.Patrol = true;
            Config.PatrolSpeedy = true;
            Config.Project = "";
            Config.ProtectionReason = "";
            Config.ProtectionTime = "";
            Config.ProxyEnabled = false;
            Config.ProxyPort = "";
            Config.QuickSight = false;
            Config.RememberMe = false;
            Config.RememberPassword = false;
            Config.DefaultLanguage = "en";
            Core.months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            Core_IO.PostLoad();
            return true;
        }

        /// <summary>
        /// Return history
        /// </summary>
        public static string history
        {
            get { return _history; }
        }

        /// <summary>
        /// Languages init
        /// </summary>
        public static void LoadLanguages()
        {
            Config.Messages.Clear();
            Core.History("Core.LoadLanguages()");
            Load_Language("bg", huggle3.Properties.Resources.bg);
            Load_Language("de", huggle3.Properties.Resources.de);
            Load_Language("en", huggle3.Properties.Resources.en);
            Load_Language("es", huggle3.Properties.Resources.es);
            Load_Language("fa", huggle3.Properties.Resources.fa);
            Load_Language("fr", huggle3.Properties.Resources.fr);
            Load_Language("hi", huggle3.Properties.Resources.hi);
            Load_Language("it", huggle3.Properties.Resources.it);
            Load_Language("ja", huggle3.Properties.Resources.ja);
            Load_Language("ka", huggle3.Properties.Resources.ka);
            Load_Language("kn", huggle3.Properties.Resources.kn);
            Load_Language("ml", huggle3.Properties.Resources.ml);
            Load_Language("mr", huggle3.Properties.Resources.mr);
            Load_Language("nl", huggle3.Properties.Resources.nl);
            Load_Language("no", huggle3.Properties.Resources.no);
            Load_Language("oc", huggle3.Properties.Resources.oc);
            Load_Language("or", huggle3.Properties.Resources.or);
            Load_Language("pt", huggle3.Properties.Resources.pt);
            Load_Language("ptb", huggle3.Properties.Resources.ptb);
            Load_Language("ru", huggle3.Properties.Resources.ru);
            Load_Language("sv", huggle3.Properties.Resources.sv);
            Load_Language("zh", huggle3.Properties.Resources.zh);
            Load_Language("ar", huggle3.Properties.Resources.ar);
        }

        /// <summary>
        /// Site path
        /// </summary>
        /// <returns></returns>
        public static string SitePath()
        {
            // return site path
            return Config.Projects[Config.Project] + Config.WikiPath;
        }

        /// <summary>
        /// Return a new page
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Page Get_NewPage(string name)
        {
            // create new page
            Page NewPage = new Page(name);
            if (name == null)
            {
                return null;
            }
            return NewPage;
        }

        /// <summary>
        /// stop everything in system, used for recovery
        /// </summary>
        /// <returns></returns>
        public static bool StopAll()
        {
            return false;
        }

        /// <summary>
        /// Initialise language
        /// </summary>
        /// <param name="language"></param>
        /// <param name="data"></param>
        public static void Load_Language(string language, string data)
        {
            WriteLog("Loading data: " + language);
            if (Config.Languages.Contains(language) == false)
            {
                if (Config.Messages.ContainsKey(language))
                {
                    Config.Messages.Remove(language);
                }
                Config.Messages.Add(language , new Dictionary<string, string>());
                foreach ( string message in data.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries) )
                {
                    if ( message.Contains(":") )
                    {
                        string message_value = message.Substring(message.IndexOf(":") + 1).Trim(' ').Replace("\n", "").Replace(Convert.ToChar(13).ToString(), "").Replace(Convert.ToChar(10).ToString(), "");
                        string message_name = message.Substring(0, message.IndexOf(":")).Trim(' ');
                        if (Config.Messages[language].ContainsKey(message_name) != true)
                        {
                            Config.Messages[language].Add(message_name, message_value);
                        }
                        else
                        {
                            if (Config.devs)
                            {
                                // we are dev so we want to know that there is a mistake in the db
                                DebugLog("Duplicate entry: "+ message_name + "\n");
                            }
                        }
                    }
                }
                Config.Languages.Add(language);
            }
        }

        /// <summary>
        /// Suspend thread
        /// </summary>
        public static void Suspend()
        {
            Core.Interrupted = true;
            while (Core.Interrupted)
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Initialisation fc
        /// </summary>
        public static void Initialise()
        {
            Uptime = DateTime.Now;
            Process = System.Diagnostics.Process.GetCurrentProcess();
            Core.History("Core.Initialise()");
            WriteLog("Huggle " + Application.ProductVersion.ToString() + " starting");
            WriteLog("OS " + Environment.OSVersion.ToString());
            Config.DefaultLanguage = "en";
            MainThread = System.Threading.Thread.CurrentThread;
            WriteLog("Kernel thread: " + MainThread.ManagedThreadId.ToString());
            Core.Threading.CreateList();
            WriteLog("Loading config");
            InitConfig();
            
            Config.Language = Config.DefaultLanguage;
            System.GC.Collect();
            LoadLanguages();
        }

        /// <summary>
        /// Throw a huggle error dialog in case of error and recover from crash
        /// </summary>
        /// <param name="error_handle"></param>
        /// <param name="panic"></param>
        /// <returns></returns>
        public static bool ExceptionHandler(Exception error_handle, bool panic = false)
        {
            core_er = error_handle;
            WriteLog("EXCEPTION: " + error_handle.Message);
            if (SpecialThreads.RecoveryThread == null)
            {
                SpecialThreads.RecoveryThread = new System.Threading.Thread(CreateEx);
                SpecialThreads.RecoveryThread.Name = "Recovery thread";
            }
            else if (SpecialThreads.RecoveryThread.ThreadState == System.Threading.ThreadState.Running)
            {
                Core.Suspend();
                return false;
            }
            else  if(SpecialThreads.RecoveryThread.ThreadState == System.Threading.ThreadState.Aborted || SpecialThreads.RecoveryThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                SpecialThreads.RecoveryThread = new System.Threading.Thread(CreateEx);
                SpecialThreads.RecoveryThread.Name = "Recovery thread";
            }
            SpecialThreads.RecoveryThread.Start();
            if (panic == true)
            {
                StopAll();
            }
            if (MainThread == System.Threading.Thread.CurrentThread)
            {
                Core.Suspend();
            }
            
            return true;
        }

        /// <summary>
        /// Create error
        /// </summary>
        public static void CreateEx()
        {
            huggle3.Forms.ExceptionForm fx = new huggle3.Forms.ExceptionForm();
            fx.error = core_er;
            Application.Run(fx);
        }

        public static void WriteLog(string text)
        {
            string x = DateTime.Now.ToString() + ": " + text;
            SystemLog.Add(x);
            Console.WriteLine(x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PageName"></param>
        /// <returns></returns>
        public static Page GetPage(string PageName)
        {
            // get a new page
            Core.History("GetPage()");
            try
            {
                Page Page = new Page(PageName);
                return Page;
            }
            catch (Exception weird)
            {
                // weird, probably out of memory or something like that
                Core.ExceptionHandler(weird);
            }
            return null;
        }
    }

    public static class Core_Scripting
    {
        public class plugin
        {
            public int ID;
        }

        public static string Main = "main.tcl";
        public static bool Enabled = true;
    }

}
