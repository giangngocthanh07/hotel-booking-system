import { get, post, postFormData } from './api';
import type { ApiResponse } from './api';

export interface LocationItem {
  id: number;
  name: string;
}

export interface PropertyTypeItem {
  id: number;
  name: string;
}

export interface HotelRegistrationDTO {
  name: string;
  description: string;
  address: string;
  propertyTypeId: number;
  propertyTypeName?: string;
  starRating?: number;
  publicPhone: string;
  publicEmail: string;
  countryId: number;
  countryName?: string;
  provinceId: number;
  provinceName?: string;
  wardId: number;
  wardName?: string;
  latitude?: number;
  longitude?: number;
  taxCode: string;
  businessLicenseUrl: string;
}

export const getCountries = () => get<LocationItem[]>('/api/v1/locations/countries');
export const getProvinces = (countryId: number) => get<LocationItem[]>(`/api/v1/locations/countries/${countryId}/provinces`);
export const getWards = (provinceId: number) => get<LocationItem[]>(`/api/v1/locations/provinces/${provinceId}/wards`);

export const getPropertyTypes = () => get<PropertyTypeItem[]>('/api/v1/hotels/property-types');

export const uploadBusinessLicense = async (file: File): Promise<ApiResponse<any>> => {
  const formData = new FormData();
  formData.append('file', file);
  return postFormData<any>('/api/v1/files/business-licenses', formData);
};

export const submitRegistration = (data: HotelRegistrationDTO) => post<any>('/api/v1/owner/hotel-registrations', data);
export const getMyRegistrations = () => get<any[]>('/api/v1/owner/hotel-registrations/me');
