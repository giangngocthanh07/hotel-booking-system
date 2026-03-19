using System.Text.Json;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RoomManagement
{
    public interface IRoomTypeService
    {
        Task<ApiResponse<int>> CreateRoomTypeAsync(RoomTypeCreateDTO request);
    }

    public class RoomTypeService : IRoomTypeService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IRoomTypeRepository _roomTypeRepo;
        private readonly IRoomTypeBedConfigRepository _bedConfigRepo;
        private readonly IRoomAttributeFacade _attributeFacade;
        private readonly IUnitOfWork _dbu;
        private readonly IValidator<RoomTypeCreateDTO> _validator;

        public RoomTypeService(IHotelRepository hotelRepo, IRoomTypeRepository roomTypeRepo, IRoomTypeBedConfigRepository bedConfigRepo, IValidator<RoomTypeCreateDTO> validator, IRoomAttributeFacade attributeFacade, IUnitOfWork dbu)
        {
            _hotelRepo = hotelRepo;
            _roomTypeRepo = roomTypeRepo;
            _bedConfigRepo = bedConfigRepo;
            _validator = validator;
            _attributeFacade = attributeFacade;
            _dbu = dbu;
        }
        public async Task<ApiResponse<int>> CreateRoomTypeAsync(RoomTypeCreateDTO request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return ResponseFactory.Failure<int>(StatusCodeResponse.BadRequest, validationResult.Errors[0].ErrorMessage);
            }

            var ghostIdValidation = await ValidateGhostIdsAsync(request);
            if (!ghostIdValidation.IsValid)
            {
                return ResponseFactory.Failure<int>(StatusCodeResponse.NotFound, ghostIdValidation.Message);
            }

            try
            {
                await _dbu.BeginTransactionAsync();

                var roomType = new RoomType
                {
                    HotelId = request.HotelId,
                    Name = request.Name,
                    Description = request.Description,
                    IsDeleted = false,
                    PricePerNight = request.PricePerNight,
                    AdultCapacity = request.AdultCapacity,
                    ChildCapacity = request.ChildCapacity,
                    Capacity = request.AdultCapacity + request.ChildCapacity,
                    UnitTypeId = request.UnitTypeId,
                    QualityId = request.QualityId,
                    RoomViewId = request.RoomViewId,
                    IsPrivateBathroom = request.IsPrivateBathroom,
                    HasBalcony = request.HasBalcony,
                    HasTerrace = request.HasTerrace,
                    CanAddExtraBed = request.CanAddExtraBed,
                    MaxExtraBeds = request.MaxExtraBeds,
                    AreaSqm = request.AreaSqm,
                    Additional = JsonSerializer.Serialize(new
                    {
                        BedTypes = request.BedTypes,
                        IsSmokingAllowed = request.IsSmokingAllowed,
                        TotalRooms = request.TotalRooms
                    })
                };

                await _roomTypeRepo.AddAsync(roomType);
                await _dbu.SaveChangesAsync();

                foreach (var bedType in request.BedTypes)
                {
                    var bedConfig = new RoomTypeBedConfig
                    {
                        RoomTypeId = roomType.Id,
                        BedTypeId = bedType.BedTypeId,
                        Quantity = bedType.Quantity
                    };
                    await _bedConfigRepo.AddAsync(bedConfig);
                }
                await _dbu.SaveChangesAsync();

                await _dbu.CommitTransactionAsync();

                return ResponseFactory.Success(roomType.Id, MessageResponse.Common.CREATE_SUCCESSFULLY);
            }
            catch (Exception)
            {
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.ServerError<int>();
            }
        }

        // --- HELPER FUNCTION: Convert child paged result to parent paged result ---
        private async Task<(bool IsValid, string Message)> ValidateGhostIdsAsync(RoomTypeCreateDTO request)
        {
            // Check Hotel
            var isHotelExisted = await _hotelRepo.AnyAsync(x => x.Id == request.HotelId);
            if (!isHotelExisted)
            {
                return (false, MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_NOT_FOUND);
            }
            // Check UnitType
            var isUnitTypeExisted = await _attributeFacade.IsUnitTypeExistedAsync(request.UnitTypeId);
            if (!isUnitTypeExisted)
            {
                return (false, MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_NOT_FOUND);
            }
            // Check Quality (if provided)
            if (request.QualityId.HasValue)
            {
                var isQualityExisted = await _attributeFacade.IsRoomQualityExistedAsync(request.QualityId.Value);
                if (!isQualityExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_NOT_FOUND);
                }
            }
            // Check RoomView (if provided)
            if (request.RoomViewId.HasValue)
            {
                var isRoomViewExisted = await _attributeFacade.IsRoomViewExistedAsync(request.RoomViewId.Value);
                if (!isRoomViewExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_NOT_FOUND);
                }
            }

            // Check BedTypes
            foreach (var bedType in request.BedTypes)
            {
                var isBedTypeExisted = await _attributeFacade.IsBedTypeExistedAsync(bedType.BedTypeId);
                if (!isBedTypeExisted)
                {
                    return (false, MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_NOT_FOUND);
                }
            }


            return (true, string.Empty);
        }
    }
}
