Public Class UnitsHelper
    Public Enum Scales
        Bits = 0
        KiloBits = 1
        MegaBits = 2

        Bytes = 3
        KiloBytes = 4
        MegaBytes = 5
    End Enum

    Public Shared Function ScaleToUnit(scale As Scales) As String
        Select Case scale
            Case Scales.Bits : Return "b"
            Case Scales.Bytes : Return "B"
            Case Scales.KiloBytes : Return "KB"
            Case Scales.KiloBits : Return "Kb"
            Case Scales.MegaBytes : Return "MB"
            Case Scales.MegaBits : Return "Mb"
            Case Else : Return ""
        End Select
    End Function

    Public Shared Function ScaleToValue(scale As Scales) As String
        Select Case scale
            Case Scales.Bytes : Return 1
            Case Scales.KiloBytes : Return 1024
            Case Scales.MegaBytes : Return 1024 * 1024

            Case Scales.Bits : Return 1 / 8
            Case Scales.KiloBits : Return 1 / 8 * 1024
            Case Scales.MegaBits : Return 1 / 8 * 1024 * 1024
            Case Else : Return ""
        End Select
    End Function
End Class
