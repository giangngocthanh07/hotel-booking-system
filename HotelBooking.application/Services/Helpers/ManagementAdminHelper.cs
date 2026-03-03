using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.application.Helpers;
using HotelBooking.application.Validators.Common;

public static class ManagementAdminHelper
{
    // --- PART 1: MENU API HELPER (Retrieve list of Types) ---
    public static async Task<ApiResponse<ManageMenuResult>> GetTypesForMenuAsync<TTypeEntity, TTypeRepo>(
        TTypeRepo typeRepo,
        // Filter: e.g. fetch only non-deleted types
        Expression<Func<TTypeEntity, bool>> activeTypeFilter,
        // Func to extract Id
        Func<TTypeEntity, int> getTypeIdFunc,
        // Func to extract Name
        Func<TTypeEntity, string> getTypeNameFunc)

        where TTypeRepo : IRepository<TTypeEntity>
        where TTypeEntity : class
    {
        try
        {
            // 1. Query DB for types
            var typeEntities = await typeRepo.WhereAsync(activeTypeFilter);

            // 2. Map to DTO
            var menuResult = new ManageMenuResult
            {
                Types = typeEntities.Select(t => new ManageTypeDTO
                {
                    Id = getTypeIdFunc(t),
                    Name = getTypeNameFunc(t)
                }).ToList()
            };

            // 3. Select default (optional hint for FE)
            // Suggest the first item as the default selection
            menuResult.DefaultSelectedId = menuResult.Types.FirstOrDefault()?.Id;

            return ResponseFactory.Success(menuResult, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<ManageMenuResult>();
        }
    }

    // --- PART 2: DATA API HELPER (Retrieve items by TypeId) ---
    public static async Task<ApiResponse<PagedManageResult<TDto>>> GetDataByTypeAsync<TEntity, TDto>(
        int? typeId,
        PagingRequest paging, // 1. Receive pagination parameters

        // If user did not provide typeId -> run this logic to get the default ID
        // Logic to retrieve default ID (when typeId == null)
        Func<Task<int?>> getDefaultIdFunc,

        // If user provided typeId -> run this logic to check existence
        // Logic to verify whether the ID exists in DB (Input: int -> Output: bool)
        Func<int, Task<bool>> checkTypeExistsFunc,

        // 2. Function to fetch paged data, returns Tuple (Items, TotalCount)
        Func<int, int, int, Task<(IEnumerable<TEntity> Items, int TotalCount)>> getPagedItemsFunc,

        // Logic to map from Entity to DTO
        Func<TEntity, TDto> mapToDtoFunc)

        where TEntity : class
        where TDto : class
    {
        try
        {
            // [STEP 1] Validate pagination using FluentValidation
            // Since this is a static helper, instantiate the validator directly (Safe & Fast)
            var pagingValidator = new PagingRequestValidator();
            var validationResult = await pagingValidator.ValidateAsync(paging);

            if (!validationResult.IsValid)
            {
                // Return the first validation error
                return ResponseFactory.Failure<PagedManageResult<TDto>>(
                    StatusCodeResponse.BadRequest,
                    validationResult.Errors[0].ErrorMessage
                );
            }

            // [STEP 2] Validate TypeId (simple manual check for performance)
            if (typeId.HasValue && typeId <= 0)
            {
                return ResponseFactory.Failure<PagedManageResult<TDto>>(
                    StatusCodeResponse.BadRequest,
                    MessageResponse.Validation.INVALID_TYPE_ID
                );
            }

            int currentTypeId;

            // --- RESOLVE TYPE ID ---
            if (typeId.HasValue)
            {
                // Case A: User provided an ID -> verify it exists in DB
                bool isExist = await checkTypeExistsFunc(typeId.Value);

                if (!isExist)
                {
                    // VALIDATION: ID does not exist in DB
                    return ResponseFactory.Failure<PagedManageResult<TDto>>(
                        StatusCodeResponse.NotFound,
                        MessageResponse.Common.NOT_FOUND
                    );
                }

                currentTypeId = typeId.Value;
            }
            else
            {
                // Case B: User did not provide typeId (null) -> fetch default
                var defaultId = await getDefaultIdFunc();

                if (defaultId.HasValue)
                {
                    currentTypeId = defaultId.Value;
                }
                else
                {
                    // Case C: No types exist in DB -> return empty list (valid state)
                    return ResponseFactory.Success(
                        new PagedManageResult<TDto>(new List<TDto>(), 0, paging.PageIndex.Value, paging.PageSize.Value, null),
                        MessageResponse.Common.EMPTY_LIST
                    );
                }
            }

            // 3. FETCH PAGED DATA (call repository)
            var (entities, totalCount) = await getPagedItemsFunc(currentTypeId, paging.PageIndex.Value, paging.PageSize.Value);

            // 4. MAP TO DTO
            var dtos = entities.Select(e => mapToDtoFunc(e)).ToList();

            // 5. WRAP RESULT (using PagedManageResult)
            // Pass currentTypeId so FE knows which menu item to highlight
            var result = new PagedManageResult<TDto>(dtos, totalCount, paging.PageIndex.Value, paging.PageSize.Value, currentTypeId);

            return ResponseFactory.Success(result, MessageResponse.Common.GET_SUCCESSFULLY);
        }
        catch (Exception)
        {
            return ResponseFactory.ServerError<PagedManageResult<TDto>>();
        }
    }
}
