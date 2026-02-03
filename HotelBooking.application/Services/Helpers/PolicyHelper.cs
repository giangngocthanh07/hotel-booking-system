using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class PolicyHelper
{
    // <summary>
    /// Hàm này đảm bảo dữ liệu thừa bị xóa bỏ dựa trên TypeId.
    /// Ví dụ: Policy về Giờ giấc thì không được lưu tiền hay phần trăm.
    /// </summary>
    public static void SanitizeEntity(Policy entity)
    {
        switch (entity.TypeId)
        {
            // CASE 1: Check-in / Check-out (ID 1002)
            // -> Chỉ giữ TimeFrom, TimeTo, Amount (phụ thu). Xóa Percent, IntValue.
            case 1002:
                entity.IntValue1 = null;
                entity.IntValue2 = null;
                entity.Percent = null;
                entity.BoolValue = false;
                // KHÔNG reset TimeFrom/TimeTo/Amount vì người dùng đã nhập
                break;

            // CASE 2: Cancellation (ID 1003)
            // -> Cần IntValue1 (Ngày), Percent (Phạt), BoolValue. Xóa Time, IntValue2.
            case 1003:
                entity.TimeFrom = null;
                entity.TimeTo = null;
                entity.IntValue2 = null;
                // entity.Amount = null; // Có thể giữ Amount nếu muốn phạt tiền cứng thay vì %
                break;

            // CASE 3: Children (ID 1004)
            // -> Cần IntValue1, IntValue2 (Tuổi), Amount. Xóa Time, Percent.
            case 1004:
                entity.TimeFrom = null;
                entity.TimeTo = null;
                entity.Percent = null;
                entity.BoolValue = false;
                break;

            // CASE 4: Pets (ID 2002)
            // -> Cần Amount, BoolValue. Xóa Time, Percent, IntValue.
            case 2002:
                entity.TimeFrom = null;
                entity.TimeTo = null;
                entity.IntValue1 = null;
                entity.IntValue2 = null;
                entity.Percent = null;
                break;
        }
    }
}