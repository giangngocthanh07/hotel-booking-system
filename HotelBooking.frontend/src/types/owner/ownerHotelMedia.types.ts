export interface OwnerHotelMedia {
  hotelId: number;
  coverImageUrl: string | null;
  gallery: { id: number; imageUrl: string; displayOrder: number }[];
}

export type HotelImageKind = "cover" | "gallery";
