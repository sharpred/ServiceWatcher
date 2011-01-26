Imports System
Imports System.ServiceProcess
Imports System.Diagnostics
Imports System.Threading

Module Watcher

    Sub Main()

        Dim ServiceName As String = My.Settings.TargetService
        Dim serviceaction As String = LCase(My.Settings.ServiceAction)
        Dim inspection As String = InspectService(ServiceName, serviceaction)

        Console.Write(inspection)

    End Sub

    Function InspectService(ByVal servicename As String, ByVal serviceaction As String) As String

        Dim scServices() As ServiceController
        scServices = ServiceController.GetServices()
        Dim serviceFound As Boolean = False
        Dim scTemp As ServiceController
        Dim servicestatus As String = Nothing
        Dim timeout As New TimeSpan(5000)
        Try
            For Each scTemp In scServices
                If scTemp.ServiceName = servicename Then
                    serviceFound = True
                    Dim sc As New ServiceController(servicename)
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
                End If
            Next
            If serviceFound = False Then
                Throw New Exception("Service " & servicename & " is not installed on this machine")
            End If
        Catch timeex As System.ServiceProcess.TimeoutException
            Return "wait for status on startup errored with service status: " & servicestatus & " and error: " & timeex.Message
        Catch ex As Exception
            Return "operation failed with service status: " & servicestatus & " and error: " & ex.Message
        End Try
        Return servicestatus
    End Function

End Module
