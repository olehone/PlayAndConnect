using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PlayAndConnect.Components
{
    public class Metch : ViewComponent
    {
        public string Invoke()
        {
            return "Test";
        }
    }
}