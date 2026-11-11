export interface OwnerHotelSummary {
  id: number;
  name: string;
  address: string;
  coverImageUrl?: string | null;
  status?: string | null;
  isVerified?: boolean | null;
  createdAt?: string | null;
}

export interface OwnerHotelDetails extends OwnerHotelSummary {
  description?: string | null;
  propertyTypeId: number;
  propertyTypeName: string;
  countryId: number;
  countryName: string;
  provinceId: number;
  provinceName: string;
  wardId: number;
  wardName: string;
  publicPhone: string;
  publicEmail: string;
  starRating?: number | null;
  latitude?: number | null;
  longitude?: number | null;
  taxCode: string;
}

export interface OwnerHotelsPageData {
  items: OwnerHotelSummary[];
  totalCount: number;
  totalPages: number;
  pageIndex: number;
  pageSize: number;
}
