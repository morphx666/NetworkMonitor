<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormScaleRange
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.RadioButtonAuto = New System.Windows.Forms.RadioButton()
        Me.RadioButtonCustom = New System.Windows.Forms.RadioButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBoxDnBandwith = New System.Windows.Forms.TextBox()
        Me.TextBoxUpBandwith = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ButtonSave = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.ComboBoxUnits = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'RadioButtonAuto
        '
        Me.RadioButtonAuto.AutoSize = True
        Me.RadioButtonAuto.Location = New System.Drawing.Point(12, 12)
        Me.RadioButtonAuto.Name = "RadioButtonAuto"
        Me.RadioButtonAuto.Size = New System.Drawing.Size(51, 19)
        Me.RadioButtonAuto.TabIndex = 0
        Me.RadioButtonAuto.TabStop = True
        Me.RadioButtonAuto.Text = "Auto"
        Me.RadioButtonAuto.UseVisualStyleBackColor = True
        '
        'RadioButtonCustom
        '
        Me.RadioButtonCustom.AutoSize = True
        Me.RadioButtonCustom.Location = New System.Drawing.Point(12, 37)
        Me.RadioButtonCustom.Name = "RadioButtonCustom"
        Me.RadioButtonCustom.Size = New System.Drawing.Size(67, 19)
        Me.RadioButtonCustom.TabIndex = 1
        Me.RadioButtonCustom.TabStop = True
        Me.RadioButtonCustom.Text = "Custom"
        Me.RadioButtonCustom.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(46, 68)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(74, 15)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Downstream"
        '
        'TextBoxDnBandwith
        '
        Me.TextBoxDnBandwith.Location = New System.Drawing.Point(126, 65)
        Me.TextBoxDnBandwith.Name = "TextBoxDnBandwith"
        Me.TextBoxDnBandwith.Size = New System.Drawing.Size(80, 23)
        Me.TextBoxDnBandwith.TabIndex = 3
        Me.TextBoxDnBandwith.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'TextBoxUpBandwith
        '
        Me.TextBoxUpBandwith.Location = New System.Drawing.Point(126, 94)
        Me.TextBoxUpBandwith.Name = "TextBoxUpBandwith"
        Me.TextBoxUpBandwith.Size = New System.Drawing.Size(80, 23)
        Me.TextBoxUpBandwith.TabIndex = 6
        Me.TextBoxUpBandwith.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(46, 97)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(58, 15)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Upstream"
        '
        'ButtonSave
        '
        Me.ButtonSave.Location = New System.Drawing.Point(111, 139)
        Me.ButtonSave.Name = "ButtonSave"
        Me.ButtonSave.Size = New System.Drawing.Size(75, 26)
        Me.ButtonSave.TabIndex = 8
        Me.ButtonSave.Text = "Save"
        Me.ButtonSave.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Location = New System.Drawing.Point(192, 139)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 26)
        Me.ButtonCancel.TabIndex = 9
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'ComboBoxUnits
        '
        Me.ComboBoxUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxUnits.FormattingEnabled = True
        Me.ComboBoxUnits.Location = New System.Drawing.Point(85, 36)
        Me.ComboBoxUnits.Name = "ComboBoxUnits"
        Me.ComboBoxUnits.Size = New System.Drawing.Size(121, 23)
        Me.ComboBoxUnits.TabIndex = 10
        '
        'FormScaleRange
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(279, 177)
        Me.ControlBox = False
        Me.Controls.Add(Me.ComboBoxUnits)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonSave)
        Me.Controls.Add(Me.TextBoxUpBandwith)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TextBoxDnBandwith)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.RadioButtonCustom)
        Me.Controls.Add(Me.RadioButtonAuto)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormScaleRange"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Scale Range"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents RadioButtonAuto As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButtonCustom As System.Windows.Forms.RadioButton
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TextBoxDnBandwith As System.Windows.Forms.TextBox
    Friend WithEvents TextBoxUpBandwith As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ButtonSave As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents ComboBoxUnits As System.Windows.Forms.ComboBox
End Class
