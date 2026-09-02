using System;
using System.Collections.Generic;

namespace ConferenceBooking.Application.DTOs.Bookings;

public record CreateBookingRequest(int RoomId, DateTime StartTime, DateTime EndTime, List<BookingServiceItem> Services);