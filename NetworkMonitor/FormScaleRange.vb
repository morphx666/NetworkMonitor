Public Class FormScaleRange

    Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    Private Sub FormScaleRange_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler RadioButtonAuto.Click, Sub() UpdateUI()
        AddHandler RadioButtonCustom.Click, Sub() UpdateUI()

        For Each s In [Enum].GetNames(GetType(UnitsHelper.Scales))
            ComboBoxUnits.Items.Add(s)
        Next

        ComboBoxUnits.SelectedIndex = My.Settings.BandwidthUnit

        If My.Settings.BandwidthAuto Then
            RadioButtonAuto.Checked = True
        Else
            RadioButtonCustom.Checked = True
        End If

        TextBoxUpBandwith.Text = My.Settings.BandwidthUp / UnitsHelper.ScaleToValue(My.Settings.BandwidthUnit)
        TextBoxDnBandwith.Text = My.Settings.BandwidthDn / UnitsHelper.ScaleToValue(My.Settings.BandwidthUnit)
    End Sub

    Private Sub UpdateUI()
        ComboBoxUnits.Enabled = RadioButtonCustom.Checked
        TextBoxDnBandwith.Enabled = RadioButtonCustom.Checked
        TextBoxUpBandwith.Enabled = RadioButtonCustom.Checked
    End Sub

    Private Sub ButtonSave_Click(sender As Object, e As EventArgs) Handles ButtonSave.Click
        My.Settings.BandwidthAuto = RadioButtonAuto.Checked
        My.Settings.BandwidthUnit = ComboBoxUnits.SelectedIndex

        Dim bDn As Double
        Dim bUp As Double

        Double.TryParse(TextBoxDnBandwith.Text, bDn)
        Double.TryParse(TextBoxUpBandwith.Text, bUp)

        My.Settings.BandwidthDn = bDn * UnitsHelper.ScaleToValue(ComboBoxUnits.SelectedIndex)
        My.Settings.BandwidthUp = bUp * UnitsHelper.ScaleToValue(ComboBoxUnits.SelectedIndex)

        Me.Close()
    End Sub
End Class