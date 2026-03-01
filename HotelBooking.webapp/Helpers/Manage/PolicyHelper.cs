namespace HotelBooking.webapp.Helpers
{
    public static class PolicyHelper
    {
        // --- 1. ÁP DỤNG CHO CREATE MODEL ---
        public static void ApplyDefaultValues(PolicyCreateVM model, int typeId)
        {
            // Không reset ở đây, để ResetForm() của Manager lo
            ApplyLogic(typeId, model.Name ?? "",
                v => { if (model.TimeFrom == null) model.TimeFrom = v; },
                v => { if (model.TimeTo == null) model.TimeTo = v; },
                v => { if (model.IntValue1 == 0 || model.IntValue1 == null) model.IntValue1 = v; },
                v => { if (model.IntValue2 == 0 || model.IntValue2 == null) model.IntValue2 = v; },
                v => { if (model.Amount == 0 || model.Amount == null) model.Amount = v; },
                v => { if (model.Percent == 0 || model.Percent == null) model.Percent = v; },
                v => { if (model.BoolValue == null) model.BoolValue = v; });
        }

        // --- 2. ÁP DỤNG CHO UPDATE MODEL ---
        public static void ApplyDefaultValues(PolicyUpdateVM model, int typeId)
        {
            ApplyLogic(typeId, model.Name ?? "",
                v => { if (model.TimeFrom == null) model.TimeFrom = v; },
                v => { if (model.TimeTo == null) model.TimeTo = v; },
                v => { if (model.IntValue1 == 0 || model.IntValue1 == null) model.IntValue1 = v; },
                v => { if (model.IntValue2 == 0 || model.IntValue2 == null) model.IntValue2 = v; },
                v => { if (model.Amount == 0 || model.Amount == null) model.Amount = v; },
                v => { if (model.Percent == 0 || model.Percent == null) model.Percent = v; },
                v => { if (model.BoolValue == null) model.BoolValue = v; });
        }

        // --- 3. LOGIC NGHIỆP VỤ (Private) ---
        private static void ApplyLogic(int typeId, string nameCheck,
            Action<TimeOnly?> setTimeFrom, Action<TimeOnly?> setTimeTo,
            Action<int?> setInt1, Action<int?> setInt2,
            Action<decimal?> setAmount, Action<double?> setPercent,
            Action<bool?> setBool)
        {
            switch (typeId)
            {
                case 1002: // Check-in & Check-out
                    if (nameCheck.Contains("Early"))
                    {
                        setTimeFrom(new TimeOnly(8, 0)); setTimeTo(new TimeOnly(14, 0));
                        setAmount(50000);
                    }
                    else if (nameCheck.Contains("Late"))
                    {
                        setTimeFrom(new TimeOnly(12, 0)); setTimeTo(new TimeOnly(18, 0));
                        setAmount(50000);
                    }
                    else
                    { // Standard
                        setTimeFrom(new TimeOnly(14, 0)); setTimeTo(new TimeOnly(12, 0));
                        setAmount(0);
                    }
                    break;

                case 1003: // Cancellation (Đã tối ưu cho Switch UI)
                    if (nameCheck.Contains("Non-Refundable"))
                    {
                        setInt1(0);      // 0 ngày = Gạt Switch sang "Áp dụng ngay"
                        setPercent(100); // Có phí phạt (%) -> Hiện ô nhập Percent
                        setAmount(0);
                    }
                    else if (nameCheck.Contains("50%"))
                    {
                        setInt1(7);      // Hạn báo trước 7 ngày
                        setPercent(50);
                        setAmount(0);
                    }
                    else
                    { // Linh hoạt / Miễn phí
                        setInt1(3);      // Hạn báo trước 3 ngày
                        setPercent(0);   // Phí = 0 -> Giao diện hiện "Miễn phí hủy"
                        setAmount(0);
                    }
                    break;

                case 1004: // Children & Extra Bed
                    if (nameCheck.Contains("Extra Bed") || nameCheck.Contains("Adult"))
                    {
                        setInt1(18); setInt2(99); setAmount(300000);
                    }
                    else
                    {
                        setInt1(6); setInt2(11); setAmount(150000);
                    }
                    break;

                case 2002: // Pets
                    setAmount(200000);
                    setBool(true);
                    break;
            }
        }
    }
}