Imports System.Web.Mvc
Imports System.Web.Http
Imports System.Web.Routing

Public Class MvcApplication
    Inherits System.Web.HttpApplication

  Protected Sub Application_Start()
    GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
    AreaRegistration.RegisterAllAreas()
    RouteConfig.RegisterRoutes(RouteTable.Routes)
    ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType(Of JsonValueProviderFactory).Single)
    ValueProviderFactories.Factories.Add(New JsonNetValueProviderFactory())
    Get_All_Cached_ReportsTo()
  End Sub
End Class
