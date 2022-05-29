namespace ChessServices;

public static class Tools
{
    public static string Serialize(SocketNotificationDTO obj)
    {
        return JsonConvert.SerializeObject(obj,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
    }
}
