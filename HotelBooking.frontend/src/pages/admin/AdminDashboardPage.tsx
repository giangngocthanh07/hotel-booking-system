import { useEffect, useState } from "react";
import { 
  Box, Grid, Card, CardContent, Typography, 
  CircularProgress, List, ListItem, ListItemText, 
  ListItemAvatar, Avatar, Divider, Paper, useTheme 
} from "@mui/material";
import AttachMoneyIcon from "@mui/icons-material/AttachMoney";
import PeopleIcon from "@mui/icons-material/People";
import HotelIcon from "@mui/icons-material/Hotel";
import BookOnlineIcon from "@mui/icons-material/BookOnline";
import AssignmentIcon from "@mui/icons-material/Assignment";
import { getDashboardOverview } from "../../services/adminService";
import type { AdminDashboardStats } from "../../types/adminDashboard.types";

export default function AdminDashboardPage() {
  const theme = useTheme();
  const [stats, setStats] = useState<AdminDashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadStats() {
      try {
        const res = await getDashboardOverview();
        if (res.statusCode === "Success" && res.content) {
          setStats(res.content);
        } else {
          setError(res.message || "Failed to load dashboard data");
        }
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : "Network error");
      } finally {
        setLoading(false);
      }
    }
    loadStats();
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
        <CircularProgress sx={{ color: '#EC4899' }} />
      </Box>
    );
  }

  if (error || !stats) {
    return (
      <Box p={3}>
        <Typography color="error">{error || "No data available."}</Typography>
      </Box>
    );
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  };

  const statCards = [
    { title: "Total Users", value: stats.totalUsers, icon: <PeopleIcon sx={{ fontSize: 40, color: '#3B82F6' }} />, gradient: "linear-gradient(135deg, rgba(59,130,246,0.1), rgba(59,130,246,0.05))" },
    { title: "Total Hotels", value: stats.totalHotels, icon: <HotelIcon sx={{ fontSize: 40, color: '#F59E0B' }} />, gradient: "linear-gradient(135deg, rgba(245,158,11,0.1), rgba(245,158,11,0.05))" },
    { title: "Total Bookings", value: stats.totalBookings, icon: <BookOnlineIcon sx={{ fontSize: 40, color: '#10B981' }} />, gradient: "linear-gradient(135deg, rgba(16,185,129,0.1), rgba(16,185,129,0.05))" },
    { title: "Revenue", value: formatCurrency(stats.totalRevenue), icon: <AttachMoneyIcon sx={{ fontSize: 40, color: '#EC4899' }} />, gradient: "linear-gradient(135deg, rgba(236,72,153,0.1), rgba(236,72,153,0.05))" },
  ];

  return (
    <Box sx={{ p: 3, maxWidth: 1200, mx: 'auto' }}>
      <Typography 
        variant="h4" 
        sx={{ 
          mb: 4, 
          fontWeight: 800, 
          background: 'linear-gradient(135deg, #3B82F6, #EC4899)', 
          WebkitBackgroundClip: 'text', 
          WebkitTextFillColor: 'transparent' 
        }}
      >
        Admin Dashboard
      </Typography>

      {/* KPI Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statCards.map((card, idx) => (
          <Grid item xs={12} sm={6} md={3} key={idx}>
            <Card sx={{ 
              borderRadius: 4, 
              boxShadow: '0 4px 20px rgba(0,0,0,0.05)',
              background: card.gradient,
              border: '1px solid rgba(255,255,255,0.5)',
              backdropFilter: 'blur(10px)',
              transition: 'transform 0.2s',
              '&:hover': { transform: 'translateY(-4px)' }
            }}>
              <CardContent sx={{ display: 'flex', alignItems: 'center', p: 3, '&:last-child': { pb: 3 } }}>
                <Box sx={{ flexGrow: 1 }}>
                  <Typography variant="overline" sx={{ color: 'text.secondary', fontWeight: 600, fontSize: '0.75rem' }}>
                    {card.title}
                  </Typography>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: 'text.primary', mt: 0.5 }}>
                    {card.value}
                  </Typography>
                </Box>
                {card.icon}
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Pending Requests Lists */}
      <Grid container spacing={4}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 0, borderRadius: 4, overflow: 'hidden', boxShadow: '0 4px 20px rgba(0,0,0,0.03)' }}>
            <Box sx={{ p: 3, borderBottom: '1px solid', borderColor: 'divider', background: '#F8FAFC' }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A' }}>
                🏨 Pending Hotel Approvals
              </Typography>
            </Box>
            <List sx={{ p: 0 }}>
              {stats.pendingHotelRequests.length === 0 ? (
                <ListItem sx={{ p: 4, justifyContent: 'center' }}>
                  <Typography color="text.secondary">No pending hotel requests</Typography>
                </ListItem>
              ) : (
                stats.pendingHotelRequests.map((req, idx) => (
                  <div key={req.id}>
                    <ListItem sx={{ py: 2, px: 3, '&:hover': { bgcolor: '#F1F5F9' } }}>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'rgba(59,130,246,0.1)', color: '#3B82F6' }}>
                          <AssignmentIcon />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={<Typography sx={{ fontWeight: 600 }}>{req.title}</Typography>}
                        secondary={`Owner: ${req.requesterName} • ${new Date(req.createdAt).toLocaleDateString()}`}
                      />
                    </ListItem>
                    {idx < stats.pendingHotelRequests.length - 1 && <Divider />}
                  </div>
                ))
              )}
            </List>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 0, borderRadius: 4, overflow: 'hidden', boxShadow: '0 4px 20px rgba(0,0,0,0.03)' }}>
            <Box sx={{ p: 3, borderBottom: '1px solid', borderColor: 'divider', background: '#F8FAFC' }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A' }}>
                👤 Pending Owner Upgrades
              </Typography>
            </Box>
            <List sx={{ p: 0 }}>
              {stats.pendingUpgradeRequests.length === 0 ? (
                <ListItem sx={{ p: 4, justifyContent: 'center' }}>
                  <Typography color="text.secondary">No pending owner upgrades</Typography>
                </ListItem>
              ) : (
                stats.pendingUpgradeRequests.map((req, idx) => (
                  <div key={req.id}>
                    <ListItem sx={{ py: 2, px: 3, '&:hover': { bgcolor: '#F1F5F9' } }}>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'rgba(236,72,153,0.1)', color: '#EC4899' }}>
                          <PeopleIcon />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={<Typography sx={{ fontWeight: 600 }}>{req.title}</Typography>}
                        secondary={`User: ${req.requesterName} • ${new Date(req.createdAt).toLocaleDateString()}`}
                      />
                    </ListItem>
                    {idx < stats.pendingUpgradeRequests.length - 1 && <Divider />}
                  </div>
                ))
              )}
            </List>
          </Paper>
        </Grid>
      </Grid>
      
      {/* Monthly Revenue Trend */}
      {stats.monthlyRevenueTrend.length > 0 && (
        <Box sx={{ mt: 4 }}>
          <Paper sx={{ p: 3, borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.03)' }}>
            <Typography variant="h6" sx={{ fontWeight: 700, mb: 3, color: '#0F172A' }}>
              📈 Revenue Trend (Last 6 Months)
            </Typography>
            <Grid container spacing={2} sx={{ alignItems: 'flex-end', height: '200px' }}>
              {stats.monthlyRevenueTrend.map((trend, idx) => {
                const maxAmount = Math.max(...stats.monthlyRevenueTrend.map(t => t.amount), 1);
                const heightPercent = `${(trend.amount / maxAmount) * 100}%`;
                return (
                  <Grid item xs key={idx} sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'flex-end', height: '100%' }}>
                    <Box 
                      sx={{ 
                        width: '40px', 
                        height: heightPercent, 
                        minHeight: '4px',
                        background: 'linear-gradient(180deg, #EC4899 0%, #3B82F6 100%)',
                        borderRadius: '8px 8px 0 0',
                        mb: 1,
                        transition: 'height 1s ease-out'
                      }} 
                    />
                    <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
                      {trend.monthName}
                    </Typography>
                  </Grid>
                );
              })}
            </Grid>
          </Paper>
        </Box>
      )}
    </Box>
  );
}
