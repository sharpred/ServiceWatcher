Imports System
Imports System.ServiceProcess
Imports System.Diagnostics
Imports System.Threading
Imports log4net

Module Watcher

    Sub Main()

        Dim ServiceName As String = My.Settings.TargetService
        Dim serviceaction As String = LCase(My.Settings.ServiceAction)
        Dim inspection As String = InspectService(ServiceName, serviceaction)

    End Sub

    Function InspectService(ByVal servicename As String, ByVal serviceaction As String) As String

        Dim scServices() As ServiceController
        scServices = ServiceController.GetServices()
        Dim serviceFound As Boolean = False
        Dim scTemp As ServiceController
        Dim servicestatus As String = Nothing
        Dim timeout As New TimeSpan(My.Settings.WaitValue)
        Dim log As log4net.ILog
        Dim errormessage As String = Nothing
        Dim sc As New ServiceController
        log4net.Config.XmlConfigurator.Configure()
        log = log4net.LogManager.GetLogger("ServiceWatcher")
        Try
            For Each scTemp In scServices
                If scTemp.ServiceName = servicename Then
                    serviceFound = True
                    sc = New ServiceController(servicename)
                    Select Case serviceaction
                        Case "inspect"
                        Case "activate"
                            If sc.Status = ServiceControllerStatus.Stopped Then
                                sc.Start()
                                sc.WaitForStatus(ServiceControllerStatus.Running, timeout)
                            End If
                        Case "deactivate"
                            If sc.Status = ServiceControllerStatus.Running Then
                                sc.Stop()
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout)
                            End If
                        Case Else
                            Throw New Exception("unknown service action defined, use 'inspect' 'deactivate' or 'activate'")
                    End Select
                    servicestatus = sc.Status.ToString()
                    log.Info("Service " & servicename & " status is " & servicestatus)
                End If
            Next
            If serviceFound = False Then
                errormessage = "Service " & servicename & " is not installed on this machine"
                Throw New Exception(errormessage)
            End If
        Catch timeex As System.ServiceProcess.TimeoutException
            servicestatus = sc.Status.ToString()
            errormessage = "wait for status on startup errored for service: " & servicename & " status: " & servicestatus & " and error: " & timeex.Message
            log.Warn(errormessage)
            Return errormessage
        Catch ex As Exception
            servicestatus = sc.Status.ToString()
            errormessage = "operation failed with service status: " & servicestatus & " and error: " & ex.Message
            log.Error(errormessage)
            Return errormessage
        End Try
        Return servicestatus
    End Function

End Module
