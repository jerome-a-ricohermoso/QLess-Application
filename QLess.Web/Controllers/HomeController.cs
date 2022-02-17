using Microsoft.AspNetCore.Mvc;
using QLess.Web.Data;
using QLess.Web.Models;
using System.Diagnostics;

namespace QLess.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private QLessContext _qLessContext;
        private QLessViewModel _qLessViewModel;

        public HomeController(ILogger<HomeController> logger, QLessContext qLessContext)
        {
            _logger = logger;
            _qLessContext = qLessContext;
            _qLessViewModel = new QLessViewModel();
        }

        public IActionResult Index()
        {
            _qLessViewModel.TransportCards = _qLessContext.TransportCards.ToList();
            _qLessViewModel.TransportCards.ForEach(x => x.Balance = _qLessContext.Transactions.Where(y => y.TransportCardId == x.TransportCardId).OrderByDescending(x =>x.TransactionId).FirstOrDefault().Balance);
            return View(_qLessViewModel);
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult GetTransactions(int transportCardId)
        {
            _qLessViewModel.Transactions = _qLessContext.Transactions.Where(x => x.TransportCardId == transportCardId).OrderByDescending(y => y.TransactionId).ToList();
            return Json(_qLessViewModel.Transactions);
        }

        public IActionResult GetDetails(int transportCardId)
        {
            _qLessViewModel.TransportCards = _qLessContext.TransportCards.Where(x => x.TransportCardId == transportCardId).ToList();
            _qLessViewModel.TransportCards.ForEach(x => x.Balance = _qLessContext.Transactions.Where(y => y.TransportCardId == x.TransportCardId).OrderByDescending(x => x.TransactionId).FirstOrDefault().Balance);
            return Json(_qLessViewModel.TransportCards);
        }

        public IActionResult RegisterCard(int cardType, int idType, string idNum)
        {
            var pwdId = string.Empty;
            var scId = string.Empty;

            if (cardType == 1)
            {
                if (idNum.Length == 10)
                {
                    scId = idNum;
                }
                else if (idNum.Length == 12)
                {
                    pwdId = idNum;
                }
            }
            
            _qLessContext.TransportCards.Add(new TransportCard()
            {
                CreatedBy = "system",
                CreatedOn = DateTime.UtcNow,
                ExpiryDate = cardType == 1 ? DateTime.UtcNow.AddYears(3) : DateTime.UtcNow.AddYears(5),
                PWDIDNumber = pwdId,
                SeniorCitizenNumber = scId
            });
            _qLessContext.SaveChanges();

            _qLessContext.Transactions.Add(new Transaction()
            {
                CreatedBy = "system",
                CreatedOn = DateTime.UtcNow,
                Balance = cardType == 1 ? 500 : 100,
                TransactionType = "enroll",
                TransportCardId = _qLessContext.TransportCards.OrderByDescending(x => x.TransportCardId).FirstOrDefault().TransportCardId
            });
            _qLessContext.SaveChanges();

            return Json(1);
        }

        public IActionResult UseCard(int cardId)
        {
            decimal price = 15;

            var card = _qLessContext.TransportCards.Where(x => x.TransportCardId == cardId).FirstOrDefault();

            var cardType = 1;

            if(String.IsNullOrWhiteSpace(card.PWDIDNumber) && String.IsNullOrWhiteSpace(card.SeniorCitizenNumber))
            {
                cardType = 0;
            }

            if (cardType == 1)
            {
                decimal multiplier = 20;
                var countOfSameDay = 0;
                var transactions = _qLessContext.Transactions.Where(x => x.TransportCardId == cardId && x.TransactionType == "use").OrderByDescending(x => x.TransactionId).Take(5).ToList();

                foreach(var transaction in transactions)
                {
                    if(transaction.CreatedOn.Value.Date == DateTime.UtcNow.Date)
                    {
                        countOfSameDay++;
                    }
                }
                if(countOfSameDay != 0 && countOfSameDay <=4)
                {
                    multiplier = 23M;
                }
                price = 10M - ((10M * multiplier) / 100M);
            }

            var balance = _qLessContext.Transactions.Where(x => x.TransportCardId == cardId).OrderByDescending(y => y.TransactionId).FirstOrDefault().Balance - price;

            _qLessContext.Transactions.Add(new Transaction()
            {
                CreatedBy = "system",
                CreatedOn = DateTime.UtcNow,
                Balance = balance,
                TransactionType = "use",
                TransportCardId = cardId
            });

            card.ExpiryDate = cardType == 1 ? DateTime.UtcNow.AddYears(3) : DateTime.UtcNow.AddYears(5);

            _qLessContext.SaveChanges();

            return Json(1);
        }

        public IActionResult ReloadCard(int cardId, int amount)
        {

            var card = _qLessContext.TransportCards.Where(x => x.TransportCardId == cardId).FirstOrDefault();

            var cardType = 1;

            if (String.IsNullOrWhiteSpace(card.PWDIDNumber) && String.IsNullOrWhiteSpace(card.SeniorCitizenNumber))
            {
                cardType = 0;
            }

            var balance = _qLessContext.Transactions.Where(y => y.TransportCardId == cardId).OrderByDescending(x => x.TransactionId).FirstOrDefault().Balance + amount;

            _qLessContext.Transactions.Add(new Transaction()
            {
                CreatedBy = "system",
                CreatedOn = DateTime.UtcNow,
                Balance = balance,
                TransactionType = "reload",
                TransportCardId = cardId
            });

            card.ExpiryDate = cardType == 1 ? DateTime.UtcNow.AddYears(3) : DateTime.UtcNow.AddYears(5);

            _qLessContext.SaveChanges();

            return Json(1);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}