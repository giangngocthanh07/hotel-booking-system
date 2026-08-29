// admin.types.ts

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

export interface BaseRequest {
  requestId: number;
  status: string;
  requestedAt: string;
  processedAt?: string;
  processedBy?: number;
  processedByName?: string;
  typeDisplay: string;
  canApprove: boolean;
  canReject: boolean;
  requesterName: string;
}

export interface HotelApprovalRequest extends BaseRequest {
  hotelId?: number;
  name: string;
  address: string;
  taxCode: string;
  businessLicenseUrl: string;
  ownerId: number;
  ownerFullName: string;
  ownerEmail: string;
  ownerPhoneNumber: string;
  ownerAddress: string;
  adminRemark?: string;
  description?: string;
  propertyTypeName: string;
  starRating?: number;
  publicPhone: string;
  publicEmail: string;
  provinceName: string;
  wardName: string;
  countryName: string;
}

export interface UpgradeRequest extends BaseRequest {
  userId: number;
  userName: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  taxCode: string;
}
