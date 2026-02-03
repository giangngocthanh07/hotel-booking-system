using HotelBooking.webapp.ViewModels.Hotel;

namespace HotelBooking.webapp.Helpers
{
    public static class ServiceHelper
    {
        /// <summary>
        /// 1. Factory Pattern: Tạo object CreateVM rỗng dựa trên TypeId
        /// </summary>
        public static ServiceCreateVM CreateNewServiceModel(int typeId)
        {
            // Bước 1: Tạo instance đúng kiểu
            ServiceCreateVM model = typeId switch
            {
                1 => new ServiceStandardCreateVM(),
                2 => new ServiceAirportCreateVM(),
                _ => new ServiceStandardCreateVM()
            };

            // Bước 2: Chạy Smart Fill ngay lập tức (Để có giá sàn 100k/350k)
            ApplyDefaultValues(model, typeId);

            return model;
        }

        /// <summary>
        /// 2. Mapping: Chuyển từ Dữ liệu hiển thị (ServiceVM) sang Dữ liệu Update (ServiceUpdateVM)
        /// Dùng khi bấm nút "Edit" trên bảng
        /// </summary>
        public static ServiceUpdateVM? MapToUpdateVM(ServiceVM source)
        {
            if (source == null) return null;

            return source switch
            {
                // Case 1: Standard Service
                ServiceStandardVM std => new ServiceStandardUpdateVM
                {
                    Name = std.Name,
                    Description = std.Description,
                    Price = std.Price,
                    Unit = std.Unit
                },

                // Case 2: Airport Transfer
                ServiceAirportTransferVM air => new ServiceAirportUpdateVM
                {
                    // Copy các trường cơ bản
                    Name = air.Name,
                    Description = air.Description,
                    Price = air.Price,

                    // Copy các trường logic
                    IsOneWayPaid = air.IsOneWayPaid,

                    HasRoundTrip = air.HasRoundTrip,
                    IsRoundTripPaid = air.IsRoundTripPaid,
                    RoundTripPrice = air.RoundTripPrice,

                    HasNightFee = air.HasNightFee,
                    AdditionalFee = air.AdditionalFee,
                    AdditionalFeeStartTime = air.AdditionalFeeStartTime,
                    AdditionalFeeEndTime = air.AdditionalFeeEndTime,

                    MaxPassengers = air.MaxPassengers,
                    MaxLuggage = air.MaxLuggage
                },

                // Default
                _ => null
            };
        }

        /// <summary>
        /// 3. Helper lấy tên hiển thị
        /// </summary>
        public static string GetServiceTypeName(int typeId)
        {
            return typeId switch
            {
                1 => "Dịch vụ tiêu chuẩn",
                2 => "Đưa đón sân bay",
                _ => "Khác",
            };
        }

        /// <summary>
        /// 4. Smart Fill: Tự động gợi ý thông số dựa trên tên dịch vụ
        /// </summary>
        // --- 1. ÁP DỤNG CHO CREATE MODEL ---
        public static void ApplyDefaultValues(ServiceCreateVM model, int typeId)
        {
            if (model == null) return;

            // BƯỚC A: GÁN GIÁ SÀN (Để Form không bao giờ trống giá)
            if (model.Price == 0)
            {
                model.Price = typeId == 2 ? 350000 : 100000;
                // Type 2 (Sân bay) mặc định 350k, Type 1 (Tiêu chuẩn) mặc định 100k
            }

            // BƯỚC B: CHẠY LOGIC TỪ KHÓA (Ghi đè giá sàn nếu khớp từ khóa)
            ApplyLogic(typeId, model.Name ?? "",

                // 1. Giá tiền: Chỉ ghi đè giá sàn hoặc giá 0
                v => { if (model.Price == 0 || model.Price == 100000 || model.Price == 350000) model.Price = v; },

                // 2. Đơn vị tính: 
                v =>
                {
                    if (model is ServiceStandardCreateVM std)
                    {
                        // Chỉ đổi nếu đang trống HOẶC đang là mặc định "Lượt"
                        if (string.IsNullOrEmpty(std.Unit) || std.Unit == "Lượt") std.Unit = v;
                    }
                },

                // 3. Sức chứa
                (pax, lug) =>
                {
                    if (model is ServiceAirportCreateVM air)
                    {
                        if (air.MaxPassengers == null || air.MaxPassengers == 4) air.MaxPassengers = pax;
                        if (air.MaxLuggage == null || air.MaxLuggage == 2) air.MaxLuggage = lug;
                    }
                },

                // 4. Phí đêm
                (nightFeeOrNot, fee, start, end) =>
                {
                    if (model is ServiceAirportCreateVM air)
                    {
                        if (!air.HasNightFee) air.HasNightFee = nightFeeOrNot;
                        if (air.AdditionalFee == 0 || air.AdditionalFee == null) air.AdditionalFee = fee;
                        if (air.AdditionalFeeStartTime == default) air.AdditionalFeeStartTime = start;
                        if (air.AdditionalFeeEndTime == default) air.AdditionalFeeEndTime = end;
                    }
                }
            );
        }

        // --- 2. ÁP DỤNG CHO UPDATE MODEL ---
        public static void ApplyDefaultValues(ServiceUpdateVM model, int typeId)
        {
            if (model == null) return;

            ApplyLogic(typeId, model.Name ?? "",
                 v => { if (model.Price == 0) model.Price = v; },
                 v => { if (model is ServiceStandardUpdateVM std && string.IsNullOrEmpty(std.Unit)) std.Unit = v; },
                 (pax, lug) => { if (model is ServiceAirportUpdateVM air) { air.MaxPassengers = pax; air.MaxLuggage = lug; } },
                 (nightFeeOrNot, fee, start, end) =>
                 {
                     if (model is ServiceAirportUpdateVM air)
                     {
                         if (air.AdditionalFee == 0 || air.AdditionalFee == null) air.AdditionalFee = fee;
                     }
                 }
             );
        }

        // --- 3. LOGIC NGHIỆP VỤ TRUNG TÂM (Private) ---
        private static void ApplyLogic(int typeId, string nameCheck,
            Action<decimal> setPrice, Action<string> setUnit,
            Action<int, int> setCapacity, Action<bool, decimal, TimeOnly, TimeOnly> setNightFee)
        {
            string name = nameCheck.ToLower();
            switch (typeId)
            {
                case 1: // Standard
                    if (name.Contains("ăn sáng") || name.Contains("breakfast")) { setPrice(150000); setUnit("Suất"); }
                    else if (name.Contains("giặt")) { setPrice(30000); setUnit("Kg"); }
                    else if (name.Contains("thuê xe")) { setPrice(200000); setUnit("Ngày"); }
                    else { setPrice(100000); setUnit("Lượt"); }
                    break;

                case 2: // Airport
                    if (name.Contains("7 chỗ")) { setPrice(500000); setCapacity(7, 4); }
                    else if (name.Contains("16 chỗ")) { setPrice(850000); setCapacity(16, 10); }
                    else { setPrice(350000); setCapacity(4, 2); }

                    setNightFee(true, 200000, new TimeOnly(22, 0), new TimeOnly(5, 0));
                    break;
            }
        }
    }
}