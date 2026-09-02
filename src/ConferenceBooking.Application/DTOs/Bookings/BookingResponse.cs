using System;
using System.Collections.Generic;

namespace ConferenceBooking.Application.DTOs.Bookings;

public record BookingResponse(int Id, int RoomId, string RoomName, DateTime StartTime, DateTime EndTime, string Status, decimal TotalPrice, List<BookingServiceResponseItem> Services);