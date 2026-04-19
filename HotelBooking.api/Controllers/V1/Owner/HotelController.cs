
using System.Security.Claims;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace HotelBooking.API.Controllers.V1.Owner
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Hotel Services")]
    public class HotelController : ControllerBase
    {

        public HotelController()
        {
            
        }


    }
}