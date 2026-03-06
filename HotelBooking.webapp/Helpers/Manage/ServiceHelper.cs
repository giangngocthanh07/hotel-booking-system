using HotelBooking.webapp.ViewModels.Admin;

namespace HotelBooking.webapp.Helpers
{
    public static class ServiceHelper
    {
        /// <summary>
        /// 1. Factory Pattern: Creates an empty CreateVM object based on TypeId
        /// </summary>
        public static ServiceCreateVM CreateNewServiceModel(int typeId)
        {
            // Step 1: Create the correct instance type
            ServiceCreateVM model = typeId switch
            {
                1 => new ServiceStandardCreateVM(),
                2 => new ServiceAirportCreateVM(),
                _ => new ServiceStandardCreateVM()
            };

            // Step 2: Execute Smart Fill immediately (to set floor prices like 100k/350k)
            ApplyDefaultValues(model, typeId);

            return model;
        }

        /// <summary>
        /// 2. Mapping: Converts from Display Data (ServiceVM) to Update Data (ServiceUpdateVM)
        /// Triggered when the "Edit" button is clicked on the management table.
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
                    // Copy basic fields
                    Name = air.Name,
                    Description = air.Description,
                    Price = air.Price,

                    // Copy logic fields
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
        /// 3. Helper to get Display Names
        /// </summary>
        public static string GetServiceTypeName(int typeId)
        {
            return typeId switch
            {
                1 => "Standard Service",
                2 => "Airport Transfer",
                _ => "Other",
            };
        }

        /// <summary>
        /// 4. Smart Fill: Automatically suggests parameters based on the service name
        /// </summary>

        // --- 4a. APPLY TO CREATE MODEL ---
        public static void ApplyDefaultValues(ServiceCreateVM model, int typeId)
        {
            if (model == null) return;

            // STEP A: SET FLOOR PRICE (Ensure the form is never empty)
            if (model.Price == 0)
            {
                model.Price = typeId == 2 ? 350000 : 100000;
                // Type 2 (Airport) defaults to 350k, Type 1 (Standard) defaults to 100k
            }

            // STEP B: KEYWORD LOGIC (Overrides floor price if keywords match)
            ApplyLogic(typeId, model.Name ?? "",

                // 1. Pricing: Only override floor price or zero price
                v => { if (model.Price == 0 || model.Price == 100000 || model.Price == 350000) model.Price = v; },

                // 2. Unit: 
                v =>
                {
                    if (model is ServiceStandardCreateVM std)
                    {
                        // Only change if empty OR current value is default "Trip"
                        if (string.IsNullOrEmpty(std.Unit) || std.Unit == "Trip" || std.Unit == "Lượt") std.Unit = v;
                    }
                },

                // 3. Capacity
                (pax, lug) =>
                {
                    if (model is ServiceAirportCreateVM air)
                    {
                        if (air.MaxPassengers == null || air.MaxPassengers == 4) air.MaxPassengers = pax;
                        if (air.MaxLuggage == null || air.MaxLuggage == 2) air.MaxLuggage = lug;
                    }
                },

                // 4. Round Trip
                (hasRT, isPaid, rtPrice) =>
                {
                    if (model is ServiceAirportCreateVM air)
                    {
                        // Enable switch only if user hasn't touched it (HasRoundTrip is off)
                        if (!air.HasRoundTrip)
                        {
                            air.HasRoundTrip = hasRT;
                            air.IsRoundTripPaid = isPaid;
                        }
                        // Fill price only if empty or old default (300k)
                        if (air.RoundTripPrice == 0 || air.RoundTripPrice == null || air.RoundTripPrice == 300000)
                        {
                            air.RoundTripPrice = rtPrice;
                        }
                    }
                },

                // 5. Night Fee
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

        // --- 4b. APPLY TO UPDATE MODEL ---
        public static void ApplyDefaultValues(ServiceUpdateVM model, int typeId)
        {
            if (model == null) return;

            ApplyLogic(typeId, model.Name ?? "",
                 v => { if (model.Price == 0) model.Price = v; },
                 v => { if (model is ServiceStandardUpdateVM std && string.IsNullOrEmpty(std.Unit)) std.Unit = v; },
                 (pax, lug) => { if (model is ServiceAirportUpdateVM air) { air.MaxPassengers = pax; air.MaxLuggage = lug; } },

                (hasRT, isPaid, rtPrice) =>
                {
                    if (model is ServiceAirportUpdateVM air)
                    {
                        if (!air.HasRoundTrip)
                        {
                            air.HasRoundTrip = hasRT;
                            air.IsRoundTripPaid = isPaid;
                        }
                        if (air.RoundTripPrice == 0 || air.RoundTripPrice == null) air.RoundTripPrice = rtPrice;
                    }
                },

                (nightFeeOrNot, fee, start, end) =>
                {
                    if (model is ServiceAirportUpdateVM air)
                    {
                        if (air.AdditionalFee == 0 || air.AdditionalFee == null) air.AdditionalFee = fee;
                    }
                }
             );
        }

        // --- 5. CENTRALIZED BUSINESS LOGIC (Private) ---
        private static void ApplyLogic(int typeId, string nameCheck,
            Action<decimal> setPrice, Action<string> setUnit,
            Action<int, int> setCapacity, Action<bool, bool, decimal> setRoundTripFee, Action<bool, decimal, TimeOnly, TimeOnly> setNightFee)
        {
            string name = nameCheck.ToLower();
            switch (typeId)
            {
                case 1: // Standard
                    if (name.Contains("ăn sáng") || name.Contains("breakfast")) { setPrice(150000); setUnit("Person"); }
                    else if (name.Contains("giặt") || name.Contains("laundry")) { setPrice(30000); setUnit("Kg"); }
                    else if (name.Contains("thuê xe") || name.Contains("rental")) { setPrice(200000); setUnit("Day"); }
                    else { setPrice(100000); setUnit("Trip"); }
                    break;

                case 2: // Airport Transfer
                    if (name.Contains("7 chỗ") || name.Contains("7-seater")) { setPrice(500000); setCapacity(7, 4); setRoundTripFee(true, true, 900000); }
                    else if (name.Contains("16 chỗ") || name.Contains("16-seater")) { setPrice(850000); setCapacity(16, 10); setRoundTripFee(true, true, 1600000); }
                    else { setPrice(350000); setCapacity(4, 2); setRoundTripFee(true, true, 650000); }

                    // General defaults for Airport Transfer
                    setRoundTripFee(true, true, 500000);
                    setNightFee(true, 200000, new TimeOnly(22, 0), new TimeOnly(5, 0));
                    break;
            }
        }
    }
}