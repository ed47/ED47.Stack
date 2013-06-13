namespace ED47.Stack.Web
{
    public class Country
    {
        public string Iso { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}