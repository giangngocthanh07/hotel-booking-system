import { getAuthToken } from "./authService";
import type { ApiResponse } from "../types/auth.types";

const BASE_URL = "http://localhost:5083/api/v1";

function getHeaders() {
  const token = getAuthToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

export interface BaseAdminItem {
  id: number;
  name: string;
  description: string | null;
  isDeleted: boolean | null;
}

export interface AmenityItem extends BaseAdminItem {
  typeId: number;
}

export interface AmenityType extends BaseAdminItem {
  iconClass: string | null;
  iconColor: string | null;
}

// AMENITIES
export async function getAmenityTypes(): Promise<ApiResponse<AmenityType[]>> {
  const response = await fetch(`${BASE_URL}/admin/management/amenity-types`, { headers: getHeaders() });
  return response.json();
}

export async function getAmenities(pageIndex = 1, pageSize = 50, typeId?: number): Promise<ApiResponse<PagedResponse<AmenityItem>>> {
  let url = `${BASE_URL}/admin/management/amenities?pageIndex=${pageIndex}&pageSize=${pageSize}`;
  if (typeId) {
    url += `&typeId=${typeId}`;
  }
  const response = await fetch(url, { headers: getHeaders() });
  return response.json();
}

export async function createAmenity(data: { name: string, description: string, typeId: number }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/amenities`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updateAmenity(id: number, data: { name: string, description: string }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/amenities/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deleteAmenity(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/amenities/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}

// ROOM ATTRIBUTES (Bed Types for instance)
export async function getBedTypes(pageIndex = 1, pageSize = 50): Promise<ApiResponse<PagedResponse<BaseAdminItem>>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types?pageIndex=${pageIndex}&pageSize=${pageSize}`, { headers: getHeaders() });
  return response.json();
}

export async function createBedType(data: { name: string, description: string }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updateBedType(id: number, data: { name: string, description: string }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deleteBedType(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}
// POLICIES
export interface PolicyType extends BaseAdminItem {}

export interface PolicyItem extends BaseAdminItem {
  typeId: number;
  discriminator: "checkInOut" | "cancellation" | "children" | "pets";
  
  // checkInOut
  checkInTime?: string;
  checkOutTime?: string;
  earlyCheckInFee?: number;
  lateCheckOutFee?: number;
  
  // cancellation
  daysBeforeCheckIn?: number;
  refundPercent?: number;
  isRefundable?: boolean;
  
  // children
  minAge?: number;
  maxAge?: number;
  extraBedFee?: number;
  
  // pets
  petFee?: number;
  isPetAllowed?: boolean;
}

export async function getPolicyTypes(): Promise<ApiResponse<PolicyType[]>> {
  const response = await fetch(`${BASE_URL}/admin/management/policy-types`, { headers: getHeaders() });
  return response.json();
}

export async function getPolicies(pageIndex = 1, pageSize = 50, typeId?: number): Promise<ApiResponse<PagedResponse<PolicyItem>>> {
  let url = `${BASE_URL}/admin/management/policies?pageIndex=${pageIndex}&pageSize=${pageSize}`;
  if (typeId) {
    url += `&typeId=${typeId}`;
  }
  const response = await fetch(url, { headers: getHeaders() });
  return response.json();
}

export async function createPolicy(type: "check-in-out" | "cancellation" | "pet" | "children", data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/policies/${type}`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updatePolicy(id: number, type: "check-in-out" | "cancellation" | "pet" | "children", data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/policies/${type}/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deletePolicy(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/policies/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}

// SERVICES
export interface ServiceType extends BaseAdminItem {}

export interface ServiceItem extends BaseAdminItem {
  typeId: number;
  discriminator: "standard" | "airportTransfer" | "airport"; // Usually server uses standard/airportTransfer for output
  price: number;
  
  // standard
  unit?: string;
  
  // airportTransfer
  isOneWayPaid?: boolean;
  hasRoundTrip?: boolean;
  isRoundTripPaid?: boolean;
  roundTripPrice?: number;
  hasNightFee?: boolean;
  additionalFee?: number;
  additionalFeeStartTime?: string;
  additionalFeeEndTime?: string;
  maxPassengers?: number;
  maxLuggage?: number;
}

export async function getServiceTypes(): Promise<ApiResponse<ServiceType[]>> {
  const response = await fetch(`${BASE_URL}/admin/management/service-types`, { headers: getHeaders() });
  return response.json();
}

export async function getServices(pageIndex = 1, pageSize = 50, typeId?: number): Promise<ApiResponse<PagedResponse<ServiceItem>>> {
  let url = `${BASE_URL}/admin/management/services?pageIndex=${pageIndex}&pageSize=${pageSize}`;
  if (typeId) {
    url += `&typeId=${typeId}`;
  }
  const response = await fetch(url, { headers: getHeaders() });
  return response.json();
}

export async function createService(type: "standard" | "airport-transfer", data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/services/${type}`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updateService(id: number, type: "standard" | "airport-transfer", data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/services/${type}/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deleteService(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/services/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}

// ROOM VIEWS
export interface RoomViewItem extends BaseAdminItem {}

export async function getRoomViews(): Promise<ApiResponse<RoomViewItem[]>> {
  const response = await fetch(`${BASE_URL}/admin/management/room-views`, { headers: getHeaders() });
  return response.json();
}

export async function createRoomView(data: { name: string, description?: string }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/room-views`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updateRoomView(id: number, data: { name: string, description?: string }): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/room-views/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deleteRoomView(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/room-views/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}

// BED TYPES
export interface BedTypeItem extends BaseAdminItem {
  defaultCapacity: number;
  minWidth: number;
  maxWidth: number;
  isVaryingSize: boolean;
  sizeDisplay: string;
}

export async function getBedTypes(): Promise<ApiResponse<BedTypeItem[]>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types`, { headers: getHeaders() });
  return response.json();
}

export async function createBedType(data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function updateBedType(id: number, data: any): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types/${id}`, {
    method: "PUT",
    headers: getHeaders(),
    body: JSON.stringify(data)
  });
  return response.json();
}

export async function deleteBedType(id: number): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/admin/management/bed-types/${id}`, {
    method: "DELETE",
    headers: getHeaders()
  });
  return response.json();
}
