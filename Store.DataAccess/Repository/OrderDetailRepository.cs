using Store.DataAccess.Data;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.DataAccess.Repository
{
	public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
	{
		private readonly ApplicationDbContext db;

		public OrderDetailRepository(ApplicationDbContext db) : base(db)
		{
			this.db = db;
		}

		public void Update(OrderDetail orderDetail)
		{
			db.OrderDetails.Update(orderDetail);
		}
	}
}
