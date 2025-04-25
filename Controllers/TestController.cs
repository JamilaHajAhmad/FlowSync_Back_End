using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public static void RunTest()
        {
            Console.WriteLine("تم تنفيذ التست بنجاح ");
        }
    }
}
