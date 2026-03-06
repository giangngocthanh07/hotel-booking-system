using HotelBooking.webapp.ViewModels.Admin;

namespace HotelBooking.webapp.Helpers
{
    /// <summary>
    /// Helper class for Policy - handles factory creation, mapping, and default values.
    /// Pattern: Similar to ServiceHelper FE - uses polymorphic ViewModels.
    /// </summary>
    public static class PolicyHelper
    {
        // ===========================================================================
        // 1. FACTORY PATTERN: Create empty CreateVM object based on TypeId
        // ===========================================================================
        public static PolicyCreateVM CreateNewPolicyModel(int typeId)
        {
            return typeId switch
            {
                (int)PolicyTypeEnum.CheckInOut => new CheckInOutPolicyCreateVM { TypeId = typeId },
                (int)PolicyTypeEnum.Cancellation => new CancellationPolicyCreateVM { TypeId = typeId },
                (int)PolicyTypeEnum.Children => new ChildrenPolicyCreateVM { TypeId = typeId },
                (int)PolicyTypeEnum.Pets => new PetPolicyCreateVM { TypeId = typeId },
                _ => new CheckInOutPolicyCreateVM { TypeId = typeId } // Default
            };
        }

        // ===========================================================================
        // 2. MAPPING: Convert from Display Data (PolicyVM) to Update Data (PolicyUpdateVM)
        // ===========================================================================
        public static PolicyUpdateVM? MapToUpdateVM(PolicyVM source)
        {
            if (source == null) return null;

            return source switch
            {
                CheckInOutPolicyVM checkInOut => new CheckInOutPolicyUpdateVM
                {
                    Name = checkInOut.Name,
                    Description = checkInOut.Description,
                    CheckInTime = checkInOut.CheckInTime,
                    CheckOutTime = checkInOut.CheckOutTime,
                    EarlyCheckInFee = checkInOut.EarlyCheckInFee,
                    LateCheckOutFee = checkInOut.LateCheckOutFee
                },
                CancellationPolicyVM cancel => new CancellationPolicyUpdateVM
                {
                    Name = cancel.Name,
                    Description = cancel.Description,
                    DaysBeforeCheckIn = cancel.DaysBeforeCheckIn,
                    RefundPercent = cancel.RefundPercent,
                    IsRefundable = cancel.IsRefundable
                },
                ChildrenPolicyVM children => new ChildrenPolicyUpdateVM
                {
                    Name = children.Name,
                    Description = children.Description,
                    MinAge = children.MinAge,
                    MaxAge = children.MaxAge,
                    ExtraBedFee = children.ExtraBedFee
                },
                PetPolicyVM pet => new PetPolicyUpdateVM
                {
                    Name = pet.Name,
                    Description = pet.Description,
                    PetFee = pet.PetFee,
                    IsPetAllowed = pet.IsPetAllowed
                },
                _ => null
            };
        }

        // ===========================================================================
        // 3. HELPER: Get display name of PolicyType
        // ===========================================================================
        public static string GetPolicyTypeName(int typeId)
        {
            return typeId switch
            {
                (int)PolicyTypeEnum.CheckInOut => "Check-In/Check-Out",
                (int)PolicyTypeEnum.Cancellation => "Cancellation",
                (int)PolicyTypeEnum.Children => "Children & Extra Beds",
                (int)PolicyTypeEnum.Pets => "Pets",
                _ => "Other"
            };
        }

        // ===========================================================================
        // 4. SMART FILL: Automatically suggest parameters based on policy name
        // ===========================================================================

        // --- 4a. APPLY TO CREATE MODEL ---
        public static void ApplyDefaultValues(PolicyCreateVM model, int typeId)
        {
            if (model == null) return;
            var name = model.Name ?? "";

            switch (model)
            {
                case CheckInOutPolicyCreateVM checkInOut:
                    ApplyCheckInOutDefaults(checkInOut, name);
                    break;
                case CancellationPolicyCreateVM cancel:
                    ApplyCancellationDefaults(cancel, name);
                    break;
                case ChildrenPolicyCreateVM children:
                    ApplyChildrenDefaults(children, name);
                    break;
                case PetPolicyCreateVM pet:
                    ApplyPetDefaults(pet, name);
                    break;
            }
        }

        // --- 4b. APPLY TO UPDATE MODEL ---
        public static void ApplyDefaultValues(PolicyUpdateVM model, int typeId)
        {
            if (model == null) return;
            var name = model.Name ?? "";

            switch (model)
            {
                case CheckInOutPolicyUpdateVM checkInOut:
                    ApplyCheckInOutDefaults(checkInOut, name);
                    break;
                case CancellationPolicyUpdateVM cancel:
                    ApplyCancellationDefaults(cancel, name);
                    break;
                case ChildrenPolicyUpdateVM children:
                    ApplyChildrenDefaults(children, name);
                    break;
                case PetPolicyUpdateVM pet:
                    ApplyPetDefaults(pet, name);
                    break;
            }
        }

        // ===========================================================================
        // PRIVATE: Business logic for each Policy type
        // ===========================================================================

        private static void ApplyCheckInOutDefaults(CheckInOutPolicyCreateVM model, string name)
        {
            if (name.Contains("Early", StringComparison.OrdinalIgnoreCase))
            {
                model.CheckInTime ??= new TimeOnly(8, 0);
                model.CheckOutTime ??= new TimeOnly(14, 0);
                model.EarlyCheckInFee ??= 50000;
            }
            else if (name.Contains("Late", StringComparison.OrdinalIgnoreCase))
            {
                model.CheckInTime ??= new TimeOnly(12, 0);
                model.CheckOutTime ??= new TimeOnly(18, 0);
                model.LateCheckOutFee ??= 50000;
            }
            else
            {
                model.CheckInTime ??= new TimeOnly(14, 0);
                model.CheckOutTime ??= new TimeOnly(12, 0);
            }
        }

        private static void ApplyCheckInOutDefaults(CheckInOutPolicyUpdateVM model, string name)
        {
            if (name.Contains("Early", StringComparison.OrdinalIgnoreCase))
            {
                model.CheckInTime ??= new TimeOnly(8, 0);
                model.CheckOutTime ??= new TimeOnly(14, 0);
                model.EarlyCheckInFee ??= 50000;
            }
            else if (name.Contains("Late", StringComparison.OrdinalIgnoreCase))
            {
                model.CheckInTime ??= new TimeOnly(12, 0);
                model.CheckOutTime ??= new TimeOnly(18, 0);
                model.LateCheckOutFee ??= 50000;
            }
            else
            {
                model.CheckInTime ??= new TimeOnly(14, 0);
                model.CheckOutTime ??= new TimeOnly(12, 0);
            }
        }

        private static void ApplyCancellationDefaults(CancellationPolicyCreateVM model, string name)
        {
            if (name.Contains("Non-Refundable", StringComparison.OrdinalIgnoreCase))
            {
                model.DaysBeforeCheckIn ??= 0;
                model.RefundPercent ??= 0;
                model.IsRefundable = false;
            }
            else if (name.Contains("50%", StringComparison.OrdinalIgnoreCase))
            {
                model.DaysBeforeCheckIn ??= 7;
                model.RefundPercent ??= 50;
                model.IsRefundable = true;
            }
            else
            {
                model.DaysBeforeCheckIn ??= 3;
                model.RefundPercent ??= 100;
                model.IsRefundable = true;
            }
        }

        private static void ApplyCancellationDefaults(CancellationPolicyUpdateVM model, string name)
        {
            if (name.Contains("Non-Refundable", StringComparison.OrdinalIgnoreCase))
            {
                model.DaysBeforeCheckIn ??= 0;
                model.RefundPercent ??= 0;
                model.IsRefundable = false;
            }
            else if (name.Contains("50%", StringComparison.OrdinalIgnoreCase))
            {
                model.DaysBeforeCheckIn ??= 7;
                model.RefundPercent ??= 50;
                model.IsRefundable = true;
            }
            else
            {
                model.DaysBeforeCheckIn ??= 3;
                model.RefundPercent ??= 100;
                model.IsRefundable = true;
            }
        }

        private static void ApplyChildrenDefaults(ChildrenPolicyCreateVM model, string name)
        {
            if (name.Contains("Extra Bed", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Adult", StringComparison.OrdinalIgnoreCase))
            {
                model.MinAge ??= 12;
                model.MaxAge ??= 17;
                model.ExtraBedFee ??= 300000;
            }
            else
            {
                model.MinAge ??= 6;
                model.MaxAge ??= 11;
                model.ExtraBedFee ??= 150000;
            }
        }

        private static void ApplyChildrenDefaults(ChildrenPolicyUpdateVM model, string name)
        {
            if (name.Contains("Extra Bed", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Adult", StringComparison.OrdinalIgnoreCase))
            {
                model.MinAge ??= 12;
                model.MaxAge ??= 17;
                model.ExtraBedFee ??= 300000;
            }
            else
            {
                model.MinAge ??= 6;
                model.MaxAge ??= 11;
                model.ExtraBedFee ??= 150000;
            }
        }

        private static void ApplyPetDefaults(PetPolicyCreateVM model, string name)
        {
            model.PetFee ??= 200000;
            // Default: allow pets
            if (!model.IsPetAllowed && model.PetFee > 0)
                model.IsPetAllowed = true;
        }

        private static void ApplyPetDefaults(PetPolicyUpdateVM model, string name)
        {
            model.PetFee ??= 200000;
            if (!model.IsPetAllowed && model.PetFee > 0)
                model.IsPetAllowed = true;
        }
    }
}