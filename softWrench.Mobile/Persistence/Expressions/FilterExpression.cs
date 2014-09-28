namespace softWrench.Mobile.Persistence.Expressions
{
    public abstract class FilterExpression
    {
        public abstract string BuildSql();
        public abstract string[] BuildParameters();
    }
}

