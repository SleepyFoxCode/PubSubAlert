using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;

namespace PubSubAlert.Pages
{
    public class IndexModel : PageModel
    {

        public IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public CurrentUser UserObject = null;
        public void OnGet()
        {

        }
        

    }
}
