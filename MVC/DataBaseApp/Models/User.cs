using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;

namespace DataBaseApp.Models
{
    public class Person
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Имя не должно быть пустым")]
        [Display(Name="Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Возраст не должен быть пустым")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Пароль не должен быть пустым")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        
        [Display(Name = "Токен")]
        public string Token { get; set; }
        
        [Display(Name = "Роль")]
        public string Role { get; set; }
    }
    public class RoomMeta
    {
        [Key]
        [Display(Name="Название")]
        public string Name { get; set; }
        [Required]
        [Display(Name="Цена в рублях")]
        public int Price { get; set; }
    }

    public class RoomInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Тип")]
        public string Type { get; set; }
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public DateTime From { get; set; }
        
        [Required]
        public DateTime To { get; set; }

        public string GetTypeFromMeta(ApplicationContext db)
        {
            return db.RoomsInfo.Where(x => x.Id == RoomId).First().Type;
        }
    }
    
    public class AuthorizationData
    {
        [Required(ErrorMessage = "Имя не должно быть пустым")]
        [Display(Name="Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Пароль не должен быть пустым")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
    
    public class AuthorizePostData
    {
        public int StatusCode;
        public string ErrorMessage;
        public Person User;

        public AuthorizePostData()
        {
            StatusCode = 1;
            ErrorMessage = "Uninitialized AuthorizePostData";
        }

        public void ParseFromPersons(IQueryable<Person> persons, AuthorizationData data)
        {
            if (persons.Count() == 0)
            {
                StatusCode = 1;
                ErrorMessage = "Not registered user";
                return;
            }

            if (persons.Count() > 1)
            {
                StatusCode = 2;
                ErrorMessage = "Internal server error";
                return;
            }

            if (persons.First().Password != data.Password)
            {
                StatusCode = 3;
                ErrorMessage = "Invalid login or password";
                return;
            }
            
            StatusCode = 0;
            ErrorMessage = "";
            User = persons.First();
        }

        public bool IsSuccessful()
        {
            return StatusCode == 0;
        }
    }

    public class PersonalAccountData 
    {
        public int StatusCode;
        public string ErrorMessage;  
        public Person User;
        public IQueryable<Order> Orders;
        public ApplicationContext db;

        public PersonalAccountData(ApplicationContext dbLocal)
        {
            db = dbLocal;
        }
        public void ParseFromPersons(IQueryable<Person> persons)
        {

            
            if (persons.Count() == 0)
            {
                StatusCode = 1;
                ErrorMessage = "Not authorized user";
                return;
            }

            if (persons.Count() > 1)
            {
                StatusCode = 2;
                ErrorMessage = "Internal server error";
                return;
            }

            User = persons.First();
            Orders = db.Orders.Where(x => x.UserId == User.Id);
        }

        public List<Order> GetOrders()
        {
            if (User is null)
            {
                return null;
            }

            System.Console.WriteLine("UTA123");
            System.Console.WriteLine(Orders.ToList().Count());

            return Orders.ToList();
        }
    }
    

    public class OrderData
    {
        [Required(ErrorMessage = "Тип номера не должен быть пустым")]
        [Display(Name="Тип номера")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Дата заезда не должна быть пустой")]
        [Display(Name = "Дата заезда")]
        public DateTime From { get; set; }
        [Required(ErrorMessage = "Дата отъезда не должна быть пустой")]
        [Display(Name = "Дата отъезда")]
        public DateTime To { get; set; }
    }
    public class OrderPostData
    {
        public int StatusCode;
        public string ErrorMessage;  
        
        public int UserId;
        public string Type;
        public DateTime From;
        public DateTime To;

        public int TotalPrice;
        
        public OrderPostData()
        {
            StatusCode = 1;
            ErrorMessage = "Wrang OrderPostData";
        }

        public Order Process(ApplicationContext db, string token)
        {
            if (From > To)
            {
                StatusCode = 2;
                ErrorMessage = "Incorrect input dates";
                return null;
            }
            var persons = db.People.Where(x => x.Token == token && x.Token != "" && x.Token != null);
            if (persons.Count() > 1)
            {
                StatusCode = 3;
                ErrorMessage = "Internal server error";
                return null;
            }
            if (persons.Count() == 0)
            {
                StatusCode = 4;
                ErrorMessage = "Bad request";
                return null;
            }

            UserId = persons.First().Id;

            TotalPrice = db.RoomsMeta.Where(x => x.Name == Type).First().Price * ((To - From).Days);

            List<RoomInfo> candidates = db.RoomsInfo.Where(x => x.Type == Type).ToList();
            
            for (int i = 0; i < candidates.Count(); ++i)
            {
                var candidate = candidates[i];
                var concurrentOrders =
                    db.Orders.Where(x => x.RoomId == candidate.Id && !(x.From >= To || x.To <= From));
                if (concurrentOrders.Count() == 0)
                {
                    Order order = new Order();
                    order.UserId = UserId;
                    order.RoomId = candidate.Id;
                    order.From = From;
                    order.To = To;
                    
                    StatusCode = 0;
                    ErrorMessage = "";

                    return order;
                }
            }
            
            StatusCode = 5;
            ErrorMessage = "No available rooms";

            return null;
        }

        public bool IsSuccessful()
        {
            return StatusCode == 0;
        }
    }
}
