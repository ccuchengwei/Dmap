<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainUI
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.Disp_ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.Exit_ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Disp_ToolStripMenuItem, Me.Exit_ToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(99, 48)
        '
        'Disp_ToolStripMenuItem
        '
        Me.Disp_ToolStripMenuItem.Name = "Disp_ToolStripMenuItem"
        Me.Disp_ToolStripMenuItem.Size = New System.Drawing.Size(98, 22)
        Me.Disp_ToolStripMenuItem.Text = "顯示"
        '
        'Exit_ToolStripMenuItem
        '
        Me.Exit_ToolStripMenuItem.Name = "Exit_ToolStripMenuItem"
        Me.Exit_ToolStripMenuItem.Size = New System.Drawing.Size(98, 22)
        Me.Exit_ToolStripMenuItem.Text = "結束"
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.NotifyIcon1.Icon = Global.osu__DMap.My.Resources.Resources.DMap_Icon
        Me.NotifyIcon1.Text = "osu! DMap"
        Me.NotifyIcon1.Visible = True
        '
        'MainUI
        '
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(253, Byte), Integer), CType(CType(253, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(300, 172)
        Me.ControlBox = False
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Location = New System.Drawing.Point(100, 100)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "MainUI"
        Me.Opacity = 0.78
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "osu! DMap"
        Me.TransparencyKey = System.Drawing.Color.FromArgb(CType(CType(253, Byte), Integer), CType(CType(253, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents Disp_ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Exit_ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NotifyIcon1 As System.Windows.Forms.NotifyIcon

End Class
