using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication2.Models
{
    public class BookSearch
    {
        //API Key
        private static string API_KEY = "AIzaSyBx75NhXDPwZzRf5VFTl4qJDH5l74_z0IE";

        //The custom search engine identifier
        private static string cx = "015598178761323117960:sbbkk2__0lo";

        public static BooksService service = new BooksService(
             new BaseClientService.Initializer
             {
                 ApplicationName = "ISBNBookSearch",
                 ApiKey = API_KEY,
             });

        public static Volume SearchISBN(string isbn)
        {
           
            var result = service.Volumes.List(isbn).Execute();
           
            if (result != null && result.Items != null)
            {
                var item = result.Items.FirstOrDefault();
                return item;
            }
            return null;
        }
    }
}
