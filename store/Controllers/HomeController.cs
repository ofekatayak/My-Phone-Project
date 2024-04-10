using Microsoft.AspNetCore.Mvc;
using store.Models;
using store.Services;
using MySqlConnector;
using store.ViewModels;

namespace store.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public HomeController(UserService userService,CartService cartService,OrderService orderService)
        {
            _userService = userService;
            _cartService = cartService;
            _orderService = orderService;
        }

        [Route("")]
        public async Task<IActionResult> HomeView()
        {
            var viewModel = await GetHomeViewModel();
            return View("HomeView", viewModel);
        }

        // get the HomeViewModel afet fetch all the data
        // u can pass to this function true if u want the view will pop up the pay side 
        private async Task<HomeViewModel> GetHomeViewModel(bool isPayVisible = false, bool isCartVisible = false)
        {
            var products = await _userService.GetProductsFromDatabase();
            var categories = await _userService.GetCategoriesFromDatabase();

            string email = HttpContext.Session.GetString("email") ?? "";
            var user = await _userService.GetUserFromDatabase(email);
            Notification notifies = await _userService.getUserNotification(email);
            var viewModel = new HomeViewModel
            {
                User = user,
                Products = products,
                Categories = categories,
                Cart = _cartService.GetCartFromSession(),
                IsPayVisible = isPayVisible,
                isCartVisible = isCartVisible,
                Notification = notifies
            };
            return viewModel;
        }

        public async Task<IActionResult> GetOrders()
        {
            string email = HttpContext.Session.GetString("email") ?? "";
            var orders = await _userService.GetOrdersWithProductDetails(email);
            
            return View("OrdersView" ,orders);
        }

        public async Task<IActionResult> SignUp(User newUser)
        {
            await _userService.AddUser(newUser);

            // Storeuser email in session
            HttpContext.Session.SetString("email", newUser.Email);

            // apply HomeView action
            return RedirectToAction("HomeView");
        }

        public async Task<IActionResult> Login(User newUser)
        {
            User user = await _userService.Login(newUser.Email, newUser.Password);

            // Storeuser email in session
            HttpContext.Session.SetString("email", newUser.Email);
            if (user.IsAdmin == true)
            {
                return RedirectToAction("GetProducts", "Admin");
            }
            else
            {
                return RedirectToAction("HomeView");
            }

        }

        public IActionResult Logout()
        {

            // Storeuser email in session
            HttpContext.Session.SetString("email", "");

            // apply HomeView action
            return RedirectToAction("HomeView");
        }

        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var emailExists = await _userService.CheckEmailExists(email);
            return Json(new { exists = emailExists });
        }

        public async Task<IActionResult> ValidateUser(string email, string password)
        {
            var userExists = await _userService.ValidateUser(email, password);
            return Json(new { exists = userExists });
        }

        //---Cart----
        public async Task <IActionResult> RemoveFromCart(string productId)
        {
            await _cartService.RemoveFromCart(productId);
            var viewModel = await GetHomeViewModel(false, true);
            return View("HomeView", viewModel);
        }

        public async Task<IActionResult> AddToCart(string productId)
        {

            await _cartService.AddToCart(productId);
            var viewModel = await GetHomeViewModel(false,true);
            return View("HomeView" , viewModel);
        }

        public async Task<IActionResult> DirectBuy(string productId)
        {
            await _cartService.AddToCart(productId);
            var viewModel = await GetHomeViewModel(true,false);
            return View("HomeView",viewModel);
        }

        public async Task<IActionResult> DecrementCartItem(string productId)
        {
            await _cartService.DecrementCartItem(productId);
            var viewModel = await GetHomeViewModel(false, true);
            return View("HomeView",viewModel);
        }

        //---Search---
        public async Task<IActionResult> Search(string query)
        {
            var products = await _userService.SearchProducts(query);
            var categories = await _userService.GetCategoriesFromDatabase();

            string email = HttpContext.Session.GetString("email") ?? "";
            var user = await _userService.GetUserFromDatabase(email);

            var viewModel = new HomeViewModel
            {
                User = user,
                Products = products,
                Categories = categories,
                Cart = _cartService.GetCartFromSession(),
            };

            return View("HomeView", viewModel);
        }

        public async Task<IActionResult> Filter(string query)
        {
            var products = _userService.GetFilteredProducts(query);
            var categories = await _userService.GetCategoriesFromDatabase();

            string email = HttpContext.Session.GetString("email") ?? "";
            var user = await _userService.GetUserFromDatabase(email);

            var viewModel = new HomeViewModel
            {
                User = user,
                Products = products,
                Categories = categories,
                Cart = _cartService.GetCartFromSession(),
            };

            return View("HomeView", viewModel);
        }

        //---Pay---
        public async Task<IActionResult> Pay(string email , string cardNumber, string expiryDate, string cvvCode, string userAddress, string userCity, string userZipCode)
        {
            Console.WriteLine("Pay");
            await _orderService.PlaceOrder(email);
            await _userService.AddCreditCardDetailsToDB(email, cardNumber, expiryDate, cvvCode);
            int ZipCode = Convert.ToInt32(userZipCode);
            await _userService.AddAddressToDB(email, userAddress, userCity, ZipCode);
            return RedirectToAction("HomeView");
        }

        //--Notification---
        public async Task<IActionResult> AddToNotification(string productId)
        {
            string email = HttpContext.Session.GetString("email") ?? "";
            if (email == "")
                return RedirectToAction("HomeView");
            var user = await _userService.GetUserFromDatabase(email);
            Task<bool> task = _userService.NotifyKeyCheck(productId, user.Email);
            bool flag = await task;
            if (flag == true)
            {
                return RedirectToAction("HomeView");
            }
            await _userService.Notify(productId, user.Email);
            return RedirectToAction("HomeView");
        }

        public async Task<IActionResult> RemoveFromNotification(string productId)
        {
            string email = HttpContext.Session.GetString("email");
            await _userService.RemoveNotify(productId, email);
            return RedirectToAction("HomeView");
        }
    }
}