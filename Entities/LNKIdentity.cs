namespace pyjump.Entities
{
    public class LNKOwner
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not LNKOwner other) return false;

            return (string.Equals(Name1, other.Name1, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Name2, other.Name2, StringComparison.OrdinalIgnoreCase))
                || (string.Equals(Name1, other.Name2, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Name2, other.Name1, StringComparison.OrdinalIgnoreCase));
        }

        public override int GetHashCode()
        {
            var name1 = Name1?.ToLowerInvariant() ?? "";
            var name2 = Name2?.ToLowerInvariant() ?? "";

            var ordered = new[] { name1, name2 }.OrderBy(x => x).ToArray();
            return HashCode.Combine(ordered[0], ordered[1]);
        }

    }
}
