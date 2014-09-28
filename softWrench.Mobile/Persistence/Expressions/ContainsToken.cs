namespace softWrench.Mobile.Persistence.Expressions
{
    public class ContainsToken : FilterExpression
    {
        private readonly string _value;

        public ContainsToken(string value)
        {
            _value = value;
        }

        public override string BuildSql()
        {
            return "((fields like '%' || ? || '%') or (fields like '%' || ? || '%'))";
        }

        public override string[] BuildParameters()
        {
            // We'll search either for a json field which
            // begins with the specified value or any word
            // beginning with the specified value (even if
            // it's located inside a sentence).
            return new string[]
            {
                string.Format("\":\"{0}", _value),
                string.Format(" {0}", _value)
            };
        }
    }
}

