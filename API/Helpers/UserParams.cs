using System;

namespace API.Helpers
{
    public class UserParams
    {
        private const int _maxPageSize = 50;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(value, _maxPageSize);
        }
        public string CurrentUserName { get; set; }
        public string Gender { get; set; }
        public int MaxAge { get; set; } = 150;
        public int MinAge { get; set; } = 18;
        public string OrderBy { get; set; } = "lastActive";
    }
}
