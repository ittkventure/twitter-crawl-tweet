namespace TK.Telegram.Domain.Service
{
    public static class TelegramMessageUltils
    {
        public static string ToEscapeMarkdownv2(string msg)
        {
            string escapedMsg = msg
                                .Replace("_", "\\_")
                                .Replace("*", "\\*")
                                .Replace("[", "\\[")
                                .Replace("]", "\\]")
                                .Replace("(", "\\(")
                                .Replace(")", "\\)")
                                .Replace("~", "\\~")
                                .Replace("`", "\\`")
                                .Replace(">", "\\>")
                                .Replace("#", "\\#")
                                .Replace("+", "\\+")
                                .Replace("-", "\\-")
                                .Replace("=", "\\=")
                                .Replace("|", "\\|")
                                .Replace("{", "\\{")
                                .Replace("}", "\\}")
                                .Replace(".", "\\.")
                                .Replace("!", "\\!");
            return escapedMsg;
        }

        public static string ToEscapeHtml(string msg)
        {
            string escapedMsg = msg
                                .Replace("<", "&lt;")
                                .Replace(">", "&gt;")
                                .Replace("&", "&amp;");
            return escapedMsg;
        }
    }
}