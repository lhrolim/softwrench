namespace softWrench.Mobile.Persistence.Expressions
{
    public class Exactly : FilterExpression
    {
        private readonly string _attribute;
        private readonly string _value;

        public Exactly(string attribute, string value)
        {
            _attribute = attribute;
            _value = value;
        }

        public override string BuildSql()
        {
            return "fields like '%' || ? || '%'";
        }

        public override string[] BuildParameters()
        {
            return new string[] { string.Format("\"{0}\":\"{1}\"", _attribute, _value) };
        }
    }   
}

