using System.Text.Json;
using HotelBooking.infrastructure.Models;

public static class PolicyHelper
{
    public static void ApplyStrictBusinessRules(Policy entity)
    {
        // Reset tất cả về null trước cho chắc ăn
        entity.TimeFrom = null; entity.TimeTo = null;
        entity.IntValue1 = null; entity.IntValue2 = null;
        entity.Amount = null; entity.Percent = null;
        entity.BoolValue = false;

        switch (entity.TypeId)
        {
            // CASE 1: Check-in / Check-out (ID 1002)
            case 1002:
                // Luôn luôn set giờ chuẩn quốc tế
                entity.TimeFrom = new TimeOnly(14, 0); // 14:00
                entity.TimeTo = new TimeOnly(12, 0);   // 12:00
                break;

            // CASE 2: Cancellation (ID 1003)
            case 1003:
                entity.IntValue1 = 3;    // Luôn cho hủy trước 3 ngày
                entity.Percent = 100;    // Luôn phạt 100%
                entity.BoolValue = true; // Luôn cho hoàn tiền
                break;

            // CASE 3: Children (ID 1004)
            case 1004:
                // Cấu hình cứng cho Trẻ em
                entity.Amount = 150000; // Giá phụ thu trẻ em mặc định
                entity.IntValue1 = 6;   // Tuổi từ
                entity.IntValue2 = 11;  // Tuổi đến
                break;

            // CASE 4: Pets (ID 2002)
            case 2002:
                entity.Amount = 200000; // Giá phụ thu thú cưng mặc định
                entity.BoolValue = true;    // Mặc định cho phép
                break;

                // Bạn có thể thêm các case con nếu muốn (dựa vào tên hoặc sub-type)
        }
    }


}