Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing

Public Module RouteConfig
    Public Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.LowercaseUrls = False
        routes.AppendTrailingSlash = True
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        'routes.IgnoreRoute("")

        'routes.MapRoute(
        '    name:="noUrl",
        '    url:="",

        '    )
        routes.MapRoute(
            name:="Default",
            url:="{controller}/{action}",
            defaults:=New With {.controller = "TC", .action = "Index"}
        )

    End Sub
End Module