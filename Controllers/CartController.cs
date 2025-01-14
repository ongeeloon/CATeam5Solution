﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CATeam5Solution.Models;
using CATeam5Solution.Method;

namespace CATeam5Solution.Controllers
{
    public class CartController : Controller
    {

        private DBContext dbContext;

        public CartController(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CheckOut()
        {
            Session session = GetSession();
            if (session == null)
            {
                return RedirectToAction("Index", "Login");
            }
            Guid userid = session.UsersId;
            TempData["Alert"] = "Successfully CheckOut!";
            CreateOrder orderMaker = new CreateOrder(userid,dbContext);
            orderMaker.MakeOrder();
            return RedirectToAction("Index", "MyPurchase");
        }
       
        [Route("Cart")]
        public IActionResult ViewCart()
        {
            Session session = GetSession();
            if (session == null)
            {
               
                ViewData["cart"] = new List<CartItem>();
                return View();
            }

            Guid userid = session.Users.Id;

            List<CartItem> cartItems = dbContext.CartItem.Where(x => x.UsersId == userid).ToList();
            ViewData["cart"] = cartItems;

            string userCartAmt = cartItems.Sum(x => x.Quantity * x.Product.UnitPrice).ToString("#,0.00");

            ViewData["userCartAmt"] = userCartAmt;

            return View();


        }

        public IActionResult Update([FromBody] UpdateCart values)
        {
            string username = Request.Cookies["Username"];
            Users user = dbContext.Users.FirstOrDefault(x => x.UserName == username);
            Guid userid = user.Id;
            int newquantity;
            string userCartAmt;

            newquantity = values.Quantity;
                                   

                CartItem cartItem = dbContext.CartItem.FirstOrDefault(x => x.UsersId == userid && x.Product.ProductID == values.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity = newquantity;
                }

                dbContext.SaveChanges();
                
                double amt = dbContext.CartItem.Where(x => x.UsersId == userid).Sum(x => x.Quantity * x.Product.UnitPrice);

                userCartAmt = Math.Round(amt, 2).ToString("#,0.00");

                return Json(new 
                { status = "success",
                  userCartAmt
                });
            

        }


        public IActionResult Remove([FromBody] RemoveCart item)
        {
            string username = Request.Cookies["Username"];
            Users user = dbContext.Users.FirstOrDefault(x => x.UserName == username);
            Guid userid = user.Id;
            int productId = item.ProductId;

            CartItem cartItem = dbContext.CartItem.FirstOrDefault(x => x.UsersId == userid && x.Product.ProductID == item.ProductId);

            dbContext.Remove(cartItem);

            dbContext.SaveChanges();

            double amt = dbContext.CartItem.Where(x => x.UsersId == userid).Sum(x => x.Quantity * x.Product.UnitPrice);

            string userCartAmt = Math.Round(amt, 2).ToString("#,0.00");



            return Json(new
            {
                status = "success",
                userCartAmt
            });
        }

        private Session GetSession()
        {
            if (Request.Cookies["SessionId"] == null)
            {
                return null;
            }

            Guid sessionId = Guid.Parse(Request.Cookies["SessionId"]);
            Session session = dbContext.Session.FirstOrDefault(x =>
                x.Id == sessionId
            );

            return session;
        }


    }
}
