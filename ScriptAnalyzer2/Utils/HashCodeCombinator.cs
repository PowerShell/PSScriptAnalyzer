namespace Microsoft.PowerShell.ScriptAnalyzer.Internal
{
    // From https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
    internal struct HashCodeCombinator
    {
        public static HashCodeCombinator Create()
        {
            return new HashCodeCombinator(iv: 17);
        }

        private int _hash;

        private HashCodeCombinator(int iv)
        {
            _hash = iv;
        }

        public HashCodeCombinator Add(int hashCode)
        {
            unchecked
            {
                _hash = 31 * _hash + hashCode;
                return this;
            }
        }

        public HashCodeCombinator Add(object obj)
        {
            unchecked
            {
                return obj == null
                    ? Add(17)
                    : Add(obj.GetHashCode());
            }
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }

}