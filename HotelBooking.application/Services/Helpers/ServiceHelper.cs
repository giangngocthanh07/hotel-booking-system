using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class ServiceHelper
{
    // Khai báo static readonly để dùng chung, không cần tạo mới liên tục -> Tối ưu bộ nhớ - ĐÂY LÀ 1 STATIC SIMPLE FACTORY PATTERN
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static int? GetTypeIdFromUpdateDto(ServiceUpdateDTO dto)
    {
        return dto switch
        {
            ServiceStandardUpdateDTO => (int)ServiceTypeEnum.Standard,
            ServiceAirportUpdateDTO => (int)ServiceTypeEnum.AirportTransfer,
            _ => null
        };
    }

    public static ServiceDTO? MapToServiceDTO(Service sv)
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
                var atData = JsonSerializer.Deserialize<ServiceAirportAdditionalData>(additionalJson, _jsonOptions);
                return new ServiceAirportTransferDTO
                {
                    Id = sv.Id,
                    Name = sv.Name,
                    Description = sv.Description,
                    Price = sv.Price,
                    IsDeleted = sv.IsDeleted,
                    TypeId = sv.TypeId,
                    // Map các câu hỏi logic
                    IsOneWayPaid = atData?.IsOneWayPaid ?? (sv.Price > 0),
                    HasRoundTrip = atData?.HasRoundTrip ?? false,
                    IsRoundTripPaid = atData?.IsRoundTripPaid ?? false,
                    // Map các giá trị thực tế
                    MaxPassengers = atData?.MaxPassengers,
                    MaxLuggage = atData?.MaxLuggage,
                    RoundTripPrice = atData?.RoundTripPrice,
                    // Logic: Nếu AdditionalFee có giá trị thì HasNightFee phải là true
                    HasNightFee = atData?.HasNightFee ?? (atData?.AdditionalFee > 0),
                    AdditionalFee = atData?.AdditionalFee,
                    // --- Chuyển từ string (JSON) về TimeOnly (DTO) ---
                    AdditionalFeeStartTime = !string.IsNullOrEmpty(atData?.AdditionalFeeStartTime)
            ? TimeOnly.Parse(atData.AdditionalFeeStartTime) : null,

                    AdditionalFeeEndTime = !string.IsNullOrEmpty(atData?.AdditionalFeeEndTime)
            ? TimeOnly.Parse(atData.AdditionalFeeEndTime) : null
                };

            default:
                return null;
        }
    }

    // THÊM METHOD NÀY: Dùng để đóng gói dữ liệu khi Create/Update - ĐÂY LÀ STATIC SIMPLE FACTORY PATTERN
    // =========================================================================
    // 2. MAP CREATE DTO -> JSON STRING (Lưu vào DB)
    // =========================================================================
    public static string MapToAdditionalJson(ServiceCreateDTO dto)
    {
        switch (dto)
        {
            case ServiceStandardCreateDTO std:
                return JsonSerializer.Serialize(new { Unit = std.Unit }, _jsonOptions);

            case ServiceAirportCreateDTO air:
                var data = new ServiceAirportAdditionalData();
                // Map và làm sạch dữ liệu (Sanitize)
                ApplyAirportLogic(data, air);
                return JsonSerializer.Serialize(data, _jsonOptions);

            default:
                return "{}";
        }
    }

    // =========================================================================
    // 3. MAP UPDATE DTO -> JSON STRING (Cập nhật DB)
    // =========================================================================
    public static string MapToAdditionalJson(ServiceUpdateDTO dto)
    {
        switch (dto)
        {
            case ServiceStandardUpdateDTO std:
                return JsonSerializer.Serialize(new { Unit = std.Unit }, _jsonOptions);

            case ServiceAirportUpdateDTO air:
                var data = new ServiceAirportAdditionalData();
                // Map và làm sạch dữ liệu (Sanitize)
                ApplyAirportLogic(data, air);
                return JsonSerializer.Serialize(data, _jsonOptions);

            default:
                return "{}";
        }
    }

    // =========================================================================
    // PRIVATE HELPER: Logic nghiệp vụ chung cho Airport (Tránh lặp code)
    // =========================================================================

    // Overload cho Create
    private static void ApplyAirportLogic(ServiceAirportAdditionalData data, ServiceAirportCreateDTO src)
    {
        data.MaxPassengers = src.MaxPassengers;
        data.MaxLuggage = src.MaxLuggage;
        data.IsOneWayPaid = src.IsOneWayPaid;

        // Logic Khứ hồi: Nếu tắt khứ hồi -> Xóa giá và flag trả phí
        data.HasRoundTrip = src.HasRoundTrip;
        data.IsRoundTripPaid = src.HasRoundTrip && src.IsRoundTripPaid;
        data.RoundTripPrice = (src.HasRoundTrip && src.IsRoundTripPaid) ? src.RoundTripPrice : null;

        // 2. Logic Phụ phí đêm: Chốt chặn an toàn ở đây
        data.HasNightFee = src.HasNightFee;
        if (src.HasNightFee && src.AdditionalFeeStartTime.HasValue && src.AdditionalFeeEndTime.HasValue)
        {
            // Tính toán độ dài khung giờ
            TimeSpan duration = src.AdditionalFeeEndTime.Value - src.AdditionalFeeStartTime.Value;
            double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

            // @--BEST_PRACTICE_LIMIT_12H--@
            if (totalHours > 12)
            {
                // Có thể throw lỗi hoặc log cảnh báo. Ở đây ta vẫn cho lưu nhưng nên xử lý ở Validation trước.
                // data.HasNightFee = false; // Một cách chặn cực đoan
            }

            data.AdditionalFee = src.AdditionalFee;
            data.AdditionalFeeStartTime = src.AdditionalFeeStartTime?.ToString("HH:mm");
            data.AdditionalFeeEndTime = src.AdditionalFeeEndTime?.ToString("HH:mm");
        }
        else
        {
            // Xóa sạch như đã làm ở bước trước
            data.AdditionalFee = null;
            data.AdditionalFeeStartTime = null;
            data.AdditionalFeeEndTime = null;
            data.HasNightFee = false;
        }
    }

    // Overload cho Update
    private static void ApplyAirportLogic(ServiceAirportAdditionalData data, ServiceAirportUpdateDTO src)
    {
        data.MaxPassengers = src.MaxPassengers;
        data.MaxLuggage = src.MaxLuggage;
        data.IsOneWayPaid = src.IsOneWayPaid;

        data.HasRoundTrip = src.HasRoundTrip;
        data.IsRoundTripPaid = src.HasRoundTrip && src.IsRoundTripPaid;
        data.RoundTripPrice = (src.HasRoundTrip && src.IsRoundTripPaid) ? src.RoundTripPrice : null;

        // 2. Logic Phụ phí đêm: Chốt chặn an toàn ở đây
        data.HasNightFee = src.HasNightFee;
        if (src.HasNightFee && src.AdditionalFeeStartTime.HasValue && src.AdditionalFeeEndTime.HasValue)
        {
            // Tính toán độ dài khung giờ
            TimeSpan duration = src.AdditionalFeeEndTime.Value - src.AdditionalFeeStartTime.Value;
            double totalHours = duration.TotalHours < 0 ? duration.TotalHours + 24 : duration.TotalHours;

            // @--BEST_PRACTICE_LIMIT_12H--@
            if (totalHours > 12)
            {
                // Có thể throw lỗi hoặc log cảnh báo. Ở đây ta vẫn cho lưu nhưng nên xử lý ở Validation trước.
                // data.HasNightFee = false; // Một cách chặn cực đoan
            }

            data.AdditionalFee = src.AdditionalFee;
            data.AdditionalFeeStartTime = src.AdditionalFeeStartTime?.ToString("HH:mm");
            data.AdditionalFeeEndTime = src.AdditionalFeeEndTime?.ToString("HH:mm");
        }
        else
        {
            // Xóa sạch như đã làm ở bước trước
            data.AdditionalFee = null;
            data.AdditionalFeeStartTime = null;
            data.AdditionalFeeEndTime = null;
            data.HasNightFee = false;
        }
    }
}