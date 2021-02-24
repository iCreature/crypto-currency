using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neptune_Wallet.Api;
using Neptune_Wallet.Models;

namespace Neptune_Wallet.Controllers
{
    public class HomeController : Controller
    {
        private static CryptoCurrency blockchain = BlockChainController.blockchain;// new currency
        public IActionResult Index()
        {
            //List<Transaction> transactions = blockchain.GetTransactions();
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
