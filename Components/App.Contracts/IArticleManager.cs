﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Entities.Search;

namespace App.Contracts
{
    public interface IArticleManager
    {
        Task<IEnumerable<Article>> GetAll();
    }
}
