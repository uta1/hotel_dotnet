using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataBaseApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

   // using System.Data.Entity.Migrations;


namespace DataBaseApp.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext db;

        public HomeController(ApplicationContext context)
        {
            db = context;
            // Добавим пользователей, если их еще нет
            //if (db.People.Count() == 0)
            //{
            //    db.People.Add(new Person { Id = 1, Name = "Boris", Age = 22 });
            //    db.People.Add(new Person { Id = 2, Name = "Ivan", Age = 27 });
            //    db.People.Add(new Person { Id = 3, Name = "Alice", Age = 21 });
            //    db.SaveChanges();
            //}
        }

        public async Task<IActionResult> Index()
        {
            //HttpCookie cookie = new HttpCookie("username", UserName);
            //Response.Cookies.Add(cookie);
            //string d = Request.Cookies["username"];
            //Response.Cookies.Append("oka", "oval");
            //var cookie = new HttpCookie() 
            //{
            //    Name ="test_cookie", 
            //    Value = DateTime.Now.ToString("dd.MM.yyyy"),
            //    Expires = DateTime.Now.AddMinutes(10),
            //};
            //Response.SetCookie(cookie);
            //DropTableOperation("People");

            return View(await db.People.ToListAsync());
        }

        public async Task<IActionResult> ListPeople()
        {
            return View(await db.People.ToListAsync());
        }
        public async Task<IActionResult> ListRoomMeta()
        {
            return View(await db.RoomsMeta.ToListAsync());
        }
        public async Task<IActionResult> ListRoomInfo()
        {
            return View(await db.RoomsInfo.ToListAsync());
        }        
        public async Task<IActionResult> ListOrders()
        {
            return View(await db.Orders.ToListAsync());
        }

        public async Task<IActionResult> About()
        {
            return View();
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Authorize()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AuthorizePost(AuthorizationData data)
        {
            AuthorizePostData authorizePostData = new AuthorizePostData();

            if (ModelState.IsValid)
            {
                var persons = db.People.Where(x => x.Name == data.Name);
                authorizePostData.ParseFromPersons(persons, data);
            }

            if (authorizePostData.IsSuccessful())
            {
                authorizePostData.User.Token = "itstoken" + authorizePostData.User.Name;
                db.SaveChanges();
                HttpContext.Response.Cookies.Append("token", authorizePostData.User.Token);
            }

            return View(authorizePostData);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Person person)
        {
            if (ModelState.IsValid)
            {
                db.People.Add(person);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(person);
        }
        
        [HttpGet]
        public IActionResult CreateRoomMeta()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoomMeta(RoomMeta meta)
        {
            if (ModelState.IsValid)
            {
                db.RoomsMeta.Add(meta);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(meta);
        }
        
        [HttpGet]
        public IActionResult CreateRoomInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoomInfo(RoomInfo info)
        {
            if (ModelState.IsValid)
            {
                db.RoomsInfo.Add(info);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(info);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpGet]
        public IActionResult PersonalAccount()
        {
            PersonalAccountData personalAccountData = new PersonalAccountData(db);

            var persons = db.People.Where(x => HttpContext.Request.Cookies["token"] != null && x.Token == HttpContext.Request.Cookies["token"] && x.Token != "" && x.Token != null);
            
            personalAccountData.ParseFromPersons(persons);
            return View("PersonalAccount", personalAccountData);
        }

      /*  [HttpGet]
        public IActionResult Orders()
        {
            string token = HttpContext.Request.Cookies["token"];
            db.People.Where(x => x.Token == token);
            
            return View("Orders", HttpContext.Request.Cookies["token"]);
        }*/
      
      [HttpGet]
      public IActionResult Order()
      {
          
          return View("Order");
      }
      [HttpPost]
      public async Task<IActionResult> OrderPost(OrderData data)
      {
          OrderPostData orderPostData = new OrderPostData();
          orderPostData.Type = data.Type;
          orderPostData.From = data.From;
          orderPostData.To = data.To;
          
          Order order = orderPostData.Process(db, HttpContext.Request.Cookies["token"]);

          if (order != null)
          {
              db.Orders.Add(order);
              await db.SaveChangesAsync();
          }

          return View(orderPostData);
      }
    }
}
