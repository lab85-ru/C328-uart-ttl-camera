<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.btCam = New System.Windows.Forms.Button
        Me.tbLog = New System.Windows.Forms.TextBox
        Me.SerialPort1 = New System.IO.Ports.SerialPort(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.pbFrame = New System.Windows.Forms.PictureBox
        Me.cbDebugLog = New System.Windows.Forms.CheckBox
        Me.cbPorts = New System.Windows.Forms.ComboBox
        Me.btPOpen = New System.Windows.Forms.Button
        Me.btScanComPort = New System.Windows.Forms.Button
        Me.gbCamera = New System.Windows.Forms.GroupBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.lblFrameCount = New System.Windows.Forms.Label
        CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbCamera.SuspendLayout()
        Me.SuspendLayout()
        '
        'btCam
        '
        Me.btCam.Location = New System.Drawing.Point(6, 12)
        Me.btCam.Name = "btCam"
        Me.btCam.Size = New System.Drawing.Size(75, 21)
        Me.btCam.TabIndex = 0
        Me.btCam.Text = "Start"
        Me.btCam.UseVisualStyleBackColor = True
        '
        'tbLog
        '
        Me.tbLog.Location = New System.Drawing.Point(12, 57)
        Me.tbLog.Multiline = True
        Me.tbLog.Name = "tbLog"
        Me.tbLog.ReadOnly = True
        Me.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.tbLog.Size = New System.Drawing.Size(384, 480)
        Me.tbLog.TabIndex = 1
        '
        'SerialPort1
        '
        Me.SerialPort1.BaudRate = 115200
        Me.SerialPort1.PortName = "COM29"
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        '
        'pbFrame
        '
        Me.pbFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pbFrame.Location = New System.Drawing.Point(402, 57)
        Me.pbFrame.Name = "pbFrame"
        Me.pbFrame.Size = New System.Drawing.Size(640, 480)
        Me.pbFrame.TabIndex = 2
        Me.pbFrame.TabStop = False
        '
        'cbDebugLog
        '
        Me.cbDebugLog.AutoSize = True
        Me.cbDebugLog.Location = New System.Drawing.Point(540, 15)
        Me.cbDebugLog.Name = "cbDebugLog"
        Me.cbDebugLog.Size = New System.Drawing.Size(79, 17)
        Me.cbDebugLog.TabIndex = 3
        Me.cbDebugLog.Text = "Debug Log"
        Me.cbDebugLog.UseVisualStyleBackColor = True
        '
        'cbPorts
        '
        Me.cbPorts.FormattingEnabled = True
        Me.cbPorts.Location = New System.Drawing.Point(158, 22)
        Me.cbPorts.Name = "cbPorts"
        Me.cbPorts.Size = New System.Drawing.Size(121, 21)
        Me.cbPorts.TabIndex = 4
        '
        'btPOpen
        '
        Me.btPOpen.Location = New System.Drawing.Point(285, 21)
        Me.btPOpen.Name = "btPOpen"
        Me.btPOpen.Size = New System.Drawing.Size(75, 23)
        Me.btPOpen.TabIndex = 5
        Me.btPOpen.Text = "Open"
        Me.btPOpen.UseVisualStyleBackColor = True
        '
        'btScanComPort
        '
        Me.btScanComPort.Location = New System.Drawing.Point(12, 9)
        Me.btScanComPort.Name = "btScanComPort"
        Me.btScanComPort.Size = New System.Drawing.Size(108, 42)
        Me.btScanComPort.TabIndex = 6
        Me.btScanComPort.Text = "Поиск портов"
        Me.btScanComPort.UseVisualStyleBackColor = True
        '
        'gbCamera
        '
        Me.gbCamera.Controls.Add(Me.Label1)
        Me.gbCamera.Controls.Add(Me.lblFrameCount)
        Me.gbCamera.Controls.Add(Me.btCam)
        Me.gbCamera.Controls.Add(Me.cbDebugLog)
        Me.gbCamera.Location = New System.Drawing.Point(407, 9)
        Me.gbCamera.Name = "gbCamera"
        Me.gbCamera.Size = New System.Drawing.Size(635, 42)
        Me.gbCamera.TabIndex = 7
        Me.gbCamera.TabStop = False
        Me.gbCamera.Text = "Camera"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(145, 17)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(96, 13)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Кадров получено:"
        '
        'lblFrameCount
        '
        Me.lblFrameCount.AutoSize = True
        Me.lblFrameCount.Location = New System.Drawing.Point(247, 17)
        Me.lblFrameCount.Name = "lblFrameCount"
        Me.lblFrameCount.Size = New System.Drawing.Size(39, 13)
        Me.lblFrameCount.TabIndex = 4
        Me.lblFrameCount.Text = "Label1"
        '
        'Form
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1048, 546)
        Me.Controls.Add(Me.gbCamera)
        Me.Controls.Add(Me.btScanComPort)
        Me.Controls.Add(Me.btPOpen)
        Me.Controls.Add(Me.cbPorts)
        Me.Controls.Add(Me.pbFrame)
        Me.Controls.Add(Me.tbLog)
        Me.Name = "Form"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "C328 JPEG to UART camera capture v1.0"
        CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbCamera.ResumeLayout(False)
        Me.gbCamera.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btCam As System.Windows.Forms.Button
    Friend WithEvents tbLog As System.Windows.Forms.TextBox
    Friend WithEvents SerialPort1 As System.IO.Ports.SerialPort
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents pbFrame As System.Windows.Forms.PictureBox
    Friend WithEvents cbDebugLog As System.Windows.Forms.CheckBox
    Friend WithEvents cbPorts As System.Windows.Forms.ComboBox
    Friend WithEvents btPOpen As System.Windows.Forms.Button
    Friend WithEvents btScanComPort As System.Windows.Forms.Button
    Friend WithEvents gbCamera As System.Windows.Forms.GroupBox
    Friend WithEvents lblFrameCount As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
