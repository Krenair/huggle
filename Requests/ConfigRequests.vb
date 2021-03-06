'This is a source code or part of Huggle project
'requests / configrequests.vb
'This file contains code for config requests

'Copyright (C) 2011 Huggle team

'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.
Imports System.Threading
Imports System.Web.HttpUtility

Namespace Requests

    Class GlobalConfigRequest : Inherits Request

        'Process global configuration page

        Protected Overrides Sub Process()
            Dim Result As ApiResult = DoApiRequest("action=query&prop=revisions&rvlimit=1&rvprop=content&titles=" & _
                Config.GlobalConfigLocation, Project:="meta")

            If Result.Error Then
                Fail(Msg("loadglobalconfig-fail"), Result.ErrorMessage)
                Exit Sub
            End If

            For Each Item As KeyValuePair(Of String, String) In ProcessConfigFile(Result.Text)

                Try
                    SetGlobalConfigOption(Item.Key, Item.Value)

                Catch ex As Exception
                    'Ignore malformed config entries
                End Try
            Next Item

#If DEBUG Then
            'If the app is in debug mode add a localhost wiki to the project list
            If Not Config.Projects.ContainsKey("localhost") Then Config.Projects.Add("localhost", "http://localhost/")
#End If

            Complete()
        End Sub

    End Class

    Class ConfigRequest : Inherits Request

        'Read project and user configuration page, user creation date, user groups and edit count

        Protected Overrides Sub Process()
            Dim Result As ApiResult

            Result = DoApiRequest("action=query&meta=userinfo&uiprop=rights|editcount" & _
                "&list=logevents|watchlistraw&letype=newusers&letitle=" & UrlEncode(User.Me.Userpage.Name) & _
                "&prop=revisions&rvprop=content&titles=" & _
                UrlEncode(Page.SanitizeTitle(Config.UserConfigLocation)) & "|" & _
                UrlEncode(Page.SanitizeTitle(Config.ProjectConfigLocation)), Config.Project)

            If Result.Error OrElse Config.ProjectConfigLocation Is Nothing Then
                Fail(Msg("login-error-config"), Result.ErrorMessage)
                Exit Sub
            End If

            Config.Watch.Clear()
            Config.Minor.Clear()

            For Each Item As String In Config.EditTypes
                Config.Watch.Add(Item, False)
                Config.Minor.Add(Item, False)
            Next Item

            Dim ProjectConfigFile As String = HtmlDecode(FindString(FindString(FindString(Result.Text, "<page", _
                "ns=""" & GetPage(Config.ProjectConfigLocation).Space.Number & """", "</page>"), "<rev "), ">", "</rev>"))

            Dim UserConfigFile As String = HtmlDecode(FindString(FindString(FindString(Result.Text, "<page", _
                "ns=""" & GetPage(Config.UserConfigLocation).Space.Number & """", "</page>"), "<rev "), ">", "</rev>"))

            'Set project config
            If ProjectConfigFile Is Nothing Then
                Fail(Msg("login-error-noconfig"))
                Exit Sub
            End If

            For Each Item As KeyValuePair(Of String, String) In ProcessConfigFile(ProjectConfigFile)
                Try
                    SetSharedConfigOption(Item.Key, Item.Value)
                    SetProjectConfigOption(Item.Key, Item.Value)

                Catch ex As Exception
                    'Ignore malformed config entries
                End Try
            Next Item

            Config.AIV = Not String.IsNullOrEmpty(Config.AIVLocation)
            Config.UAA = Not String.IsNullOrEmpty(Config.UAALocation)
            Config.TRR = Not String.IsNullOrEmpty(Config.TRRLocation)
            Config.SockReports = Not String.IsNullOrEmpty(Config.SockReportLocation)

            If Config.DefaultQueue IsNot Nothing AndAlso Queue.All.ContainsKey(Config.DefaultQueue) _
                Then Queue.Default = Queue.All(Config.DefaultQueue)
            If Config.DefaultQueue2 IsNot Nothing AndAlso Queue.All.ContainsKey(Config.DefaultQueue2) _
                Then Queue.SecondDefault = Queue.All(Config.DefaultQueue2)

            'Set user config
            If UserConfigFile IsNot Nothing Then
                For Each Item As KeyValuePair(Of String, String) In ProcessConfigFile(UserConfigFile)
                    Try
                        SetSharedConfigOption(Item.Key, Item.Value)
                        SetUserConfigOption(Item.Key, Item.Value)

                    Catch ex As Exception
                        'Ignore malformed config entries
                    End Try
                Next Item
            End If

            If Config.TemplateMessages.Count = 0 Then Config.TemplateMessages = Config.TemplateMessagesGlobal
            If String.IsNullOrEmpty(Config.IrcChannel) Then Config.UseIrc = False

            If Config.WarnSummary2 Is Nothing Then Config.WarnSummary2 = Config.WarnSummary
            If Config.WarnSummary3 Is Nothing Then Config.WarnSummary3 = Config.WarnSummary
            If Config.WarnSummary4 Is Nothing Then Config.WarnSummary4 = Config.WarnSummary

            Dim Userinfo As String = FindString(Result.Text, "<userinfo", "</userinfo>")

            If Userinfo IsNot Nothing AndAlso Userinfo.Contains("<rights>") Then
                If Userinfo.Contains("anon=""""") Then
                    'If we get here, somehow the user is not logged in
                    'Fail(Msg("login-error-rights"), Msg("login-error-unknown"))
                    Fail("Error when loggin in", "")
                    Exit Sub
                End If

                'Check user's edit count
                Dim EditCount As Integer = CInt(GetParameter(Userinfo, "editcount"))
                If Config.Devs <> True Then
                    If EditCount < Config.RequireEdits Then
                        Fail(Msg("login-error-count", CStr(Config.RequireEdits)))
                        Exit Sub
                    End If

                    Config.Rights = New List(Of String)(FindString(Userinfo, "<rights>", "</rights>").Replace("</r>", "") _
                        .Split(New String() {"<r>"}, StringSplitOptions.RemoveEmptyEntries))

                    If Config.RequireAdmin AndAlso Not Config.Rights.Contains("block") Then
                        Fail(Msg("login-error-admin"))
                        Exit Sub
                    End If

                    If Config.RequireAutoconfirmed AndAlso Not Config.Rights.Contains("autoconfirmed") Then
                        Fail(Msg("login-error-autoconfirmed"))
                        Exit Sub
                    End If

                    If Config.UsePending = True And False Then
                        Config.RightPending = Config.Rights.Contains("review")
                    End If

                    If Config.RequireRollback AndAlso Not Config.Rights.Contains("rollback") Then
                        Fail(Msg("login-error-rollback"))
                        Exit Sub
                    End If

                    If Not Config.Rights.Contains("writeapi") Then
                        Fail("Your account is not permitted to edit through the MediaWiki API, which is required by Huggle")
                        Exit Sub
                    End If

                    If Not Config.Rights.Contains("edit") Then
                        Fail("Your account is not permitted to edit.")
                        Exit Sub
                    End If
                End If
            Else
                Fail(Msg("login-error-rights"))
                Exit Sub

            End If
            If Config.RequireTime > 0 Then
                'We know the user exists, so if we get an empty result the user must have been 
                'created in 2005 or earlier, before the log existed
                If Result.Text.Contains("<logevents>") Then
                    Dim CreationDate As Date = Date.MinValue
                    Date.TryParse(GetParameter(FindString(Result.Text, "<logevents>"), "timestamp"), CreationDate)

                    If CreationDate = Date.MinValue OrElse CreationDate.AddDays(Config.RequireTime) > Date.UtcNow Then
                        Fail(Msg("login-error-age", CStr(Config.RequireTime)))
                        Exit Sub
                    End If
                End If
            End If

            'Get watchlist
            Dim WatchlistText As String = FindString(Result.Text, "<watchlistraw>", "</watchlistraw>")

            If WatchlistText IsNot Nothing Then
                For Each Item As String In Split(WatchlistText, LF)
                    Dim Page As Page = GetPage(GetParameter(Item, "title"))
                    If Not Watchlist.Contains(Page) Then Watchlist.Add(Page)
                Next Item
            End If

            Complete()
        End Sub

    End Class

    Class SaveUserConfigRequest : Inherits Request

        'Update user configuration subpage

        Protected Overrides Sub Process()
            LogProgress(Msg("saveuserconfig-progress"))

            Dim Items As New List(Of String)

            Items.Add("<nowiki>")
            Items.Add("enable:true")
            Items.Add("version:" & Config.Version.Major.ToString & "." & Config.Version.Minor.ToString & "." & _
                Config.Version.Build.ToString)
            Items.Add("")
            Items.Add("admin:" & CStr(Config.UseAdminFunctions).ToLower)
            Items.Add("patrol-speedy:" & CStr(Config.PatrolSpeedy).ToLower)
            Items.Add("speedy-message-title:" & Config.SpeedyMessageTitle)
            Items.Add("report-summary:" & CStr(Config.ReportSummary))
            Items.Add("prod-message-summary:" & CStr(Config.ProdMessageSummary))
            Items.Add("warn-summary-4:" & CStr(Config.WarnSummary4))
            Items.Add("warn-summary-3:" & Config.WarnSummary3)
            Items.Add("warn-summary-2:" & Config.WarnSummary2)
            Items.Add("warn-summary:" & Config.WarnSummary)
            'Items.Add("block-message:" & CStr(Config.BlockMessage))
            Items.Add("auto-advance:" & CStr(Config.AutoAdvance).ToLower)
            Items.Add("auto-whitelist:" & CStr(Config.AutoWhitelist).ToLower)
            Items.Add("confirm-multiple:" & CStr(Config.ConfirmMultiple).ToLower)
            Items.Add("confirm-range:" & CStr(Config.ConfirmRange).ToLower)
            Items.Add("confirm-page:" & CStr(Config.ConfirmPage).ToLower)
            Items.Add("confirm-same:" & CStr(Config.ConfirmSame).ToLower)
            Items.Add("confirm-self-revert:" & CStr(Config.ConfirmSelfRevert).ToLower)
            Items.Add("confirm-warned:" & CStr(Config.ConfirmWarned).ToLower)
            Items.Add("default-summary:" & CStr(Config.DefaultSummary))
            Items.Add("diff-font-size:" & Config.DiffFontSize)
            Items.Add("extend-reports:" & CStr(Config.ExtendReports).ToLower)
            Items.Add("irc-port:" & CStr(Config.IrcPort))
            Items.Add("prod-log:" & CStr(Config.ProdLogs))

            Dim MinorItems As New List(Of String)

            For Each Item As KeyValuePair(Of String, Boolean) In Config.Minor
                If Item.Value Then MinorItems.Add(Item.Key)
            Next Item

            If MinorItems.Count = 0 Then MinorItems.Add("none")
            MinorItems.Sort()

            Items.Add("minor:" & String.Join(",", MinorItems.ToArray))

            Items.Add("open-in-browser:" & CStr(Config.OpenInBrowser).ToLower)
            Items.Add("preload:" & CStr(Config.Preloads))

            If Config.AutoReport = True Then
                Items.Add("report:auto")
            ElseIf Config.PromptForReport Then
                Items.Add("report:prompt")
            Else
                Items.Add("report:none")
            End If

            If Config.CustomRevertSummaries.Count > 0 Then Items.Add("revert-summaries:" & LF & "    " & _
                String.Join("," & LF & "    ", Config.CustomRevertSummaries.ToArray))
            Items.Add("customtsumm:" & CStr(Config.UseCSummaries).ToLower)
            Items.Add("rollback:" & CStr(Config.UseRollback).ToLower)
            Items.Add("show-log:" & CStr(Config.ShowLog).ToLower)
            Items.Add("show-new-edits:" & CStr(Config.ShowNewEdits).ToLower)
            Items.Add("show-queue:" & CStr(Config.ShowQueue).ToLower)

            If Config.UseCSummaries = True Then
                Items.Add("template-summ:")
                For Each ts As String In Config.TemplateSummary.Keys
                    Items.Add(ts & ";" & Config.TemplateSummary(ts) & ",")
                Next ts
            End If
            Items.Add("")

            Items.Add("show-tool-tips:" & CStr(Config.ShowToolTips).ToLower)

            Dim Templates As New List(Of String)

            For Each Item As String In Config.TemplateMessages
                If Not Config.TemplateMessagesGlobal.Contains(Item) Then Templates.Add(Item)
            Next Item

            Items.Add("templates:" & LF & "    " & String.Join("," & LF & "    ", Config.TemplateMessages.ToArray))
            Items.Add("tray-icon:" & CStr(Config.TrayIcon).ToLower)
            Items.Add("undo-summary:" & Config.UndoSummary)
            Items.Add("update-whitelist:" & CStr(Config.UpdateWhitelist).ToLower)
            Items.Add("username-listed:" & CStr(Config.UsernameListed).ToLower)

            Dim WatchItems As New List(Of String)

            For Each Item As KeyValuePair(Of String, Boolean) In Config.Watch
                If Item.Value Then WatchItems.Add(Item.Key)
            Next Item

            If Config.WatchDelete Then WatchItems.Add("delete")
            If WatchItems.Count = 0 Then WatchItems.Add("none")
            WatchItems.Sort()

            Items.Add("watch:" & String.Join(",", WatchItems.ToArray))
            Items.Add("</nowiki>")

            Dim Result As ApiResult = PostEdit(Config.UserConfigLocation, String.Join(LF, Items.ToArray), _
                Config.ConfigSummary, Minor:=True)

            If Result.Error Then Fail(Msg("saveuserconfig-fail"), Result.ErrorMessage) Else Complete()
        End Sub

    End Class

End Namespace
