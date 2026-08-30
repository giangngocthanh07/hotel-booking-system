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
