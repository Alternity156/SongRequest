using System.Linq;
using System.Text.RegularExpressions;

namespace AudicaModding
{
    public class ParsedTwitchMessage
    {
        public string BadgeInfo   = "";
        public string Badges      = "";
        public string Bits        = "";
        public string ClientNonce = "";
        public string Color       = "";
        public string DisplayName = "";
        public string Emotes      = "";
        public string Flags       = "";
        public string Id          = "";
        public string Message     = "";
        public string Mod         = "";
        public string RoomId      = "";
        public string TmiSentTs   = "";
        public string User        = "";
        public string UserId      = "";

        public ParsedTwitchMessage(string rawMsg)
        {
            string separator = ":";
            string tagSeparator = ";";

            // pre-process message, could contain flags that mess with the separators
            string[] components = Regex.Replace(rawMsg, "flags=(.*);", "").Split(separator.ToCharArray());

            string tags = components[0];
            User        = components[1];
            Message     = string.Join(":", components, 2, components.Length - 2);

            foreach (string str in tags.Split(tagSeparator.ToCharArray()).ToList())
            {
                if (str.Contains("badge-info="))
                {
                    BadgeInfo = str.Replace("badge-info=", "");
                }
                else if (str.Contains("badges="))
                {
                    Badges = str.Replace("badges=", "");
                }
                else if (str.Contains("bits="))
                {
                    Bits = str.Replace("bits=", "");
                }
                else if (str.Contains("client-nonce="))
                {
                    ClientNonce = str.Replace("client-nonce=", "");
                }
                else if (str.Contains("color="))
                {
                    Color = str.Replace("color=", "");
                }
                else if (str.Contains("display-name="))
                {
                    DisplayName = str.Replace("display-name=", "");
                }
                else if (str.Contains("emotes="))
                {
                    Emotes = str.Replace("emotes=", "");
                }
                else if (str.Contains("flags="))
                {
                    Flags = str.Replace("flags=", "");
                }
                else if (str.Substring(0, 3) == "id=")
                {
                    Id = str.Replace("id=", "");
                }
                else if (str.Contains("mod="))
                {
                    Mod = str.Replace("mod=", "");
                }
                else if (str.Contains("room-id="))
                {
                    RoomId = str.Replace("room-id=", "");
                }
                else if (str.Contains("tmi-sent-ts="))
                {
                    TmiSentTs = str.Replace("tmi-sent-ts=", "");
                }
                else if (str.Contains("user-id="))
                {
                    UserId = str.Replace("user-id=", "");
                }
            }
        }
    }
}



