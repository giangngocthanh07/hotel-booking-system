using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class ServiceHelper
{
    // Khai báo static readonly để dùng chung, không cần tạo mới liên tục -> Tối ưu bộ nhớ
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    public static ServiceBaseDTO? MapToServiceDTO(Service sv)
    {
        var additionalJson = sv.Additional ?? "{}";

        switch ((ServiceTypeEnum)sv.ServiceTypeId)
        {
            case ServiceTypeEnum.Standard: // ID = 1
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(additionalJson, _jsonOptions);
                return new ServiceStandardDTO
                {
                    Id = sv.Id,
                    Name = sv.Name,
                    Description = sv.Description,
                    Price = sv.Price,
                    IsDeleted = sv.IsDeleted,
                    ServiceTypeId = sv.ServiceTypeId,
                    // Map riêng
                    Unit = dict?.GetValueOrDefault("Unit", "") ?? ""
                };

            case ServiceTypeEnum.AirportTransfer: // ID = 2
                var atData = JsonSerializer.Deserialize<ServiceAdditionalDataAT>(additionalJson, _jsonOptions);
                return new ServiceAirportTransferDTO
                {
                    Id = sv.Id,
                    Name = sv.Name,
                    Description = sv.Description,
                    Price = sv.Price,
                    IsDeleted = sv.IsDeleted,
                    ServiceTypeId = sv.ServiceTypeId,
                    // Map riêng
                    MaxPassengers = atData?.MaxPassengers,
                    MaxLuggage = atData?.MaxLuggage,
                    RoundTripPrice = atData?.RoundTripPrice,
                    AdditionalFee = atData?.AdditionalFee,
                    AdditionalFeeStartTime = atData?.AdditionalFeeStartTime,
                    AdditionalFeeEndTime = atData?.AdditionalFeeEndTime
                };

            default:
                return null;
        }
    }
}