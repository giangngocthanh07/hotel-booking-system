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

    // --- HÀM MỚI: CLONE DATA ---
    public static ServiceBaseVM? CloneService(ServiceBaseVM source)
    {
        if (source == null) return null;

        // Dùng Pattern Matching để xác định kiểu con và copy dữ liệu
        switch (source)
        {
            case ServiceStandardVM stdSource:
                return new ServiceStandardVM
                {
                    // Copy property chung
                    Id = stdSource.Id,
                    ServiceTypeId = stdSource.ServiceTypeId,
                    Name = stdSource.Name,
                    Description = stdSource.Description,
                    Price = stdSource.Price,

                    // Copy property riêng
                    Unit = stdSource.Unit
                };

            case ServiceAirportTransferVM aptSource:
                return new ServiceAirportTransferVM
                {
                    // Copy property chung
                    Id = aptSource.Id,
                    ServiceTypeId = aptSource.ServiceTypeId,
                    Name = aptSource.Name,
                    Description = aptSource.Description,
                    Price = aptSource.Price,

                    // Copy property riêng
                    MaxPassengers = aptSource.MaxPassengers,
                    MaxLuggage = aptSource.MaxLuggage,
                    RoundTripPrice = aptSource.RoundTripPrice,
                    AdditionalFee = aptSource.AdditionalFee
                };

            default:
                // Fallback cho trường hợp Base hoặc loại chưa định nghĩa
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
