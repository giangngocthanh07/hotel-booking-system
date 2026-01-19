using System.Text.Json;


namespace HotelBooking.webapp.Helpers
{
    public static class PolicyHelper
    {
        /// <summary>
        /// Hàm này giúp điền giá trị mặc định vào ViewModel để hiển thị lên giao diện (Preview)
        /// </summary>
        public static void ApplyDefaultValues(PolicyVM model, int typeId)
        {
            // Reset các giá trị về null/default trước khi gán
            model.TimeFrom = null; model.TimeTo = null;
            model.IntValue1 = null; model.IntValue2 = null;
            model.Amount = null; model.Percent = null;
            model.BoolValue = false;

            // Gán TypeId
            model.TypeId = typeId;

            switch (typeId)
            {
                case 1002: // Check-in & Check-out
                    model.TimeFrom = new TimeOnly(14, 0); // 14:00
                    model.TimeTo = new TimeOnly(12, 0);   // 12:00
                    break;

                case 1003: // Cancellation (Hủy phòng)
                    model.IntValue1 = 3;    // Trước 3 ngày
                    model.Percent = 100;    // Phạt 100%
                    model.BoolValue = true; // Có hoàn tiền
                    break;

                case 1004: // Children (Trẻ em)
                    model.IntValue1 = 6;
                    model.IntValue2 = 11;
                    model.Amount = 150000;
                    break;

                case 2002: // Pets (Thú cưng)
                    model.Amount = 200000;
                    model.BoolValue = true;
                    break;
            }
        }
    }

}