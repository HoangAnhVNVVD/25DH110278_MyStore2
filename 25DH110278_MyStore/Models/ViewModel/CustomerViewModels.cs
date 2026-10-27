using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace _25DH110278_MyStore.Models.ViewModel
{
    public class HomeProductVM
    {
        public List<Product> FeaturedProducts { get; set; }
        public IPagedList<Product> NewProducts { get; set; }
        public string SearchTerm { get; set; }
        public int? CategoryID { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public List<Product> TopProducts { get; set; }
        public int Quantity { get; set; }
    }

    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public string CustomerName { get; set; }
        public string UserRole { get; set; }
        public int CartCount { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Mật khẩu phải từ 4 ký tự trở lên")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
