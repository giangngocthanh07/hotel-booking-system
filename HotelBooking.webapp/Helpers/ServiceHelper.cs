using HotelBooking.webapp.ViewModels.Hotel;

public static class ServiceHelper
{
    public static ServiceBaseVM? CreateNewServiceInstance(int typeId)
    {
        switch (typeId)
        {
            case 1:
                return new ServiceStandardVM { ServiceTypeId = 1 };

            case 2:
                return new ServiceAirportTransferVM { ServiceTypeId = 2 };

            // case 3: return new ServiceSpaVM { ServiceTypeId = 3 }; 
            // case 4: return new ServiceTourVM { ServiceTypeId = 4 };

            default:
                return null;
        }
    }

    public static string GetServiceTypeName(int typeId)
    {
        return typeId switch
        {
            1 => "Standard Service",
            2 => "Airport Transfer",
            _ => "Unknown Service Type",
        };
    }
}
