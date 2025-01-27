﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagment.Data
{
    public class CommentViewModel
    {
        public string Content { get; set; }
        public bool IsCurrentUserComment { get; set; }
        public string UserName { get; set; } // Имя пользователя, оставившего комментарий
        public string userId { get; set; } // ID пользователя, оставившего комментарий
    }
}

