using System.ComponentModel.DataAnnotations;
using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FSharp.Core;

namespace Web.Controllers
{
    public class Testowy
    {
        [Required]
        public string Bla { get; set; }
    }
    
    [ApiController]
    [Route("[controller]")]
    public class TestingController : ControllerBase
    {
        [BindProperty(SupportsGet = true)]
        public Testowy TestModel { get; set; }
        
        [HttpGet("bla")]
        public IActionResult Bla()
        {
            if (TestModel.Bla != null)
            {
                var d = Logic.Cos.Disc.NewDwa(TestModel.Bla);
                var r = new Cos.Rek(0, FSharpOption<string>.None);
                //var r = new Logic.Cos.Rek(0, "jeden");
                var res = Logic.Cos.fu(d, r);
                return Ok(res);
            }

            return BadRequest("no bla");
        }
    }
}