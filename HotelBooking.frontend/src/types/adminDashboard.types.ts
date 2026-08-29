export interface RecentRequestSummary {
  id: number;
  title: string;
  requesterName: string;
  createdAt: string;
}

export interface RevenueTrend {
  monthName: string;
  amount: number;
}

export interface AdminDashboardStats {
  totalRevenue: number;
  totalUsers: number;
  totalHotels: number;
  totalBookings: number;
  pendingHotelRequests: RecentRequestSummary[];
  pendingUpgradeRequests: RecentRequestSummary[];
  monthlyRevenueTrend: RevenueTrend[];
}
