Imports System
Imports System.IO
Imports System.IO.BinaryReader
Imports System.IO.File
Imports System.IO.Ports


Public Class Form
    Dim fs As FileInfo                                      ' файл
    Dim FLStream As StreamWriter

    '                               0    1    2    3    4    5
    Dim CAM_SYNC_PKT() As Byte = {&HAA, &HD, &H0, &H0, &H0, &H0}
    Dim CAM_INIT_PKT() As Byte = {&HAA, &H1, &H0, &H7, &H7, &H7} 'jpeg 640x480
    Dim CAM_ACK_PKT() As Byte = {&HAA, &HE, &H0, &H0, &H0, &H0} ' в 2 байте передается код подтверждения предыдущего запроса из 1 байта
    Dim CAM_PAKSIZE_PKT() As Byte = {&HAA, &H6, &H8, &H0, &H2, &H0} '512
    Dim CAM_SHAPSHOT_PKT() As Byte = {&HAA, &H5, &H0, &H0, &H0, &H0} 'snapshot
    Dim CAM_GETPIC_PKT() As Byte = {&HAA, &H4, &H1, &H0, &H0, &H0} 'get picture

    Dim uart_queue_rx As New Queue(Of Byte) ' очередь - принятых байт от камеры

    Dim Ports As String()              ' список портов в системе

    Const PORT_OPEN As String = "Открыть"
    Const PORT_CLOSE As String = "Закрыть"

    Const CAM_CAP_START As String = "Захват"
    Const CAM_CAP_STOP As String = "Стоп"

    Enum port_status_e
        open = 1
        close = 0
    End Enum

    Dim CPortStatus As port_status_e   ' состояниесом порта открыт - закрыт

    ' Состояние автомата - работы с камерой
    Enum capture_state
        CAP_STOP = 0
        CAP_SYNC = 1
        CAP_SYNC_ACK = 2
        CAP_INIT = 3
        CAP_PAKSIZE = 4
        CAP_SNAPSHOT = 5
        CAP_GETPIC = 6
        CAP_DATA = 7
        CAP_END = 8
    End Enum

    Dim capture_st As capture_state = capture_state.CAP_STOP

    Dim image_id_count As UInt16 = 0 ' номер пакета
    Dim image_size As Integer = 0 ' размер картинки от камеры из ее пакета
    Dim data_count As Integer = 0 ' колличество данных полученных от камеры, только данные
    Dim cur_pak_size As Integer = 0 ' длинна пакета от камеры - текущая, вичисляем, последний пакет может быть короче 512 байт
    Dim count As Integer = 0 ' глобальный счетчик проходов
    Dim frame_count As Integer ' количество полученных кадров 
    Dim timer_count As Integer ' счетчик вызовов таймера, для обрезания лога

    Const PAK_HEAD_LEN As Integer = 4 ' длинна заголока пакета данных в байтах
    Const PAK_SUM_LEN As Integer = 2 ' длинна контрольной суммы в пакете данных
    Const PAK_DATA_SIZE As Integer = 512 ' длинна пакета - с данными и заголовком от камеры
    Const PAK_CFG_LEN As Integer = 6 'длинна пакета управления в байтах

    Dim pic_buffer(1024 * 1024) As Byte ' буфер для сборки картинки

    Private Sub Form_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gbCamera.Enabled = False
        btCam.Text = CAM_CAP_START
        btPOpen.Text = PORT_OPEN
        lblFrameCount.Text = Str(frame_count)

    End Sub

    Private Sub btCam_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btCam.Click

        If btCam.Text = CAM_CAP_START Then
            btCam.Text = CAM_CAP_STOP
            uart_queue_rx.Clear()
            count = 0
            capture_st = capture_state.CAP_SYNC
            frame_count = 0
            lblFrameCount.Text = Str(frame_count)
        Else
            btCam.Text = CAM_CAP_START
            System.Threading.Thread.Sleep(1000)
            count = 0
            capture_st = capture_state.CAP_STOP
            frame_count = 0
            lblFrameCount.Text = Str(frame_count)
        End If

    End Sub

    ' создает строку с текущей датой и расширением .jpg
    Function CreateTxtFileName() As String
        Dim str As String
        Dim cc As String
        Dim i As Integer
        Dim str_date As String
        Dim str_date_out As String

        str_date = DateTime.Now
        str_date_out = str_date.ToString()

        str = ""
        For i = 1 To Len(str_date_out)
            cc = Mid(str_date_out, i, 1)
            If cc = "." Or cc = ":" Or cc = " " Then
                cc = "_"
            End If
            str = str + cc
        Next
        str = str + ".jpg"

        CreateTxtFileName = str
    End Function

    Private Sub SerialPort1_DataReceived(ByVal sender As System.Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim i As Integer
        Dim l As Integer
        Dim buf(4096) As Byte

        l = SerialPort1.BytesToRead
        For i = 0 To l - 1
            SerialPort1.Read(buf, i, 1)
            uart_queue_rx.Enqueue(buf(i))
        Next i

    End Sub

    Sub out_array_hex(ByRef array() As Byte, ByVal len As UInt32)
        Dim m, j As UInt32
        Dim string_out As String

        string_out = ""

        m = 0                       ' выводим взятый пакет
        For j = 0 To len - 1
            string_out = string_out + String.Format("{0:X2}", array(j)) + " "
            m = m + 1
            If m = 16 Then
                m = 0
                string_out = string_out + vbCrLf
            End If
        Next
        Log(string_out + vbCrLf)
    End Sub

    Function cam_send_paket(ByRef array() As Byte) As Byte

        If cbDebugLog.Checked = True Then
            Log("TX: ")
            out_array_hex(array, PAK_CFG_LEN)
        End If

        SerialPort1.Write(array, 0, PAK_CFG_LEN)
        'System.Threading.Thread.Sleep(5)

    End Function

    ' возвращает пакет из буфера приема от камеры
    ' в начале камеры должен быть код AA
    Function get_paket(ByVal len As Integer) As Byte()
        Dim i As Integer
        Dim buf_tmp(4096) As Byte

        For i = 0 To len - 1
            buf_tmp(i) = uart_queue_rx.Dequeue()
        Next
        get_paket = buf_tmp

        If cbDebugLog.Checked = True Then
            Log("RX: ")
            out_array_hex(buf_tmp, len)
        End If

    End Function

    ' сравнение двух массивов
    ' =0, массивы равны
    ' =1, массивы различаются
    Function cmp_pkt(ByRef buf1() As Byte, ByVal len As Integer, ByRef buf2() As Byte)
        Dim i As Integer

        For i = 0 To len - 1
            If buf1(i) <> buf2(i) Then
                cmp_pkt = 1
                Exit Function
            End If

        Next
        cmp_pkt = 0
    End Function

    Sub Log(ByVal str As String)
        tbLog.AppendText(str)
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Dim result As Byte
        Dim uart_rx_count As Integer

        uart_rx_count = uart_queue_rx.Count

        If capture_st <> capture_state.CAP_STOP Then
            Select Case capture_st
                Case capture_state.CAP_SYNC
                    count = count + 1
                    uart_queue_rx.Clear()

                    Log("Sync: ")
                    result = cam_send_paket(CAM_SYNC_PKT)
                    capture_st = capture_state.CAP_SYNC_ACK

                Case capture_state.CAP_SYNC_ACK
                    ' PAK_CFG_LEN * 2 - т.к. ожидаем два пакета: ACK и запрос SYNC от камеры
                    'If uart_rx_count > 0 And uart_rx_count <> PAK_CFG_LEN * 2 Then
                    'Exit Sub ' не весь пакет приняли подождем
                    'End If

                    'If uart_rx_count >= PAK_CFG_LEN * 2 Then
                    'Log("ACK ok")
                    'capture_st = capture_state.CAP_END
                    'Log("end")
                    'End If

                    If uart_rx_count = 0 And count < 60 Then
                        Log("wait " + Str(count) + ".")
                        capture_st = capture_state.CAP_SYNC
                    End If

                    If count = 60 Then
                        Log("No ACK" + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If

                    If uart_rx_count = PAK_CFG_LEN * 2 Then
                        Dim pkt_ack() As Byte = get_paket(PAK_CFG_LEN)
                        If cmp_pkt(pkt_ack, 2, CAM_ACK_PKT) <> 0 Then ' проверяе только первые 2 байта, т.к. 3 - код, 4 байт хх !!!
                            Log("ERROR: No ACK paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                            Exit Sub
                        End If

                        Dim pkt() As Byte = get_paket(PAK_CFG_LEN)

                        If cmp_pkt(pkt, PAK_CFG_LEN, CAM_SYNC_PKT) <> 0 Then
                            Log("ERROR: No SYNC paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                            Exit Sub
                        End If
                        cam_send_paket(pkt_ack) ' посылаем камере подтверждение. полученное от неё-же.

                        Log("OK" + vbCrLf)
                        'System.Threading.Thread.Sleep(2000) 'задержка для камеры - камера должна настроить AGC and AEC
                        capture_st = capture_state.CAP_INIT
                        count = 0
                    End If



                Case capture_state.CAP_INIT

                    If count = 0 Then
                        Log("Capture init..." + vbCrLf)
                        result = cam_send_paket(CAM_INIT_PKT)
                    End If

                    count = count + 1

                    If uart_rx_count >= PAK_CFG_LEN Then
                        Dim pkt() As Byte = get_paket(PAK_CFG_LEN)
                        If cmp_pkt(pkt, 2, CAM_ACK_PKT) = 0 Then
                            Log("ACK ok" + vbCrLf)
                            capture_st = capture_state.CAP_PAKSIZE
                            count = 0
                        Else
                            Log("ERROR: No ack paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                        End If
                    End If

                    If count = 100 Then
                        Log("TIME OUT..." + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If

                Case capture_state.CAP_PAKSIZE
                    If count = 0 Then
                        Log("Set pak size..." + vbCrLf)
                        result = cam_send_paket(CAM_PAKSIZE_PKT)
                    End If

                    count = count + 1

                    If uart_rx_count >= PAK_CFG_LEN Then
                        Dim pkt() As Byte = get_paket(PAK_CFG_LEN)
                        If cmp_pkt(pkt, 2, CAM_ACK_PKT) = 0 Then
                            Log("ACK ok" + vbCrLf)
                            capture_st = capture_state.CAP_SNAPSHOT
                            count = 0
                        Else
                            Log("ERROR: No ack paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                        End If
                    End If

                    If count = 100 Then
                        Log("TIME OUT..." + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If



                Case capture_state.CAP_SNAPSHOT
                    If count = 0 Then
                        Log("Get snap shot..." + vbCrLf)
                        result = cam_send_paket(CAM_SHAPSHOT_PKT)
                    End If

                    count = count + 1

                    If uart_rx_count >= PAK_CFG_LEN Then
                        Dim pkt() As Byte = get_paket(PAK_CFG_LEN)
                        If cmp_pkt(pkt, 2, CAM_ACK_PKT) = 0 Then
                            Log("ACK ok" + vbCrLf)
                            capture_st = capture_state.CAP_GETPIC
                            count = 0
                        Else
                            Log("ERROR: No ack paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                        End If
                    End If

                    If count = 100 Then
                        Log("TIME OUT..." + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If

                Case capture_state.CAP_GETPIC
                    If count = 0 Then
                        Log("Get picture..." + vbCrLf)
                        result = cam_send_paket(CAM_GETPIC_PKT)
                    End If

                    count = count + 1

                    If uart_rx_count = PAK_CFG_LEN * 2 Then
                        Dim pkt_ack() As Byte = get_paket(PAK_CFG_LEN)
                        If cmp_pkt(pkt_ack, 2, CAM_ACK_PKT) <> 0 Then ' проверяе только первые 2 байта, т.к. 3 - код, 4 байт хх !!!
                            Log("ERROR: No ACK paket" + vbCrLf)
                            capture_st = capture_state.CAP_END
                            Exit Sub
                        End If

                        Dim pkt() As Byte = get_paket(PAK_CFG_LEN) ' в этом пакете в последних трех байтах длинна картинки

                        image_size = pkt(5) * 256 * 256 + pkt(4) * 256 + pkt(3)
                        Log("Image size = " + Str(image_size) + vbCrLf)

                        image_id_count = 0
                        data_count = 0

                        Log("OK" + vbCrLf)
                        count = 0
                        capture_st = capture_state.CAP_DATA
                        Timer1.Interval = 5
                    End If

                    If count = 100 Then
                        Log("TIME OUT..." + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If

                Case capture_state.CAP_DATA
                    Dim pkt_ack(PAK_CFG_LEN) As Byte

                    Log(".")

                    If count = 0 Then
                        pkt_ack(0) = &HAA
                        pkt_ack(1) = &HE
                        pkt_ack(2) = &H0
                        pkt_ack(3) = &H0
                        pkt_ack(4) = image_id_count And &HFF
                        pkt_ack(5) = (image_id_count And &HFF00) / 256

                        cam_send_paket(pkt_ack) ' посылаем камере подтверждение.
                        image_id_count = image_id_count + 1

                        ' Вычисление размера принимаемого пакета ! последний пакет может бвть не равен 512 !
                        If image_size - data_count > PAK_DATA_SIZE Then
                            cur_pak_size = PAK_DATA_SIZE
                        Else
                            cur_pak_size = image_size - data_count + PAK_HEAD_LEN + PAK_SUM_LEN
                        End If

                    End If

                    count = count + 1

                    If uart_rx_count >= cur_pak_size Then
                        Dim buf(cur_pak_size) As Byte
                        buf = get_paket(cur_pak_size)

                        ' копируем данные из пакета в буфер картинки
                        Dim i As Integer
                        For i = data_count To data_count + cur_pak_size - (PAK_HEAD_LEN + PAK_SUM_LEN)
                            pic_buffer(i) = buf(i - data_count + PAK_HEAD_LEN)
                        Next

                        data_count = data_count + (cur_pak_size - (PAK_HEAD_LEN + PAK_SUM_LEN))
                        count = 0
                        If image_size - data_count = 0 Then
                            'If image_size - data_count <= 512 - 6 Then
                            pkt_ack(0) = &HAA
                            pkt_ack(1) = &HE
                            pkt_ack(2) = &H0
                            pkt_ack(3) = &H0
                            pkt_ack(4) = &HF0
                            pkt_ack(5) = &HF0

                            cam_send_paket(pkt_ack) ' посылаем камере подтверждение. последний пакет
                            Log(vbCrLf + "Image load OK " + vbCrLf)
                            'capture_state = CAP_STOP
                            capture_st = capture_state.CAP_INIT
                            Timer1.Interval = 100

                            frame_count = frame_count + 1
                            lblFrameCount.Text = Str(frame_count)

                            ' выводим картинку на экран в форму
                            Dim my_image As Image
                            Dim ms As System.IO.MemoryStream = New System.IO.MemoryStream(pic_buffer)
                            my_image = System.Drawing.Image.FromStream(ms)
                            pbFrame.Image = my_image
                            pbFrame.Refresh()

                            ' Запись картинки в файл
                            Dim fs As New FileStream(CreateTxtFileName(), FileMode.CreateNew)
                            ' Create the writer for data.
                            Dim w As New BinaryWriter(fs)
                            ' Write data to Test.data.
                            w.Write(pic_buffer, 0, image_size)
                            w.Close()
                            fs.Close()

                        End If
                    End If

                    If count = 100 Then
                        Log(vbCrLf + "ERROR: time out " + vbCrLf)
                        capture_st = capture_state.CAP_END
                    End If


                    'pbFrame.Image = Image.FromStream(ms)
                    'frame_counter = frame_counter + 1
                    'tbFrame.Text = Str(frame_counter)

                Case capture_state.CAP_STOP
                    capture_st = capture_state.CAP_STOP
                    btCam.Text = CAM_CAP_STOP
                    uart_queue_rx.Clear()
                    count = 0
                    capture_st = capture_state.CAP_SYNC

                Case capture_state.CAP_END
                    Timer1.Interval = 100
                    Log("Delay -> New Sync." + vbCrLf)
                    System.Threading.Thread.Sleep(1000)
                    uart_queue_rx.Clear()
                    count = 0
                    capture_st = capture_state.CAP_SYNC


            End Select
        End If

        ' обрезаем лог если стал слишком большим
        timer_count = timer_count + 1
        If (timer_count = 10000) Then
            If Len(tbLog.Text) > 32000 Then
                Log(Mid(tbLog.Text, Len(tbLog.Text) - 1000, 1000))
                timer_count = 0
            End If
        End If

    End Sub

    Private Sub btPOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btPOpen.Click

        If cbPorts.SelectedItem <> "" And CPortStatus = port_status_e.close Then

            CPortStatus = port_status_e.open
            SerialPort1.PortName = cbPorts.SelectedItem
            SerialPort1.Open()
            btPOpen.Text = PORT_CLOSE

            ' Гасим меню недаем выбрать- пока не закроют порт
            gbCamera.Enabled = True

            Log(vbCrLf + "Порт Открыт." + vbCrLf)
            capture_st = capture_state.CAP_STOP
            uart_queue_rx.Clear()

        ElseIf CPortStatus = port_status_e.open Then
            SerialPort1.Close()

            ' Включаем меню даем выбрать
            gbCamera.Enabled = False

            CPortStatus = port_status_e.close
            btPOpen.Text = PORT_OPEN
            Log(vbCrLf + "Порт == закрыт ===." + vbCrLf)
            capture_st = capture_state.CAP_STOP
            uart_queue_rx.Clear()
            btCam.Text = CAM_CAP_START
        End If

    End Sub

    Private Sub btScanComPort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btScanComPort.Click
        Ports = SerialPort.GetPortNames
        Dim port As String

        cbPorts.Items.Clear()

        For Each port In Ports
            Log(port + vbCrLf)
            cbPorts.Items.Add(port)
        Next port
    End Sub


End Class
