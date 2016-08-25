﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class BitcoinUtils
    {
        public static decimal SatoshiToBtc(double satoshi)
        {
            return (decimal)(satoshi * 0.00000001);
        }
    }
}
