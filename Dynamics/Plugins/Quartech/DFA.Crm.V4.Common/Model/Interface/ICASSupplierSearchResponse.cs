﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASSupplierSearchResponse : ICASBaseSearchResponse
    {
        string Suppliers { get; set; }

    }
}
