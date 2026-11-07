-- FR-ONB-016 / FR-CAT-006/007: public search requires approval, Active state and no soft deletion.
-- Database-first deployment: execute against the intended application database, not master.
-- CityName remains the API alias for Province name; retain existing pricing/capacity/overlap logic.
CREATE OR ALTER PROCEDURE dbo.sp_SearchHotels
    @CityName NVARCHAR(100),
    @CheckIn DATE = NULL,
    @CheckOut DATE = NULL,
    @Adults INT = NULL,
    @Children INT = NULL,
    @Rooms INT = NULL
AS
BEGIN
    SET NOCOUNT ON;


    SELECT
        h.Id,
        h.Name,
        h.Address,
        c.Name AS CityName,
        co.Name AS CountryName,
        h.CoverImageUrl,
        CAST(MIN(rt.PricePerNight) AS DECIMAL(18,2)) AS PriceFrom,
        CAST(MAX(rt.AdultCapacity) AS INT) AS MaxAdultCapacity,
        CAST(MAX(rt.ChildCapacity) AS INT) AS MaxChildCapacity,
        CAST(ISNULL(rv.AvgRating, 0) AS DECIMAL(18,2)) AS AvgRating,
        CAST(ISNULL(rv.ReviewCount, 0) AS INT) AS ReviewCount,
        CAST(ISNULL(ar.AvailableRooms, 0) AS INT) AS AvailableRooms
    FROM Hotels h
    JOIN Provinces c ON h.ProvinceId = c.Id
    JOIN Countries co ON h.CountryId = co.Id
    JOIN RoomTypes rt ON h.Id = rt.HotelId
    JOIN Rooms rm ON rt.Id = rm.RoomTypeId


    -- Subquery review
    LEFT JOIN (
        SELECT HotelId,
               CAST(AVG(Rating * 1.0) AS DECIMAL(18,2)) AS AvgRating,
               COUNT(Id) AS ReviewCount
        FROM Reviews
        GROUP BY HotelId
    ) rv ON h.Id = rv.HotelId


    -- Subquery available rooms
    LEFT JOIN (
        SELECT rt.HotelId,
               COUNT(rm.Id) AS AvailableRooms
        FROM RoomTypes rt
        JOIN Rooms rm ON rt.Id = rm.RoomTypeId
        WHERE rm.IsDeleted = 0
          AND NOT EXISTS (
              SELECT 1
              FROM BookingRooms br
              JOIN Bookings b ON br.BookingId = b.Id
              WHERE br.RoomId = rm.Id
                AND b.Status IN ('Confirmed', 'Pending')
                -- Check overlap logic
                AND (b.CheckInDate < @CheckOut AND b.CheckOutDate > @CheckIn)
          )
        GROUP BY rt.HotelId
    ) ar ON h.Id = ar.HotelId


    WHERE h.IsVerified = 1 AND h.Status = 'Active'
      AND (h.IsDeleted = 0 OR h.IsDeleted IS NULL)
      AND c.Name LIKE '%' + @CityName + '%'
      AND (
     ( (@Adults IS NULL OR rt.AdultCapacity >= @Adults)
       AND (@Children IS NULL OR rt.ChildCapacity >= @Children) )
     OR (@Adults IS NOT NULL OR @Children IS NOT NULL) 
        AND ((ISNULL(@Adults,0) + ISNULL(@Children,0)) <= (rt.AdultCapacity + rt.ChildCapacity))
)


    GROUP BY h.Id, h.Name, h.Address, c.Name, co.Name, h.CoverImageUrl,
             rv.AvgRating, rv.ReviewCount, ar.AvailableRooms
    HAVING (@Rooms IS NULL OR ISNULL(ar.AvailableRooms, 0) >= @Rooms)
    ORDER BY rv.AvgRating DESC, PriceFrom ASC;
END
