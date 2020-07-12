using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace RazorPagesTwitchPubSub.Pages
{
    public class IndexModel : PageModel
    {

        public IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public CurrentUser user;
        public void OnGet()
        {

        }
        

    }
}
