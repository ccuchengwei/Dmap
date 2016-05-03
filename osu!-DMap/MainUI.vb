Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Thread
Imports System.Net
Imports System.IO
Imports System.ComponentModel

Public Class MainUI

#Region "[Win32API] [Structure] Declare"

    Const MAX_MODULE_NAME32 As Integer = 255
    Const MAX_PATH As Integer = 260

    Const TOKEN_ADJUST_PRIVILEGES As Integer = &H20
    Const TOKEN_QUERY As Integer = &H8
    Const ANYSIZE_ARRAY As Integer = &H1
    Const SE_PRIVILEGE_ENABLED As Integer = &H2
    Const ERROR_SUCCESS As Integer = &H0

    Const PROCESS_ALL_ACCESS As Integer = &H1F0FFF

    <StructLayout(LayoutKind.Sequential)> _
    Structure TOKEN_PRIVILEGES
        Public PrivilegeCount As Integer
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=ANYSIZE_ARRAY)> _
        Public Privileges() As LUID_AND_ATTRIBUTES
    End Structure

    Structure LUID_AND_ATTRIBUTES
        Public Luid As LUID
        Public Attributes As Integer
    End Structure

    Structure LUID
        Public LowPart As Integer
        Public HighPart As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Structure OSVERSIONINFOEX
        Public dwOSVersionInfoSize As Integer
        Public dwMajorVersion As Integer
        Public dwMinorVersion As Integer
        Public dwBuildNumber As Integer
        Public dwPlatformId As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)> _
        Public szCSDVersion As String
        Public wServicePackMajor As Short
        Public wServicePackMinor As Short
        Public wSuiteMask As Short
        Public wProductType As Byte
        Public wReserved As Byte
    End Structure

    Private Enum SnapshotFlags As Integer
        TH32CS_SNAPHEAPLIST = &H1
        TH32CS_SNAPPROCESS = &H2
        TH32CS_SNAPTHREAD = &H4
        TH32CS_SNAPMODULE = &H8
        TH32CS_SNAPALL = (TH32CS_SNAPHEAPLIST Or TH32CS_SNAPPROCESS Or TH32CS_SNAPTHREAD Or TH32CS_SNAPMODULE)
        TH32CS_INHERIT = &H80000000
    End Enum

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Structure PROCESSENTRY32
        Public dwSize As Integer
        Public cntUsage As Integer
        Public th32ProcessID As Integer
        Public th32DefaultHeapID As Integer
        Public th32ModuleID As Integer
        Public cntThreads As Integer
        Public th32ParentProcessID As Integer
        Public pcPriClassBase As Integer
        Public dwFlags As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public szExeFile As String
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Structure MODULEENTRY32
        Public dwSize As Integer
        Public th32ModuleID As Integer
        Public th32ProcessID As Integer
        Public GlblcntUsage As Integer
        Public ProccntUsage As Integer
        Public modBaseAddr As Integer
        Public modBaseSize As Integer
        Public hModule As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_MODULE_NAME32 + 1)> _
        Public szModule As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public szExePath As String
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)> _
    Structure IMAGE_DOS_HEADER
        Public e_magic As Short 'Magic number
        Public e_cblp As Short 'Bytes on last page of file
        Public e_cp As Short 'Pages in file
        Public e_crlc As Short 'Relocations
        Public e_cparhdr As Short 'Size of header in paragraphs
        Public e_minalloc As Short 'Minimum extra paragraphs needed
        Public e_maxalloc As Short 'Maximum extra paragraphs needed
        Public e_ss As Short 'Initial (relative) SS value
        Public e_sp As Short 'Initial SP value
        Public e_csum As Short 'Checksum
        Public e_ip As Short 'Initial IP value
        Public e_cs As Short 'Initial (relative) CS value
        Public e_lfarlc As Short 'File address of relocation table
        Public e_ovno As Short 'Overlay number
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)> _
        Public e_res() As Short 'Reserved words
        Public e_oemid As Short 'OEM identifier (for e_oeminfo)
        Public e_oeminfo As Short 'OEM information e_oemid specific
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=10)> _
        Public e_res2() As Short 'Reserved words
        Public e_lfanew As Integer 'File address of new exe header
    End Structure

    Structure IMAGE_NT_HEADERS
        Public Signature As Integer 'pe
        Public FileHeader As IMAGE_FILE_HEADER
        Public OptionalHeader As IMAGE_OPTIONAL_HEADER
    End Structure

    Structure IMAGE_FILE_HEADER
        Public Machine As Short
        Public NumberOfSections As Short
        Public TimeDateStamp As Integer
        Public PointerToSymbolTable As Integer
        Public NumberOfSymbols As Integer
        Public SizeOfOptionalHeader As Short
        Public Characteristics As Short
    End Structure

    Structure IMAGE_OPTIONAL_HEADER
        'Standard fields.
        Public Magic As Short
        Public MajorLinkerVersion As Byte
        Public MinorLinkerVersion As Byte
        Public SizeOfCode As Integer
        Public SizeOfInitializedData As Integer
        Public SizeOfUninitializedData As Integer
        Public AddressOfEntryPoint As Integer
        Public BaseOfCode As Integer
        Public BaseOfData As Integer
        'NT additional fields.
        Public ImageBase As Integer
        Public SectionAlignment As Integer
        Public FileAlignment As Integer
        Public MajorOperatingSystemVersion As Short
        Public MinorOperatingSystemVersion As Short
        Public MajorImageVersion As Short
        Public MinorImageVersion As Short
        Public MajorSubsystemVersion As Short
        Public MinorSubsystemVersion As Short
        Public Win32VersionValue As Integer
        Public SizeOfImage As Integer
        Public SizeOfHeaders As Integer
        Public CheckSum As Integer
        Public Subsystem As Short
        Public DllCharacteristics As Short
        Public SizeOfStackReserve As Integer
        Public SizeOfStackCommit As Integer
        Public SizeOfHeapReserve As Integer
        Public SizeOfHeapCommit As Integer
        Public LoaderFlags As Integer
        Public NumberOfRvaAndSizes As Integer
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)> _
        Public DataDirectory() As IMAGE_DATA_DIRECTORY
    End Structure

    Structure IMAGE_DATA_DIRECTORY
        Public VirtualAddress As Integer
        Public Size As Integer
    End Structure

    Structure IMAGE_EXPORT_DIRECTORY
        Public Characteristics As Integer
        Public TimeDateStamp As Integer
        Public MajorVersion As Short
        Public MinorVersion As Short
        Public Name As Integer
        Public Base As Integer
        Public NumberOfFunctions As Integer
        Public NumberOfNames As Integer
        Public AddressOfFunctions As Integer     'RVA from base of image
        Public AddressOfNames As Integer         'RVA from base of image
        Public AddressOfNameOrdinals As Integer  'RVA from base of image
    End Structure

    Structure _RECT
        Public left As Integer
        Public top As Integer
        Public right As Integer
        Public bottom As Integer
    End Structure

    Structure _POINT
        Public x As Integer
        Public y As Integer
    End Structure

    '記憶體型態
    Enum AllocationType As Integer
        Commit = &H1000
        Reserve = &H2000
        Decommit = &H4000
        Release = &H8000
        Reset = &H80000
        Physical = &H400000
        TopDown = &H100000
        WriteWatch = &H200000
        LargePages = &H20000000
    End Enum

    Enum FreeType As Integer
        Decommit = &H4000
        Release = &H8000
    End Enum

    '記憶體保護狀態
    Enum MemoryProtection As Integer
        NoAccess = &H1
        [ReadOnly] = &H2
        ReadWrite = &H4
        WriteCopy = &H8
        Execute = &H10
        ExecuteRead = &H20
        ExecuteReadWrite = &H40
        ExecuteWriteCopy = &H80
        GuardModifierflag = &H100
        NoCacheModifierflag = &H200
        WriteCombineModifierflag = &H400
    End Enum

    Public Enum WindowLongFlags As Integer
        GWL_EXSTYLE = -20
        GWLP_HINSTANCE = -6
        GWLP_HWNDPARENT = -8
        GWL_ID = -12
        GWL_STYLE = -16
        GWL_USERDATA = -21
        GWL_WNDPROC = -4
        DWLP_USER = &H8
        DWLP_MSGRESULT = &H0
        DWLP_DLGPROC = &H4
    End Enum

    Enum SW As Integer
        Hide = 0 'Hide the window.
        ShowNormal = 1 'Show the window and activate it (as usual).
        ShowMinimize = 2 'Show the window minimized.
        ShowMaximize = 3 'Show the window maximized.
        ShowNoActivate = 4 'Show the window in its most recent size and position but do not activate it.
        Show = 5 'Show the window.
        Minimize = 6 'Minimize the window.
        ShowMinNoActive = 7 'Show the window minimized but do not activate it.
        ShowNA = 8 'Show the window in its current state but do not activate it.
        Restore = 9 'Restore the window (not maximized nor minimized).
        ShowDefault = 10
    End Enum

    Private Enum SetWindowPosFlags As Integer
        SynchronousWindowPosition = &H4000
        DeferErase = &H2000
        DrawFrame = &H20
        FrameChanged = &H20
        HideWindow = &H80
        DoNotActivate = &H10
        DoNotCopyBits = &H100
        IgnoreMove = &H2
        DoNotChangeOwnerZOrder = &H200
        DoNotRedraw = &H8
        DoNotReposition = &H200
        DoNotSendChangingEvent = &H400
        IgnoreResize = &H1
        IgnoreZOrder = &H4
        ShowWindow = &H40
    End Enum

    Const WS_EX_LAYERED As Integer = &H80000
    Const WS_EX_TRANSPARENT As Integer = &H20& '使滑鼠點擊會點擊到後方視窗
    Const LWA_COLORKEY As Integer = &H1
    Const LWA_ALPHA As Integer = &H2


    Private Declare Function GetForegroundWindow Lib "user32" () As Integer
    Private Declare Function SetForegroundWindow Lib "user32" (ByVal hwnd As Integer) As Boolean
    Private Declare Function ClientToScreen Lib "user32" (ByVal hWnd As Integer, ByRef lpPoint As _POINT) As Boolean
    Private Declare Function ShowWindowAsync Lib "user32" (ByVal hWnd As Integer, ByVal nCmdShow As SW) As Boolean
    Private Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As WindowLongFlags) As Integer
    Private Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As WindowLongFlags, ByVal dwNewLong As Integer) As Integer
    Private Declare Function SetWindowPos Lib "user32" (ByVal hWnd As Integer, ByVal hWndInsertAfter As Integer, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As SetWindowPosFlags) As Boolean
    Private Declare Function SetLayeredWindowAttributes Lib "user32" (ByVal hWnd As Integer, ByVal crKey As Integer, ByVal bAlpha As Byte, ByVal dwFlags As Integer) As Boolean
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowW" (<MarshalAs(UnmanagedType.LPWStr)> ByVal lpClassName As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpWindowName As String) As Integer
    Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExW" (ByVal hWndParent As Integer, ByVal hWndChildAfter As Integer, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpClassName As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpWindowName As String) As Integer
    Private Declare Function SetParent Lib "user32" (ByVal hWndChild As Integer, ByVal hWndNewParent As Integer) As Integer

    Private Declare Function OpenProcessToken Lib "Advapi32" (ByVal ProcessHandle As Integer, ByVal DesiredAccess As Integer, ByRef TokenHandle As Integer) As Boolean
    Private Declare Function GetCurrentProcess Lib "kernel32" () As Integer
    Private Declare Function LookupPrivilegeValue Lib "Advapi32" Alias "LookupPrivilegeValueW" (<MarshalAs(UnmanagedType.LPWStr)> ByVal lpSystemName As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpName As String, ByRef lpLuid As LUID) As Boolean
    Private Declare Function AdjustTokenPrivileges Lib "advapi32" (ByVal TokenHandle As Integer, ByVal DisableAllPrivileges As Integer, ByRef NewState As TOKEN_PRIVILEGES, ByVal BufferLength As Integer, ByRef PreviousState As TOKEN_PRIVILEGES, ByRef ReturnLength As Integer) As Boolean
    Private Declare Function GetLastError Lib "kernel32" () As Integer
    Private Declare Function GetVersionEx Lib "kernel32" Alias "GetVersionExW" (ByRef lpVersionInformation As OSVERSIONINFOEX) As Boolean
    Private Declare Function GetWindowThreadProcessId Lib "user32" (ByVal hwnd As Integer, ByRef lpdwProcessId As Integer) As Integer
    Private Declare Function CreateToolhelp32Snapshot Lib "kernel32" (ByVal dwFlags As SnapshotFlags, ByVal th32ProcessID As Integer) As Integer
    Private Declare Function CloseHandle Lib "kernel32" (ByVal Handle As Integer) As Boolean
    Private Declare Function Process32First Lib "kernel32" Alias "Process32FirstW" (ByVal hSnapshot As Integer, ByRef lppe As PROCESSENTRY32) As Boolean
    Private Declare Function Process32Next Lib "kernel32" Alias "Process32NextW" (ByVal hSnapshot As Integer, ByRef lppe As PROCESSENTRY32) As Boolean
    Private Declare Function Module32First Lib "kernel32" Alias "Module32FirstW" (ByVal hSnapshot As Integer, ByRef lpme As MODULEENTRY32) As Boolean
    Private Declare Function Module32Next Lib "kernel32" Alias "Module32NextW" (ByVal hSnapshot As Integer, ByRef lpme As MODULEENTRY32) As Boolean
    Private Declare Function OpenProcess Lib "Kernel32" (ByVal dwDesiredAccessas As Integer, ByVal bInheritHandle As Integer, ByVal dwProcId As Integer) As Integer
    Private Overloads Declare Function ReadProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef Value As Integer, ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
    Private Overloads Declare Function ReadProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As IntPtr, ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
    Private Overloads Declare Function ReadProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Byte(), ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
    Private Overloads Declare Function ReadProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Short(), ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
    Private Overloads Declare Function ReadProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Integer(), ByVal iSize As Integer, ByRef lpNumberOfBytesRead As Integer) As Boolean
    Public Overloads Declare Function WriteProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef Value As Integer, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Private Overloads Declare Function WriteProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As IntPtr, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Private Overloads Declare Function WriteProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Byte(), ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Private Overloads Declare Function WriteProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Short(), ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Private Overloads Declare Function WriteProcessMemory Lib "Kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer As Integer(), ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Private Declare Function VirtualAllocEx Lib "kernel32" (ByVal hProcess As Integer, ByVal lpAddress As Integer, ByVal dwSize As Integer, ByVal flAllocationType As AllocationType, ByVal flProtect As MemoryProtection) As Integer
    Private Declare Function VirtualFreeEx Lib "kernel32" (ByVal hProcess As Integer, ByVal lpAddress As Integer, ByVal dwSize As Integer, ByVal dwFreeType As FreeType) As Boolean


    Enum CharType As Integer
        Ansi = 1
        Unicode = 2
    End Enum

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Structure HookPar
        Public hwnd As Integer
        Public blnMessage As Integer
        Public blnHook As Integer
        Public WinRect As _RECT
        Public CurPos As _POINT
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public LinkTitle0 As String 'http://
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public LinkTitle1 As String 'https://
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public WebLink0 As String  'osu.ppy.sh/b/
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public WebLink1 As String  'osu.ppy.sh/s/
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public WebLink2 As String  'bloodcat.com/osu/m/
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public WebLink3 As String  'osu.ppy.sh/d/
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public HttpLink As String
    End Structure

#End Region

    Const intConnLimit As Integer = 7
    Dim intCaseCount As Integer = 0

    Dim objCookieContainer As New CookieContainer

    Dim intBarSizeX As Integer = 500
    Dim intBarSizeY As Integer = 15
    Const intBarInterval As Integer = 3
    Dim imgBG As Image
    Dim imgLG As Image
    Dim objPictureBox As New PictureBox

    Dim strUrlRegister(0 To intConnLimit - 1) As String
    Dim objDownloader(0 To intConnLimit - 1) As FileDownloader
    Dim datBarDate(0 To intConnLimit - 1) As Date
    Dim strBarText(0 To intConnLimit - 1) As String
    Dim dblBarValue(0 To intConnLimit - 1) As Double
    Dim blnBarVisble(0 To intConnLimit - 1) As Boolean

    Dim intTaskStatus(0 To intConnLimit - 1) As TaskStatus
    Dim blnDownloadEnd(0 To intConnLimit - 1) As Boolean
    Dim intIniStatus As InitializeStatus
    Dim datLastConnection As Date

    Public blnHookMessage As Boolean
    Public blnOpenHttp As Boolean
    Public blnOpenFile As Boolean
    Public blnCopyUrl As Boolean
    Public strPath As String
    Public objBarTextColor As Color
    Dim objBarFont As Font

    Dim blnExit As Boolean = False
    Dim blnEndOSU As Boolean = False
    Dim blnMainRun As Boolean = False
    Dim OSU_hdlHWND, hwndSELF As Integer
    Dim OSU_hdlPID As Integer
    Dim OSU_objProc As Process

    Dim HOOK_hdlProcess As Integer
    Dim HOOK_intHookAddr As Integer
    Dim HOOK_hdlFunc As Integer
    Dim HOOK_hdlVar As Integer

    Dim Thread_Hook As New Thread(AddressOf HookLoop)
    Dim Thread_bloodcat As New Thread(AddressOf TaskLoop_bloodcat)
    Dim objIniWebClient As New WebClientPlus

    '▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄

    Private Sub MainUI_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim blnTmp As Boolean
        '======================================================================================
        Dim objOsVer As New OSVERSIONINFOEX
        objOsVer.dwOSVersionInfoSize = Marshal.SizeOf(objOsVer)
        GetVersionEx(objOsVer)
        If objOsVer.dwMajorVersion < 5 Or (objOsVer.dwMajorVersion = 5 And objOsVer.dwMinorVersion < 1) Then
            ErrorInfo("本程式只能在 Windows XP 以上版本執行") : Exit Sub
        End If
        '======================================================================================
        '提升系統權限
        Dim hToken As Integer
        Dim objTmp As New TOKEN_PRIVILEGES '空的
        Dim tkp As TOKEN_PRIVILEGES
        ReDim tkp.Privileges(0)

        blnTmp = OpenProcessToken(GetCurrentProcess, TOKEN_ADJUST_PRIVILEGES Or TOKEN_QUERY, hToken)
        If Not blnTmp Then ErrorInfo("出錯655-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        blnTmp = LookupPrivilegeValue("", "SeDebugPrivilege", tkp.Privileges(0).Luid)
        If Not blnTmp Then ErrorInfo("出錯533-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        tkp.PrivilegeCount = 1
        tkp.Privileges(0).Attributes = SE_PRIVILEGE_ENABLED
        blnTmp = AdjustTokenPrivileges(hToken, 0, tkp, Marshal.SizeOf(tkp), objTmp, 0)
        If Not blnTmp Then ErrorInfo("出錯421-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        '======================================================================================
        '此舉可能會需要提升系統權限
        Dim hdlSnapshot As Integer
        Dim objProcEntry As New PROCESSENTRY32
        Dim intZOrder As Integer
        Dim hdlThr As Integer

        hdlSnapshot = CreateToolhelp32Snapshot(SnapshotFlags.TH32CS_SNAPPROCESS, 0)
        If hdlSnapshot = -1 Then ErrorInfo("出錯103-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub

        objProcEntry.dwSize = Marshal.SizeOf(objProcEntry)
        If Process32First(hdlSnapshot, objProcEntry) Then
            Do
                If String.Compare(objProcEntry.szExeFile, "osu!.exe") = 0 Then
                    intZOrder = 0
                    Do
                        OSU_hdlHWND = FindWindowEx(0, intZOrder, vbNullString, "osu!")
                        If OSU_hdlHWND = 0 Then Exit Do
                        hdlThr = GetWindowThreadProcessId(OSU_hdlHWND, OSU_hdlPID)
                        If OSU_hdlPID = objProcEntry.th32ProcessID Then
                            OSU_objProc = Process.GetProcessById(objProcEntry.th32ProcessID)
                            Exit Do
                        End If
                        intZOrder = OSU_hdlHWND
                    Loop
                    If OSU_objProc IsNot Nothing Then Exit Do
                End If
            Loop While Process32Next(hdlSnapshot, objProcEntry)
        Else
            CloseHandle(hdlSnapshot)
            ErrorInfo(objProcEntry.dwSize.ToString & "出錯524-" & CStr(GetLastError) & "，程式即將結束! ") : Exit Sub
        End If
        CloseHandle(hdlSnapshot)
        '======================================================================================
        If OSU_objProc Is Nothing Then
            Dim OSU_strCommandLine, OSU_strPath As String
            Dim i1 As Integer
            OSU_strCommandLine = CStr(Microsoft.Win32.Registry.GetValue("HKEY_CLASSES_ROOT\osu!\shell\open\command", "", "")).Trim
            If OSU_strCommandLine.IndexOf(""""c) = 0 Then
                i1 = OSU_strCommandLine.IndexOf(""""c, 1)
                OSU_strPath = OSU_strCommandLine.Substring(1, i1 - 1)
            Else
                i1 = OSU_strCommandLine.IndexOfAny(New Char() {" "c, """"c})
                OSU_strPath = OSU_strCommandLine.Substring(0, i1)
            End If

            Dim OSU_objEXE As New FileInfo(OSU_strPath)
            If OSU_objEXE.Exists Then
                Dim OSU_objFileVersionInfo = FileVersionInfo.GetVersionInfo(OSU_strPath)
                If OSU_objFileVersionInfo.ProductName = "osu!" Then
                    OSU_objProc = Process.Start(OSU_strPath)
                    OSU_hdlPID = OSU_objProc.Id
                    Try
                        For cc As Integer = 1 To 100
                            If OSU_objProc.MainWindowHandle.ToInt32 <> 0 Then Exit For
                            OSU_objProc.Close()
                            OSU_objProc.Dispose()
                            OSU_objProc = Nothing
                            Sleep(1000)
                            OSU_objProc = Process.GetProcessById(OSU_hdlPID)
                        Next
                        If OSU_objProc IsNot Nothing AndAlso OSU_objProc.MainWindowHandle.ToInt32 <> 0 Then
                            OSU_hdlHWND = OSU_objProc.MainWindowHandle.ToInt32
                        Else
                            OSU_objProc.Close()
                            OSU_objProc.Dispose()
                            OSU_objProc = Nothing
                        End If
                    Catch ex As Exception
                        If OSU_objProc IsNot Nothing Then
                            OSU_objProc.Close()
                            OSU_objProc.Dispose()
                            OSU_objProc = Nothing
                        End If
                    End Try
                End If
            End If
            If OSU_objProc Is Nothing Then ErrorInfo("找不到osu!，請正確安裝osu!再啟動本程式！") : Exit Sub
        End If
        '======================================================================================
        blnHookMessage = My.Settings.blnHookMessage
        blnOpenHttp = My.Settings.blnOpenHttp
        blnOpenFile = My.Settings.blnOpenFile
        blnCopyUrl = My.Settings.blnCopyUrl
        strPath = My.Settings.strPath
        objBarTextColor = My.Settings.objBarTextColor
        If strPath = "" Then
            strPath = My.Application.Info.DirectoryPath
        Else
            Dim objDirInfo As New IO.DirectoryInfo(strPath)
            If Not objDirInfo.Exists Then
                Try
                    objDirInfo.Create()
                Catch ex As Exception
                    MessageBox.Show("目錄無效，自動換成桌面", "無效的路徑", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    strPath = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                End Try
            End If
        End If
        '======================================================================================
        Dim Ret As Integer
        hwndSELF = Me.Handle.ToInt32
        Ret = GetWindowLong(hwndSELF, WindowLongFlags.GWL_EXSTYLE)
        Ret = Ret Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
        SetWindowLong(hwndSELF, WindowLongFlags.GWL_EXSTYLE, Ret)
        'SetLayeredWindowAttributes(hwndSELF, RGB(&HFF, &HFF, &HFF), 180, LWA_COLORKEY Or LWA_ALPHA)

        Dim i As Integer
        For i = 0 To intConnLimit - 1
            objDownloader(i) = New FileDownloader
            objDownloader(i).BufferSize = 128 * 1024
            objDownloader(i).ConnectionLimit = intConnLimit
            objDownloader(i).Timeout = 10000
            AddHandler objDownloader(i).DownloadEnd, AddressOf Me.DownloadEndEvent
        Next

        Dim imgTmp0, imgTmp1 As Image
        Dim strBgPath As String = My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "dmap-bg.png")
        Dim strFgPath As String = My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "dmap-fg.png")
        Try
            If My.Computer.FileSystem.FileExists(strBgPath) Then
                imgTmp0 = Image.FromFile(strBgPath)
            End If
        Catch ex As Exception
        End Try
        If imgTmp0 Is Nothing Then imgTmp0 = My.Resources.dmap_bg
        Try
            If My.Computer.FileSystem.FileExists(strFgPath) Then
                imgTmp1 = Image.FromFile(strFgPath)
            End If
        Catch ex As Exception
        End Try
        If imgTmp1 Is Nothing Then imgTmp1 = My.Resources.dmap_fg

        If imgTmp0.Width > imgTmp1.Width Then
            intBarSizeX = imgTmp0.Width
        Else
            intBarSizeX = imgTmp1.Width
        End If
        If imgTmp0.Height > imgTmp1.Height Then
            intBarSizeY = imgTmp0.Height
        Else
            intBarSizeY = imgTmp1.Height
        End If
        If intBarSizeX < 200 Then intBarSizeX = 200
        If intBarSizeX > 640 - intBarInterval * 2 Then intBarSizeX = 640 - intBarInterval * 2
        If intBarSizeY < 15 Then intBarSizeY = 15
        If intBarSizeY > 40 Then intBarSizeY = 40
        imgBG = New Bitmap(imgTmp0, intBarSizeX, intBarSizeY)
        imgLG = New Bitmap(imgTmp1, intBarSizeX, intBarSizeY)
        objBarFont = New Font("Arial", CInt((intBarSizeY - 15) / 3 + 10), FontStyle.Bold)
        imgTmp0.Dispose()
        imgTmp1.Dispose()

        Me.Size = New Size(intBarSizeX + intBarInterval, (intBarSizeY + intBarInterval) * intConnLimit)
        objPictureBox.Size = Me.Size
        Me.Controls.Add(objPictureBox)
        '======================================================================================
        Dim intModAddr As Integer
        Dim intTmp As Integer
        Dim intGCRaddr, intGCPaddr, intSTCaddr As Integer

        HOOK_hdlProcess = OpenProcess(PROCESS_ALL_ACCESS, 0, OSU_hdlPID)
        If HOOK_hdlProcess = 0 Then ErrorInfo("出錯642-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub

        intModAddr = GetModuleHandleEx(OSU_hdlPID, "shell32.dll")
        If intModAddr = 0 Then ErrorInfo("出錯627-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub

        intTmp = GetProcAddressEx(HOOK_hdlProcess, intModAddr, "ShellExecuteExW")
        If intTmp = 0 Then ErrorInfo("出錯769-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        HOOK_intHookAddr = intModAddr + intTmp

        intModAddr = GetModuleHandleEx(OSU_hdlPID, "user32.dll")
        If intModAddr = 0 Then ErrorInfo("出錯255-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub

        intTmp = GetProcAddressEx(HOOK_hdlProcess, intModAddr, "GetClientRect")
        If intTmp = 0 Then ErrorInfo("出錯378-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        intGCRaddr = intModAddr + intTmp

        intTmp = GetProcAddressEx(HOOK_hdlProcess, intModAddr, "GetCursorPos")
        If intTmp = 0 Then ErrorInfo("出錯051-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        intGCPaddr = intModAddr + intTmp

        intTmp = GetProcAddressEx(HOOK_hdlProcess, intModAddr, "ScreenToClient")
        If intTmp = 0 Then ErrorInfo("出錯781-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        intSTCaddr = intModAddr + intTmp

        HOOK_hdlFunc = VirtualAllocEx(HOOK_hdlProcess, 0, 4096, AllocationType.Commit Or AllocationType.Reserve, MemoryProtection.ExecuteReadWrite)
        If HOOK_hdlFunc = 0 Then ErrorInfo("出錯336-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub

        HOOK_hdlVar = VirtualAllocEx(HOOK_hdlProcess, 0, 4096, AllocationType.Commit Or AllocationType.Reserve, MemoryProtection.ExecuteReadWrite)
        If HOOK_hdlVar = 0 Then ErrorInfo("出錯984-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        '======================================================================================
        Dim arrByteCode() As Byte
        arrByteCode = My.Resources.Code
        WriteArray(arrByteCode, &HB, HOOK_hdlVar + 12) 'WinRect
        WriteArray(arrByteCode, &H11, HOOK_hdlVar + 0) 'hwnd
        WriteArray(arrByteCode, &H1C, HOOK_hdlVar + 24) 'WinRect.bottom
        WriteArray(arrByteCode, &H28, HOOK_hdlVar + 28) 'CurPos
        WriteArray(arrByteCode, &H32, HOOK_hdlVar + 28) 'CurPos
        WriteArray(arrByteCode, &H38, HOOK_hdlVar + 0) 'hwnd
        WriteArray(arrByteCode, &H42, HOOK_hdlVar + 24) 'WinRect.bottom
        WriteArray(arrByteCode, &H56, HOOK_hdlVar + 32) 'CurPos.y
        WriteArray(arrByteCode, &H62, HOOK_hdlVar + 4) 'blnMessage
        WriteArray(arrByteCode, &H6A, HOOK_hdlVar + 24) 'WinRect.bottom
        WriteArray(arrByteCode, &H7E, HOOK_hdlVar + 32) 'CurPos.y
        WriteArray(arrByteCode, &H89, HOOK_hdlVar + 36 + MAX_PATH * 0) 'LinkTitle0
        WriteArray(arrByteCode, &HA5, HOOK_hdlVar + 36 + MAX_PATH * 2) 'LinkTitle1
        WriteArray(arrByteCode, &HC5, HOOK_hdlVar + 36 + MAX_PATH * 4) 'WebLink0
        WriteArray(arrByteCode, &HE0, HOOK_hdlVar + 36 + MAX_PATH * 6) 'WebLink1
        WriteArray(arrByteCode, &HFB, HOOK_hdlVar + 36 + MAX_PATH * 8) 'WebLink2
        WriteArray(arrByteCode, &H116, HOOK_hdlVar + 36 + MAX_PATH * 10) 'WebLink3
        WriteArray(arrByteCode, &H132, HOOK_hdlVar + 8) 'blnHook
        WriteArray(arrByteCode, &H13D, HOOK_hdlVar + 36 + MAX_PATH * 12) 'HttpLink
        WriteArray(arrByteCode, &H153, HOOK_hdlVar + 8) 'blnHook
        WriteArray(arrByteCode, &H16A, HOOK_hdlFunc + &H180) 'OrgAddr
        WriteArray(arrByteCode, &H170, HOOK_hdlFunc + &H184) 'GetClientRect
        WriteArray(arrByteCode, &H176, HOOK_hdlFunc + &H188) 'GetCursorPos
        WriteArray(arrByteCode, &H17C, HOOK_hdlFunc + &H18C) 'ScreenToClient
        WriteArray(arrByteCode, &H180, HOOK_intHookAddr + 2) 'OrgAddr
        WriteArray(arrByteCode, &H184, intGCRaddr) 'GetClientRect
        WriteArray(arrByteCode, &H188, intGCPaddr) 'GetCursorPos
        WriteArray(arrByteCode, &H18C, intSTCaddr) 'ScreenToClient
        blnTmp = WriteProcessMemory(HOOK_hdlProcess, HOOK_hdlFunc, arrByteCode, arrByteCode.Length, 0)
        If Not blnTmp Then ErrorInfo("出錯100-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        '===========================================
        Dim objPar As New HookPar
        objPar.hwnd = OSU_hdlHWND
        objPar.blnMessage = CType(blnHookMessage, Integer)
        objPar.LinkTitle0 = "http://"
        objPar.LinkTitle1 = "https://"
        objPar.WebLink0 = "osu.ppy.sh/b/"
        objPar.WebLink1 = "osu.ppy.sh/s/"
        objPar.WebLink2 = "bloodcat.com/osu/m/"
        objPar.WebLink3 = "osu.ppy.sh/d/"
        blnTmp = WriteProcessMemoryEx(HOOK_hdlProcess, HOOK_hdlVar, objPar)
        If Not blnTmp Then ErrorInfo("出錯799-" & CStr(GetLastError) & "，程式即將結束!") : Exit Sub
        '======================================================================================
        Thread_Hook.IsBackground = True
        Thread_bloodcat.IsBackground = True
        objIniWebClient.ConnectionLimit = intConnLimit
    End Sub

    Private Sub MainUI_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Static blnRun As Boolean = False
        If blnRun Then Exit Sub
        blnRun = True
        blnMainRun = True
        '======================================================================================
        Dim blnFocus As Boolean
        Dim hwndTmp As Integer
        Const constPosFlags As Integer = SetWindowPosFlags.IgnoreMove Or SetWindowPosFlags.IgnoreResize Or SetWindowPosFlags.IgnoreZOrder
        Dim intTmp As SetWindowPosFlags
        Dim intTmp2 As Integer
        Dim np, op As _POINT
        op.x = -1 : op.y = -1
        '=====================================
        Dim datNow As Date
        Dim ci As Integer
        Dim intLevel(0 To intConnLimit - 1) As Integer
        Dim intStatus As FileDownloader._Status
        Dim intDivider As Integer
        Dim blnVisble As Boolean
        '=====================================
        Dim blnIni As Boolean
        Dim intIniLevel As Integer
        Dim objDE As New DownloadEvent
        '===============================================================
        ShowWindowAsync(OSU_hdlHWND, SW.Restore)
        SetWindowPos(OSU_hdlHWND, 0, 0, 0, 0, 0, SetWindowPosFlags.IgnoreMove Or SetWindowPosFlags.IgnoreResize)
        SetForegroundWindow(OSU_hdlHWND)
        blnFocus = True
        Sleep(100)

        np.x = 0 : np.x = 0 : ClientToScreen(OSU_hdlHWND, np)
        SetWindowPos(hwndSELF, -1, np.x, np.x, 0, 0, SetWindowPosFlags.IgnoreResize)
        blnVisble = True
        intCaseCount = 1

        Do Until blnExit Or blnEndOSU
            If intCaseCount <> 0 Then
                datNow = Date.Now
                '===========================================
                np.x = 0 : np.y = 0 : ClientToScreen(OSU_hdlHWND, np)
                hwndTmp = GetForegroundWindow()
                intTmp = SetWindowPosFlags.IgnoreResize
                intTmp2 = 0
                '===========================================
                If (op.x = np.x AndAlso op.y = np.y) Then
                    intTmp = intTmp Or SetWindowPosFlags.IgnoreMove
                Else
                    op = np
                End If

                If hwndTmp = OSU_hdlHWND Then
                    If blnFocus Then
                        intTmp = intTmp Or SetWindowPosFlags.IgnoreZOrder
                    Else
                        blnFocus = True
                        intTmp2 = -1
                    End If
                Else
                    If hwndTmp = hwndSELF Then
                        SetForegroundWindow(OSU_hdlHWND)
                    Else
                        If blnFocus Then
                            blnFocus = False
                            intTmp2 = -2
                        Else
                            intTmp = intTmp Or SetWindowPosFlags.IgnoreZOrder
                        End If
                    End If
                End If

                If intTmp <> constPosFlags Then
                    SetWindowPos(hwndSELF, intTmp2, np.x, np.y, 0, 0, intTmp)
                End If
                '===========================================
                If intTmp2 = -2 Then
                    SetWindowPos(hwndTmp, 0, 0, 0, 0, 0, SetWindowPosFlags.IgnoreResize Or SetWindowPosFlags.IgnoreMove)
                End If
                '===================================================================================
                If intDivider = 0 Then
                    If Not blnIni Then
                        '===================================================================================
                        Select Case intIniLevel
                            Case 0
                                blnBarVisble(0) = True
                                strBarText(0) = "Initialize ..."
                                dblBarValue(0) = 0.0#
                                datBarDate(0) = Date.MaxValue

                                intIniStatus = InitializeStatus.None
                                objDE.EventType = DownloadEventType.DownloadInitialize
                                Queue_bloodcat.Enqueue(objDE)
                                objDE = Nothing
                                Thread_bloodcat.Start()
                                intIniLevel = 1
                            Case 1
                                If intIniStatus <> InitializeStatus.None Then
                                    If intIniStatus = InitializeStatus.Succeed Then
                                        datBarDate(0) = Date.Now.AddSeconds(7)
                                        strBarText(0) = "osu!  DMap v" & My.Application.Info.Version.ToString
                                        dblBarValue(0) = 1.0#
                                        datLastConnection = Date.Now
                                    Else
                                        datBarDate(0) = Date.Now.AddSeconds(15)
                                        strBarText(0) = "'bloodcat.com'  Not Found"
                                    End If
                                    intLevel(0) = 7
                                    Thread_Hook.Start()
                                    blnIni = True
                                End If
                        End Select
                        '===================================================================================
                    Else
                        '===================================================================================
                        For ci = 0 To intConnLimit - 1
                            If datNow > datBarDate(ci) Then
                                If blnBarVisble(ci) Then
                                    blnBarVisble(ci) = False
                                    datBarDate(ci) = Date.MinValue
                                    intLevel(ci) = 0
                                    intCaseCount -= 1
                                    GC.Collect()
                                End If
                            Else
                                intStatus = objDownloader(ci).Status
                                Select Case intLevel(ci)
                                    Case 0 'Initialization
                                        blnBarVisble(ci) = True
                                        strBarText(ci) = "Search ..."
                                        dblBarValue(ci) = 0.0#
                                        intLevel(ci) = 1
                                    Case 1 'Search
                                        If intTaskStatus(ci) <> TaskStatus.None Then
                                            Select Case intTaskStatus(ci)
                                                Case TaskStatus.Succeed
                                                    strBarText(ci) = "Connection ..."
                                                    intLevel(ci) = 2
                                                Case TaskStatus.NotFound
                                                    intLevel(ci) = 5
                                                Case TaskStatus.Fail
                                                    intLevel(ci) = 6
                                            End Select
                                        End If
                                    Case 2 'Connection
                                        If intStatus = FileDownloader._Status.Download Then
                                            Dim strNameTmp As String = objDownloader(ci).FileName
                                            strNameTmp = strNameTmp.Substring(strNameTmp.IndexOf(" "c) + 1).Replace(".osz", "")
                                            strNameTmp = "(" & CStr(CInt(objDownloader(ci).FileSize / 104857.6#) / 10) & "MB)  " & strNameTmp
                                            If blnCopyUrl Then
                                                Try
                                                    My.Computer.Clipboard.Clear()
                                                    My.Computer.Clipboard.SetText(objDownloader(ci).URL)
                                                Catch ex As Exception
                                                    strNameTmp = "[Clipboard Error] " & strNameTmp
                                                End Try
                                            End If
                                            strBarText(ci) = strNameTmp
                                            intLevel(ci) = 3
                                        ElseIf intStatus <> FileDownloader._Status.Connection Then
                                            If intStatus = FileDownloader._Status.Complete Then
                                                intLevel(ci) = 4
                                            ElseIf intStatus = FileDownloader._Status.NotFound Then
                                                intLevel(ci) = 5
                                            Else
                                                intLevel(ci) = 6
                                            End If
                                        End If
                                    Case 3 'Download
                                        If intStatus = FileDownloader._Status.Download Then
                                            dblBarValue(ci) = objDownloader(ci).Progress
                                        Else
                                            If intStatus = FileDownloader._Status.Complete Then
                                                intLevel(ci) = 4
                                            Else
                                                intLevel(ci) = 6
                                            End If
                                        End If
                                    Case 4 'Complete
                                        dblBarValue(ci) = 1.0#
                                        If blnOpenFile Then
                                            strBarText(ci) = "ok!  -  Wait..."
                                        Else
                                            strBarText(ci) = "ok!"
                                        End If
                                        If blnDownloadEnd(ci) Then
                                            If blnOpenFile Then
                                                strBarText(ci) = "ok!  -  Import..."
                                                datBarDate(ci) = Date.Now.AddSeconds(1.0#)
                                            Else
                                                datBarDate(ci) = Date.Now.AddSeconds(2.0#)
                                            End If
                                            intLevel(ci) = 7 'End
                                        End If
                                    Case 5 'Not Found
                                        strBarText(ci) = "Not Found"
                                        datBarDate(ci) = Date.Now.AddSeconds(7)
                                        intLevel(ci) = 7 'End
                                    Case 6 'Download Error
                                        strBarText(ci) = "Download Error"
                                        datBarDate(ci) = Date.Now.AddSeconds(7)
                                        intLevel(ci) = 7 'End
                                End Select
                            End If
                        Next
                        '===================================================================================
                    End If
                    DrawProgress()
                End If
                intDivider += 1
                If intDivider >= 3 Then intDivider = 0
                If Not blnVisble Then
                    blnVisble = True
                    Me.Visible = True
                End If
            Else
                If blnVisble Then
                    blnVisble = False
                    Me.Visible = False
                End If
            End If
            '===========================================
            Application.DoEvents()
            Sleep(27)
        Loop
ExitShow:
        blnMainRun = False
        If blnEndOSU Then
            ExitSub()
        End If
    End Sub

    Private Sub DrawProgress()
        Static objRectAll As New Rectangle(0, 0, intBarSizeX + intBarInterval, (intBarSizeY + intBarInterval) * intConnLimit)
        Static intCount As Integer

        Dim bmpBuff As New Bitmap(intBarSizeX + intBarInterval, (intBarSizeY + intBarInterval) * intConnLimit)
        Dim objBuff As Graphics = Graphics.FromImage(bmpBuff)
        Dim objBrush As Brush = New SolidBrush(objBarTextColor)

        objBuff.Clear(Color.Transparent)
        For ci As Integer = 0 To intConnLimit - 1
            If blnBarVisble(ci) Then
                objBuff.DrawImage(imgBG, _
                                  CInt(intBarSizeX * dblBarValue(ci) + intBarInterval), _
                                  CInt((intBarSizeY + intBarInterval) * ci + intBarInterval), _
                                  New Rectangle(CInt(intBarSizeX * dblBarValue(ci)), _
                                                0, _
                                                CInt(intBarSizeX * (1.0# - dblBarValue(ci))), _
                                                intBarSizeY), _
                                  GraphicsUnit.Pixel)

                objBuff.DrawImage(imgLG, _
                                  intBarInterval, _
                                  CInt((intBarSizeY + intBarInterval) * ci + intBarInterval), _
                                  New Rectangle(0, _
                                                0, _
                                                CInt(intBarSizeX * dblBarValue(ci)), _
                                                intBarSizeY), _
                                  GraphicsUnit.Pixel)

                Dim sz As SizeF = objBuff.MeasureString(strBarText(ci), objBarFont)
                Dim x, y As Single
                x = (intBarSizeX - sz.Width) / 2.0! + 0.5F : If x < 0 Then x = 0
                y = (intBarSizeY - sz.Height) / 2.0! + 0.5F + (intBarSizeY + intBarInterval) * ci
                objBuff.DrawString(strBarText(ci), objBarFont, objBrush, x + intBarInterval, y + intBarInterval)
            End If
        Next

        Dim objTmp As Image = objPictureBox.Image
        objPictureBox.Image = bmpBuff
        If objTmp IsNot Nothing Then objTmp.Dispose()
        objBrush.Dispose()
        objBuff.Dispose()

        intCount += 1
        If intCount >= 30 Then
            intCount = 0
            GC.Collect()
        End If
    End Sub

    Public Sub ResetValue()
        MainUI.WriteProcessMemory(HOOK_hdlProcess, HOOK_hdlVar + 4, CType(blnHookMessage, Integer), 4, 0)
        For i As Integer = 0 To MainUI.intConnLimit - 1
            Dim objFontTmp As Font = objBarFont
            objBarFont = New Font("Arial", CInt((intBarSizeY - 15) / 3 + 10), FontStyle.Bold)
            objFontTmp.Dispose()
        Next
    End Sub

    Private Sub Disp_ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Disp_ToolStripMenuItem.Click
        SetUI.Show()
    End Sub

    Private Sub Exit_ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Exit_ToolStripMenuItem.Click
        ExitSub()
    End Sub

    Private Sub NotifyIcon1_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles NotifyIcon1.MouseClick
        If e.Button = Windows.Forms.MouseButtons.Left Then
            SetUI.Show()
        End If
    End Sub

    Private Sub ErrorInfo(ByVal info As String)
        MessageBox.Show(info, "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ExitSub()
    End Sub

    Private Sub ExitSub()
        blnExit = True

        If Thread_bloodcat IsNot Nothing Then
            Do While Thread_bloodcat.IsAlive
                Sleep(20)
            Loop
        End If

        If Thread_Hook IsNot Nothing Then
            Do While Thread_Hook.IsAlive
                Sleep(20)
            Loop
        End If

        If HOOK_hdlVar <> 0 Then VirtualFreeEx(HOOK_hdlProcess, HOOK_hdlVar, 0, FreeType.Release)
        If HOOK_hdlFunc <> 0 Then VirtualFreeEx(HOOK_hdlProcess, HOOK_hdlFunc, 0, FreeType.Release)
        If HOOK_hdlProcess <> 0 Then CloseHandle(HOOK_hdlProcess)

        For ci As Integer = 0 To intConnLimit - 1
            If objDownloader(ci) IsNot Nothing Then
                If objDownloader(ci).IsBusy() Then
                    objDownloader(ci).Stop()
                End If
                Do While objDownloader(ci).IsBusy
                    Sleep(20)
                Loop
            End If
        Next

        If OSU_objProc IsNot Nothing Then
            OSU_objProc.Close()
            OSU_objProc.Dispose()
        End If

        If objIniWebClient IsNot Nothing Then objIniWebClient.Dispose()

        Me.Close()
    End Sub

    '▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄

    Private Sub HookLoop()
        Dim arrByteHook(0 To 6) As Byte
        '===========================================
        arrByteHook(0) = &HE9 'jmp near ptr [HOOK_hdlFunc]
        WriteArray(arrByteHook, 1, HOOK_hdlFunc - HOOK_intHookAddr)
        arrByteHook(5) = &HEB 'jmp short -7
        arrByteHook(6) = &HF9
        WriteProcessMemory(HOOK_hdlProcess, HOOK_intHookAddr - 5, arrByteHook, arrByteHook.Length, 0)
        '===========================================
        Dim blnHook As Integer
        Dim strHttpLink As String = Nothing
        Dim ci As Integer
        Dim objEvent As DownloadEvent
        Dim objParam As DownloadStartParam
        Dim datTmp As Date

        Do
            If Not ReadProcessMemory(HOOK_hdlProcess, HOOK_hdlVar + 8, blnHook, 4, 0) Then
                blnEndOSU = True
                Exit Sub
            End If
            If blnHook <> 0 Then
                ReadProcessMemoryEx(HOOK_hdlProcess, HOOK_hdlVar + 36 + MAX_PATH * 12, strHttpLink, CharType.Unicode)
                WriteProcessMemory(HOOK_hdlProcess, HOOK_hdlVar + 8, 0, 4, 0)
                datTmp = Date.Now
                For ci = 0 To intConnLimit - 1
                    If datBarDate(ci) <> Date.MinValue AndAlso (strHttpLink = strUrlRegister(ci)) Then Exit For
                Next
                If ci >= intConnLimit Then
                    For ci = 0 To intConnLimit - 1
                        If datBarDate(ci) = Date.MinValue Then
                            strUrlRegister(ci) = strHttpLink

                            objEvent = New DownloadEvent
                            objParam = New DownloadStartParam
                            objEvent.EventType = DownloadEventType.DownloadStart
                            objParam.ConnectionIndex = ci
                            objParam.URL = strHttpLink
                            objEvent.Parameter = objParam
                            Queue_bloodcat.Enqueue(objEvent)
                            intCaseCount += 1
                            datBarDate(ci) = Date.MaxValue
                            intTaskStatus(ci) = TaskStatus.None
                            blnDownloadEnd(ci) = False
                            objEvent = Nothing
                            objParam = Nothing

                            Exit For
                        End If
                    Next
                End If
            End If
            Sleep(30)
        Loop Until blnExit
        '===========================================
        arrByteHook(0) = &H0 'nop
        arrByteHook(1) = &H0 'nop
        arrByteHook(2) = &H0 'nop
        arrByteHook(3) = &H0 'nop
        arrByteHook(4) = &H0 'nop
        arrByteHook(5) = &H8B 'mov edi,edi
        arrByteHook(6) = &HFF
        WriteProcessMemory(HOOK_hdlProcess, HOOK_intHookAddr - 5, arrByteHook, arrByteHook.Length, 0)
        '===========================================
    End Sub

    '▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄

#Region "[Enum] [Structure] Declare"

    Enum InitializeStatus As Integer
        None = 0
        Succeed = 1
        Fail = 2
    End Enum

    Enum TaskStatus As Integer
        None = 0
        NotFound = 1
        Succeed = 2
        Fail = 3
    End Enum

    Enum UrlType As Integer
        osu_d = 0
        osu_s = 1
        bloodcat = 2
    End Enum

    Enum DownloadEventType As Integer
        DownloadStart = 0
        DownloadEnd = 1
        DownloadInitialize = 2
    End Enum

    Structure DownloadEvent
        Public EventType As DownloadEventType
        Public Parameter As Object
    End Structure

    Structure DownloadStartParam
        Public ConnectionIndex As Integer
        Public URL As String
    End Structure

#End Region

    Dim Queue_bloodcat As New Queue

    Private Sub TaskLoop_bloodcat()
        Dim objEventTmp As DownloadEvent
        Dim objParam As DownloadStartParam

        Do Until blnExit
            If Queue_bloodcat.Count <> 0 Then
                objEventTmp = DirectCast(Queue_bloodcat.Dequeue, DownloadEvent)

                If objEventTmp.EventType = DownloadEventType.DownloadStart Then
                    objParam = DirectCast(objEventTmp.Parameter, DownloadStartParam)
                    Call DownloadStartSub(objParam.ConnectionIndex, objParam.URL)
                ElseIf objEventTmp.EventType = DownloadEventType.DownloadInitialize Then
                    Call DownloadInitializeSub()
                End If

                objEventTmp = Nothing
                objParam = Nothing
            End If
            Sleep(30)
        Loop
    End Sub

    Private Sub DownloadStartSub(ByVal ci As Integer, ByVal URL As String)
        If DateDiff(DateInterval.Second, Date.Now, datLastConnection) > 200 Then
            DownloadInitializeSub()
            If blnExit Then Exit Sub
            If intIniStatus = InitializeStatus.Succeed Then
                datLastConnection = Date.Now
            Else
                intTaskStatus(ci) = TaskStatus.NotFound
                If blnOpenHttp Then Process.Start(strUrlRegister(ci))
                Exit Sub
            End If
        End If

        Dim strUrl As String = UrlTransform(URL, UrlType.bloodcat)
        If strUrl Is Nothing Then
            intTaskStatus(ci) = TaskStatus.NotFound
            If blnOpenHttp Then Process.Start(strUrlRegister(ci))
            Exit Sub
        End If

        objDownloader(ci).CookieContainer = objCookieContainer
        If objDownloader(ci).DownloadFile(URL:=strUrl, Path:=strPath, userToken:=ci) Then
            intTaskStatus(ci) = TaskStatus.Succeed
        Else
            intTaskStatus(ci) = TaskStatus.Fail
        End If
    End Sub

    Private Sub DownloadInitializeSub()
        Dim strHTML As String
        For c As Integer = 1 To 3
            If blnExit Then Exit Sub
            Try
                strHTML = objIniWebClient.DownloadString("http://bloodcat.com/osu/") '先連線到 http://bloodcat.com/osu/ 防止下載錯誤
                If strHTML IsNot Nothing Then
                    If strHTML <> "" Then
                        intIniStatus = InitializeStatus.Succeed
                        Exit Sub
                    End If
                End If
            Catch ex As Exception
                Sleep(300)
            End Try
        Next
        intIniStatus = InitializeStatus.Fail
    End Sub

    Private Function UrlTransform(ByVal URL As String, ByVal type As UrlType) As String
        Dim i, j As Integer
        Dim UrlList(0 To 3) As String
        Dim strSerial As String

        URL = URL.ToLower
        If URL.IndexOf("http") <> 0 Then Return Nothing

        UrlList(0) = "osu.ppy.sh/d/"
        UrlList(1) = "osu.ppy.sh/s/"
        UrlList(2) = "bloodcat.com/osu/m/"
        UrlList(3) = "osu.ppy.sh/b/"

        For i = 0 To 2
            j = URL.IndexOf(UrlList(i))
            If j <> -1 Then
                If type = 2 Then
                    If URL.IndexOf("https://") <> -1 Then
                        URL = URL.Replace("https://", "http://")
                        j -= 1
                    End If
                End If
                strSerial = Val(URL.Substring(j + UrlList(i).Length)).ToString()
                If strSerial <> "0" Then
                    Return URL.Substring(0, j) & UrlList(type) & strSerial
                Else
                    Return Nothing
                End If
            End If
        Next

        j = URL.IndexOf(UrlList(3))
        If j <> -1 Then
            strSerial = Val(URL.Substring(j + UrlList(3).Length)).ToString()
            If strSerial = "0" Then Return Nothing

            Dim objWebClient As New WebClientPlus
            Dim i0, i1 As Integer
            Dim strTmp As String
            objWebClient.ConnectionLimit = intConnLimit

            If type = UrlType.bloodcat Then
                Try
                    strTmp = objWebClient.DownloadString("http://bloodcat.com/osu/?q=" & strSerial)
                Catch ex As Exception
                    GoTo ErrorExit
                End Try
                If strTmp Is Nothing Then GoTo ErrorExit
                i0 = strTmp.IndexOf("<a class=""title"" href=""")
                If i0 = -1 Then GoTo ErrorExit
                i0 += 23
                i0 += 2
                'i0 = strTmp.IndexOf("<div id=""beatmaps"">")
                'If i0 = -1 Then GoTo ErrorExit
                'i0 += 19
                'i1 = strTmp.IndexOf("<div class=""empty"">", i0)
                'If i1 <> -1 Then GoTo ErrorExit
                'i0 = strTmp.IndexOf("data-id=""", i0)
                'If i0 = -1 Then GoTo ErrorExit
                'i0 = i0 + 9
                'i1 = strTmp.IndexOf(""""c, i0)
                'If i1 = -1 Then GoTo ErrorExit
            Else
                Try
                    strTmp = objWebClient.DownloadString(URL)
                Catch ex As Exception
                    GoTo ErrorExit
                End Try
                If strTmp Is Nothing Then GoTo ErrorExit
                i0 = strTmp.IndexOf("content-with-bg")
                If i0 = -1 Then GoTo ErrorExit
                i0 = strTmp.IndexOf("data:", i0 + 15)
                If i0 = -1 Then GoTo ErrorExit
                i0 = strTmp.IndexOf(""""c, i0 + 5)
                If i0 = -1 Then GoTo ErrorExit
                i0 = i0 + 1
            End If
            i1 = strTmp.IndexOf(""""c, i0)
            If i1 = -1 Then GoTo ErrorExit
            strSerial = strTmp.Substring(i0, i1 - i0)

            objWebClient.Dispose()
            objWebClient = Nothing
            Return URL.Substring(0, j) & UrlList(type) & strSerial
ErrorExit:
            objWebClient.Dispose()
            objWebClient = Nothing
        End If
        Return Nothing
    End Function

    '▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄

    Private Sub DownloadEndEvent(ByVal FullName As String, ByVal Status As FileDownloader._Status, ByVal userToken As Object)
        Static blnRun As Boolean = False
        Do While blnRun
            Sleep(20)
        Loop
        blnRun = True
        '======================================================
        Dim ci As Integer = CInt(userToken)
        blnDownloadEnd(ci) = True
        If Status = FileDownloader._Status.Complete Then
            datLastConnection = Date.Now
            If blnOpenFile Then
                Process.Start(FullName)
                For c As Integer = 1 To 50
                    Sleep(50)
                    If blnExit Then Exit For
                Next
            End If
        Else
            If FullName IsNot Nothing Then
                Dim objFileInfo As New IO.FileInfo(FullName)
                If objFileInfo.Exists Then
                    objFileInfo.Delete()
                End If
            End If
            datLastConnection = Date.MinValue
            If blnOpenHttp Then
                Process.Start(strUrlRegister(ci))
            End If
        End If
        '======================================================
        blnRun = False
    End Sub

    '█████████████████████████████████████████████████████████

#Region "[Funtion] [Sub]"

    Private Function GetModuleHandleEx(ByVal ProcessID As Integer, ByVal ModuleName As String) As Integer
        Dim hdlSnapshot As Integer
        Dim objModEntry As New MODULEENTRY32
        Dim blnTmp As Boolean = False

        hdlSnapshot = CreateToolhelp32Snapshot(SnapshotFlags.TH32CS_SNAPMODULE, ProcessID)
        If hdlSnapshot = -1 Then GetModuleHandleEx = 0 : Exit Function

        objModEntry.dwSize = Marshal.SizeOf(objModEntry)
        If Module32First(hdlSnapshot, objModEntry) Then
            Do
                If objModEntry.szModule = ModuleName Then
                    GetModuleHandleEx = objModEntry.modBaseAddr
                    blnTmp = True
                    Exit Do
                End If
            Loop While Module32Next(hdlSnapshot, objModEntry)
        End If
        If Not blnTmp Then GetModuleHandleEx = 0

        CloseHandle(hdlSnapshot)
    End Function

    Private Function GetProcAddressEx(ByVal hdlProcess As Integer, ByVal ModuleAddress As Integer, ByVal ProcName As String) As Integer
        Dim tmp_Address As Integer
        '===========================================
        Dim objDosHeader As New IMAGE_DOS_HEADER
        Dim objNtHeader As New IMAGE_NT_HEADERS
        Dim objExportDir As IMAGE_EXPORT_DIRECTORY

        tmp_Address = ModuleAddress
        objDosHeader = CType(ReadProcessMemoryEx(hdlProcess, tmp_Address, objDosHeader.GetType), IMAGE_DOS_HEADER)
        If CInt(objDosHeader.e_magic) <> &H5A4D Then Return 0 '"MZ" = &H5A4D

        tmp_Address = tmp_Address + objDosHeader.e_lfanew
        objNtHeader = CType(ReadProcessMemoryEx(hdlProcess, tmp_Address, objNtHeader.GetType), IMAGE_NT_HEADERS)
        If objNtHeader.Signature <> &H4550 Then Return 0 '"PE  " = &H00004550

        tmp_Address = ModuleAddress + objNtHeader.OptionalHeader.DataDirectory(0).VirtualAddress
        objExportDir = CType(ReadProcessMemoryEx(hdlProcess, tmp_Address, objExportDir.GetType), IMAGE_EXPORT_DIRECTORY)
        '===========================================
        Dim strFunName As String = Nothing
        Dim arrFuntionsRVA(0 To objExportDir.NumberOfFunctions - 1) As Integer
        Dim arrNamesRVA(0 To objExportDir.NumberOfNames - 1) As Integer
        Dim arrNameOrdinals(0 To objExportDir.NumberOfNames - 1) As Short

        tmp_Address = ModuleAddress + objExportDir.AddressOfFunctions
        ReadProcessMemory(hdlProcess, tmp_Address, arrFuntionsRVA, arrFuntionsRVA.Length * 4, 0)
        tmp_Address = ModuleAddress + objExportDir.AddressOfNames
        ReadProcessMemory(hdlProcess, tmp_Address, arrNamesRVA, arrNamesRVA.Length * 4, 0)
        tmp_Address = ModuleAddress + objExportDir.AddressOfNameOrdinals
        ReadProcessMemory(hdlProcess, tmp_Address, arrNameOrdinals, arrNameOrdinals.Length * 2, 0)

        Dim intC As Integer
        For intC = 0 To objExportDir.NumberOfNames - 1
            tmp_Address = ModuleAddress + arrNamesRVA(intC)
            ReadProcessMemoryEx(hdlProcess, tmp_Address, strFunName, CharType.Ansi)
            If String.Compare(strFunName, ProcName) = 0 Then Exit For
        Next
        If intC >= objExportDir.NumberOfNames Then Return 0
        Return arrFuntionsRVA(arrNameOrdinals(intC))
        '===========================================
    End Function

    Private Sub WriteArray(ByRef DestArray As Byte(), ByVal Index As Integer, ByVal value As Integer)
        Dim bytes As Byte() = BitConverter.GetBytes(value)
        For c = 0 To 3
            DestArray(Index + c) = bytes(c)
        Next
    End Sub

    Private Overloads Function ReadProcessMemoryEx(ByVal hdlProcess As Integer, ByVal Address As Integer, ByVal BufferType As Type) As Object
        Dim ObjPtr As IntPtr
        Dim BufferSize As Integer = Marshal.SizeOf(BufferType)
        ObjPtr = Marshal.AllocHGlobal(BufferSize)
        If ReadProcessMemory(hdlProcess, Address, ObjPtr, BufferSize, 0) Then
            ReadProcessMemoryEx = Marshal.PtrToStructure(ObjPtr, BufferType)
        Else
            ReadProcessMemoryEx = Nothing
        End If
        Marshal.FreeHGlobal(ObjPtr)
    End Function

    Private Overloads Function ReadProcessMemoryEx(ByVal hdlProcess As Integer, ByVal Address As Integer, ByRef Buffer As String, ByVal StrType As CharType) As Boolean
        Dim ObjPtr As IntPtr
        Dim BufferSize As Integer = MAX_PATH * StrType
        ObjPtr = Marshal.AllocHGlobal(BufferSize)
        If ReadProcessMemory(hdlProcess, Address, ObjPtr, BufferSize, 0) Then
            If StrType = CharType.Ansi Then
                Buffer = Marshal.PtrToStringAnsi(ObjPtr)
            ElseIf StrType = CharType.Unicode Then
                Buffer = Marshal.PtrToStringUni(ObjPtr)
            End If
            ReadProcessMemoryEx = True
        Else
            ReadProcessMemoryEx = False
        End If
        Marshal.FreeHGlobal(ObjPtr)
    End Function

    Private Overloads Function WriteProcessMemoryEx(ByVal hdlProcess As Integer, ByVal Address As Integer, ByVal Buffer As Object) As Boolean
        Dim ObjPtr As IntPtr
        Dim BufferSize As Integer = Marshal.SizeOf(Buffer)
        ObjPtr = Marshal.AllocHGlobal(BufferSize)
        Marshal.StructureToPtr(Buffer, ObjPtr, False)
        WriteProcessMemoryEx = WriteProcessMemory(hdlProcess, Address, ObjPtr, BufferSize, 0)
        Marshal.FreeHGlobal(ObjPtr)
    End Function

#End Region

End Class








'Public Class TextProgressBar '可以顯示文字的ProgressBar
'    Inherits ProgressBar

'    Private Const WM_PAINT As Integer = &HF
'    Private strText As String = ""
'    Private objTextColor As Color = Color.Black
'    Private objFont As New Font("Arial", 9)

'    Public Overrides Property Text() As String
'        Get
'            Return Me.strText
'        End Get
'        Set(ByVal value As String)
'            If String.Compare(Me.strText, value) <> 0 Then
'                Me.strText = value
'                Me.OnPrint(New PaintEventArgs(Me.CreateGraphics(), Me.ClientRectangle))
'                If Me.Visible Then
'                    Me.DrawText() '繪出文字
'                End If
'            End If
'        End Set
'    End Property

'    Public Overrides Property Font() As Font
'        Get
'            Return Me.objFont
'        End Get
'        Set(ByVal value As Font)
'            Me.objFont = value
'        End Set
'    End Property

'    Public Property TextColor() As Color
'        Get
'            Return Me.objTextColor
'        End Get
'        Set(ByVal value As Color)
'            Me.objTextColor = value
'        End Set
'    End Property

'    Protected Overloads Overrides Sub WndProc(ByRef m As Message)
'        MyBase.WndProc(m) '這行要先執行，否則看不到文字
'        If m.Msg = WM_PAINT Then
'            Me.DrawText() '繪出文字
'        End If
'    End Sub

'    Private Sub DrawText()
'        If (Me.strText IsNot Nothing) AndAlso (Me.strText <> "") Then
'            Dim g As Graphics = Me.CreateGraphics()
'            Dim rc As Rectangle = Me.ClientRectangle
'            Dim sz As SizeF = g.MeasureString(Me.strText, Me.objFont)
'            Dim x As Single = (CSng(rc.Width) - sz.Width) / 2 + 0.5F
'            If x < 0 Then x = 0
'            Dim y As Single = (CSng(rc.Height) - sz.Height) / 2 + 0.5F
'            g.DrawString(Me.strText, Me.objFont, New SolidBrush(Me.objTextColor), New PointF(x, y))
'        End If
'    End Sub
'End Class







Public Class WebClientPlus '可以改變最大連線數、逾時時間的WebClient
    Inherits WebClient

    Private intConnectionLimit As Integer = 2 '連線數
    Private intTimeout As Integer = 10000 '(毫秒為單位)

    Public Property ConnectionLimit() As Integer
        Get
            Return intConnectionLimit
        End Get
        Set(ByVal value As Integer)
            intConnectionLimit = value
        End Set
    End Property

    Public Property Timeout() As Integer
        Get
            Return intTimeout
        End Get
        Set(ByVal value As Integer)
            intTimeout = value
        End Set
    End Property

    Protected Overrides Function GetWebRequest(ByVal address As Uri) As WebRequest
        Dim req As HttpWebRequest = DirectCast(MyBase.GetWebRequest(address), HttpWebRequest)
        req.ServicePoint.ConnectionLimit = intConnectionLimit
        req.Timeout = intTimeout
        Return DirectCast(req, WebRequest)
    End Function
End Class







Public Class FileDownloader

    Public Enum _Status As Integer
        Complete = 0
        Connection = 1
        Download = 2
        NotFound = 3
        Timeout = 4
        [Error] = 5
        [stop] = 6
    End Enum

    Private trdDownloadThread As Thread
    Private intConnectionLimit As Integer = 2 '連線數
    Private intTimeout As Integer = 10000 '(毫秒為單位)
    Private objCookieContainer As CookieContainer
    Private blnAutoRedirect As Boolean = True
    Private strFullName As String
    Private strFileName As String
    Private strPath As String
    Private strUrl As String
    Private lngFileSize As Long
    Private intBufferSize As Integer = 4096
    Private dblProgress As Double = 0.0#
    Private blnStop As Boolean = False
    Private intStatus As _Status = _Status.Complete
    Private objUserToken As Object

    Public Event DownloadStart(ByVal userToken As Object)
    Public Event DownloadEnd(ByVal FullName As String, ByVal Status As _Status, ByVal userToken As Object)

    Public Property ConnectionLimit() As Integer
        Get
            Return Me.intConnectionLimit
        End Get
        Set(ByVal value As Integer)
            Me.intConnectionLimit = value
        End Set
    End Property

    Public Property Timeout() As Integer
        Get
            Return Me.intTimeout
        End Get
        Set(ByVal value As Integer)
            Me.intTimeout = value
        End Set
    End Property

    Public Property CookieContainer() As CookieContainer
        Get
            Return Me.objCookieContainer
        End Get
        Set(ByVal value As CookieContainer)
            Me.objCookieContainer = value
        End Set
    End Property

    Public Property AutoRedirect() As Boolean
        Get
            Return Me.blnAutoRedirect
        End Get
        Set(ByVal value As Boolean)
            Me.blnAutoRedirect = value
        End Set
    End Property

    Public Property BufferSize() As Integer
        Get
            Return Me.intBufferSize
        End Get
        Set(ByVal value As Integer)
            Me.intBufferSize = value
        End Set
    End Property

    Public ReadOnly Property FullName() As String
        Get
            Return Me.strFullName
        End Get
    End Property

    Public ReadOnly Property FileName() As String
        Get
            Return Me.strFileName
        End Get
    End Property

    Public ReadOnly Property URL() As String
        Get
            Return Me.strUrl
        End Get
    End Property

    Public ReadOnly Property FileSize() As Long
        Get
            Return Me.lngFileSize
        End Get
    End Property

    Public ReadOnly Property Progress() As Double
        Get
            Return Me.dblProgress
        End Get
    End Property

    Public ReadOnly Property Status() As _Status
        Get
            Return Me.intStatus
        End Get
    End Property

    Public ReadOnly Property IsBusy() As Boolean
        Get
            Return Me.trdDownloadThread IsNot Nothing
        End Get
    End Property

    Public Sub [Stop]()
        Me.blnStop = True
    End Sub

    Public Function DownloadFile(ByVal URL As String, ByVal Path As String, Optional ByVal FileName As String = Nothing, Optional ByVal userToken As Object = Nothing) As Boolean
        If Me.IsBusy Then Return False
        Me.strUrl = URL
        Me.strPath = Path
        Me.strFileName = FileName
        Me.strFullName = Nothing
        Me.objUserToken = userToken
        '====================================================
        RaiseEvent DownloadStart(Me.objUserToken)
        Me.trdDownloadThread = New Thread(AddressOf Me.DownloadSub)
        Me.trdDownloadThread.IsBackground = True
        Me.blnStop = False
        Me.intStatus = _Status.Connection
        Me.dblProgress = 0.0#
        Me.trdDownloadThread.Start()
        Return True
    End Function

    Private Sub DownloadSub()
        Dim objHttpRequest As HttpWebRequest
        Dim objWebResponse As WebResponse
        Dim strHeader As String
        Dim i0, i1 As Integer

        objHttpRequest = DirectCast(WebRequest.Create(Me.strUrl), HttpWebRequest)

        objHttpRequest.AllowAutoRedirect = Me.blnAutoRedirect
        objHttpRequest.ServicePoint.ConnectionLimit = Me.intConnectionLimit
        objHttpRequest.CookieContainer = Me.objCookieContainer
        objHttpRequest.Timeout = Me.intTimeout
        Try
            objWebResponse = objHttpRequest.GetResponse
        Catch ex As Exception
            'MessageBox.Show("01 " & ex.Message)
            Me.intStatus = _Status.Timeout : GoTo ExitDownload2
        End Try
        Me.lngFileSize = objWebResponse.ContentLength

        'If Me.lngFileSize <= 0& Then MessageBox.Show("02 File Size") : Me.intStatus = _Status.Error : GoTo ExitDownload
        If Me.lngFileSize <= 0& Then Me.intStatus = _Status.Error : GoTo ExitDownload
        '===============================================================
        If Me.strFileName Is Nothing Then
            strHeader = objWebResponse.Headers.Item("Content-Disposition")
            'If strHeader Is Nothing Then MessageBox.Show("03 Nothing") : Me.intStatus = _Status.NotFound : GoTo ExitDownload
            If strHeader Is Nothing Then Me.intStatus = _Status.NotFound : GoTo ExitDownload

            i0 = strHeader.IndexOf("filename=""")
            'If i0 = -1 Then MessageBox.Show("04 " & strHeader) : Me.intStatus = _Status.Error : GoTo ExitDownload
            If i0 = -1 Then Me.intStatus = _Status.Error : GoTo ExitDownload
            i0 += 10
            i1 = strHeader.IndexOf(""""c, i0)
            Me.strFileName = strHeader.Substring(i0, i1 - i0)
            For Each c As Char In IO.Path.GetInvalidFileNameChars
                Me.strFileName = strFileName.Replace(c, "_"c)
            Next
        End If
        Me.strFullName = My.Computer.FileSystem.CombinePath(strPath, strFileName)
        '===============================================================
        Dim objSourceStream As Stream = objWebResponse.GetResponseStream
        Dim objSaveStream As Stream = New FileStream(Me.strFullName, FileMode.Create)
        Dim lngCompleteLength As Long = 0
        Dim bytBuffer(Me.intBufferSize) As Byte
        Dim intReadLength As Integer

        Me.intStatus = _Status.Download
        objSourceStream.ReadTimeout = Me.intTimeout
        Try
            Do
                intReadLength = objSourceStream.Read(bytBuffer, 0, Me.intBufferSize)
                If intReadLength <= 0 Then
                    If lngCompleteLength = Me.lngFileSize Then
                        Me.intStatus = _Status.Complete
                    Else
                        'MessageBox.Show("05 Timeout")
                        Me.intStatus = _Status.Timeout
                    End If
                    Exit Do
                End If
                objSaveStream.Write(bytBuffer, 0, intReadLength)
                lngCompleteLength += intReadLength
                Me.dblProgress = lngCompleteLength / Me.lngFileSize
                'If Me.blnStop Then MessageBox.Show("06 stop") : Me.intStatus = _Status.stop : Exit Do
                If Me.blnStop Then Me.intStatus = _Status.stop : Exit Do
            Loop
        Catch ex As Exception
            MessageBox.Show("07 " & ex.Message)
            Me.intStatus = _Status.Error
        End Try

        objSourceStream.Close()
        objSourceStream.Dispose()
        objSaveStream.Close()
        objSaveStream.Dispose()
        '===============================================================
ExitDownload:
        objHttpRequest.Abort()
        objWebResponse.Close()
ExitDownload2:
        Me.trdDownloadThread = Nothing
        RaiseEvent DownloadEnd(Me.strFullName, Me.intStatus, Me.objUserToken)
    End Sub
End Class