namespace OpenAIVisionApp.Models
{
    public class AOAI_Response
    {
        public string id { get; set; }
        public Choice[] choices { get; set; }
    }

    public class Choice
    {
        public string finish_reason { get; set; }
        public int index { get; set; }
        public Message message { get; set; }
        public Enhancements enhancements { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Enhancements
    {
        public Grounding grounding { get; set; }
    }

    public class Grounding
    {
        public Line[] lines { get; set; }
        public string status { get; set; }
    }

    public class Line
    {
        public string text { get; set; }
        public Span[] spans { get; set; }
    }

    public class Span
    {
        public string text { get; set; }
        public int length { get; set; }
        public int offset { get; set; }
        public Polygon[] polygon { get; set; }
    }

    public class Polygon
    {
        public float x { get; set; }
        public float y { get; set; }
    }

}
