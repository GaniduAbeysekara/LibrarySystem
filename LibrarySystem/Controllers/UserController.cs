using LibrarySystem.Model;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
          return Ok();
        }


        [HttpPost]
        public IActionResult Create([FromBody] UserDetail user )
        {
            return Ok(user);
        }


    }


}
