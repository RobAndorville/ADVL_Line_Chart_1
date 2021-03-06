﻿Public Class ChartInfo
    'The ChartInfo class stores information that is not stored within the Chart control.


    'Dataset used to hold points for plotting:
    Public ds As New DataSet

    Public dictSeriesInfo As New Dictionary(Of String, SeriesInfo) 'dictSeriesInfo is indexed using the Chart SeriesName. dictSeriesInfo contains information about each Series in the Chart: .XValuesFieldName, .YValuesFieldName, ChartArea. 

    Public dictAreaInfo As New Dictionary(Of String, AreaInfo) 'dictAreaInfo is indexed using the Chart Area Name. dictAreaInfo contains AutoMinimum, AutoMaximum and AutoMajorGridInterval settings for each axis in the ChartArea. (These are not stored in the chart control.)

    Public DataLocation As New ADVL_Utilities_Library_1.FileLocation 'Stores information about the data location in the Project - used to read the chart settings files.

#Region " Properties" '---------------------------------------------------------------------------------------------------

    Private _fileName As String = "" 'The file name (with extension) of the chart settings. This file is stored in the Project.
    Property FileName As String
        Get
            Return _fileName
        End Get
        Set(value As String)
            _fileName = value
        End Set
    End Property

    Private _inputDataType As String = "Database" 'Database or Dataset
    Property InputDataType As String
        Get
            Return _inputDataType
        End Get
        Set(value As String)
            _inputDataType = value
        End Set
    End Property

    Private _inputDatabasePath As String = ""
    Property InputDatabasePath As String
        Get
            Return _inputDatabasePath
        End Get
        Set(value As String)
            _inputDatabasePath = value
        End Set
    End Property

    Private _inputQuery As String = ""
    Property InputQuery As String
        Get
            Return _inputQuery
        End Get
        Set(value As String)
            _inputQuery = value
        End Set
    End Property

    Private _inputDataDescr As String = "" 'A description of the data selected for charting.
    Property InputDataDescr As String
        Get
            Return _inputDataDescr
        End Get
        Set(value As String)
            _inputDataDescr = value
        End Set
    End Property


#End Region 'Properties --------------------------------------------------------------------------------------------------

#Region "Methods" '-------------------------------------------------------------------------------------------------------

    Public Sub LoadFile(ByRef myFileName As String, ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Load the Line Chart settings from the selected file.
        'This will update properties in ChartInfo and the myChart control.

        If myFileName.Trim = "" Then
            Exit Sub
        End If

        Dim XDoc As System.Xml.Linq.XDocument
        DataLocation.ReadXmlData(myFileName, XDoc)

        If XDoc Is Nothing Then
            RaiseEvent ErrorMessage("Xml list file is blank." & vbCrLf)
            Exit Sub
        End If

        'Restore Input Data settings:
        If XDoc.<ChartSettings>.<InputDataType>.Value <> Nothing Then InputDataType = XDoc.<ChartSettings>.<InputDataType>.Value
        If XDoc.<ChartSettings>.<InputDatabasePath>.Value <> Nothing Then InputDatabasePath = XDoc.<ChartSettings>.<InputDatabasePath>.Value
        If XDoc.<ChartSettings>.<InputQuery>.Value <> Nothing Then InputQuery = XDoc.<ChartSettings>.<InputQuery>.Value
        If XDoc.<ChartSettings>.<InputDataDescr>.Value <> Nothing Then InputDataDescr = XDoc.<ChartSettings>.<InputDataDescr>.Value

        'Restore Series Info: SeriesName, XValuesFieldName, YValuesFieldName:
        Dim SeriesInfo = From item In XDoc.<ChartSettings>.<SeriesInfoList>.<SeriesInfo>
        Dim SeriesInfoName As String
        dictSeriesInfo.Clear() 'Clear the dictionary of Series Information. New Field entries will be added below.
        For Each item In SeriesInfo
            SeriesInfoName = item.<Name>.Value
            dictSeriesInfo.Add(SeriesInfoName, New SeriesInfo)
            dictSeriesInfo(SeriesInfoName).XValuesFieldName = item.<XValuesFieldName>.Value
            dictSeriesInfo(SeriesInfoName).YValuesFieldName = item.<YValuesFieldName>.Value
            If item.<ChartArea>.Value <> Nothing Then dictSeriesInfo(SeriesInfoName).ChartArea = item.<ChartArea>.Value
        Next

        'Restore Area Info: 
        Dim AreaInfo = From item In XDoc.<ChartSettings>.<AreaInfoList>.<AreaInfo>
        Dim AreaInfoName As String
        dictAreaInfo.Clear() 'Clear the dictionary of Chart Area Information. New Field entries will be added below.
        For Each item In AreaInfo
            AreaInfoName = item.<Name>.Value
            dictAreaInfo.Add(AreaInfoName, New AreaInfo)
            dictAreaInfo(AreaInfoName).AutoXAxisMinimum = item.<AutoXAxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoXAxisMaximum = item.<AutoXAxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoXAxisMajorGridInterval = item.<AutoXAxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMinimum = item.<AutoX2AxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMaximum = item.<AutoX2AxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMajorGridInterval = item.<AutoX2AxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMinimum = item.<AutoYAxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMaximum = item.<AutoYAxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMajorGridInterval = item.<AutoYAxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMinimum = item.<AutoY2AxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMaximum = item.<AutoY2AxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMajorGridInterval = item.<AutoY2AxisMajorGridInterval>.Value
        Next

        'Restore Titles:
        Dim TitlesInfo = From item In XDoc.<ChartSettings>.<TitlesCollection>.<Title>
        Dim TitleName As String
        Dim myFontStyle As FontStyle
        Dim myFontSize As Single
        myChart.Titles.Clear()
        For Each item In TitlesInfo
            TitleName = item.<Name>.Value
            myChart.Titles.Add(TitleName).Name = TitleName 'The name needs to be explicitly declared!
            myChart.Titles(TitleName).Text = item.<Text>.Value
            myChart.Titles(TitleName).TextOrientation = [Enum].Parse(GetType(DataVisualization.Charting.TextOrientation), item.<TextOrientation>.Value)
            myChart.Titles(TitleName).Alignment = [Enum].Parse(GetType(ContentAlignment), item.<Alignment>.Value)
            myChart.Titles(TitleName).ForeColor = Color.FromArgb(item.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<Font>.<Size>.Value
            myChart.Titles(TitleName).Font = New Font(item.<Font>.<Name>.Value, myFontSize, myFontStyle)
        Next

        'Restore Chart Series:
        Dim Series = From item In XDoc.<ChartSettings>.<SeriesCollection>.<Series>
        Dim SeriesName As String
        myChart.Series.Clear()
        For Each item In Series
            SeriesName = item.<Name>.Value
            myChart.Series.Add(SeriesName)
            'myChart.Series(SeriesName).ChartType = [Enum].Parse(GetType(DataVisualization.Charting.SeriesChartType), item.<Type>.Value)
            myChart.Series(SeriesName).ChartType = [Enum].Parse(GetType(DataVisualization.Charting.SeriesChartType), item.<ChartType>.Value)
            If item.<ChartArea>.Value <> Nothing Then myChart.Series(SeriesName).ChartArea = item.<ChartArea>.Value
            myChart.Series(SeriesName).Legend = item.<Legend>.Value
            myChart.Series(SeriesName).SetCustomProperty("EmptyPointValue", item.<EmptyPointValue>.Value)
            myChart.Series(SeriesName).SetCustomProperty("LabelStyle", item.<LabelStyle>.Value)
            myChart.Series(SeriesName).SetCustomProperty("PixelPointDepth", item.<PixelPointDepth>.Value)
            myChart.Series(SeriesName).SetCustomProperty("PixelPointGapDepth", item.<PixelPointGapDepth>.Value)
            myChart.Series(SeriesName).SetCustomProperty("ShowMarkerLines", item.<ShowMarkerLines>.Value)
            myChart.Series(SeriesName).AxisLabel = item.<AxisLabel>.Value
            myChart.Series(SeriesName).XAxisType = [Enum].Parse(GetType(DataVisualization.Charting.AxisType), item.<XAxisType>.Value)
            myChart.Series(SeriesName).YAxisType = [Enum].Parse(GetType(DataVisualization.Charting.AxisType), item.<YAxisType>.Value)
            If item.<XValueType>.Value <> Nothing Then myChart.Series(SeriesName).XValueType = [Enum].Parse(GetType(DataVisualization.Charting.ChartValueType), item.<XValueType>.Value)
            If item.<YValueType>.Value <> Nothing Then myChart.Series(SeriesName).YValueType = [Enum].Parse(GetType(DataVisualization.Charting.ChartValueType), item.<YValueType>.Value)
            If item.<Marker>.<BorderColor>.Value <> Nothing Then myChart.Series(SeriesName).MarkerBorderColor = Color.FromArgb(item.<Marker>.<BorderColor>.Value)
            If item.<Marker>.<BorderWidth>.Value <> Nothing Then myChart.Series(SeriesName).MarkerBorderWidth = item.<Marker>.<BorderWidth>.Value
            If item.<Marker>.<Color>.Value <> Nothing Then myChart.Series(SeriesName).MarkerColor = Color.FromArgb(item.<Marker>.<Color>.Value)
            If item.<Marker>.<Size>.Value <> Nothing Then myChart.Series(SeriesName).MarkerSize = item.<Marker>.<Size>.Value
            If item.<Marker>.<Step>.Value <> Nothing Then myChart.Series(SeriesName).MarkerStep = item.<Marker>.<Step>.Value
            If item.<Marker>.<Style>.Value <> Nothing Then myChart.Series(SeriesName).MarkerStyle = [Enum].Parse(GetType(DataVisualization.Charting.MarkerStyle), item.<Marker>.<Style>.Value)
            If item.<Color>.Value <> Nothing Then myChart.Series(SeriesName).Color = Color.FromArgb(item.<Color>.Value)
        Next

        'Restore Chart Areas:
        Dim Areas = From item In XDoc.<ChartSettings>.<ChartAreasCollection>.<ChartArea>
        Dim AreaName As String
        myChart.ChartAreas.Clear()
        For Each item In Areas
            AreaName = item.<Name>.Value
            myChart.ChartAreas.Add(AreaName)
            'AxisX Properties:
            myChart.ChartAreas(AreaName).AxisX.Title = item.<AxisX>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisX.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisX>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisX.TitleForeColor = Color.FromArgb(item.<AxisX>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisX>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisX>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisX>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisX>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisX>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisX.TitleFont = New Font(item.<AxisX>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisX>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisX.LabelStyle.Format = item.<AxisX>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisX.Minimum = item.<AxisX>.<Minimum>.Value

            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMinimum Then myChart.ChartAreas(AreaName).AxisX.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisX.Maximum = item.<AxisX>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMaximum Then myChart.ChartAreas(AreaName).AxisX.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisX.LineWidth = item.<AxisX>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisX.Interval = item.<AxisX>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisX.IntervalOffset = item.<AxisX>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisX.Crossing = item.<AxisX>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisX.MajorGrid.Interval = item.<AxisX>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisX.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisX.MajorGrid.IntervalOffset = item.<AxisX>.<MajorGrid>.<IntervalOffset>.Value

            'AxisX2 Properties:
            myChart.ChartAreas(AreaName).AxisX2.Title = item.<AxisX2>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisX2.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisX2>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisX2.TitleForeColor = Color.FromArgb(item.<AxisX2>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisX2>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisX2>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisX2>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisX2>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisX2>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisX2.TitleFont = New Font(item.<AxisX2>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisX2>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisX2.LabelStyle.Format = item.<AxisX2>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisX2.Minimum = item.<AxisX2>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMinimum Then myChart.ChartAreas(AreaName).AxisX2.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisX2.Maximum = item.<AxisX2>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMaximum Then myChart.ChartAreas(AreaName).AxisX2.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisX2.LineWidth = item.<AxisX2>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisX2.Interval = item.<AxisX2>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisX2.IntervalOffset = item.<AxisX2>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisX2.Crossing = item.<AxisX2>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisX2.MajorGrid.Interval = item.<AxisX2>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisX2.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisX2.MajorGrid.IntervalOffset = item.<AxisX2>.<MajorGrid>.<IntervalOffset>.Value

            'AxisY Properties:
            myChart.ChartAreas(AreaName).AxisY.Title = item.<AxisY>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisY.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisY>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisY.TitleForeColor = Color.FromArgb(item.<AxisY>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisY>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisY>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisY>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisY>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisY>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisY.TitleFont = New Font(item.<AxisY>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisY>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisY.LabelStyle.Format = item.<AxisY>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisY.Minimum = item.<AxisY>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMinimum Then myChart.ChartAreas(AreaName).AxisY.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisY.Maximum = item.<AxisY>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMaximum Then myChart.ChartAreas(AreaName).AxisY.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisY.LineWidth = item.<AxisY>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisY.Interval = item.<AxisY>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisY.IntervalOffset = item.<AxisY>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisY.Crossing = item.<AxisY>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisY.MajorGrid.Interval = item.<AxisY>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisY.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisY.MajorGrid.IntervalOffset = item.<AxisY>.<MajorGrid>.<IntervalOffset>.Value

            'AxisY2 Properties:
            myChart.ChartAreas(AreaName).AxisY2.Title = item.<AxisY2>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisY2.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisY2>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisY2.TitleForeColor = Color.FromArgb(item.<AxisY2>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisY2>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisY2>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisY2>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisY2>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisY2>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisY2.TitleFont = New Font(item.<AxisY2>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisY2>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisY2.LabelStyle.Format = item.<AxisY2>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisY2.Minimum = item.<AxisY2>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMinimum Then myChart.ChartAreas(AreaName).AxisY2.Minimum = Double.NaN 'Set to Auto Minimum
            End If

            myChart.ChartAreas(AreaName).AxisY2.Maximum = item.<AxisY2>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMaximum Then myChart.ChartAreas(AreaName).AxisY2.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisY2.LineWidth = item.<AxisY2>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisY2.Interval = item.<AxisY2>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisY2.IntervalOffset = item.<AxisY2>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisY2.Crossing = item.<AxisY2>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisY2.MajorGrid.Interval = item.<AxisY2>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisY2.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisY2.MajorGrid.IntervalOffset = item.<AxisY2>.<MajorGrid>.<IntervalOffset>.Value
        Next

    End Sub

    Public Sub LoadXml(ByRef XDoc As System.Xml.Linq.XDocument, ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Load the Line Chart settings from the XDocument.
        'This will update properties in ChartInfo and the myChart control.

        If XDoc Is Nothing Then
            RaiseEvent ErrorMessage("Xml list file is blank." & vbCrLf)
            Exit Sub
        End If

        'Restore Input Data settings:
        If XDoc.<ChartSettings>.<InputDataType>.Value <> Nothing Then InputDataType = XDoc.<ChartSettings>.<InputDataType>.Value
        If XDoc.<ChartSettings>.<InputDatabasePath>.Value <> Nothing Then InputDatabasePath = XDoc.<ChartSettings>.<InputDatabasePath>.Value
        If XDoc.<ChartSettings>.<InputQuery>.Value <> Nothing Then InputQuery = XDoc.<ChartSettings>.<InputQuery>.Value
        If XDoc.<ChartSettings>.<InputDataDescr>.Value <> Nothing Then InputDataDescr = XDoc.<ChartSettings>.<InputDataDescr>.Value

        'Restore Series Info: SeriesName, XValuesFieldName, YValuesFieldName:
        Dim SeriesInfo = From item In XDoc.<ChartSettings>.<SeriesInfoList>.<SeriesInfo>
        Dim SeriesInfoName As String
        dictSeriesInfo.Clear() 'Clear the dictionary of Series Information. New Field entries will be added below.
        For Each item In SeriesInfo
            SeriesInfoName = item.<Name>.Value
            dictSeriesInfo.Add(SeriesInfoName, New SeriesInfo)
            dictSeriesInfo(SeriesInfoName).XValuesFieldName = item.<XValuesFieldName>.Value
            dictSeriesInfo(SeriesInfoName).YValuesFieldName = item.<YValuesFieldName>.Value
            If item.<ChartArea>.Value <> Nothing Then dictSeriesInfo(SeriesInfoName).ChartArea = item.<ChartArea>.Value
        Next

        'Restore Area Info: 
        Dim AreaInfo = From item In XDoc.<ChartSettings>.<AreaInfoList>.<AreaInfo>
        Dim AreaInfoName As String
        dictAreaInfo.Clear() 'Clear the dictionary of Chart Area Information. New Field entries will be added below.
        For Each item In AreaInfo
            AreaInfoName = item.<Name>.Value
            dictAreaInfo.Add(AreaInfoName, New AreaInfo)
            dictAreaInfo(AreaInfoName).AutoXAxisMinimum = item.<AutoXAxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoXAxisMaximum = item.<AutoXAxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoXAxisMajorGridInterval = item.<AutoXAxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMinimum = item.<AutoX2AxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMaximum = item.<AutoX2AxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoX2AxisMajorGridInterval = item.<AutoX2AxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMinimum = item.<AutoYAxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMaximum = item.<AutoYAxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoYAxisMajorGridInterval = item.<AutoYAxisMajorGridInterval>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMinimum = item.<AutoY2AxisMinimum>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMaximum = item.<AutoY2AxisMaximum>.Value
            dictAreaInfo(AreaInfoName).AutoY2AxisMajorGridInterval = item.<AutoY2AxisMajorGridInterval>.Value
        Next

        'Restore Titles:
        Dim TitlesInfo = From item In XDoc.<ChartSettings>.<TitlesCollection>.<Title>
        Dim TitleName As String
        Dim myFontStyle As FontStyle
        Dim myFontSize As Single
        myChart.Titles.Clear()
        For Each item In TitlesInfo
            TitleName = item.<Name>.Value
            myChart.Titles.Add(TitleName).Name = TitleName 'The name needs to be explicitly declared!
            myChart.Titles(TitleName).Text = item.<Text>.Value
            myChart.Titles(TitleName).TextOrientation = [Enum].Parse(GetType(DataVisualization.Charting.TextOrientation), item.<TextOrientation>.Value)
            myChart.Titles(TitleName).Alignment = [Enum].Parse(GetType(ContentAlignment), item.<Alignment>.Value)
            myChart.Titles(TitleName).ForeColor = Color.FromArgb(item.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<Font>.<Size>.Value
            myChart.Titles(TitleName).Font = New Font(item.<Font>.<Name>.Value, myFontSize, myFontStyle)
        Next

        'Restore Chart Series:
        Dim Series = From item In XDoc.<ChartSettings>.<SeriesCollection>.<Series>
        Dim SeriesName As String
        myChart.Series.Clear()
        For Each item In Series
            SeriesName = item.<Name>.Value
            myChart.Series.Add(SeriesName)
            'myChart.Series(SeriesName).ChartType = [Enum].Parse(GetType(DataVisualization.Charting.SeriesChartType), item.<Type>.Value)
            myChart.Series(SeriesName).ChartType = [Enum].Parse(GetType(DataVisualization.Charting.SeriesChartType), item.<ChartType>.Value)
            If item.<ChartArea>.Value <> Nothing Then myChart.Series(SeriesName).ChartArea = item.<ChartArea>.Value
            myChart.Series(SeriesName).Legend = item.<Legend>.Value
            myChart.Series(SeriesName).SetCustomProperty("EmptyPointValue", item.<EmptyPointValue>.Value)
            myChart.Series(SeriesName).SetCustomProperty("LabelStyle", item.<LabelStyle>.Value)
            myChart.Series(SeriesName).SetCustomProperty("PixelPointDepth", item.<PixelPointDepth>.Value)
            myChart.Series(SeriesName).SetCustomProperty("PixelPointGapDepth", item.<PixelPointGapDepth>.Value)
            myChart.Series(SeriesName).SetCustomProperty("ShowMarkerLines", item.<ShowMarkerLines>.Value)
            myChart.Series(SeriesName).AxisLabel = item.<AxisLabel>.Value
            myChart.Series(SeriesName).XAxisType = [Enum].Parse(GetType(DataVisualization.Charting.AxisType), item.<XAxisType>.Value)
            myChart.Series(SeriesName).YAxisType = [Enum].Parse(GetType(DataVisualization.Charting.AxisType), item.<YAxisType>.Value)
            If item.<XValueType>.Value <> Nothing Then myChart.Series(SeriesName).XValueType = [Enum].Parse(GetType(DataVisualization.Charting.ChartValueType), item.<XValueType>.Value)
            If item.<YValueType>.Value <> Nothing Then myChart.Series(SeriesName).YValueType = [Enum].Parse(GetType(DataVisualization.Charting.ChartValueType), item.<YValueType>.Value)
            If item.<Marker>.<BorderColor>.Value <> Nothing Then myChart.Series(SeriesName).MarkerBorderColor = Color.FromArgb(item.<Marker>.<BorderColor>.Value)
            If item.<Marker>.<BorderWidth>.Value <> Nothing Then myChart.Series(SeriesName).MarkerBorderWidth = item.<Marker>.<BorderWidth>.Value
            If item.<Marker>.<Color>.Value <> Nothing Then myChart.Series(SeriesName).MarkerColor = Color.FromArgb(item.<Marker>.<Color>.Value)
            If item.<Marker>.<Size>.Value <> Nothing Then myChart.Series(SeriesName).MarkerSize = item.<Marker>.<Size>.Value
            If item.<Marker>.<Step>.Value <> Nothing Then myChart.Series(SeriesName).MarkerStep = item.<Marker>.<Step>.Value
            If item.<Marker>.<Style>.Value <> Nothing Then myChart.Series(SeriesName).MarkerStyle = [Enum].Parse(GetType(DataVisualization.Charting.MarkerStyle), item.<Marker>.<Style>.Value)
            If item.<Color>.Value <> Nothing Then myChart.Series(SeriesName).Color = Color.FromArgb(item.<Color>.Value)
        Next

        'Restore Chart Areas:
        Dim Areas = From item In XDoc.<ChartSettings>.<ChartAreasCollection>.<ChartArea>
        Dim AreaName As String
        myChart.ChartAreas.Clear()
        For Each item In Areas
            AreaName = item.<Name>.Value
            myChart.ChartAreas.Add(AreaName)
            'AxisX Properties:
            myChart.ChartAreas(AreaName).AxisX.Title = item.<AxisX>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisX.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisX>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisX.TitleForeColor = Color.FromArgb(item.<AxisX>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisX>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisX>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisX>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisX>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisX>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisX.TitleFont = New Font(item.<AxisX>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisX>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisX.LabelStyle.Format = item.<AxisX>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisX.Minimum = item.<AxisX>.<Minimum>.Value

            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMinimum Then myChart.ChartAreas(AreaName).AxisX.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisX.Maximum = item.<AxisX>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMaximum Then myChart.ChartAreas(AreaName).AxisX.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisX.LineWidth = item.<AxisX>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisX.Interval = item.<AxisX>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisX.IntervalOffset = item.<AxisX>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisX.Crossing = item.<AxisX>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisX.MajorGrid.Interval = item.<AxisX>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoXAxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisX.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisX.MajorGrid.IntervalOffset = item.<AxisX>.<MajorGrid>.<IntervalOffset>.Value

            'AxisX2 Properties:
            myChart.ChartAreas(AreaName).AxisX2.Title = item.<AxisX2>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisX2.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisX2>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisX2.TitleForeColor = Color.FromArgb(item.<AxisX2>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisX2>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisX2>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisX2>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisX2>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisX2>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisX2.TitleFont = New Font(item.<AxisX2>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisX2>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisX2.LabelStyle.Format = item.<AxisX2>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisX2.Minimum = item.<AxisX2>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMinimum Then myChart.ChartAreas(AreaName).AxisX2.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisX2.Maximum = item.<AxisX2>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMaximum Then myChart.ChartAreas(AreaName).AxisX2.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisX2.LineWidth = item.<AxisX2>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisX2.Interval = item.<AxisX2>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisX2.IntervalOffset = item.<AxisX2>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisX2.Crossing = item.<AxisX2>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisX2.MajorGrid.Interval = item.<AxisX2>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoX2AxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisX2.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisX2.MajorGrid.IntervalOffset = item.<AxisX2>.<MajorGrid>.<IntervalOffset>.Value


            'AxisY Properties:
            myChart.ChartAreas(AreaName).AxisY.Title = item.<AxisY>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisY.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisY>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisY.TitleForeColor = Color.FromArgb(item.<AxisY>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisY>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisY>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisY>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisY>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisY>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisY.TitleFont = New Font(item.<AxisY>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisY>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisY.LabelStyle.Format = item.<AxisY>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisY.Minimum = item.<AxisY>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMinimum Then myChart.ChartAreas(AreaName).AxisY.Minimum = Double.NaN 'Set to Auto Minimum
            End If


            myChart.ChartAreas(AreaName).AxisY.Maximum = item.<AxisY>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMaximum Then myChart.ChartAreas(AreaName).AxisY.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisY.LineWidth = item.<AxisY>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisY.Interval = item.<AxisY>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisY.IntervalOffset = item.<AxisY>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisY.Crossing = item.<AxisY>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisY.MajorGrid.Interval = item.<AxisY>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoYAxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisY.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisY.MajorGrid.IntervalOffset = item.<AxisY>.<MajorGrid>.<IntervalOffset>.Value

            'AxisY2 Properties:
            myChart.ChartAreas(AreaName).AxisY2.Title = item.<AxisY2>.<Title>.<Text>.Value
            myChart.ChartAreas(AreaName).AxisY2.TitleAlignment = [Enum].Parse(GetType(StringAlignment), item.<AxisY2>.<Title>.<Alignment>.Value)
            myChart.ChartAreas(AreaName).AxisY2.TitleForeColor = Color.FromArgb(item.<AxisY2>.<Title>.<ForeColor>.Value)
            myFontStyle = FontStyle.Regular
            If item.<AxisY2>.<Title>.<Font>.<Bold>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Bold
            If item.<AxisY2>.<Title>.<Font>.<Italic>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Italic
            If item.<AxisY2>.<Title>.<Font>.<Strikeout>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Strikeout
            If item.<AxisY2>.<Title>.<Font>.<Underline>.Value = True Then myFontStyle = myFontStyle Or FontStyle.Underline
            myFontSize = item.<AxisY2>.<Title>.<Font>.<Size>.Value
            myChart.ChartAreas(AreaName).AxisY2.TitleFont = New Font(item.<AxisY2>.<Title>.<Font>.<Name>.Value, myFontSize, myFontStyle)
            If item.<AxisY2>.<LabelStyleFormat>.Value <> Nothing Then myChart.ChartAreas(AreaName).AxisY2.LabelStyle.Format = item.<AxisY2>.<LabelStyleFormat>.Value
            myChart.ChartAreas(AreaName).AxisY2.Minimum = item.<AxisY2>.<Minimum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMinimum Then myChart.ChartAreas(AreaName).AxisY2.Minimum = Double.NaN 'Set to Auto Minimum
            End If

            myChart.ChartAreas(AreaName).AxisY2.Maximum = item.<AxisY2>.<Maximum>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMaximum Then myChart.ChartAreas(AreaName).AxisY2.Maximum = Double.NaN 'Set to Auto Maximum
            End If

            myChart.ChartAreas(AreaName).AxisY2.LineWidth = item.<AxisY2>.<LineWidth>.Value
            myChart.ChartAreas(AreaName).AxisY2.Interval = item.<AxisY2>.<Interval>.Value
            myChart.ChartAreas(AreaName).AxisY2.IntervalOffset = item.<AxisY2>.<IntervalOffset>.Value
            myChart.ChartAreas(AreaName).AxisY2.Crossing = item.<AxisY2>.<Crossing>.Value
            myChart.ChartAreas(AreaName).AxisY2.MajorGrid.Interval = item.<AxisY2>.<MajorGrid>.<Interval>.Value
            If dictAreaInfo.ContainsKey(AreaName) Then
                If dictAreaInfo(AreaName).AutoY2AxisMajorGridInterval Then myChart.ChartAreas(AreaName).AxisY2.MajorGrid.Interval = 0 'Set to Auto Interval
            End If

            myChart.ChartAreas(AreaName).AxisY2.MajorGrid.IntervalOffset = item.<AxisY2>.<MajorGrid>.<IntervalOffset>.Value
        Next

    End Sub

    Public Function ToXDoc(ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart) As System.Xml.Linq.XDocument
        'Function to return the Line Chart settings in an XDocument.

        Dim XDoc = <?xml version="1.0" encoding="utf-8"?>
                   <!---->
                   <!--Line Chart Settings File-->
                   <ChartSettings>
                       <!--Input Data:-->
                       <InputDataType><%= InputDataType %></InputDataType>
                       <InputDatabasePath><%= InputDatabasePath %></InputDatabasePath>
                       <InputQuery><%= InputQuery %></InputQuery>
                       <InputDataDescr><%= InputDataDescr %></InputDataDescr>
                       <SeriesInfoList>
                           <%= From item In dictSeriesInfo
                               Select
                                   <SeriesInfo>
                                       <Name><%= item.Key %></Name>
                                       <XValuesFieldName><%= item.Value.XValuesFieldName %></XValuesFieldName>
                                       <YValuesFieldName><%= item.Value.YValuesFieldName %></YValuesFieldName>
                                       <ChartArea><%= item.Value.ChartArea %></ChartArea>
                                   </SeriesInfo> %>
                       </SeriesInfoList>
                       <AreaInfoList>
                           <%= From item In dictAreaInfo
                               Select
                                    <AreaInfo>
                                        <Name><%= item.Key %></Name>
                                        <AutoXAxisMinimum><%= item.Value.AutoXAxisMinimum %></AutoXAxisMinimum>
                                        <AutoXAxisMaximum><%= item.Value.AutoXAxisMaximum %></AutoXAxisMaximum>
                                        <AutoXAxisMajorGridInterval><%= item.Value.AutoXAxisMajorGridInterval %></AutoXAxisMajorGridInterval>
                                        <AutoX2AxisMinimum><%= item.Value.AutoX2AxisMinimum %></AutoX2AxisMinimum>
                                        <AutoX2AxisMaximum><%= item.Value.AutoX2AxisMaximum %></AutoX2AxisMaximum>
                                        <AutoX2AxisMajorGridInterval><%= item.Value.AutoX2AxisMajorGridInterval %></AutoX2AxisMajorGridInterval>
                                        <AutoYAxisMinimum><%= item.Value.AutoYAxisMinimum %></AutoYAxisMinimum>
                                        <AutoYAxisMaximum><%= item.Value.AutoYAxisMaximum %></AutoYAxisMaximum>
                                        <AutoYAxisMajorGridInterval><%= item.Value.AutoYAxisMajorGridInterval %></AutoYAxisMajorGridInterval>
                                        <AutoY2AxisMinimum><%= item.Value.AutoY2AxisMinimum %></AutoY2AxisMinimum>
                                        <AutoY2AxisMaximum><%= item.Value.AutoY2AxisMaximum %></AutoY2AxisMaximum>
                                        <AutoY2AxisMajorGridInterval><%= item.Value.AutoY2AxisMajorGridInterval %></AutoY2AxisMajorGridInterval>
                                    </AreaInfo> %>
                       </AreaInfoList>
                       <!--Chart Properties:-->
                       <TitlesCollection>
                           <%= From item In myChart.Titles
                               Select
                               <Title>
                                   <Name><%= item.Name %></Name>
                                   <Text><%= item.Text %></Text>
                                   <TextOrientation><%= item.TextOrientation %></TextOrientation>
                                   <Alignment><%= item.Alignment %></Alignment>
                                   <ForeColor><%= item.ForeColor.ToArgb.ToString %></ForeColor>
                                   <Font>
                                       <Name><%= item.Font.Name %></Name>
                                       <Size><%= item.Font.Size %></Size>
                                       <Bold><%= item.Font.Bold %></Bold>
                                       <Italic><%= item.Font.Italic %></Italic>
                                       <Strikeout><%= item.Font.Strikeout %></Strikeout>
                                       <Underline><%= item.Font.Underline %></Underline>
                                   </Font>
                               </Title> %>
                       </TitlesCollection>
                       <SeriesCollection>
                           <%= From item In myChart.Series
                               Select
                                   <Series>
                                       <Name><%= item.Name %></Name>
                                       <ChartType><%= item.ChartType %></ChartType>
                                       <ChartArea><%= item.ChartArea %></ChartArea>
                                       <Legend><%= item.Legend %></Legend>
                                       <EmptyPointValue><%= item.GetCustomProperty("EmptyPointValue") %></EmptyPointValue>
                                       <LabelStyle><%= item.GetCustomProperty("LabelStyle") %></LabelStyle>
                                       <PixelPointDepth><%= item.GetCustomProperty("PixelPointDepth") %></PixelPointDepth>
                                       <PixelPointGapDepth><%= item.GetCustomProperty("PixelPointGapDepth") %></PixelPointGapDepth>
                                       <ShowMarkerLines><%= item.GetCustomProperty("ShowMarkerLines") %></ShowMarkerLines>
                                       <AxisLabel><%= item.AxisLabel %></AxisLabel>
                                       <XAxisType><%= item.XAxisType %></XAxisType>
                                       <XValueType><%= item.XValueType %></XValueType>
                                       <YAxisType><%= item.YAxisType %></YAxisType>
                                       <YValueType><%= item.YValueType %></YValueType>
                                       <Marker>
                                           <BorderColor><%= item.MarkerBorderColor.ToArgb.ToString %></BorderColor>
                                           <BorderWidth><%= item.MarkerBorderWidth %></BorderWidth>
                                           <Color><%= item.MarkerColor.ToArgb.ToString %></Color>
                                           <Size><%= item.MarkerSize %></Size>
                                           <Step><%= item.MarkerStep %></Step>
                                           <Style><%= item.MarkerStyle %></Style>
                                       </Marker>
                                       <Color><%= item.Color.ToArgb.ToString %></Color>
                                   </Series> %>
                       </SeriesCollection>
                       <ChartAreasCollection>
                           <%= From item In myChart.ChartAreas
                               Select
                               <ChartArea>
                                   <Name><%= item.Name %></Name>
                                   <AxisX>
                                       <Title>
                                           <Text><%= item.AxisX.Title %></Text>
                                           <Alignment><%= item.AxisX.TitleAlignment %></Alignment>
                                           <ForeColor><%= item.AxisX.TitleForeColor.ToArgb.ToString %></ForeColor>
                                           <Font>
                                               <Name><%= item.AxisX.TitleFont.Name %></Name>
                                               <Size><%= item.AxisX.TitleFont.Size %></Size>
                                               <Bold><%= item.AxisX.TitleFont.Bold %></Bold>
                                               <Italic><%= item.AxisX.TitleFont.Italic %></Italic>
                                               <Strikeout><%= item.AxisX.TitleFont.Strikeout %></Strikeout>
                                               <Underline><%= item.AxisX.TitleFont.Underline %></Underline>
                                           </Font>
                                       </Title>
                                       <LabelStyleFormat><%= item.AxisX.LabelStyle.Format %></LabelStyleFormat>
                                       <Minimum><%= item.AxisX.Minimum %></Minimum>
                                       <Maximum><%= item.AxisX.Maximum %></Maximum>
                                       <LineWidth><%= item.AxisX.LineWidth %></LineWidth>
                                       <Interval><%= item.AxisX.Interval %></Interval>
                                       <IntervalOffset><%= item.AxisX.IntervalOffset %></IntervalOffset>
                                       <Crossing><%= item.AxisX.Crossing %></Crossing>
                                       <MajorGrid>
                                           <Interval><%= item.AxisX.MajorGrid.Interval %></Interval>
                                           <IntervalOffset><%= item.AxisX.MajorGrid.IntervalOffset %></IntervalOffset>
                                       </MajorGrid>
                                   </AxisX>
                                   <AxisX2>
                                       <Title>
                                           <Text><%= item.AxisX2.Title %></Text>
                                           <Alignment><%= item.AxisX2.TitleAlignment %></Alignment>
                                           <ForeColor><%= item.AxisX2.TitleForeColor.ToArgb.ToString %></ForeColor>
                                           <Font>
                                               <Name><%= item.AxisX2.TitleFont.Name %></Name>
                                               <Size><%= item.AxisX2.TitleFont.Size %></Size>
                                               <Bold><%= item.AxisX2.TitleFont.Bold %></Bold>
                                               <Italic><%= item.AxisX2.TitleFont.Italic %></Italic>
                                               <Strikeout><%= item.AxisX2.TitleFont.Strikeout %></Strikeout>
                                               <Underline><%= item.AxisX2.TitleFont.Underline %></Underline>
                                           </Font>
                                       </Title>
                                       <LabelStyleFormat><%= item.AxisX2.LabelStyle.Format %></LabelStyleFormat>
                                       <Minimum><%= item.AxisX2.Minimum %></Minimum>
                                       <Maximum><%= item.AxisX2.Maximum %></Maximum>
                                       <LineWidth><%= item.AxisX2.LineWidth %></LineWidth>
                                       <Interval><%= item.AxisX2.Interval %></Interval>
                                       <IntervalOffset><%= item.AxisX2.IntervalOffset %></IntervalOffset>
                                       <Crossing><%= item.AxisX2.Crossing %></Crossing>
                                       <MajorGrid>
                                           <Interval><%= item.AxisX2.MajorGrid.Interval %></Interval>
                                           <IntervalOffset><%= item.AxisX2.MajorGrid.IntervalOffset %></IntervalOffset>
                                       </MajorGrid>
                                   </AxisX2>
                                   <AxisY>
                                       <Title>
                                           <Text><%= item.AxisY.Title %></Text>
                                           <Alignment><%= item.AxisY.TitleAlignment %></Alignment>
                                           <ForeColor><%= item.AxisY.TitleForeColor.ToArgb.ToString %></ForeColor>
                                           <Font>
                                               <Name><%= item.AxisY.TitleFont.Name %></Name>
                                               <Size><%= item.AxisY.TitleFont.Size %></Size>
                                               <Bold><%= item.AxisY.TitleFont.Bold %></Bold>
                                               <Italic><%= item.AxisY.TitleFont.Italic %></Italic>
                                               <Strikeout><%= item.AxisY.TitleFont.Strikeout %></Strikeout>
                                               <Underline><%= item.AxisY.TitleFont.Underline %></Underline>
                                           </Font>
                                       </Title>
                                       <LabelStyleFormat><%= item.AxisY.LabelStyle.Format %></LabelStyleFormat>
                                       <Minimum><%= item.AxisY.Minimum %></Minimum>
                                       <Maximum><%= item.AxisY.Maximum %></Maximum>
                                       <LineWidth><%= item.AxisY.LineWidth %></LineWidth>
                                       <Interval><%= item.AxisY.Interval %></Interval>
                                       <IntervalOffset><%= item.AxisY.IntervalOffset %></IntervalOffset>
                                       <Crossing><%= item.AxisY.Crossing %></Crossing>
                                       <MajorGrid>
                                           <Interval><%= item.AxisY.MajorGrid.Interval %></Interval>
                                           <IntervalOffset><%= item.AxisY.MajorGrid.IntervalOffset %></IntervalOffset>
                                       </MajorGrid>
                                   </AxisY>
                                   <AxisY2>
                                       <Title>
                                           <Text><%= item.AxisY2.Title %></Text>
                                           <Alignment><%= item.AxisY2.TitleAlignment %></Alignment>
                                           <ForeColor><%= item.AxisY2.TitleForeColor.ToArgb.ToString %></ForeColor>
                                           <Font>
                                               <Name><%= item.AxisY2.TitleFont.Name %></Name>
                                               <Size><%= item.AxisY2.TitleFont.Size %></Size>
                                               <Bold><%= item.AxisY2.TitleFont.Bold %></Bold>
                                               <Italic><%= item.AxisY2.TitleFont.Italic %></Italic>
                                               <Strikeout><%= item.AxisY2.TitleFont.Strikeout %></Strikeout>
                                               <Underline><%= item.AxisY2.TitleFont.Underline %></Underline>
                                           </Font>
                                       </Title>
                                       <LabelStyleFormat><%= item.AxisY2.LabelStyle.Format %></LabelStyleFormat>
                                       <Minimum><%= item.AxisY2.Minimum %></Minimum>
                                       <Maximum><%= item.AxisY2.Maximum %></Maximum>
                                       <LineWidth><%= item.AxisY2.LineWidth %></LineWidth>
                                       <Interval><%= item.AxisY2.Interval %></Interval>
                                       <IntervalOffset><%= item.AxisY2.IntervalOffset %></IntervalOffset>
                                       <Crossing><%= item.AxisY2.Crossing %></Crossing>
                                       <MajorGrid>
                                           <Interval><%= item.AxisY2.MajorGrid.Interval %></Interval>
                                           <IntervalOffset><%= item.AxisY2.MajorGrid.IntervalOffset %></IntervalOffset>
                                       </MajorGrid>
                                   </AxisY2>
                               </ChartArea> %>
                       </ChartAreasCollection>
                   </ChartSettings>

        Return XDoc

        '<Type><%= item.ChartType %></Type> 

        '<ForeColor><%= SimplifyColorString(item.AxisX.TitleForeColor.ToString) %></ForeColor>
        '  <ForeColor><%= item.ForeColor.Name %></ForeColor>
        '<ForeColor><%= item.ForeColor.ToKnownColor %></ForeColor>
        '  <ForeColor><%= item.ForeColor.Name %></ForeColor>
        '  <ForeColor><%= SimplifyColorString(item.ForeColor.ToString) %></ForeColor>
        '                           <%= From item In dictFields
        '                           <%= From item In dictAreas
        '     <ChartArea><%= item.ChartArea %></ChartArea>

        '<ForeColor><%= item.ForeColor.ToString %></ForeColor>
        'Special values:
        'Chart1.ChartAreas(0).AxisX.MajorGrid.Interval = 0  'The major grid interval is automatic.
        'Chart1.ChartAreas(0).AxisX.Minimum = [Double].NaN  'The Axis minimum is automatic.
        'Chart1.ChartAreas(0).AxisX.Interval = 0            'The Axis interval is automatic.


        'https://msdn.microsoft.com/en-us/data/dd489252(v=vs.95)
        'Custom attributes:
        'EmptyPointValue    (Average, Zero)
        'LabelStyle         (Auto, Top, Bottom, Right, Left, TopLeft, TopRight, BottomLeft, BottomRight, Center)
        'PixelPointDepth    (Any integer > 0)
        'PixelPointGapDepth (Any integer > 0)
        'ShowMarkerLines    (True, False)



        'myChart.Series(0).

        'Assume:
        'myChart.Series(0).YValuesPerPoint = 1
        'myChart.Series(0).ChartType = DataVisualization.Charting.SeriesChartType.Line

        '   <Name><%= item.Name %></Name> 'The axis name can only be X, Y, X2 or Y2.

        'Axis Types:
        'myChart.Series(0).YAxisType = DataVisualization.Charting.AxisType.Primary
        'myChart.Series(0).YAxisType = DataVisualization.Charting.AxisType.Secondary
        'myChart.Series(0).XAxisType = DataVisualization.Charting.AxisType.Primary
        'myChart.Series(0).XAxisType = DataVisualization.Charting.AxisType.Secondary

        'Chart Area Settings:
        'myChart.ChartAreas(0).AxisX.Enabled = DataVisualization.Charting.AxisEnabled.Auto
        'myChart.ChartAreas(0).AxisX.IsLogarithmic = False
        'myChart.ChartAreas(0).AxisX.IsReversed = False
        'myChart.ChartAreas(0).AxisX.IsStartedFromZero = True

        'NOTE use separate style elements instead of:
        '<Style><%= myChart.Titles("Title1").Font.Style %></Style>
        '<Style><%= item.Font.Style %></Style>
        '<Style><%= item.AxisX.TitleFont.Style %></Style>
        '<Style><%= item.AxisX2.TitleFont.Style %></Style>
        '<Style><%= item.AxisY.TitleFont.Style %></Style>
        '<Style><%= item.AxisY2.TitleFont.Style %></Style>

        '<Title1>
        '    <Text><%= myChart.Titles("Title1").Text %></Text>
        '    <Alignment><%= myChart.Titles("Title1").Alignment %></Alignment>
        '    <Font>
        '        <Name><%= myChart.Titles("Title1").Font.Name %></Name>
        '        <Size><%= myChart.Titles("Title1").Font.Size %></Size>
        '        <Bold><%= myChart.Titles("Title1").Font.Bold %></Bold>
        '        <Italic><%= myChart.Titles("Title1").Font.Italic %></Italic>
        '        <Strikeout><%= myChart.Titles("Title1").Font.Strikeout %></Strikeout>
        '        <Underline><%= myChart.Titles("Title1").Font.Underline %></Underline>
        '    </Font>
        '</Title1>

    End Function

    Private Function SimplifyColorString(ByVal ColorString As String) As String
        'Simplify a Color string.
        '  eg: convert Color [Color [Color [Color [Black]]]] to Color [Black]
        'NOTE: This method is no longer used:
        '          Colors are now saved using Color.ToArgb.ToString and restored using Color.FromArgb()
        '          The save method produces a simple string representation of an integer, such as -16777216 that does not need to be simplified.

        Dim InputString As String = ColorString
        Dim SimplifiedString = InputString.Trim

        While SimplifiedString.StartsWith("Color [")
            If SimplifiedString.EndsWith("]") Then
                InputString = SimplifiedString
                SimplifiedString = InputString.Substring(7, InputString.Length - 8)
            Else
                'Missing closing square bracket!
                Exit While
            End If
        End While

        Return InputString

    End Function

    Public Sub SaveFile(ByVal myFileName As String, ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Save the Point Chart settings in a file named FileName.
        If myFileName = "" Then 'No stock chart settings file has been selected.
            Exit Sub
        End If

        'Clean the AreaInfo and SeriesInfo dictionaries before saving:
        CleanAreaInfo(myChart)
        CleanSeriesInfo(myChart)

        DataLocation.SaveXmlData(myFileName, ToXDoc(myChart))
    End Sub

    Public Sub Clear(ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Clear the Line Chart settings and apply defaults.

        'Clear the myChart properties:
        myChart.ChartAreas.Clear()
        myChart.Series.Clear()

        'Clear the ChartInfo properties:
        FileName = ""
        InputDataType = "Database"
        InputDatabasePath = ""
        InputQuery = ""
        InputDataDescr = ""

        ds.Clear() 'Clear the dataset containin the points to be plotted in the line chart.
        dictSeriesInfo.Clear() 'Clear the dictionary of Series Information.
        dictAreaInfo.Clear()   'Clear the dictionary of Area Information
    End Sub

    Public Sub CleanSeriesInfo(ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Clean the SeriesInfo dictionary of Series that are no longer in the Chart:

        Dim list As New List(Of String)(dictSeriesInfo.Keys) 'Get the list of keys in the SeriesInfo dictionary.

        Dim KeyFound As Boolean = False 'If the SeriesInfo dictionary key is found in the Chart control, KeyFound is True.
        For Each KeyStr In list
            'Check if the dictionary key (Series name) is found in myChart:
            For Each item In myChart.Series
                If item.Name = KeyStr Then
                    KeyFound = True
                    Exit For 'Key found, stop looking.
                End If
            Next
            If KeyFound = False Then
                'Remove the entry from the dictionary:
                dictSeriesInfo.Remove(KeyStr)
            Else
                'The key was found - do not remove the dictionary entry.
                KeyFound = False 'Reset the flas to False before searching for the next key.
            End If
        Next
    End Sub

    Public Sub CleanAreaInfo(ByRef myChart As System.Windows.Forms.DataVisualization.Charting.Chart)
        'Clean the AreaInfo dictionary of Chart Areas that are no longer in the Chart:

        Dim list As New List(Of String)(dictAreaInfo.Keys) 'Get the list of keys in the AreaInfo dictionary.

        Dim KeyFound As Boolean = False 'If the AreaInfo dictionary key is found in the Chart control, KeyFound is True.
        For Each KeyStr In list
            'Check if the dictionary key (ChartArea name) is found in myChart:
            For Each item In myChart.ChartAreas
                If item.Name = KeyStr Then
                    KeyFound = True
                    Exit For 'Key found, stop looking.
                End If
            Next
            If KeyFound = False Then
                'Remove the entry from the dictionary:
                dictAreaInfo.Remove(KeyStr)
            Else
                'The key was found - do not remove the dictionary entry.
                KeyFound = False 'Reset the flas to False before searching for the next key.
            End If
        Next
    End Sub

    Public Sub ApplyQuery()
        'Use the Query to fill the ds dataset

        If InputDatabasePath = "" Then
            RaiseEvent ErrorMessage("InputDatabasePath is not defined!" & vbCrLf)
            Exit Sub
        End If

        'Database access for MS Access:
        Dim connectionString As String 'Declare a connection string for MS Access - defines the database or server to be used.
        Dim conn As System.Data.OleDb.OleDbConnection 'Declare a connection for MS Access - used by the Data Adapter to connect to and disconnect from the database.
        Dim commandString As String 'Declare a command string - contains the query to be passed to the database.

        'Specify the connection string (Access 2007):
        connectionString = "provider=Microsoft.ACE.OLEDB.12.0;" +
        "data source = " + InputDatabasePath

        'Connect to the Access database:
        conn = New System.Data.OleDb.OleDbConnection(connectionString)
        conn.Open()

        'Specify the commandString to query the database:
        commandString = InputQuery
        Dim dataAdapter As New System.Data.OleDb.OleDbDataAdapter(commandString, conn)

        ds.Clear()
        ds.Reset()

        dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey

        Try
            dataAdapter.Fill(ds, "SelTable")
            'UpdateChartQuery() 'NOT NEEDED??? 'This was originally used to set PointChart or StockChart .Input Query to the property InputQuery. (See the Chart app code.)
        Catch ex As Exception
            RaiseEvent ErrorMessage("Error applying query." & vbCrLf)
            RaiseEvent ErrorMessage(ex.Message & vbCrLf)
        End Try

        conn.Close()

    End Sub 'ApplyQuery

#End Region 'Methods -----------------------------------------------------------------------------------------------------


#Region "Events" '--------------------------------------------------------------------------------------------------------

    Event ErrorMessage(ByVal Message As String) 'Send an error message.
    Event Message(ByVal Message As String) 'Send a normal message.

#End Region 'Events ------------------------------------------------------------------------------------------------------

End Class 'ChartInfo

'Public Class TableFields
Public Class SeriesInfo
    'Used to store the X Values Field Name and Y Values Field Name.
    'These are the names of the fields in a database table used for the X and Y values in a chart.

    Private _xValuesFieldName As String = "" 'The name of the table field containing the X Values
    Property XValuesFieldName As String
        Get
            Return _xValuesFieldName
        End Get
        Set(value As String)
            _xValuesFieldName = value
        End Set
    End Property

    Private _yValuesFieldName As String = "" 'The name of the table field containing the Y Values
    Property YValuesFieldName As String
        Get
            Return _yValuesFieldName
        End Get
        Set(value As String)
            _yValuesFieldName = value
        End Set
    End Property

    Private _chartArea As String = "" 'The name of the Chart Area used to display the series.
    Property ChartArea As String
        Get
            Return _chartArea
        End Get
        Set(value As String)
            _chartArea = value
        End Set
    End Property

End Class

'Public Class AreaFields
Public Class AreaInfo
    'Used to indicate if chart area parameters are determined automatically or not.
    'These parameters cannot be stored in the Chart.

    Private _autoXAxisMinimum As Boolean = False 'If True, the X Axis minimum value is determined automatically.
    Property AutoXAxisMinimum As Boolean
        Get
            Return _autoXAxisMinimum
        End Get
        Set(value As Boolean)
            _autoXAxisMinimum = value
        End Set
    End Property

    Private _autoXAxisMaximum As Boolean = False 'If True, the X Axis maximum value is determined automatically.
    Property AutoXAxisMaximum As Boolean
        Get
            Return _autoXAxisMaximum
        End Get
        Set(value As Boolean)
            _autoXAxisMaximum = value
        End Set
    End Property

    'Private _autoXAxisInterval As Boolean 'If True, the X Axis maximum value is determined automatically.
    'Property AutoXAxisInterval As Boolean
    '    Get
    '        Return _autoXAxisInterval
    '    End Get
    '    Set(value As Boolean)
    '        _autoXAxisInterval = value
    '    End Set
    'End Property

    Private _autoXAxisMajorGridInterval As Boolean = False 'If True, the X Axis Major Grid Interval value is determined automatically.
    Property AutoXAxisMajorGridInterval As Boolean
        Get
            Return _autoXAxisMajorGridInterval
        End Get
        Set(value As Boolean)
            _autoXAxisMajorGridInterval = value
        End Set
    End Property

    Private _autoX2AxisMinimum As Boolean = False 'If True, the X2 Axis minimum value is determined automatically.
    Property AutoX2AxisMinimum As Boolean
        Get
            Return _autoX2AxisMinimum
        End Get
        Set(value As Boolean)
            _autoX2AxisMinimum = value
        End Set
    End Property

    Private _autoX2AxisMaximum As Boolean = False 'If True, the X2 Axis maximum value is determined automatically.
    Property AutoX2AxisMaximum As Boolean
        Get
            Return _autoX2AxisMaximum
        End Get
        Set(value As Boolean)
            _autoX2AxisMaximum = value
        End Set
    End Property

    Private _autoX2AxisMajorGridInterval As Boolean = False 'If True, the X2 Axis Major Grid Interval value is determined automatically.
    Property AutoX2AxisMajorGridInterval As Boolean
        Get
            Return _autoX2AxisMajorGridInterval
        End Get
        Set(value As Boolean)
            _autoX2AxisMajorGridInterval = value
        End Set
    End Property

    Private _autoYAxisMinimum As Boolean = False 'If True, the Y Axis minimum value is determined automatically.
    Property AutoYAxisMinimum As Boolean
        Get
            Return _autoYAxisMinimum
        End Get
        Set(value As Boolean)
            _autoYAxisMinimum = value
        End Set
    End Property

    Private _autoYAxisMaximum As Boolean = False 'If True, the Y Axis maximum value is determined automatically.
    Property AutoYAxisMaximum As Boolean
        Get
            Return _autoYAxisMaximum
        End Get
        Set(value As Boolean)
            _autoYAxisMaximum = value
        End Set
    End Property

    Private _autoYAxisMajorGridInterval As Boolean = False 'If True, the Y Axis Major Grid Interval value is determined automatically.
    Property AutoYAxisMajorGridInterval As Boolean
        Get
            Return _autoYAxisMajorGridInterval
        End Get
        Set(value As Boolean)
            _autoYAxisMajorGridInterval = value
        End Set
    End Property

    Private _autoY2AxisMinimum As Boolean = False 'If True, the Y2 Axis minimum value is determined automatically.
    Property AutoY2AxisMinimum As Boolean
        Get
            Return _autoY2AxisMinimum
        End Get
        Set(value As Boolean)
            _autoY2AxisMinimum = value
        End Set
    End Property

    Private _autoY2AxisMaximum As Boolean = False 'If True, the Y2 Axis maximum value is determined automatically.
    Property AutoY2AxisMaximum As Boolean
        Get
            Return _autoY2AxisMaximum
        End Get
        Set(value As Boolean)
            _autoY2AxisMaximum = value
        End Set
    End Property

    Private _autoY2AxisMajorGridInterval As Boolean = False 'If True, the Y2 Axis Major Grid Interval value is determined automatically.
    Property AutoY2AxisMajorGridInterval As Boolean
        Get
            Return _autoY2AxisMajorGridInterval
        End Get
        Set(value As Boolean)
            _autoY2AxisMajorGridInterval = value
        End Set
    End Property

End Class

'NOTE: MOST OF THIS CLASS JUST DUPLICATES PROPERTIES IN THE CHART() CLASS.
'      THE CHART CLASS WILL BE USED TO STORE CHART PROPERTIES INSTEAD OF THIS CLASS.
'      THIS CLASS WILL BE USED ONLY TO STORE THE PROPERTIES NON INCLUDED IN THE CHART CLASS.
'Public Class AxisProperties
'    'Axis Properties

'    Public Title As LabelProperties = New LabelProperties 'Title contains Text, FontName, Color, Size, Bold, Italic, Underline and Strikeout properties


'    Private _titleAlignment As System.Drawing.StringAlignment = StringAlignment.Center 'Near (0) Center (1) Far (2)
'    Property TitleAlignment As System.Drawing.StringAlignment
'        Get
'            Return _titleAlignment
'        End Get
'        Set(value As System.Drawing.StringAlignment)
'            _titleAlignment = value
'        End Set
'    End Property

'    'If True, the Axis minimum value is determined automatically.
'    'If False, the Minimum property is used.
'    Private _autoMinimum As Boolean = True
'    Property AutoMinimum As Boolean
'        Get
'            Return _autoMinimum
'        End Get
'        Set(value As Boolean)
'            _autoMinimum = value
'        End Set
'    End Property

'    'The minimum value displayed along the axis.
'    Private _minimum As Single = 0
'    Property Minimum As Single
'        Get
'            Return _minimum
'        End Get
'        Set(value As Single)
'            _minimum = value
'        End Set
'    End Property

'    'If True, the Axis maximum value is determined automatically.
'    'If False, the Maximum property is used.
'    Private _autoMaximum As Boolean = True
'    Property AutoMaximum As Boolean
'        Get
'            Return _autoMaximum
'        End Get
'        Set(value As Boolean)
'            _autoMaximum = value
'        End Set
'    End Property

'    'The maximum value displayed along the axis.
'    Private _maximum As Single = 1
'    Property Maximum As Single
'        Get
'            Return _maximum
'        End Get
'        Set(value As Single)
'            _maximum = value
'        End Set
'    End Property

'    Private _autoInterval As Boolean = True 'If True, the axis annotation interval is determined automatically.
'    Property AutoInterval As Boolean
'        Get
'            Return _autoInterval
'        End Get
'        Set(value As Boolean)
'            _autoInterval = value
'        End Set
'    End Property


'    Private _interval As Double = 0 'The Axis annotation interval. 0 = Auto.
'    Property Interval As Double
'        Get
'            Return _interval
'        End Get
'        Set(value As Double)
'            _interval = value
'        End Set
'    End Property

'    Private _autoMajorGridInterval As Boolean = True 'If True, the axis major grid interval is determined automatically.
'    Property AutoMajorGridInterval As Boolean
'        Get
'            Return _autoMajorGridInterval
'        End Get
'        Set(value As Boolean)
'            _autoMajorGridInterval = value
'        End Set
'    End Property

'    Private _majorGridInterval As Double = 0 'The major grid interval. 0 = Auto.
'    Property MajorGridInterval As Double
'        Get
'            Return _majorGridInterval
'        End Get
'        Set(value As Double)
'            _majorGridInterval = value
'        End Set
'    End Property

'End Class

'Public Class ChartLabelProperties
'    'Chart Label Properties

'    'The name of the label (used by the chart control to reference to label).
'    Private _name As String = "Label1"
'    Property Name As String
'        Get
'            Return _name
'        End Get
'        Set(value As String)
'            _name = value
'        End Set
'    End Property

'    'The text displayed by the Chart Label.
'    Private _text = ""
'    Property Text As String
'        Get
'            Return _text
'        End Get
'        Set(value As String)
'            _text = value
'        End Set
'    End Property

'    'The label alignment relative to the chart.
'    'Private _alignment As LabelAlignment = LabelAlignment.TopCenter
'    Private _alignment As System.Drawing.ContentAlignment = ContentAlignment.TopCenter
'    'BottomCenter (512) BottomLeft (256) BottomRight (1024) MiddleCenter (32) MiddleLeft (16) MiddleRight (64) TopCenter (2) TopLeft (1) TopRight (4)
'    Property Alignment As ContentAlignment
'        Get
'            Return _alignment
'        End Get
'        Set(value As ContentAlignment)
'            _alignment = value
'        End Set
'    End Property

'    'The name of the font used to display the label.
'    Private _fontName As String = "Arial"
'    Property FontName As String
'        Get
'            Return _fontName
'        End Get
'        Set(value As String)
'            _fontName = value
'        End Set
'    End Property

'    ''The colour of the label text.
'    'Private _color As String = "Black" 'Selected from System.Drawing.Color
'    'Property Color As String
'    '    Get
'    '        Return _color
'    '    End Get
'    '    Set(value As String)
'    '        _color = value
'    '    End Set
'    'End Property

'    'The colour of the label text.
'    Private _color As System.Drawing.Color = Color.Black
'    Property Color As System.Drawing.Color
'        Get
'            Return _color
'        End Get
'        Set(value As System.Drawing.Color)
'            _color = value
'        End Set
'    End Property

'    'The size of the label text.
'    Private _size As Single = 14
'    Property Size As Single
'        Get
'            Return _size
'        End Get
'        Set(value As Single)
'            _size = value
'        End Set
'    End Property

'    'Indicates if the label text is bold.
'    Private _bold As Boolean = True
'    Property Bold As Boolean
'        Get
'            Return _bold
'        End Get
'        Set(value As Boolean)
'            _bold = value
'        End Set
'    End Property

'    'Indicates if the label text is italic.
'    Private _italic As Boolean = False
'    Property Italic As Boolean
'        Get
'            Return _italic
'        End Get
'        Set(value As Boolean)
'            _italic = value
'        End Set
'    End Property

'    'Indicates if the label text is underlined.
'    Private _underline As Boolean = False
'    Property Underline As Boolean
'        Get
'            Return _underline
'        End Get
'        Set(value As Boolean)
'            _underline = value
'        End Set
'    End Property

'    'Indicates if the label text is strikeout.
'    Private _strikeout As Boolean = False
'    Property Strikeout As Boolean
'        Get
'            Return _strikeout
'        End Get
'        Set(value As Boolean)
'            _strikeout = value
'        End Set
'    End Property

'End Class

'Public Class LabelProperties
'    'Label properties.

'    'The text displayed by the Label.
'    Private _text = ""
'    Property Text As String
'        Get
'            Return _text
'        End Get
'        Set(value As String)
'            _text = value
'        End Set
'    End Property

'    'The name of the font used to display the label.
'    Private _fontName As String = "Arial"
'    Property FontName As String
'        Get
'            Return _fontName
'        End Get
'        Set(value As String)
'            _fontName = value
'        End Set
'    End Property

'    'The colour of the label text.
'    Private _color As String = "Black" 'Selected from System.Drawing.Color
'    Property Color As String
'        Get
'            Return _color
'        End Get
'        Set(value As String)
'            _color = value
'        End Set
'    End Property

'    'The size of the label text.
'    Private _size As Single = 14
'    Property Size As Single
'        Get
'            Return _size
'        End Get
'        Set(value As Single)
'            _size = value
'        End Set
'    End Property

'    'Indicates if the label text is bold.
'    Private _bold As Boolean = True
'    Property Bold As Boolean
'        Get
'            Return _bold
'        End Get
'        Set(value As Boolean)
'            _bold = value
'        End Set
'    End Property

'    'Indicates if the label text is italic.
'    Private _italic As Boolean = False
'    Property Italic As Boolean
'        Get
'            Return _italic
'        End Get
'        Set(value As Boolean)
'            _italic = value
'        End Set
'    End Property

'    'Indicates if the label text is underlined.
'    Private _underline As Boolean = False
'    Property Underline As Boolean
'        Get
'            Return _underline
'        End Get
'        Set(value As Boolean)
'            _underline = value
'        End Set
'    End Property

'    'Indicates if the label text is strikeout.
'    Private _strikeout As Boolean = False
'    Property Strikeout As Boolean
'        Get
'            Return _strikeout
'        End Get
'        Set(value As Boolean)
'            _strikeout = value
'        End Set
'    End Property
'End Class

'Public Class SeriesProperties
'    'Series properties.

'    Private _name As String 'The name of the data series to be plotted as a line graph.
'    Property Name As String
'        Get
'            Return _name
'        End Get
'        Set(value As String)
'            _name = value
'        End Set
'    End Property

'End Class

'Public Class LineChart
'    'Line Chart Properties

'#Region " Variables" '----------------------------------------------------------------------------------------------------
'    Public ChartLabel As New ChartLabelProperties
'    Public XAxis As New AxisProperties
'    Public Y1Axis As New AxisProperties
'    Public Y2Axis As New AxisProperties
'    Public DataLocation As New ADVL_Utilities_Library_1.FileLocation 'Stores information about the data location in the Project - used to read the chart settings files.
'#End Region 'Variables ---------------------------------------------------------------------------------------------------

'#Region " Properties" '---------------------------------------------------------------------------------------------------

'    Private _fileName As String = "" 'The file name (with extension) of the chart settings. This file is stored in the Project.
'    Property FileName As String
'        Get
'            Return _fileName
'        End Get
'        Set(value As String)
'            _fileName = value
'        End Set
'    End Property

'    Private _inputDataType As String = "Database" 'Database or Dataset
'    Property InputDataType As String
'        Get
'            Return _inputDataType
'        End Get
'        Set(value As String)
'            _inputDataType = value
'        End Set
'    End Property

'    Private _inputDatabasePath As String = ""
'    Property InputDatabasePath As String
'        Get
'            Return _inputDatabasePath
'        End Get
'        Set(value As String)
'            _inputDatabasePath = value
'        End Set
'    End Property

'    Private _inputQuery As String = ""
'    Property InputQuery As String
'        Get
'            Return _inputQuery
'        End Get
'        Set(value As String)
'            _inputQuery = value
'        End Set
'    End Property

'    Private _inputDataDescr As String = "" 'A description of the data selected for charting.
'    Property InputDataDescr As String
'        Get
'            Return _inputDataDescr
'        End Get
'        Set(value As String)
'            _inputDataDescr = value
'        End Set
'    End Property

'    Private _seriesName As String = "Series1" 'The name of the data series being plotted.
'    Property SeriesName As String
'        Get
'            Return _seriesName
'        End Get
'        Set(value As String)
'            _seriesName = value
'        End Set
'    End Property

'    'The name of the Field containing the X values for the Point Chart.
'    Private _xValuesFieldName As String = ""
'    Property XValuesFieldName As String
'        Get
'            Return _xValuesFieldName
'        End Get
'        Set(value As String)
'            _xValuesFieldName = value
'        End Set
'    End Property

'    'The name of the Field containing the Y values for the Point Chart.
'    Private _yValuesFieldName As String = ""
'    Property YValuesFieldName As String
'        Get
'            Return _yValuesFieldName
'        End Get
'        Set(value As String)
'            _yValuesFieldName = value
'        End Set
'    End Property

'    'Specifies the value to be used for empty points. This property determines how an empty point is treated when the chart is drawn. (Average, Zero)
'    Private _emptyPointValue As String = "Average"
'    Property EmptyPointValue As String
'        Get
'            Return _emptyPointValue
'        End Get
'        Set(value As String)
'            _emptyPointValue = value
'        End Set
'    End Property

'    'Specifies the label position of the data point. (Auto, Top, Bottom, Right, Left, TopLeft, TopRight, BottomLeft, BottomRight, Center)
'    Private _labelStyle As String = "Auto"
'    Property LabelStyle As String
'        Get
'            Return _labelStyle
'        End Get
'        Set(value As String)
'            _labelStyle = value
'        End Set
'    End Property

'    'The Custom Property PixelPointDepth. Value range: Any integer > 0.
'    Private _pixelPointDepth As Integer = 0 'Default value
'    Property PixelPointDepth As Integer
'        Get
'            Return _pixelPointDepth
'        End Get
'        Set(value As Integer)
'            _pixelPointDepth = value
'        End Set
'    End Property

'    'The Custom Property PixelPointGapDepth. Value range: Any integer > 0.
'    Private _pixelPointGapDepth As Integer = 0 'Default value
'    Property PixelPointGapDepth As Integer
'        Get
'            Return _pixelPointGapDepth
'        End Get
'        Set(value As Integer)
'            _pixelPointGapDepth = value
'        End Set
'    End Property

'#End Region 'Properties --------------------------------------------------------------------------------------------------

'#Region "Methods" '-------------------------------------------------------------------------------------------------------

'    'Load the Stock Chart settings from the selected file.
'    Public Sub LoadFile(ByRef myFileName As String)

'        If myFileName = "" Then 'No stock point settings file has been selected.
'            Exit Sub
'        End If

'        Dim XDoc As System.Xml.Linq.XDocument
'        DataLocation.ReadXmlData(myFileName, XDoc)

'        If XDoc Is Nothing Then
'            RaiseEvent ErrorMessage("Xml list file is blank." & vbCrLf)
'            Exit Sub
'        End If

'        FileName = myFileName

'        'If XDoc.<ChartSettings>.<ChartType>.Value <> Nothing Then ChartType = [Enum].Parse(GetType(DataVisualization.Charting.SeriesChartType), XDoc.<ChartSettings>.<ChartType>.Value)

'        'Input Data:
'        If XDoc.<ChartSettings>.<InputDataType>.Value <> Nothing Then InputDataType = XDoc.<ChartSettings>.<InputDataType>.Value 'Database or Dataset
'        If XDoc.<ChartSettings>.<InputDatabasePath>.Value <> Nothing Then InputDatabasePath = XDoc.<ChartSettings>.<InputDatabasePath>.Value
'        If XDoc.<ChartSettings>.<InputQuery>.Value <> Nothing Then InputQuery = XDoc.<ChartSettings>.<InputQuery>.Value
'        If XDoc.<ChartSettings>.<InputDataDescr>.Value <> Nothing Then InputDataDescr = XDoc.<ChartSettings>.<InputDataDescr>.Value

'        'Chart Properties:
'        If XDoc.<ChartSettings>.<SeriesName>.Value <> Nothing Then SeriesName = XDoc.<ChartSettings>.<SeriesName>.Value
'        If XDoc.<ChartSettings>.<XValuesFieldName>.Value <> Nothing Then XValuesFieldName = XDoc.<ChartSettings>.<XValuesFieldName>.Value
'        If XDoc.<ChartSettings>.<YValuesFieldName>.Value <> Nothing Then YValuesFieldName = XDoc.<ChartSettings>.<YValuesFieldName>.Value
'        If XDoc.<ChartSettings>.<EmptyPointValue>.Value <> Nothing Then EmptyPointValue = XDoc.<ChartSettings>.<EmptyPointValue>.Value
'        If XDoc.<ChartSettings>.<LabelStyle>.Value <> Nothing Then LabelStyle = XDoc.<ChartSettings>.<LabelStyle>.Value
'        If XDoc.<ChartSettings>.<PixelPointGapDepth>.Value <> Nothing Then PixelPointGapDepth = XDoc.<ChartSettings>.<PixelPointGapDepth>.Value

'        'Chart Label:
'        If XDoc.<ChartSettings>.<ChartLabel>.<Name>.Value <> Nothing Then ChartLabel.Name = XDoc.<ChartSettings>.<ChartLabel>.<Name>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Text>.Value <> Nothing Then ChartLabel.Text = XDoc.<ChartSettings>.<ChartLabel>.<Text>.Value
'        'If XDoc.<ChartSettings>.<ChartLabel>.<Alignment>.Value <> Nothing Then ChartLabel.Alignment = XDoc.<ChartSettings>.<ChartLabel>.<Alignment>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Alignment>.Value <> Nothing Then ChartLabel.Alignment = [Enum].Parse(GetType(ContentAlignment), XDoc.<ChartSettings>.<ChartLabel>.<Alignment>.Value)
'        If XDoc.<ChartSettings>.<ChartLabel>.<FontName>.Value <> Nothing Then ChartLabel.FontName = XDoc.<ChartSettings>.<ChartLabel>.<FontName>.Value
'        'If XDoc.<ChartSettings>.<ChartLabel>.<Color>.Value <> Nothing Then ChartLabel.Color = XDoc.<ChartSettings>.<ChartLabel>.<Color>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Color>.Value <> Nothing Then ChartLabel.Color = Color.FromName(XDoc.<ChartSettings>.<ChartLabel>.<Color>.Value)
'        If XDoc.<ChartSettings>.<ChartLabel>.<Size>.Value <> Nothing Then ChartLabel.Size = XDoc.<ChartSettings>.<ChartLabel>.<Size>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Bold>.Value <> Nothing Then ChartLabel.Bold = XDoc.<ChartSettings>.<ChartLabel>.<Bold>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Italic>.Value <> Nothing Then ChartLabel.Italic = XDoc.<ChartSettings>.<ChartLabel>.<Italic>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Underline>.Value <> Nothing Then ChartLabel.Underline = XDoc.<ChartSettings>.<ChartLabel>.<Underline>.Value
'        If XDoc.<ChartSettings>.<ChartLabel>.<Strikeout>.Value <> Nothing Then ChartLabel.Strikeout = XDoc.<ChartSettings>.<ChartLabel>.<Strikeout>.Value

'        'X Axis:
'        If XDoc.<ChartSettings>.<XAxis>.<TitleText>.Value <> Nothing Then XAxis.Title.Text = XDoc.<ChartSettings>.<XAxis>.<TitleText>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleFontName>.Value <> Nothing Then XAxis.Title.FontName = XDoc.<ChartSettings>.<XAxis>.<TitleFontName>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleFontColor>.Value <> Nothing Then XAxis.Title.Color = XDoc.<ChartSettings>.<XAxis>.<TitleFontColor>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleSize>.Value <> Nothing Then XAxis.Title.Size = XDoc.<ChartSettings>.<XAxis>.<TitleSize>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleBold>.Value <> Nothing Then XAxis.Title.Bold = XDoc.<ChartSettings>.<XAxis>.<TitleBold>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleItalic>.Value <> Nothing Then XAxis.Title.Italic = XDoc.<ChartSettings>.<XAxis>.<TitleItalic>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleUnderline>.Value <> Nothing Then XAxis.Title.Underline = XDoc.<ChartSettings>.<XAxis>.<TitleUnderline>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleStrikeout>.Value <> Nothing Then XAxis.Title.Strikeout = XDoc.<ChartSettings>.<XAxis>.<TitleStrikeout>.Value
'        'If XDoc.<ChartSettings>.<XAxis>.<TitleAlignment>.Value <> Nothing Then XAxis.TitleAlignment = XDoc.<ChartSettings>.<XAxis>.<TitleAlignment>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<TitleAlignment>.Value <> Nothing Then XAxis.TitleAlignment = [Enum].Parse(GetType(StringAlignment), XDoc.<ChartSettings>.<XAxis>.<TitleAlignment>.Value)
'        If XDoc.<ChartSettings>.<XAxis>.<AutoMinimum>.Value <> Nothing Then XAxis.AutoMinimum = XDoc.<ChartSettings>.<XAxis>.<AutoMinimum>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<Minimum>.Value <> Nothing Then XAxis.Minimum = XDoc.<ChartSettings>.<XAxis>.<Minimum>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<AutoMaximum>.Value <> Nothing Then XAxis.AutoMaximum = XDoc.<ChartSettings>.<XAxis>.<AutoMaximum>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<Maximum>.Value <> Nothing Then XAxis.Maximum = XDoc.<ChartSettings>.<XAxis>.<Maximum>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<AutoInterval>.Value <> Nothing Then XAxis.AutoInterval = XDoc.<ChartSettings>.<XAxis>.<AutoInterval>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<Interval>.Value <> Nothing Then XAxis.Interval = XDoc.<ChartSettings>.<XAxis>.<Interval>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<AutoMajorGridInterval>.Value <> Nothing Then XAxis.AutoMajorGridInterval = XDoc.<ChartSettings>.<XAxis>.<AutoMajorGridInterval>.Value
'        If XDoc.<ChartSettings>.<XAxis>.<MajorGridInterval>.Value <> Nothing Then XAxis.MajorGridInterval = XDoc.<ChartSettings>.<XAxis>.<MajorGridInterval>.Value

'        'X Axis:
'        If XDoc.<ChartSettings>.<YAxis>.<TitleText>.Value <> Nothing Then Y1Axis.Title.Text = XDoc.<ChartSettings>.<YAxis>.<TitleText>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleFontName>.Value <> Nothing Then Y1Axis.Title.FontName = XDoc.<ChartSettings>.<YAxis>.<TitleFontName>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleFontColor>.Value <> Nothing Then Y1Axis.Title.Color = XDoc.<ChartSettings>.<YAxis>.<TitleFontColor>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleSize>.Value <> Nothing Then Y1Axis.Title.Size = XDoc.<ChartSettings>.<YAxis>.<TitleSize>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleBold>.Value <> Nothing Then Y1Axis.Title.Bold = XDoc.<ChartSettings>.<YAxis>.<TitleBold>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleItalic>.Value <> Nothing Then Y1Axis.Title.Italic = XDoc.<ChartSettings>.<YAxis>.<TitleItalic>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleUnderline>.Value <> Nothing Then Y1Axis.Title.Underline = XDoc.<ChartSettings>.<YAxis>.<TitleUnderline>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleStrikeout>.Value <> Nothing Then Y1Axis.Title.Strikeout = XDoc.<ChartSettings>.<YAxis>.<TitleStrikeout>.Value
'        'If XDoc.<ChartSettings>.<YAxis>.<TitleAlignment>.Value <> Nothing Then YAxis.TitleAlignment = XDoc.<ChartSettings>.<YAxis>.<TitleAlignment>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<TitleAlignment>.Value <> Nothing Then Y1Axis.TitleAlignment = [Enum].Parse(GetType(StringAlignment), XDoc.<ChartSettings>.<YAxis>.<TitleAlignment>.Value)
'        If XDoc.<ChartSettings>.<YAxis>.<AutoMinimum>.Value <> Nothing Then Y1Axis.AutoMinimum = XDoc.<ChartSettings>.<YAxis>.<AutoMinimum>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<Minimum>.Value <> Nothing Then Y1Axis.Minimum = XDoc.<ChartSettings>.<YAxis>.<Minimum>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<AutoMaximum>.Value <> Nothing Then Y1Axis.AutoMaximum = XDoc.<ChartSettings>.<YAxis>.<AutoMaximum>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<Maximum>.Value <> Nothing Then Y1Axis.Maximum = XDoc.<ChartSettings>.<YAxis>.<Maximum>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<AutoInterval>.Value <> Nothing Then Y1Axis.AutoInterval = XDoc.<ChartSettings>.<YAxis>.<AutoInterval>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<Interval>.Value <> Nothing Then Y1Axis.Interval = XDoc.<ChartSettings>.<YAxis>.<Interval>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<AutoMajorGridInterval>.Value <> Nothing Then Y1Axis.AutoMajorGridInterval = XDoc.<ChartSettings>.<YAxis>.<AutoMajorGridInterval>.Value
'        If XDoc.<ChartSettings>.<YAxis>.<MajorGridInterval>.Value <> Nothing Then Y1Axis.MajorGridInterval = XDoc.<ChartSettings>.<YAxis>.<MajorGridInterval>.Value

'    End Sub

'    'Function to return the Point Chart settings in an XDocument.
'    Public Function ToXDoc() As System.Xml.Linq.XDocument
'        Dim XDoc = <?xml version="1.0" encoding="utf-8"?>
'                   <!---->
'                   <!--Point Chart Settings File-->
'                   <ChartSettings>
'                       <!--Input Data:-->
'                       <InputDataType><%= InputDataType %></InputDataType>
'                       <InputDatabasePath><%= InputDatabasePath %></InputDatabasePath>
'                       <InputQuery><%= InputQuery %></InputQuery>
'                       <InputDataDescr><%= InputDataDescr %></InputDataDescr>
'                       <!--Chart Properties:-->
'                       <SeriesName><%= SeriesName %></SeriesName>
'                       <XValuesFieldName><%= XValuesFieldName %></XValuesFieldName>
'                       <YValuesFieldName><%= YValuesFieldName %></YValuesFieldName>
'                       <EmptyPointValue><%= EmptyPointValue %></EmptyPointValue>
'                       <LabelStyle><%= LabelStyle %></LabelStyle>
'                       <PixelPointDepth><%= PixelPointDepth %></PixelPointDepth>
'                       <PixelPointGapDepth><%= PixelPointGapDepth %></PixelPointGapDepth>
'                       <ChartLabel>
'                           <Name><%= ChartLabel.Name %></Name>
'                           <Text><%= ChartLabel.Text %></Text>
'                           <Alignment><%= ChartLabel.Alignment %></Alignment>
'                           <FontName><%= ChartLabel.FontName %></FontName>
'                           <Color><%= ChartLabel.Color %></Color>
'                           <Size><%= ChartLabel.Size %></Size>
'                           <Bold><%= ChartLabel.Bold %></Bold>
'                           <Italic><%= ChartLabel.Italic %></Italic>
'                           <Underline><%= ChartLabel.Underline %></Underline>
'                           <Strikeout><%= ChartLabel.Strikeout %></Strikeout>
'                       </ChartLabel>
'                       <XAxis>
'                           <TitleText><%= XAxis.Title.Text %></TitleText>
'                           <TitleFontName><%= XAxis.Title.FontName %></TitleFontName>
'                           <TitleFontColor><%= XAxis.Title.Color %></TitleFontColor>
'                           <TitleSize><%= XAxis.Title.Size %></TitleSize>
'                           <TitleBold><%= XAxis.Title.Bold %></TitleBold>
'                           <TitleItalic><%= XAxis.Title.Italic %></TitleItalic>
'                           <TitleUnderline><%= XAxis.Title.Underline %></TitleUnderline>
'                           <TitleStrikeout><%= XAxis.Title.Strikeout %></TitleStrikeout>
'                           <TitleAlignment><%= XAxis.TitleAlignment %></TitleAlignment>
'                           <AutoMinimum><%= XAxis.AutoMinimum %></AutoMinimum>
'                           <Minimum><%= XAxis.Minimum %></Minimum>
'                           <AutoMaximum><%= XAxis.AutoMaximum %></AutoMaximum>
'                           <Maximum><%= XAxis.Maximum %></Maximum>
'                           <AutoInterval><%= XAxis.AutoInterval %></AutoInterval>
'                           <Interval><%= XAxis.Interval %></Interval>
'                           <AutoMajorGridInterval><%= XAxis.AutoMajorGridInterval %></AutoMajorGridInterval>
'                           <MajorGridInterval><%= XAxis.MajorGridInterval %></MajorGridInterval>
'                       </XAxis>
'                       <YAxis>
'                           <TitleText><%= Y1Axis.Title.Text %></TitleText>
'                           <TitleFontName><%= Y1Axis.Title.FontName %></TitleFontName>
'                           <TitleFontColor><%= Y1Axis.Title.Color %></TitleFontColor>
'                           <TitleSize><%= Y1Axis.Title.Size %></TitleSize>
'                           <TitleBold><%= Y1Axis.Title.Bold %></TitleBold>
'                           <TitleItalic><%= Y1Axis.Title.Italic %></TitleItalic>
'                           <TitleUnderline><%= Y1Axis.Title.Underline %></TitleUnderline>
'                           <TitleStrikeout><%= Y1Axis.Title.Strikeout %></TitleStrikeout>
'                           <TitleAlignment><%= Y1Axis.TitleAlignment %></TitleAlignment>
'                           <AutoMinimum><%= Y1Axis.AutoMinimum %></AutoMinimum>
'                           <Minimum><%= Y1Axis.Minimum %></Minimum>
'                           <AutoMaximum><%= Y1Axis.AutoMaximum %></AutoMaximum>
'                           <Maximum><%= Y1Axis.Maximum %></Maximum>
'                           <AutoInterval><%= Y1Axis.AutoInterval %></AutoInterval>
'                           <Interval><%= Y1Axis.Interval %></Interval>
'                           <AutoMajorGridInterval><%= Y1Axis.AutoMajorGridInterval %></AutoMajorGridInterval>
'                           <MajorGridInterval><%= Y1Axis.MajorGridInterval %></MajorGridInterval>
'                       </YAxis>
'                   </ChartSettings>

'        Return XDoc
'    End Function

'    'Save the Point Chart settings in a file named FileName.
'    Public Sub SaveFile(ByVal myFileName As String)

'        If myFileName = "" Then 'No stock chart settings file has been selected.
'            Exit Sub
'        End If

'        DataLocation.SaveXmlData(myFileName, ToXDoc)

'    End Sub

'    'Clear the Stock Chart settings and apply defaults.
'    Public Sub Clear()

'        'ChartType = DataVisualization.Charting.SeriesChartType.Point

'        FileName = ""
'        InputDataType = "Database"
'        InputDatabasePath = ""
'        InputQuery = ""
'        InputDataDescr = ""

'        SeriesName = "Series1"
'        XValuesFieldName = ""
'        YValuesFieldName = ""
'        EmptyPointValue = "Average"
'        LabelStyle = "Auto"
'        PixelPointDepth = 0
'        PixelPointGapDepth = 0

'        ChartLabel.Name = "Label1"
'        ChartLabel.Text = ""
'        ChartLabel.Alignment = ContentAlignment.TopCenter
'        ChartLabel.FontName = "Arial"
'        'ChartLabel.Color = "Black"
'        ChartLabel.Color = Color.Black
'        ChartLabel.Size = 14
'        ChartLabel.Bold = True
'        ChartLabel.Italic = False
'        ChartLabel.Underline = False
'        ChartLabel.Strikeout = False

'        XAxis.Title.Text = ""
'        XAxis.Title.FontName = "Arial"
'        XAxis.Title.Color = "Black"
'        XAxis.Title.Size = 14
'        XAxis.Title.Bold = True
'        XAxis.Title.Italic = False
'        XAxis.Title.Underline = False
'        XAxis.Title.Strikeout = False
'        XAxis.TitleAlignment = StringAlignment.Center
'        XAxis.AutoMinimum = True
'        XAxis.Minimum = 0
'        XAxis.AutoMaximum = True
'        XAxis.Maximum = 1
'        XAxis.AutoInterval = True
'        XAxis.Interval = 0 'The Axis annotation interval. 0 = Auto.
'        XAxis.AutoMajorGridInterval = True
'        XAxis.MajorGridInterval = 0 'The major grid interval. 0 = Auto.

'        Y1Axis.Title.Text = ""
'        Y1Axis.Title.FontName = "Arial"
'        Y1Axis.Title.Color = "Black"
'        Y1Axis.Title.Size = 14
'        Y1Axis.Title.Bold = True
'        Y1Axis.Title.Italic = False
'        Y1Axis.Title.Underline = False
'        Y1Axis.Title.Strikeout = False
'        Y1Axis.TitleAlignment = StringAlignment.Center
'        Y1Axis.AutoMinimum = True
'        Y1Axis.Minimum = 0
'        Y1Axis.AutoMaximum = True
'        Y1Axis.Maximum = 1
'        Y1Axis.AutoInterval = True
'        Y1Axis.Interval = 0 'The Axis annotation interval. 0 = Auto.
'        Y1Axis.AutoMajorGridInterval = True
'        Y1Axis.MajorGridInterval = 0 'The major grid interval. 0 = Auto.

'        'Leave DataLocation unchanged.

'    End Sub


'#End Region 'Methods -----------------------------------------------------------------------------------------------------

'#Region "Events" '--------------------------------------------------------------------------------------------------------

'    Event ErrorMessage(ByVal Message As String) 'Send an error message.
'    Event Message(ByVal Message As String) 'Send a normal message.

'#End Region 'Events ------------------------------------------------------------------------------------------------------

'End Class
