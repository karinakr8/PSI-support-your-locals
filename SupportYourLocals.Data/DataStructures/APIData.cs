using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class SellerInfo
    {
        public SellerData sellerData { get; set; }
        public double accuracy { get; set; } // How well does the seller match the query

        public SellerInfo () { }

        public SellerInfo (SellerData seller, double acc)
        {
            sellerData = seller;
            accuracy = acc;
        }
    }
}
