using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Models;
using WebApplication4.ViewModel;

namespace WebApplication4.Controllers
{
    public class ShoppingController : Controller
    {
        private ECartDBEntities1 objECartDBEntities1;
        private List<ShoppingCartModel> listOfShoppingCartModels;
        
        public ShoppingController()
        {
            objECartDBEntities1 = new ECartDBEntities1();
            listOfShoppingCartModels = new List<ShoppingCartModel>();
        }
        // GET: Shopping
        public ActionResult Index()
        {
            IEnumerable<ShoppingViewModel> listOfshoppingViewModels = (from objItem in objECartDBEntities1.Items
                                                                       join
                                                                       objCate in objECartDBEntities1.Categories
                                                                       on objItem.CategoryId equals objCate.CategoryId
                                                                       select new ShoppingViewModel()
                                                                       {
                                                                         ImagePath = objItem.ImagePath,
                                                                         ItemName = objItem.ItemName,
                                                                         Description = objItem.Description,
                                                                         ItemPrice = objItem.ItemPrice,
                                                                         ItemId = objItem.Itemid,
                                                                         Category = objCate.CategoryName,
                                                                         ItemCode = objItem.ItemCode,
                                                                       }
                                                                       ).ToList();
            return View(listOfshoppingViewModels);
        }
        [HttpPost]
        public JsonResult Index(String ItemId)
        {
            ShoppingCartModel objshoppingCartModel = new ShoppingCartModel();
            Item objItem = objECartDBEntities1.Items.Single(model => model.Itemid.ToString() == ItemId);
            if (Session["CartCounter"] != null)
            {
                listOfShoppingCartModels = Session["CartItem"] as List<ShoppingCartModel>;
            }
            if(listOfShoppingCartModels.Any(model => model.ItemId == ItemId))
            {
                objshoppingCartModel = listOfShoppingCartModels.Single(model => model.ItemId == ItemId);
                objshoppingCartModel.Quantity = objshoppingCartModel.Quantity + 1;
                objshoppingCartModel.Total = objshoppingCartModel.Quantity * objshoppingCartModel.UnitPrice;
            }
            else
            {
                objshoppingCartModel.ItemId = ItemId;
                objshoppingCartModel.ImagePath = objItem.ImagePath;
                objshoppingCartModel.ItemName = objItem.ItemName;
                objshoppingCartModel.Quantity = 1;
                objshoppingCartModel.Total = objItem.ItemPrice;
                objshoppingCartModel.UnitPrice = objItem.ItemPrice;
                listOfShoppingCartModels.Add(objshoppingCartModel);
            }
            Session["CartCounter"] = listOfShoppingCartModels.Count;
            Session["CartItem"] = listOfShoppingCartModels;

            return Json(new {Success = true, Counter = listOfShoppingCartModels .Count}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShoppingCart()
        {
            listOfShoppingCartModels = Session["CartItem"] as List<ShoppingCartModel>;
            return View(listOfShoppingCartModels);
        }

        [HttpPost]
        public ActionResult AddOrder()
        {
            int OrderId = 0;
            listOfShoppingCartModels = Session["CartItem"] as List<ShoppingCartModel>;
            Order orderObj = new Order()
            {
                OrderDate = DateTime.Now,
                OrderNumber = String.Format("{0:ddmmyyyyHHmmsss}",DateTime.Now)
            };
            objECartDBEntities1.Orders.Add(orderObj);
            objECartDBEntities1.SaveChanges();
            OrderId = orderObj.OrderId;

            foreach(var item in listOfShoppingCartModels)
            {
                OrderDetail objOrderDetail = new OrderDetail();
                objOrderDetail.Total = item.Total;
                objOrderDetail.ItemId = item.ItemId;
                objOrderDetail.OrderId = OrderId;
                objOrderDetail.Quantity = item.Quantity;
                objOrderDetail.UnitPrice = item.UnitPrice;
                objECartDBEntities1.OrderDetails.Add(objOrderDetail);
                objECartDBEntities1.SaveChanges();
            }
            Session["CartItem"] = null;
            Session["CartCounter"] = null;
            return RedirectToAction("Index");
        }
    }
}