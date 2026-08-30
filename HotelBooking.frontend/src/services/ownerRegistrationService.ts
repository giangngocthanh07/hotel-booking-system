import { getAuthToken } from "./authService";
import { API_BASE_URL } from "../config/api";

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

export interface ApiResponse<T> {
  statusCode: string;
  message: string;
  content?: T;
  errors?: string[];
}

// Internal generic fetch helper
async function request<T>(endpoint: string, options: RequestInit = {}): Promise<ApiResponse<T>> {
  const token = getAuthToken();
  const headers = new Headers(options.headers || {});
  
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  // Set default content type if not provided and not FormData
  if (!headers.has("Content-Type") && !(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers
  });

  // Handle No Content (204)
  if (response.status === 204) {
    return { statusCode: "Success", message: "Success" };
  }

  const text = await response.text();
  let data: any;
  
  try {
    data = text ? JSON.parse(text) : null;
  } catch (err) {
    data = { statusCode: "Error", message: "Invalid JSON response" };
  }

  if (!response.ok) {
    throw {
      response: { data }
    };
  }

  return data as ApiResponse<T>;
}

export const getCountries = () => request<LocationItem[]>('/locations/countries');
export const getProvinces = (countryId: number) => request<LocationItem[]>(`/locations/countries/${countryId}/provinces`);
export const getWards = (provinceId: number) => request<LocationItem[]>(`/locations/provinces/${provinceId}/wards`);

export const getPropertyTypes = () => request<PropertyTypeItem[]>('/hotels/property-types');

export const uploadBusinessLicense = async (file: File): Promise<ApiResponse<any>> => {
  const formData = new FormData();
  formData.append('file', file);
  return request<any>('/files/business-licenses', {
    method: 'POST',
    body: formData
  });
};

export const submitRegistration = (data: HotelRegistrationDTO) => request<any>('/owner/hotel-registrations', {
  method: 'POST',
  body: JSON.stringify(data)
});

export const getMyRegistrations = () => request<any[]>('/owner/hotel-registrations/me');
