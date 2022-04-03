namespace Timekeeper.DataModel
{
    public class Answer
    {
        public int Count { get; set; }

        public bool IsCorrect { get; set; }

        public string Letter { get; set; }

        public double Ratio { get; set; }

        public string TitleHtml { get; set; }

        public string TitleMarkdown { get; set; }

        public void Reset()
        {
            Count = 0;
            Ratio = 0.0;
        }
    }
}