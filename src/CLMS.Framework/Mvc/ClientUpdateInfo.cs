﻿using System;

namespace CLMS.Framework.Mvc
{
    public class ClientUpdateInfo
    {
        public object Instance
        {
            get;
            set;
        }
        public Func<object, object> DtoConverter
        {
            get;
            set;
        }
        public int Order
        {
            get;
            set;
        }
    }

    
}
