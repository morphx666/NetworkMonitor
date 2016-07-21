Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports System.Reflection

Public Class frmMain
    Private pcBytesRecv As PerformanceCounter
    Private pcBytesSend As PerformanceCounter

    Private curRecv As Single
    Private curSend As Single
    Private lastRecv As Single
    Private lastSend As Single
    Private maxValueUp As Single = 1024 * 1024 / 8 * 1 ' 1 MBit
    Private maxValueDn As Single = 1024 * 1024 / 8 * 1 ' 1 MBit
    Private minThreshold As Single = 64

    Private gSend As Single
    Private gRecv As Single

    Private unit As String
    Private unitValue As Double

    Private monitorThread As Thread
    Private smoothAnimThread As Thread
    Private cancelThreads As Boolean

    Private isDragging As Boolean
    Private mouseDownLocation As Point
    Private meLocation As Point
    Private pointerPosition As MousePositionInWindow

    Private isReady As Boolean = False

    Private Structure DataPoint
        Public Value As Single
        Public MaxValue As Single

        Public Sub New(value As Single, maxValue As Single)
            Me.Value = value
            Me.MaxValue = maxValue
        End Sub
    End Structure
    Private historySize As Integer
    Private historyRecv As New Concurrent.ConcurrentQueue(Of DataPoint)
    Private historySend As New Concurrent.ConcurrentQueue(Of DataPoint)

    Private ptsRecv() As Point
    Private ptsSend() As Point

    Private Enum DisplayModes
        <Description("Led Bars")> LedBars = 0
        <Description("Line Graph")> LineGraph = 1
        <Description("Solid Graph")> SolidGraph = 2
        <Description("Flat")> Flat = 3
    End Enum

    Private Enum HorizontalDragPosition
        HorizontalLeft = 1
        HorizontalMiddle = 2
        HorizontalRight = 4
    End Enum

    Private Enum VerticalDragPosition
        VerticalTop = 8
        VerticalMiddle = 16
        VerticalBottom = 32
    End Enum

    Private Enum MousePositionInWindow
        LeftTop = HorizontalDragPosition.HorizontalLeft + VerticalDragPosition.VerticalTop
        LeftMiddle = HorizontalDragPosition.HorizontalLeft + VerticalDragPosition.VerticalMiddle
        LeftBottom = HorizontalDragPosition.HorizontalLeft + VerticalDragPosition.VerticalBottom

        RightTop = HorizontalDragPosition.HorizontalRight + VerticalDragPosition.VerticalTop
        RightMiddle = HorizontalDragPosition.HorizontalRight + VerticalDragPosition.VerticalMiddle
        RightBottom = HorizontalDragPosition.HorizontalRight + VerticalDragPosition.VerticalBottom

        TopMiddle = HorizontalDragPosition.HorizontalMiddle + VerticalDragPosition.VerticalTop
        BottomMiddle = HorizontalDragPosition.HorizontalMiddle + VerticalDragPosition.VerticalBottom

        Body = HorizontalDragPosition.HorizontalMiddle + VerticalDragPosition.VerticalMiddle
    End Enum

    <DllImport("user32.dll", EntryPoint:="DestroyIcon", CharSet:=CharSet.Auto, ExactSpelling:=True)> _
    Public Shared Function DestroyIcon(ByVal hIcon As HandleRef) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Sub frmMain_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        cancelThreads = True

        Do While (monitorThread.ThreadState = ThreadState.Running) OrElse (smoothAnimThread.ThreadState = ThreadState.Running)
            Application.DoEvents()
        Loop

        pcBytesRecv.Close()
        pcBytesRecv.Dispose()

        pcBytesSend.Close()
        pcBytesSend.Dispose()
    End Sub

    Private Sub frmMain_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True

        AddHandler ShowHideToolStripMenuItem.Click, Sub() ShowHide()
        AddHandler ExitToolStripMenuItem.Click, Sub() Me.Close()
        AddHandler Me.SizeChanged, Sub() SetHistorySize()

        InitializeCounters()

        SetUpSubMenu(ScaleToolStripMenuItem, GetType(UnitsHelper.Scales), Sub(style As UnitsHelper.Scales) SetScale(style))
        SetUpSubMenu(StyleToolStripMenuItem, GetType(DisplayModes), Sub(style As DisplayModes) SetDisplayMode(style))
        ScaleToolStripMenuItem.DropDownItems.Add("-")
        AddHandler ScaleToolStripMenuItem.DropDownItems.Add("Show Range").Click, Sub()
                                                                                     Dim mi As ToolStripMenuItem = CType(ScaleToolStripMenuItem.DropDownItems(ScaleToolStripMenuItem.DropDownItems.Count - 1), ToolStripMenuItem)
                                                                                     mi.Checked = Not mi.Checked
                                                                                     My.Settings.ShowRange = mi.Checked
                                                                                 End Sub

        AddHandler BandwidthToolStripMenuItem.Click, Sub()
                                                         Dim f As New FormScaleRange()
                                                         f.ShowDialog(Me)
                                                     End Sub

        Me.Opacity = 0

        If My.Settings.WindowSize.Width <> 0 Then
            Me.Location = My.Settings.WindowLocation
            Me.Size = My.Settings.WindowSize
        End If

        SetScale(My.Settings.Scale)
        SetDisplayMode(My.Settings.DisplayMode)
        CType(ScaleToolStripMenuItem.DropDownItems(ScaleToolStripMenuItem.DropDownItems.Count - 1), ToolStripMenuItem).Checked = My.Settings.ShowRange

        isReady = True
    End Sub

    Private Sub SetUpSubMenu(menuItem As ToolStripMenuItem, enumType As Type, handler As [Delegate])
        Dim names() = [Enum].GetNames(enumType)
        Dim values() As Integer = [Enum].GetValues(enumType)
        Dim data As New Dictionary(Of Integer, String)

        For i = 0 To names.Length - 1
            data.Add(values(i), names(i))
        Next

        For Each d In data.OrderBy(Function(v) v.Key)
            Dim enumName As String = ""

            Dim field As FieldInfo = enumType.GetField(d.Value)
            If field IsNot Nothing Then
                Dim attrb As DescriptionAttribute = Attribute.GetCustomAttribute(field, GetType(DescriptionAttribute))
                enumName = If(attrb Is Nothing, d.Value, attrb.Description)
            End If

            Dim tsi = menuItem.DropDownItems.Add(enumName)
            tsi.Tag = d.Key
            AddHandler tsi.Click, Sub() handler.DynamicInvoke(d.Key)
        Next
    End Sub

    Private Sub SwitchInstance(sender As Object, e As EventArgs)
        Dim instance As String = CType(sender, ToolStripMenuItem).Text

        For Each mi As ToolStripItem In AdaptersToolStripMenuItem.DropDownItems
            If TypeOf mi Is ToolStripMenuItem Then
                CType(mi, ToolStripMenuItem).Checked = (mi.Text = instance)
            End If
        Next

        SetInstance(instance)
    End Sub

    Private Sub SetInstance(instance As String)
        Try
            pcBytesRecv.InstanceName = instance
            pcBytesSend.InstanceName = instance

            Me.Text = "Network Monitor: " + instance
            niIcon.Text = Me.Text

            curRecv = 0
            gRecv = 0
            curSend = 0
            gSend = 0

            My.Settings.InstanceName = instance
        Catch ex As Exception
            MsgBox("Unable to select interface '" + instance + "'" + vbCrLf + vbCrLf + ex.Message)
        End Try
    End Sub

    Private Sub SetDisplayMode(style As DisplayModes)
        My.Settings.DisplayMode = style

        For Each mi As ToolStripItem In StyleToolStripMenuItem.DropDownItems
            If TypeOf mi Is ToolStripMenuItem Then
                CType(mi, ToolStripMenuItem).Checked = (mi.Tag = style)
            End If
        Next

        Me.Invalidate()
    End Sub

    Private Sub SetScale(scale As UnitsHelper.Scales)
        My.Settings.Scale = scale

        For Each mi As ToolStripItem In ScaleToolStripMenuItem.DropDownItems
            If TypeOf mi Is ToolStripMenuItem Then
                CType(mi, ToolStripMenuItem).Checked = (mi.Tag = scale)
            End If
        Next

        unit = UnitsHelper.ScaleToUnit(scale)
        unitValue = UnitsHelper.ScaleToValue(scale)

        Me.Invalidate()
    End Sub

    Private Sub MonitorSub()
        Dim curValue As Single
        Dim values As New List(Of Single)

        Do
            Try
                curRecv = pcBytesRecv.NextValue()
                curSend = pcBytesSend.NextValue()

                If My.Settings.BandwidthAuto Then
                    curValue = Math.Max(curSend, curRecv)

                    If values.Count >= 50 Then
                        values = values.OrderBy(Function(k) k).ToList()
                        values.RemoveRange(0, 11)
                        values.RemoveAt(values.Count - 1)
                    End If

                    values.Add(curValue)
                    Dim avg As Single = values.Average()

                    If avg / maxValueUp > 0.9 Then
                        maxValueUp += avg / 100
                    Else
                        maxValueUp -= avg / 100
                    End If
                    maxValueUp = Math.Max(1024 * 1024 / 8 * 1, maxValueUp)
                    maxValueDn = maxValueUp
                Else
                    maxValueUp = My.Settings.BandwidthUp
                    maxValueDn = My.Settings.BandwidthDn
                End If

                If My.Settings.DisplayMode = DisplayModes.LedBars Then
                    If lastRecv <> curRecv OrElse lastSend <> curSend Then
                        Me.Invalidate()
                        DrawTryIcon()

                        lastRecv = curRecv
                        lastSend = curSend
                    End If
                Else
                    If lastRecv <> curRecv OrElse lastSend <> curSend Then DrawTryIcon()

                    historyRecv.Enqueue(New DataPoint(curRecv, maxValueDn))
                    historySend.Enqueue(New DataPoint(curSend, maxValueUp))
                    Dim dp As DataPoint
                    While historyRecv.Count >= historySize
                        If Not historyRecv.TryDequeue(dp) Then Thread.Sleep(10)
                    End While
                    While historySend.Count >= historySize
                        If Not historySend.TryDequeue(dp) Then Thread.Sleep(10)
                    End While
                    Me.Invalidate()

                    lastRecv = curRecv
                    lastSend = curSend
                End If

                Thread.Sleep(250)
            Catch
            End Try
        Loop Until cancelThreads
    End Sub

    Private Sub SmoothAnim()
        Dim delta As Single
        Dim refresh As Boolean
        Static stp As Integer = 50

        Do
            Thread.Sleep(100)
        Loop Until lastRecv <> 0 Or cancelThreads

        Do
            refresh = False

            If gRecv <> curRecv Then
                delta = curRecv - gRecv
                If Math.Abs(delta) = stp \ 2 Then
                    gRecv = curRecv
                Else
                    gRecv += CLng(delta / stp)
                End If
                refresh = True
            End If

            If gSend <> curSend Then
                delta = curSend - gSend
                If Math.Abs(delta) <= stp \ 2 Then
                    gSend = curSend
                Else
                    gSend += CLng(delta / stp)
                End If
                refresh = True
            End If

            If refresh Then Me.Invalidate()
            Thread.Sleep(10)
        Loop Until cancelThreads
    End Sub

    Private Sub frmMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If maxValueDn = 0 Then Return

        Dim g As Graphics = e.Graphics
        Dim rSend As Rectangle = Me.DisplayRectangle
        Dim rRecv As Rectangle = Me.DisplayRectangle

        'g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        Dim style = CType(My.Settings.DisplayMode, DisplayModes)
        Dim m As Integer = If(style = DisplayModes.Flat, 1, 2)

        rRecv.Height = rRecv.Height \ 2 + Math.Ceiling(m / 2)
        rRecv.Inflate(-m, -m)

        rSend.Y += (rRecv.Height + m)
        rSend.Inflate(-m, -m)
        rSend.Height = rRecv.Height

        Select Case style
            Case DisplayModes.LedBars, DisplayModes.Flat
                m = If(style = DisplayModes.LedBars, 1, 1)

                DrawBar(g, CInt(rRecv.Width * (gRecv / maxValueDn)), rRecv, Brushes.DarkGreen, Brushes.LightGreen, m)
                DrawBar(g, CInt(rSend.Width * (gSend / maxValueUp)), rSend, Brushes.DarkRed, Brushes.Red, m)

                If style = DisplayModes.LedBars Then
                    For x As Integer = rRecv.Left To rRecv.Right Step 2
                        g.DrawLine(Pens.Black, x, rRecv.Top, x, rSend.Bottom)
                    Next
                End If

                If My.Settings.ShowRange Then
                    PrintText(g, String.Format("{0:N1} / {1:N1} {2}/s", gSend / unitValue, maxValueUp / unitValue, unit), rSend)
                    PrintText(g, String.Format("{0:N1} / {1:N1} {2}/s", gRecv / unitValue, maxValueDn / unitValue, unit), rRecv)
                Else
                    PrintText(g, String.Format("{0:N1} {1}/s", gSend / unitValue, unit), rSend)
                    PrintText(g, String.Format("{0:N1} {1}/s", gRecv / unitValue, unit), rRecv)
                End If

            Case DisplayModes.LineGraph, DisplayModes.SolidGraph
                If historyRecv.Count = 0 Then Exit Sub

                Dim sizeDif As Integer = historySize - historyRecv.Count
                Dim max As Single = historyRecv.Max(Function(dp) dp.MaxValue)
                Dim x As Integer

                Dim rRecv2 As Rectangle = rRecv
                rRecv2.Inflate(0, -1)
                Dim rSend2 As Rectangle = rSend
                rSend2.Inflate(0, -1)

                For i As Integer = 0 To historySize - 1
                    x = rRecv2.X + i / historySize * rRecv2.Width
                    If i < sizeDif Then
                        ptsRecv(i) = New Point(x, rRecv2.Bottom)
                        ptsSend(i) = New Point(x, rSend2.Bottom)
                    Else
                        ptsRecv(i) = New Point(x, rRecv2.Bottom - historyRecv(i - sizeDif).Value / max * rRecv2.Height)
                        ptsSend(i) = New Point(x, rSend2.Bottom - historySend(i - sizeDif).Value / max * rSend2.Height)
                    End If
                Next
                g.FillRectangle(Brushes.DarkGreen, rRecv)
                g.FillRectangle(Brushes.DarkRed, rSend)

                g.DrawLines(Pens.LightGreen, ptsRecv)
                g.DrawLines(Pens.Red, ptsSend)

                If My.Settings.DisplayMode = DisplayModes.SolidGraph Then
                    ClosePolygon(ptsRecv, rRecv2.Bottom)
                    g.FillPolygon(Brushes.LightGreen, ptsRecv)
                    ClosePolygon(ptsSend, rSend2.Bottom)
                    g.FillPolygon(Brushes.Red, ptsSend)
                End If
        End Select
    End Sub

    Private Sub ClosePolygon(ByRef pts() As Point, y As Integer)
        pts(0).Y = y
        pts(pts.Length - 1).Y = y
    End Sub

    Private Sub DrawTryIcon()
        Dim bmpIcon As Bitmap = Nothing

        If curRecv <> 0 OrElse curSend <> 0 Then
            bmpIcon = New Bitmap(16, 16)

            Dim b As SolidBrush
            Dim g As Graphics = Graphics.FromImage(bmpIcon)

            If curSend > minThreshold AndAlso Math.Abs(curSend - lastSend) > maxValueUp / 10000 Then
                g.DrawImageUnscaled(My.Resources.ledon, 4, 0)
                b = New SolidBrush(Color.FromArgb(200, Color.Red))
                g.FillRectangle(b, 6, 2, 8, 6)
                b.Dispose()
            Else
                g.DrawImageUnscaled(My.Resources.ledoff, 4, 0)
            End If

            If curRecv > minThreshold AndAlso Math.Abs(curRecv - lastRecv) > maxValueUp / 10000 Then
                g.DrawImageUnscaled(My.Resources.ledon, 0, 6)
                b = New SolidBrush(Color.FromArgb(200, Color.LimeGreen))
                g.FillRectangle(b, 2, 8, 8, 6)
                b.Dispose()
            Else
                g.DrawImageUnscaled(My.Resources.ledoff, 0, 6)
            End If

            g.Dispose()
        Else
            bmpIcon = My.Resources.neticon
        End If

        Dim oldIcon As Icon = niIcon.Icon

        Try
            niIcon.Icon = Drawing.Icon.FromHandle(bmpIcon.GetHicon)
        Catch
            bmpIcon = My.Resources.neticon
        End Try

        DestroyIcon(New HandleRef(Me, oldIcon.Handle))
        oldIcon.Dispose()
        bmpIcon.Dispose()
    End Sub

    Private Sub DrawBar(g As Graphics, value As Integer, r As Rectangle, backColor As Brush, foreColor As Brush, margin As Integer)
        g.FillRectangle(backColor, r.Left, r.Top, r.Width, r.Height)
        r.Inflate(-margin, -margin)
        g.FillRectangle(foreColor, r.Left, r.Top, Math.Min(value, r.Width), r.Height)
    End Sub

    Private Sub PrintText(ByVal g As Graphics, ByVal text As String, ByVal r As Rectangle)
        Dim f As StringFormat = New StringFormat()
        f.Alignment = StringAlignment.Far
        f.LineAlignment = StringAlignment.Center

        r.Offset(1, 1)
        g.DrawString(text, Me.Font, Brushes.Black, r, f)
        r.Offset(-1, -1)
        g.DrawString(text, Me.Font, Brushes.White, r, f)
    End Sub

    Private Sub niIcon_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles niIcon.MouseDoubleClick
        ShowHide()
    End Sub

    Private Sub frmMain_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        Static isRecursing As Boolean

        If isRecursing OrElse Not isReady Then Exit Sub
        isRecursing = True

        UpdateWindowMetrics()

        If Me.WindowState = FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Normal
            Me.Hide()
        End If

        isRecursing = False
    End Sub

    Private Sub frmMain_LocationChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LocationChanged
        UpdateWindowMetrics()
    End Sub

    Private Sub ShowHide()
        If Me.Visible Then
            Me.Hide()
        Else
            Me.Show()
        End If
    End Sub

    Private Sub SetHistorySize()
        historySize = Me.Width
        ReDim Preserve ptsRecv(historySize - 1)
        ReDim Preserve ptsSend(historySize - 1)
    End Sub

    Private Sub UpdateWindowMetrics()
        If Me.WindowState = FormWindowState.Normal And isReady Then
            SetHistorySize()
            My.Settings.WindowLocation = Me.Location
            My.Settings.WindowSize = Me.Size
        End If
    End Sub

    Private Sub InitializeCounters()
        Dim categoryName As String = ""
        Dim counterNameReceived As String = ""
        Dim counterNameSent As String = ""
        Dim counterNameBandwidth As String = ""
        Dim firstInstanceName As String = ""

        Dim countersNames() As String = CType(My.Computer.Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Windows NT\CurrentVersion\Perflib\CurrentLanguage").GetValue("Counter"), String())

        For i As Integer = 0 To countersNames.Length - 2 Step 2
            If countersNames(i) = "510" Then categoryName = countersNames(i + 1)
            If countersNames(i) = "264" Then counterNameReceived = countersNames(i + 1)
            If countersNames(i) = "506" Then counterNameSent = countersNames(i + 1)
            If countersNames(i) = "520" Then counterNameBandwidth = countersNames(i + 1)
        Next

        pcBytesRecv = New PerformanceCounter(categoryName, counterNameReceived, True)
        pcBytesSend = New PerformanceCounter(categoryName, counterNameSent, True)

        For Each pc As PerformanceCounterCategory In PerformanceCounterCategory.GetCategories
            If pc.CategoryName = pcBytesRecv.CategoryName Then
                For Each instance As String In pc.GetInstanceNames
                    If instance.StartsWith("isatap") Then Continue For
                    If instance.StartsWith("6TO4") Then Continue For

                    CType(AdaptersToolStripMenuItem.DropDownItems.Add(instance, Nothing, AddressOf SwitchInstance), ToolStripMenuItem).Checked = (instance = My.Settings.InstanceName)
                    If firstInstanceName = "" Then firstInstanceName = instance
                Next
            End If
        Next

        monitorThread = New Thread(AddressOf MonitorSub)
        monitorThread.Start()

        smoothAnimThread = New Thread(AddressOf SmoothAnim)
        smoothAnimThread.Start()

        If My.Settings.InstanceName = "" Then
            SetInstance(firstInstanceName)
        Else
            SetInstance(My.Settings.InstanceName)
        End If
    End Sub

    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Hide()
        Application.DoEvents()
        Me.Opacity = 100
    End Sub

    Private Sub frmMain_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        isDragging = True
        mouseDownLocation = e.Location
        meLocation = Me.Location
    End Sub

    Private Sub frmMain_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        isDragging = False
    End Sub

    Private Sub frmMain_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If isDragging Then
            Dim dx As Integer = (e.Location.X - mouseDownLocation.X)
            Dim dy As Integer = (e.Location.Y - mouseDownLocation.Y)

            Select Case pointerPosition
                Case MousePositionInWindow.LeftMiddle
                    Me.Left += dx
                    Me.Width -= dx
                Case MousePositionInWindow.LeftTop
                    Me.Left += dx
                    Me.Width -= dx

                    Me.Top += dy
                    Me.Height -= dy
                Case MousePositionInWindow.LeftBottom
                    Me.Left += dx
                    Me.Width -= dx

                    Me.Height += dy
                    mouseDownLocation.Y = e.Location.Y
                Case MousePositionInWindow.BottomMiddle
                    Me.Height += dy
                    mouseDownLocation.Y = e.Location.Y
                Case MousePositionInWindow.TopMiddle
                    Me.Top += dy
                    Me.Height -= dy
                Case MousePositionInWindow.RightTop
                    Me.Width += dx
                    mouseDownLocation.X = e.Location.X

                    Me.Top += dy
                    Me.Height -= dy
                Case MousePositionInWindow.RightMiddle
                    Me.Width += dx
                    mouseDownLocation.X = e.Location.X
                Case MousePositionInWindow.RightBottom
                    Me.Width += dx
                    mouseDownLocation.X = e.Location.X

                    Me.Height += dy
                    mouseDownLocation.Y = e.Location.Y
                Case MousePositionInWindow.Body
                    Me.Left += dx
                    Me.Top += dy
            End Select

            SetHistorySize()
        Else
            pointerPosition = GetMousePositionInWindow(e.Location)
            SetCursor()
        End If
    End Sub

    Private Sub SetCursor()
        Select Case pointerPosition
            Case MousePositionInWindow.LeftTop : Me.Cursor = Cursors.SizeNWSE
            Case MousePositionInWindow.LeftMiddle : Me.Cursor = Cursors.SizeWE
            Case MousePositionInWindow.LeftBottom : Me.Cursor = Cursors.SizeNESW

            Case MousePositionInWindow.RightTop : Me.Cursor = Cursors.SizeNESW
            Case MousePositionInWindow.RightMiddle : Me.Cursor = Cursors.SizeWE
            Case MousePositionInWindow.RightBottom : Me.Cursor = Cursors.SizeNWSE

            Case MousePositionInWindow.TopMiddle : Me.Cursor = Cursors.SizeNS
            Case MousePositionInWindow.BottomMiddle : Me.Cursor = Cursors.SizeNS

            Case MousePositionInWindow.Body : Me.Cursor = Cursors.SizeAll
        End Select
    End Sub

    Private Function GetMousePositionInWindow(ByVal p As Point)
        Const margin As Integer = 5
        Dim cornerH As HorizontalDragPosition
        Dim cornerV As VerticalDragPosition

        Select Case p.X
            Case Is <= margin
                cornerH = HorizontalDragPosition.HorizontalLeft
            Case Is >= Me.Width - margin
                cornerH = HorizontalDragPosition.HorizontalRight
            Case Else
                cornerH = HorizontalDragPosition.HorizontalMiddle
        End Select

        Select Case p.Y
            Case Is <= margin
                cornerV = VerticalDragPosition.VerticalTop
            Case Is >= Me.Height - margin
                cornerV = VerticalDragPosition.VerticalBottom
            Case Else
                cornerV = VerticalDragPosition.VerticalMiddle
        End Select

        Return [Enum].Parse(GetType(MousePositionInWindow), cornerH + cornerV)
    End Function
End Class
