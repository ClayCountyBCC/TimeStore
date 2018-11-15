Imports System.Web.Mvc
Imports System.Web.Http
Imports System.Web.Routing
Imports TimeStore.Models

Public Class MvcApplication
    Inherits System.Web.HttpApplication

  Protected Sub Application_Start()
    GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
    AreaRegistration.RegisterAllAreas()
    RouteConfig.RegisterRoutes(RouteTable.Routes)
    ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType(Of JsonValueProviderFactory).Single)
    ValueProviderFactories.Factories.Add(New JsonNetValueProviderFactory())
    Timecard_Access.Get_All_Cached_ReportsTo()
    Timecard_Access.Get_All_Cached_Access_List()

    'Timecard_Access.Get_All_Cached_Access_Dict()
  End Sub
End Class
