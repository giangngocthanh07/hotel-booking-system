// hotel.types.ts
// TypeScript interfaces for hotel search feature.

// Maps to HotelSearchRequestDTO on the backend
export interface HotelSearchRequest {
  cityName: string;
  checkIn: string;   // ISO date string: "YYYY-MM-DD"
  checkOut: string;  // ISO date string: "YYYY-MM-DD"
  adults: number;
  children: number;
  rooms: number;
}

// Maps to SearchHotelResultDTO on the backend
export interface SearchHotelResult {
  id: number;
  name: string;
  address: string;
  description: string;
  cityName: string;
  countryName: string;
  coverImageUrl: string;
  priceFrom: number;
  maxAdultCapacity: number;
  maxChildCapacity: number;
  avgRating: number;
  reviewCount: number;
  availableRooms: number;
}
