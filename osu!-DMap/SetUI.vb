Public Class SetUI
    Dim objColorTmp As Color
    Dim blnConfirm As Boolean

    Private Sub SetUI_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not blnConfirm Then
            Button1_Click()
        End If
    End Sub

    Private Sub SetUI_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        CheckBox1.Checked = MainUI.blnHookMessage
        CheckBox2.Checked = MainUI.blnOpenFile
        CheckBox3.Checked = MainUI.blnOpenHttp
        CheckBox4.Checked = MainUI.blnCopyUrl
        TextBox1.Text = MainUI.strPath
        ColorDialog1.Color = MainUI.objBarTextColor
        Button3.BackColor = MainUI.objBarTextColor
        objColorTmp = MainUI.objBarTextColor
        SetStyle(ControlStyles.ResizeRedraw Or ControlStyles.SupportsTransparentBackColor, True)
    End Sub

    Private Sub Button1_Click() Handles Button1.Click
        Dim strTmp As String = TextBox1.Text
        If strTmp = "C:\" Then
            MessageBox.Show("目錄不能是C:\", "目錄無法使用", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        For Each c As Char In IO.Path.GetInvalidPathChars
            If strTmp.IndexOf(c) <> -1 Then
                MessageBox.Show("目錄中有無效的字元「 " & c & " 」", "無效的字元", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If
        Next
        If strTmp Like "[A-z]:\*" Then
            strTmp = strTmp.Substring(3)
            For Each c As Char In IO.Path.GetInvalidFileNameChars
                If c = "\"c Then Continue For
                If strTmp.IndexOf(c) <> -1 Then
                    MessageBox.Show("目錄中有無效的字元「 " & c & " 」", "無效的字元", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End If
            Next
        Else
            If strTmp = "" Then TextBox1.Text = My.Application.Info.DirectoryPath
        End If

        Dim objDirInfo As New IO.DirectoryInfo(TextBox1.Text)
        If Not objDirInfo.Exists Then
            Try
                objDirInfo.Create()
            Catch ex As Exception
                MessageBox.Show("目錄無效，請檢查是否有此磁碟機", "無效的路徑", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End Try
        End If
        TextBox1.Text = objDirInfo.FullName
        '==================================================
        MainUI.blnHookMessage = CheckBox1.Checked
        MainUI.blnOpenFile = CheckBox2.Checked
        MainUI.blnOpenHttp = CheckBox3.Checked
        MainUI.blnCopyUrl = CheckBox4.Checked
        MainUI.strPath = TextBox1.Text
        MainUI.objBarTextColor = objColorTmp
        MainUI.ResetValue()
        My.Settings.blnHookMessage = CheckBox1.Checked
        My.Settings.blnOpenFile = CheckBox2.Checked
        My.Settings.blnOpenHttp = CheckBox3.Checked
        My.Settings.blnCopyUrl = CheckBox4.Checked
        My.Settings.strPath = TextBox1.Text
        My.Settings.objBarTextColor = objColorTmp
        My.Settings.Save()
        blnConfirm = True
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        blnConfirm = True
        Me.Close()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ColorDialog1.Color = objColorTmp
        If ColorDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            objColorTmp = ColorDialog1.Color
            Button3.BackColor = ColorDialog1.Color
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("http://osu.ppy.sh/u/1537981")
    End Sub

    Private Sub LinkLabel2_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start("http://osu.ppy.sh/p/support")
    End Sub
End Class

