﻿namespace _25DH110278_MyStore.Models.ViewModel
{
    using System;
    using System.Collections.Generic;
    using PagedList;

    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public IPagedList<Product> Products { get; set; }
    }
}
