Class ReportForm

    Public ThisUser As User

    Private Sub UserReportForm_Load() Handles Me.Load
        Icon = My.Resources.icon_red_button
        Text = "Report " & ThisUser.Name
        ReportTo.SelectedIndex = 0
        Message.Focus()

        WarnLog.Columns.Add("", 300)
        WarnLog.Items.Add("Retrieving warnings, please wait...")

        Dim NewWarnLogRequest As New WarningLogRequest
        NewWarnLogRequest.Target = WarnLog
        NewWarnLogRequest.ThisUser = ThisUser
        NewWarnLogRequest.Start()
    End Sub

    Private Sub UserReportForm_KeyDown(ByVal s As Object, ByVal e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then Close()
    End Sub

    Private Sub UserReportForm_FormClosing() Handles Me.FormClosing
        If Me.DialogResult <> DialogResult.OK Then Me.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub Cancel_Click() Handles Cancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OK_Click() Handles OK.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub ReportTo_SelectedIndexChanged() _
        Handles ReportTo.SelectedIndexChanged

        Select Case ReportTo.Text
            Case "Administrator intervention against vandalism"
                If Message.Text = "" OrElse Message.Text = "inappropriate username" Then Message.Text = "vandalism"
            Case "Usernames for administrator attention"
                If Message.Text = "" OrElse Message.Text = "vandalism" Then Message.Text = "inappropriate username"
        End Select
    End Sub

    Private Sub Message_TextChanged() Handles Message.TextChanged
        OK.Enabled = (Message.Text <> "")
    End Sub

End Class