using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASBaseSearchResponse
    {
        bool Result { get; set; }
        string ErrorMessage { get; set; }

        string APIResult { get; set; }

    }
}
