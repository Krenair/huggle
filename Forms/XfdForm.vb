Class XfdForm

    Public Page As Page

    Private Sub XfdForm_Load(ByVal s As Object, ByVal e As EventArgs) Handles Me.Load
        Text = "Nominate '" & Page.Name & "' for deletion"
        Category.Visible = (Page.Namespace = "")
        CategoryLabel.Visible = (Page.Namespace = "")
        If Category.Visible Then Category.SelectedIndex = 0

        Select Case Page.Namespace
            Case "" : NominationType.Text = "Article"
            Case "Category" : NominationType.Text = "Category"
            Case "Template" : NominationType.Text = "Template"
            Case "Image" : NominationType.Text = "Image"
            Case Else : NominationType.Text = "Miscellaneous"
        End Select
    End Sub

    Private Sub XfdForm_FormClosing(ByVal s As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
        If DialogResult <> DialogResult.OK Then DialogResult = DialogResult.Cancel
    End Sub

    Private Sub XfdForm_KeyDown(ByVal s As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            DialogResult = DialogResult.Cancel
            Close()
        End If
    End Sub

    Private Sub Reason_TextChanged(ByVal s As Object, ByVal e As EventArgs) Handles Reason.TextChanged
        OK.Enabled = (Reason.Text <> "")
    End Sub

    Private Sub OK_Click(ByVal s As Object, ByVal e As EventArgs) Handles OK.Click
        Select Case Page.Namespace
            Case ""
                Dim NewRequest As New AfdRequest
                NewRequest.Category = Category.Text.Substring(0, 1).Replace("(", "?")
                NewRequest.Page = Page
                NewRequest.Reason = Reason.Text
                NewRequest.Notify = Notify.Checked
                NewRequest.Start()

            Case "Category"
                Dim NewRequest As New CfdRequest
                NewRequest.Page = Page
                NewRequest.Reason = Reason.Text
                NewRequest.Notify = Notify.Checked
                NewRequest.Start()

            Case "Template"
                Dim NewRequest As New TfdRequest
                NewRequest.Page = Page
                NewRequest.Reason = Reason.Text
                NewRequest.Notify = Notify.Checked
                NewRequest.Start()

            Case "Image"
                Dim NewRequest As New IfdRequest
                NewRequest.Page = Page
                NewRequest.Reason = Reason.Text
                NewRequest.Notify = Notify.Checked
                NewRequest.Start()

            Case Else
                Dim NewRequest As New MfdRequest
                NewRequest.Page = Page
                NewRequest.Reason = Reason.Text
                NewRequest.Notify = Notify.Checked
                NewRequest.Start()
        End Select

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub Cancel_Click(ByVal s As Object, ByVal e As EventArgs) Handles Cancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub Notify_KeyDown(ByVal s As Object, ByVal e As KeyEventArgs) Handles Notify.KeyDown
        If e.KeyCode = Keys.Enter AndAlso Reason.Text <> "" Then
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

End Class