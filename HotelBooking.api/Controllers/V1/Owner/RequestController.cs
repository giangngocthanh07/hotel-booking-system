using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Owner
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Request")]
    public class RequestController : ControllerBase
    {
        public RequestController()
        {
        }

    
    }
}