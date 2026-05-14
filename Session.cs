namespace SuperShop
{
    public static class Session
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;

        public static void Logout()
        {
            UserId = 0;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}
