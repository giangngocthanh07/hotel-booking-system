using HotelBooking.application.DTOs.Booking;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace HotelBooking.application.Services.Domains.BookingManagement;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IUnitOfWork _dbu;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IBookingRepository bookingRepo,
        IUnitOfWork dbu,
        ILogger<PaymentService> logger)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
        _dbu = dbu;
        _logger = logger;
    }

    public async Task<ApiResponse<PaymentResponseDTO>> CreatePaymentUrlAsync(CreatePaymentRequestDTO request, int userId)
    {
        try
        {
            // 1. Validate Booking ownership
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null || booking.CustomerId != userId)
            {
                return ResponseFactory.Failure<PaymentResponseDTO>(StatusCodeResponse.NotFound, "Booking not found.");
            }

            // 2. Create Payment record
            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                PaymentMethod = request.PaymentMethod,
                TransactionId = $"TX-{Guid.NewGuid().ToString().Substring(0, 8)}", // Temporary until gateway returns one
                Status = "Pending",
                PaidAt = null,
                Additional = null
            };

            await _paymentRepo.AddAsync(payment);
            await _dbu.SaveChangesAsync();

            // 3. Generate Mock Payment URL (Integration with VNPay/Stripe would go here)
            string mockUrl = $"https://mock-gateway.com/pay?amount={booking.TotalPrice}&paymentId={payment.Id}";

            return ResponseFactory.Success(new PaymentResponseDTO
            {
                PaymentId = payment.Id,
                PaymentUrl = mockUrl,
                Status = payment.Status
            }, "Payment URL generated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment URL for booking {BookingId}", request.BookingId);
            return ResponseFactory.ServerError<PaymentResponseDTO>();
        }
    }

    public async Task<ApiResponse<bool>> ProcessPaymentCallbackAsync(PaymentCallbackDTO callback)
    {
        await _dbu.BeginTransactionAsync();
        try
        {
            // 1. Fetch Payment
            var payment = await _paymentRepo.GetByIdAsync(callback.PaymentId);
            if (payment == null) return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, "Payment record not found.");

            // 2. Fetch Booking
            var booking = await _bookingRepo.GetByIdAsync(payment.BookingId);
            if (booking == null) return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, "Booking record not found.");

            // 3. Update Statuses
            if (callback.IsSuccess)
            {
                payment.Status = "Success";
                payment.PaidAt = DateTime.Now;
                payment.TransactionId = callback.TransactionId;
                
                booking.Status = "Confirmed"; // Update booking status to confirmed
            }
            else
            {
                payment.Status = "Failed";
                payment.Additional = callback.Message;
                
                booking.Status = "PaymentFailed";
            }

            await _paymentRepo.UpdateAsync(payment);
            await _bookingRepo.UpdateAsync(booking);
            
            var saved = await _dbu.SaveChangesAsync() > 0;
            if (saved)
            {
                await _dbu.CommitTransactionAsync();
                return ResponseFactory.Success(true, callback.IsSuccess ? "Payment completed successfully." : "Payment marked as failed.");
            }
            else
            {
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.Failure<bool>(StatusCodeResponse.Error, "Database save failed.");
            }
        }
        catch (Exception ex)
        {
            await _dbu.RollBackTransactionAsync();
            _logger.LogError(ex, "Error processing payment callback for payment {PaymentId}", callback.PaymentId);
            return ResponseFactory.ServerError<bool>();
        }
    }
}
