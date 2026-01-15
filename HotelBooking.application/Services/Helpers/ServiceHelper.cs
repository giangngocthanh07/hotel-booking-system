using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class ServiceHelper
{
    // Khai báo static readonly để dùng chung, không cần tạo mới liên tục -> Tối ưu bộ nhớ - ĐÂY LÀ 1 STATIC SIMPLE FACTORY PATTERN
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    public static ServiceBaseDTO? MapToServiceDTO(Service sv)
    {
        var additionalJson = sv.Additional ?? "{}";

        switch ((ServiceTypeEnum)sv.TypeId)
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
                    TypeId = sv.TypeId,
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
                    TypeId = sv.TypeId,
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

    // THÊM METHOD NÀY: Dùng để đóng gói dữ liệu khi Create/Update - ĐÂY LÀ STATIC SIMPLE FACTORY PATTERN
    public static string MapToAdditionalJson(ServiceCreateOrUpdateDTO dto, string? existingAdditional = null)
    {
        switch (dto)
        {
            case StdServiceCreateOrUpdateDTO stdDto:
                var dict = new Dictionary<string, string>
                {
                    { "Unit", stdDto.Unit }
                };
                return JsonSerializer.Serialize(dict, _jsonOptions);


            case AirportTransServiceCreateOrUpdateDTO _:

                // Logic: "Update giữ nguyên, Create lấy mặc định"

                // 1. Nếu đang Update (có existingAdditional) -> Trả về cái cũ
                if (!string.IsNullOrEmpty(existingAdditional) && existingAdditional != "{}")
                {
                    return existingAdditional;
                }

                // 2. Nếu đang Create -> Tạo dữ liệu mặc định (hoặc null tùy ý bạn)
                var defaultAtData = new ServiceAdditionalDataAT
                {
                    MaxPassengers = null,
                    MaxLuggage = null,
                    RoundTripPrice = null,
                    AdditionalFee = null,
                    AdditionalFeeStartTime = null,
                    AdditionalFeeEndTime = null
                };
                return JsonSerializer.Serialize(defaultAtData, _jsonOptions);

            default:
                return "{}";
        }
    }
}